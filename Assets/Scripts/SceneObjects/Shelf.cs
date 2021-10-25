using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shelf : MonoBehaviour
{
    public ItemGenerator ItemGenerator;

    private Player _playerBuffer;

    [HideInInspector]
    public ulong ItemTypeCode { get; private set; } = Item.NO_ITEMTYPE_CODE;

    private Action<Item> _itemAction = null;
    private Interactable _interactScript = null;

    // ShelfType

    private void Awake()
    {
        _interactScript = gameObject.GetComponentInChildren<Interactable>();

        if(_interactScript == null)
        {
            return;
        }

        _interactScript.OnLookEnter += ShowButtonPrompt;
        _interactScript.OnLookExit  += HideButtonPrompt;
        _interactScript.OnInteract  += GiveItemToPlayer;
    }

    private void OnDestroy()
    {
        if(_interactScript == null)
        {
            return;
        }

        _interactScript.OnLookEnter -= ShowButtonPrompt;
        _interactScript.OnLookExit  -= HideButtonPrompt;
        _interactScript.OnInteract  -= GiveItemToPlayer;
    }

    private void ShowButtonPrompt(GameObject player)
    {
        if(ItemGenerator.IsDepleted)
        {
            return;
        }

        var playerScript = player.GetComponent<Player>();
        if(playerScript != null && !playerScript.IsHoldingItem)
        {
            _interactScript.InteractUI.SetActive(true);
        }
    }

    private void HideButtonPrompt(GameObject player)
    {
        _interactScript.InteractUI.SetActive(false);
    }

    private void GiveItemToPlayer(GameObject player)
    {
        if(ItemGenerator.IsDepleted)
        {
            return;
        }

        if(player.TryGetComponent<Player>(out _playerBuffer))
        {
            _playerBuffer.HeldItemType.Value = ItemGenerator.TakeItem();
        }
    }
}
