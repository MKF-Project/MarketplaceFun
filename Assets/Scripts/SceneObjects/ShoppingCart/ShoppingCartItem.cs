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

    private int _nextIndex = 0;

    private int[] _itemCodes;
    private bool[] _occupiedPositions;
    private List<GameObject> _itemPositions;

    private float _lastCollision = 0;

    private void Awake()
    {
        // Items
        _itemPositions = gameObject.FindChildrenWithTag(ITEM_POSITIONS_TAG);
        _itemCodes = new int[_itemPositions.Count];
        _occupiedPositions = new bool[_itemPositions.Count];

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
        else if(_ownerID.Value == NO_OWNER_ID && other.gameObject.tag == PLAYER_TAG && _occupiedPositions.Any(pos => pos))
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
            var shoppingList = Owner.GetComponent<ShoppingList>();
            for(int i = 0; i < _occupiedPositions.Length; i++)
            {
                if(_occupiedPositions[i])
                {
                    shoppingList.CheckItem(_itemCodes[i]);
                }
            }

            if(shoppingList.IsListChecked())
            {
                Owner.ListComplete();
            }
        }
    }

    private void addItemToCart(Item item)
    {
        if(item != null && IsServer)
        {
            item.DestroyItem_ClientRpc();

            setNextItem_ClientRpc(item.ItemTypeCode);
            setNextItem(item.ItemTypeCode);
            _lastCollision = Time.unscaledTime;
        }
    }

    private void setNextItem(int itemTypeCode)
    {
        // Destroy previous model
        _itemPositions[_nextIndex].transform.DestroyAllChildren();

        // Update item logic
        if(Owner == NetworkController.SelfPlayer && IsClient)
        {
            var shoppingList = Owner.GetComponent<ShoppingList>();

            // Uncheck player list if this was the only item of this type in the cart
            if(_itemCodes.Unique(code => code == _itemCodes[_nextIndex]))
            {
                shoppingList.UncheckItem(_itemCodes[_nextIndex]);
            }

            // Add item to player list and check if finished
            if(shoppingList.CheckItem(itemTypeCode) && shoppingList.IsListChecked())
            {
                Owner.ListComplete();
            }
        }

        _occupiedPositions[_nextIndex] = true;
        _itemCodes[_nextIndex] = itemTypeCode;

        // Create new mesh
        var itemPrefab = ItemTypeList.ItemList[itemTypeCode].ItemPrefab;
        var itemVisuals = itemPrefab?.GetComponent<Item>()?.ItemVisuals;

        if(itemVisuals != null)
        {
            var generatedItem = Instantiate(itemVisuals.gameObject, Vector3.zero, Quaternion.identity, _itemPositions[_nextIndex].transform);

            generatedItem.transform.localPosition = Vector3.zero;
            generatedItem.transform.localRotation = Quaternion.identity;
            generatedItem.transform.localScale = itemVisuals.transform.localScale;
        }

        _nextIndex = (_nextIndex + 1) % _itemPositions.Count;
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

            _ownerID.Value = playerID;
        }
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
    private void setNextItem_ClientRpc(int itemTypeCode)
    {
        if(!IsServer)
        {
            setNextItem(itemTypeCode);
        }
    }
}
