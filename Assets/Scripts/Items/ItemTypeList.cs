using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemTypeList : MonoBehaviour
{
    public Sprite StartingCheckedImage;
    
    public static Sprite CheckedImage;
    
    public List<ItemType> StartingItemsList;

    public static List<ItemType> ItemList;

    

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        PopulatePrefabCodes();
        ItemList = StartingItemsList;
        CheckedImage = StartingCheckedImage;
    }

    public void PopulatePrefabCodes()
    {
        foreach (ItemType itemType in StartingItemsList)
        {
            itemType.ItemPrefab.GetComponent<Item>().ItemTypeCode = itemType.Code;
        }
    }
    

}
