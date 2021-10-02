using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;

public class ShoppingCartInteract : NetworkBehaviour
{
    private const string SHOPPING_CART_POSITION_NAME = "ShoppingCartPosition";

    private Interactable _interactScript = null;
    private ShoppingCartItem _cartScript = null;

    private NetworkTransform _netTransform = null;
    private NetRigidbody _netRigidbody = null;

    private Rigidbody _rigidbody = null;
    private RigidbodyTemplate _rigidBodyTemplate;

    private void Awake()
    {
        _interactScript = GetComponentInChildren<Interactable>();
        _cartScript = GetComponent<ShoppingCartItem>();

        _netTransform = GetComponent<NetworkTransform>();
        _netRigidbody = GetComponent<NetRigidbody>();

        _rigidbody = GetComponent<Rigidbody>();

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
        print("TODO: Grab Cart!");

        var playerScript = playerObject.GetComponent<Player>();

        // Cannot grab cart if it is owned by a different player than the current player
        if((_cartScript.Owner != null && _cartScript != playerScript))
        {
            return;
        }

        // Ensure the player has Network Ownership of the cart that has been interacted with
        if(OwnerClientId != playerObject.GetComponent<NetworkObject>().OwnerClientId)
        {
            _netRigidbody.RequestObjectOwnership_ServerRpc();
        }

        // If this cart has no onwer, this player now ows it
        if(_cartScript.Owner == null)
        {
            _cartScript.requestCartOwnership_ServerRpc();
        }

        attachCartToPlayer(playerScript);
    }

    private void attachCartToPlayer(Player player)
    {
        var cartPosition = player.transform.Find(SHOPPING_CART_POSITION_NAME);

        // We disable this cart's Network Transform and Rigidbody
        // beacause while the cart is contained in the player object
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

        transform.SetParent(cartPosition, false);

        // Update player Controls
        player.GetComponent<PlayerControls>().switchControlScheme();
    }
}
