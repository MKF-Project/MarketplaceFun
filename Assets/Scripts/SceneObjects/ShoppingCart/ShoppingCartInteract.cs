using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;

public class ShoppingCartInteract : NetworkBehaviour
{
    private const string SHOPPING_CART_TAG = "ShoppingCart";
    private const string SHOPPING_CART_POSITION_NAME = "ShoppingCartPosition";

    private Interactable _interactScript = null;
    private ShoppingCartItem _cartScript = null;

    private NetworkTransform _netTransform = null;
    private NetRigidbody _netRigidbody = null;

    private Rigidbody _rigidbody = null;
    private RigidbodyTemplate _rigidBodyTemplate;

    private Transform _cartPlayerPosition;

    [SerializeField]
    private PhysicMaterial _playerBodyMaterial = null;

    private List<Collider> _cartColliders;
    private List<PhysicMaterial> _cartMaterials;

    private void Awake()
    {
        _interactScript = GetComponentInChildren<Interactable>();
        _cartScript = GetComponent<ShoppingCartItem>();

        _netTransform = GetComponent<NetworkTransform>();
        _netRigidbody = GetComponent<NetRigidbody>();

        _rigidbody = GetComponent<Rigidbody>();

        // Store list of Physics materials
        _cartColliders = new List<Collider>(7);
        _rigidbody.GetComponentsInChildren<Collider>(false, _cartColliders);
        _cartColliders.RemoveAll(collider => collider.isTrigger == true);

        _cartMaterials = _cartColliders.Select(collider => collider.material).ToList();

        _interactScript.OnLookEnter += showButtonPrompt;
        _interactScript.OnLookExit += hideButtonPrompt;
        _interactScript.OnInteract += grabCart;
    }

    private void OnDestroy()
    {
        _interactScript.OnLookEnter -= showButtonPrompt;
        _interactScript.OnLookExit -= hideButtonPrompt;
        _interactScript.OnInteract -= grabCart;
    }

    /** ---- Interaction ---- **/
    private void showButtonPrompt(GameObject player)
    {
        // Show UI if not holding item or driving a shopping cart
        var playerScript = player.GetComponent<Player>();
        if(playerScript != null && playerScript.CanInteract)
        {
            _interactScript.InteractUI.SetActive(true);
        }
    }

    private void hideButtonPrompt(GameObject player)
    {
        _interactScript.InteractUI.SetActive(false);
    }

    private void grabCart(GameObject playerObject)
    {
        var playerScript = playerObject.GetComponent<Player>();

        // Cannot grab cart if it is owned by a different player than the current player
        if((_cartScript.Owner != null && _cartScript.Owner != playerScript))
        {
            return;
        }

        // Ensure the player has Network Ownership of the cart that has been interacted with
        if(OwnerClientId != playerObject.GetComponent<NetworkObject>().OwnerClientId)
        {
            _netRigidbody.RequestObjectOwnership_ServerRpc();
        }

        // If this cart has no onwer, and the player doesn't yet own a cart, the player now ows it
        if(_cartScript.Owner == null && GameObject.FindObjectsOfType<ShoppingCartItem>().None(cart => cart._ownerID.Value == playerScript.OwnerClientId))
        {
            print($"[{gameObject.name}] Ownership Request: {playerScript.OwnerClientId}");
            _cartScript.requestCartOwnership_ServerRpc();
        }

        attachCart_ServerRpc();

        clientAttachCart(playerScript);
        hideButtonPrompt(playerObject);
    }

    public void DetachCartFromPlayer(Player player)
    {
        // Can only request detaching for self
        if(!player.IsOwner)
        {
            return;
        }

        detachCart_ServerRpc();

        clientDetachCart(player);
    }

    private void clientAttachCart(Player player)
    {
        if(player == null)
        {
            return;
        }

        _cartPlayerPosition = player.transform.Find(SHOPPING_CART_POSITION_NAME);

        // Do not allow interaction if the player is already holding another cart
        if(_cartPlayerPosition.FindChildWithTag(SHOPPING_CART_TAG) != null)
        {
            return;
        }

        player.IsDrivingCart = true;

        // We disable this cart's Network Transform and Rigidbody
        // because while the cart is contained in the player object
        // we let the player prefab handle network Positioning and Physics
        _netTransform.enabled = false;
        _netRigidbody.enabled = false;

        // Similarly, we'll not simply DISABLE, but rather REMOVE
        // the Rigidbody component from the shopping cart while it's
        // part of the Player, so that the Player's rigidbody is in
        // direct control of the cart's colliders
        _rigidBodyTemplate = _rigidbody.ExtractToTemplate();
        _rigidbody = null;

        // Disallow interacting with a cart that's currently in the hands of a player
        _interactScript.enabled = false;

        // Move the cart gameObject to the player
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        transform.SetParent(_cartPlayerPosition, false);

        // Update cart Physics Materials
        _cartColliders.ForEach(collider => collider.material = _playerBodyMaterial);

        // Update player Controls
        player.GetComponent<PlayerControls>().switchControlScheme();

        // Make sure we detach the cart from this player if they disconnect
        player.OnBeforeDestroy += clientDetachCart;
    }

    private void clientDetachCart(Player player)
    {
        player.IsDrivingCart = false;

        // Decouple cart from player
        transform.SetParent(null);
        player.OnBeforeDestroy -= clientDetachCart;

        // Reacquire a rigidbody component with the same
        // configurations as the one that was previously removed
        _rigidbody = _rigidBodyTemplate.ImportFromTemplate(gameObject);

        // Realow cart interaction
        _interactScript.enabled = true;

        // Reenable network components
        _netTransform.enabled = true;
        _netRigidbody.enabled = true;

        // Return Physics materials to default
        for(int i = 0; i < _cartColliders.Count; i++)
        {
            _cartColliders[i].material = _cartMaterials[i];
        }

        // Make sure the cart position is synced between clients
        _netTransform.Teleport(_cartPlayerPosition.position, _cartPlayerPosition.rotation);

        // Keep cart momentum from player
        if(IsOwner)
        {
            var velocity = player.GetComponent<Rigidbody>().velocity;
            _rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        // Update player controls
        foreach (var controlScript in player.GetComponents<PlayerControls>())
        {
            if(controlScript.isActiveAndEnabled)
            {
                controlScript.switchControlScheme();
                break;
            }
        }
    }

    /** ---- RPCs ---- **/
    [ServerRpc(RequireOwnership = false)]
    private void attachCart_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        // Echo attach cart request back to other players
        attachCart_ClientRpc(rpcReceiveParams.Receive.SenderClientId, rpcReceiveParams.ReturnRpcToOthers());
    }

    [ClientRpc]
    private void attachCart_ClientRpc(ulong playerID, ClientRpcParams clientRpcParams = default)
    {
        if(playerID != NetworkController.SelfID)
        {
            clientAttachCart(NetworkController.GetPlayerByID(playerID));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void detachCart_ServerRpc(ServerRpcParams rpcReceiveParams = default)
    {
        // Echo detach cart request back to other players
        detachCart_ClientRpc(rpcReceiveParams.Receive.SenderClientId, rpcReceiveParams.ReturnRpcToOthers());
    }

    [ClientRpc]
    private void detachCart_ClientRpc(ulong playerID, ClientRpcParams clientRpcParams = default)
    {
        if(playerID != NetworkController.SelfID)
        {
            clientDetachCart(NetworkController.GetPlayerByID(playerID));
        }
    }
}
