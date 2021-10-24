using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;


public class ItemTypeList : MonoBehaviour
{
    public Sprite StartingCheckedImage;

    public static Sprite CheckedImage;

    public List<ItemType> StartingItemsList;

    public static Dictionary<ulong, ItemType> ItemList { get; private set; } = null;



    public void Awake()
    {
        if(ItemList != null)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.EnsureObjectDontDestroy();

        InitializeDictionary();

        CheckedImage = StartingCheckedImage;
    }

    private void InitializeDictionary()
    {
        ItemList = new Dictionary<ulong, ItemType>(StartingItemsList.Count);

        NetworkObject networkItemPlacement = null;

        StartingItemsList.ForEach(itemType => {
            itemType.ItemPrefab.TryGetComponent<NetworkObject>(out networkItemPlacement);
            var itemCode = networkItemPlacement != null? networkItemPlacement.PrefabHash : Item.NO_ITEMTYPE_CODE;

            if(ItemList.ContainsKey(itemCode))
            {
                #if UNITY_EDITOR
                    Debug.LogError($"[ItemTypeList]: Can't register {itemType.ItemPrefab.name} with ID {itemCode}, already registered to {ItemList[itemCode].ItemPrefab.name}");
                #endif

                return;
            }

            ItemList.Add(itemCode, itemType);

            networkItemPlacement = null;
        });
    }
}
