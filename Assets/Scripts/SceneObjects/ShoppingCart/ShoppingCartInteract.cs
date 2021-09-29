using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class ShoppingCartInteract : NetworkBehaviour
{
    private Interactable _interactScript = null;

    private void Awake()
    {
        _interactScript = gameObject.GetComponentInChildren<Interactable>();

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

    private void grabCart(GameObject player)
    {
        print("TODO: Grab Cart!");
    }
}
