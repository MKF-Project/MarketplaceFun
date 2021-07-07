using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject HoldingItem;


    public bool IsHoldingItem; //{get;  set; }

    private void Awake()
    {
        IsHoldingItem = false;
    }

    public void HoldItem(GameObject item)
    {
        HoldingItem = item;
        IsHoldingItem = true;
    }

    public void DropItem()
    {
        HoldingItem = null;
        IsHoldingItem = false;
    }


}
