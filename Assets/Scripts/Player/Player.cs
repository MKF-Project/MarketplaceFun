using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public bool IsListComplete;
    
    public GameObject HoldingItem;

    public bool IsHoldingItem;
    
    public Transform HeldPosition;


    public override void NetworkStart()
    {
        if (IsOwner)
        {
            MatchManager.Instance.MainPlayer = this;
        }
    }

    private void Awake()
    {
        IsHoldingItem = false;
        IsListComplete = false;
    }

    public void HoldItem(GameObject item)
    {
        HoldingItem = item;
        IsHoldingItem = true;
    }

    public void DropItem()
    {
        HoldingItem.GetComponent<Item>().BeDropped();
        HoldingItem = null;
        IsHoldingItem = false;
    }


    public Item GetItemComponent()
    {
        return HoldingItem.GetComponent<Item>();
    }

    public void ListComplete()
    {
        MatchMessages.Instance.EditMessage("Your list is complete");
        MatchMessages.Instance.ShowMessage();
        IsListComplete = true;
        
    }
}
