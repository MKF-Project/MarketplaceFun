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
    private bool _canShowButton;
    public  bool _canPickUpItem;
    public ItemGenerator _ItemGenerator;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _canShowButton = true;
        _canPickUpItem = false;
    }

    private void Update()
    {
        if (_player.IsHoldingItem)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                DropItem();
            }
        }else if (_canPickUpItem)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                _ItemGenerator.GetItem(PickItem);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner)
        {
            if (other.gameObject.CompareTag("ItemGenerator") && _canShowButton)
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

    public void PickItem(GameObject item)
    {
        _player.HoldItem(item);
        item.GetComponent<Item>().BeHeld(HeldPosition);
        PickItemButton.SetActive(false);
        _canShowButton = false;
    }

    public void DropItem()
    {
        _player.HoldingItem.GetComponent<Item>().BeDropped();
        _player.DropItem();
        _canShowButton = true;

    }
}
