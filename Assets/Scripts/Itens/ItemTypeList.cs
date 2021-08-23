using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemTypeList : MonoBehaviour
{
    public List<ItemType> StartingItemsList;

    public static List<ItemType> ItemList;

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ItemList = StartingItemsList;
    }
    
}
