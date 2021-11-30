using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

[SelectionBase]
public class ShoppingCartItem : NetworkBehaviour
{
    private const string SHOPPING_CART_TAG = "ShoppingCart";
    private const string PLAYER_TAG = "Player";
    private const string ITEM_POSITIONS_TAG = "Item";
    private const string ITEM_TAG = "Item";
    private const float COLLISION_COOLDOWN = 2;

    internal const ulong NO_OWNER_ID = NetworkController.NO_CLIENT_ID;

    // Adding Items
    public Player Owner { get; private set; } = null;
    internal NetworkVariableULong _ownerID = new NetworkVariableULong(NO_OWNER_ID);

    private List<GameObject> _itemPositions;

    // List of items needs to be of fixed size
    // So we keep track of count in addition to capacity
    private int _itemCount = 0;
    private ulong[] _itemIDs;

    private float _lastCollision = 0;

    public MeshRenderer WheelsColor;

    private void Awake()
    {
        // Items
        _itemPositions = gameObject.FindChildrenWithTag(ITEM_POSITIONS_TAG);
        _itemIDs = new ulong[_itemPositions.Count];

        _ownerID.OnValueChanged += onOwnershipChanged;
    }

    private void OnDestroy()
    {
        _ownerID.OnValueChanged -= onOwnershipChanged;
    }

    /** ---- Items ---- **/
    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == ITEM_TAG)
        {
            if(IsClient && !IsServer)
            {
                additemToCart_ServerRpc(other.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }

            if(IsServer && Time.unscaledTime - _lastCollision > COLLISION_COOLDOWN)
            {
                updateCartOwnership(NetworkController.SelfID);
                addItemToCart(other.gameObject.GetComponent<Item>());
            }
        }

        // Acquire cart when touching it, if it has no owner and already has items inside
        else if(_ownerID.Value == NO_OWNER_ID && other.gameObject.tag == PLAYER_TAG && _itemCount > 0)
        {
            var playerScript = other.gameObject.GetComponent<Player>();
            if(playerScript == null || !playerScript.IsOwner)
            {
                return;
            }

            requestCartOwnership_ServerRpc();
        }
    }

    private void onOwnershipChanged(ulong previousOwner, ulong currentOwner)
    {
        // We use NO_OWNER_ID as placeholder for when the owner of this cart hasn't been set yet
        if(currentOwner != NO_OWNER_ID)
        {
            Owner = NetworkController.GetPlayerByID(_ownerID.Value);

            if(Owner != NetworkController.SelfPlayer)
            {
                return;
            }

            // Update player shopping list when acquiring ownership of a shopping cart
            // for this client's player
            Owner.TryGetComponent(out _shoppingListBuffer);
            for(int i = 0; i < _itemCount; i++)
            {
                _shoppingListBuffer.CheckItem(_itemIDs[i]);
            }

            /*
            if(shoppingList.IsListChecked())
            {
                Owner.ListComplete();
            }
            */
        }
    }

    private void addItemToCart(Item item)
    {
        if(item != null && IsServer)
        {
            item.DestroyItem_ClientRpc();

            var index = GetItemCartIndex(item);

            if(index != -1)
            {
                setNextItem_ClientRpc(item.ItemTypeCode, index);
                setNextItem(item.ItemTypeCode, index);
            }

            _lastCollision = Time.unscaledTime;
        }
    }

    private ShoppingList _shoppingListBuffer;
    private int GetItemCartIndex(Item item)
    {
        if(!IsServer)
        {
            return -1;
        }

        // Insert the item at the end, if we still have space for it
        if(_itemCount < _itemIDs.Length)
        {
            return _itemCount;
        }

        // If the player that threw the item into the cart also owns said cart,
        // we attempt to fit it in an advantageous position,
        // preferably not replacing items from the player's shopping list
        if(Owner.OwnerClientId == item.OwnerClientId)
        {
            if(_shoppingListBuffer == null)
            {
                if(!Owner.TryGetComponent(out _shoppingListBuffer))
                {
                    return -1;
                }
            }

            var preferredItems = _shoppingListBuffer.ItemDictionary.Keys;

            var previouslySeen = new HashSet<ulong>();
            for(int i = 0; i < _itemCount; i++)
            {
                // This item is not part of this player's shopping list
                if(!preferredItems.Contains(_itemIDs[i]))
                {
                    return i;
                }

                // This item is a duplicate of a item on the player's list
                if(!previouslySeen.Add(_itemIDs[i]))
                {
                    return i;
                }
            }
        }

        // If the player that threw the item was not the cart onwer,
        // or no suitable advantageous position was found,
        // we simply select a random position in the shopping cart
        return Random.Range(0, _itemCount);;

    }

    private void setNextItem(ulong itemTypeCode, int itemIndex)
    {
        // Destroy previous model if it is replacing a previous item
        if(itemIndex != _itemCount)
        {
            _itemPositions[itemIndex].transform.DestroyAllChildren();
        }

        // Update item logic
        if(Owner == NetworkController.SelfPlayer && IsClient)
        {
            // Uncheck player list if this was the only item of this type in the cart
            if(itemIndex != _itemCount && _itemIDs.Unique(code => code == _itemIDs[itemIndex]))
            {
                _shoppingListBuffer.UncheckItem(_itemIDs[itemIndex]);
            }

            _shoppingListBuffer.CheckItem(itemTypeCode);
            // Add item to player list and check if finished
            /*
            if(_shoppingListBuffer.CheckItem(itemTypeCode) && _shoppingListBuffer.IsListChecked())
            {
                Owner.ListComplete();
            }
            */
        }

        _itemIDs[itemIndex] = itemTypeCode;
        if(itemIndex == _itemCount)
        {
            _itemCount++;
        }

        // Create new mesh
        var itemPrefab = NetworkItemManager.NetworkItemPrefabs[itemTypeCode];
        var itemVisuals = itemPrefab?.GetComponent<Item>()?.ItemVisuals;

        if(itemVisuals != null)
        {
            var generatedItem = Instantiate(itemVisuals.gameObject, Vector3.zero, Quaternion.identity, _itemPositions[itemIndex].transform);

            generatedItem.transform.localPosition = Vector3.zero;
            generatedItem.transform.localRotation = Quaternion.identity;
            generatedItem.transform.localScale = itemVisuals.transform.localScale;
        }
    }

    private void updateCartOwnership(ulong playerID)
    {
        if(_ownerID.Value == NO_OWNER_ID)
        {
            // Player can only own one cart, the first cart it added an item to that didn't already have an owner
            if(GameObject.FindObjectsOfType<ShoppingCartItem>().Any(cart => cart._ownerID.Value == playerID))
            {
                return;
            }

            int colorNumber = NetworkManager.ConnectedClients[playerID].PlayerObject.GetComponent<PlayerInfo>().PlayerData.Color;
            UpdateCartColor(colorNumber);
            setCartColor_ClientRpc(colorNumber);

            _ownerID.Value = playerID;
        }
    }

    private void UpdateCartColor(int colorNumber)
    {
        Material material = ColorManager.Instance.GetColor(colorNumber);
        Material[] materials = WheelsColor.materials;
        materials[0] = material;
        WheelsColor.materials = materials;
    }

    /** ---- RPCs ---- **/
    [ServerRpc(RequireOwnership = false)]
    public void requestCartOwnership_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        updateCartOwnership(rpcReceiveParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void additemToCart_ServerRpc(ulong itemNetworkID, ServerRpcParams rpcReceiveParams = default)
    {

        if(Time.unscaledTime - _lastCollision > COLLISION_COOLDOWN)
        {
            // Acquire cart when adding the first item to it
            updateCartOwnership(rpcReceiveParams.Receive.SenderClientId);

            var item = NetworkObjects.GetNetworkObjectComponent<Item>(itemNetworkID);
            addItemToCart(item);
        }
    }

    [ClientRpc]
    private void setNextItem_ClientRpc(ulong itemTypeCode, int itemIndex)
    {
        if(!IsServer)
        {
            setNextItem(itemTypeCode, itemIndex);
        }
    }

    [ClientRpc]
    private void setCartColor_ClientRpc(int colorNumber)
    {
        UpdateCartColor(colorNumber);
    }
}
