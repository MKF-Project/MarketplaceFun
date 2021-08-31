using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class Pick : NetworkBehaviour
{
    private Player _player;
    public Transform HeldPosition;
    public GameObject PickItemButton;
    public  bool _canPickUpItem;
    public ItemGenerator _ItemGenerator;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _canPickUpItem = false;

        InputController.OnInteractOrThrow += OnInteract;
    }

    private void OnDestroy()
    {
        InputController.OnInteractOrThrow -= OnInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner)
        {
            if (other.gameObject.CompareTag("ItemGenerator") && !_player.IsHoldingItem)
            {
                _canPickUpItem = true;
                _ItemGenerator = other.gameObject.GetComponent<ItemGenerator>();
                PickItemButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsOwner)
        {
            if (other.gameObject.CompareTag("ItemGenerator"))
            {
                PickItemButton.SetActive(false);
                _canPickUpItem = false;
                _ItemGenerator = null;
            }
        }
    }

    private void OnInteract()
    {
        if(_player.IsHoldingItem)
        {
            DropItem();
        }
        else if(_canPickUpItem)
        {
            _ItemGenerator.GetItem(PickItem);
        }
    }

    public void PickItem(GameObject item)
    {
        _player.HoldItem(item);
        item.GetComponent<Item>().BeHeld(HeldPosition);
        PickItemButton.SetActive(false);
    }

    public void DropItem()
    {
        _player.DropItem();

    }
}
