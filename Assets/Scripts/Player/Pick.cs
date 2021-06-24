using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pick : MonoBehaviour
{
    private Player _player;
    public Transform HeldPosition;
    public GameObject PickItemButton;
    private bool _canShowButton;
    private bool _canPickUpItem;
    public GameObject _pickableItem;

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
                PickItem(_pickableItem);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Item") && _canShowButton)
        {
            _canPickUpItem = true;
            _pickableItem = other.gameObject;
            PickItemButton.SetActive(true);
        }
        
    }
    

    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Item") && !_player.IsHoldingItem)
        {
            if (Input.GetKey(KeyCode.E))
            {
                PickItem(other.gameObject);
            }
        }
    }*/
    
    

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Item"))
        {
            _canPickUpItem = false;
            _pickableItem = null;
            PickItemButton.SetActive(false);
        }
    }
    

    private void PickItem(GameObject item)
    {
        _player.HoldItem(item);
        item.GetComponent<Item>().BeHeld(HeldPosition);
        PickItemButton.SetActive(false);
        _canShowButton = false;
    }

    private void DropItem()
    {
        _player.HoldingItem.GetComponent<Item>().BeDropped();
        _player.DropItem();
        _canShowButton = true;

    }
}
