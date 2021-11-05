using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

/** ---- UNUSED AT THE MOMENT ---- **/
public class Put : NetworkBehaviour
{
    private Player _player;
    private ShoppingList _shoppingList;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _shoppingList = GetComponent<ShoppingList>();
    }

    public override void NetworkStart()
    {
        if (!IsOwner)
        {
            Destroy(this);
        }
    }

    public void OnPut()
    {
        if (_player.IsHoldingItem)
        {
            bool isChecked = CheckItem(_player.HeldItemType.Value);
            if (isChecked)
            {
                if (_shoppingList.IsListChecked())
                {
                    //_player.ListComplete();
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