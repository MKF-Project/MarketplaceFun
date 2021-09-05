using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public bool IsListComplete;

    private const string HELP_POSITION_NAME = "HeldPosition";
    public Transform HeldPosition {get; private set;}

    public GameObject HoldingItem;
    public bool IsHoldingItem;


    public override void NetworkStart()
    {
        if (IsOwner)
        {
            MatchManager.Instance.MainPlayer = this;
        }
    }

    private void Awake()
    {
        HeldPosition = gameObject.transform.Find(HELP_POSITION_NAME);

        IsHoldingItem = false;
        IsListComplete = false;
    }

    public void HoldItem(GameObject item)
    {
        HoldingItem = item;
        item.GetComponent<Item>().BeHeld(HeldPosition);
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
