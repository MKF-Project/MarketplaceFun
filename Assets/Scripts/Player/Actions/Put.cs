using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class Put : NetworkBehaviour
{
    private Player _player;
    private ShoppingList _shoppingList;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _shoppingList = GetComponent<ShoppingList>();
        InputController.OnPut += OnPut;
    }


    public override void NetworkStart()
    {
        if (!IsOwner)
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        InputController.OnPut -= OnPut;
    }

    public void OnPut()
    {
        if (_player.IsHoldingItem)
        {
            bool isChecked = CheckItem(_player.GetItemComponent().ItemTypeCode);
            if (isChecked)
            {
                if (_shoppingList.IsListChecked())
                {
                    _player.ListComplete();
                }

                _player.DropItem();
            }
        }
    }

    public bool CheckItem(int itemTypeCode)
    {
        return _shoppingList.CheckItem(itemTypeCode);
    }


}