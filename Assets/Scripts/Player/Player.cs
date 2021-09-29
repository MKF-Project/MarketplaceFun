using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public bool IsListComplete;

    private const string HELD_POSITION_NAME = "HeldPosition";
    private Throw _throwScript = null;

    public Transform HeldPosition {get; private set;}

    public GameObject HoldingItem {get; private set;}
    public bool IsHoldingItem {get; private set;}

    
    
    public override void NetworkStart()
    {
        if(IsOwner)
        {
            transform.Rotate(Vector3.up, 180);
            MatchManager.Instance.MainPlayer = this;
        }
    }

    private void Awake()
    {
        _throwScript = GetComponent<Throw>();
        HeldPosition = gameObject.transform.Find(HELD_POSITION_NAME);

        #if UNITY_EDITOR
            if(_throwScript == null)
            {
                Debug.LogError($"[{gameObject.name}]: Could not find Throw Script");
            }

            if(HeldPosition == null)
            {
                Debug.LogError($"[{gameObject.name}]: Could not find Held Position");
            }
        #endif

        IsHoldingItem = false;
        IsListComplete = false;
    }
    
    

    public void HoldItem(GameObject item)
    {
        HoldingItem = item;
        item.GetComponent<Item>().BeHeld(HeldPosition);
        IsHoldingItem = true;
        AimCanvas.Instance.ActivateAim();
    }

    public void ThrowItem()
    {
        _throwScript.OnThrow();
    }

    public void DropItem()
    {
        HoldingItem.GetComponent<Item>().BeDropped();
        HoldingItem = null;
        IsHoldingItem = false;
        AimCanvas.Instance.DisableAim();
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
