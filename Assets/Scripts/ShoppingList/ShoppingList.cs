using System;
using System.Collections.Generic;
using System.Linq;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class ShoppingList : NetworkBehaviour
{
    public Dictionary<int, ShoppingListItem> ItemDictionary;
    //public List<ShoppingListItem> ItemList;
    public ShoppingListUI ShoppingListUi;

    // Start is called before the first frame update
    void Start()
    {
        ItemDictionary = new Dictionary<int, ShoppingListItem>();
        
        if (IsOwner)
        {
            ShoppingListUi = GameObject.FindGameObjectsWithTag("ShoppingListUI")[0].GetComponent<ShoppingListUI>();
            Debug.Log("Colocou Owner");    
            NetworkController.OnDisconnected += EraseListClient;
        }
        
        if (IsServer)
        {
            Debug.Log("Colocou Server");    

            SceneManager.OnMatchLoaded += GenerateList_OnMatchLoaded;
            NetworkController.OnOtherClientDisconnected += EraseListServer;
        }
    }
    
    private void OnDestroy()
    {
        //Didn't verify IsOwner or IsClient(bug if verify)
        NetworkController.OnDisconnected -= EraseListClient;
        SceneManager.OnMatchLoaded -= GenerateList_OnMatchLoaded;
        NetworkController.OnOtherClientDisconnected -= EraseListServer;
    }
    
    public void GenerateList_OnMatchLoaded(string _sceneName)
    {
        GenerateList(5, ItemTypeList.ItemList.ToList());
    }


    //Only on server
    public void GenerateList(int numberOfItems, List<ItemType> allItemsList)
    {
        Random random = new Random();
        while (numberOfItems > 0)
        {
            int randomIndex = random.Next(0, allItemsList.Count);
            Debug.Log(randomIndex);
            ShoppingListItem shoppingListItem = new ShoppingListItem(allItemsList[randomIndex].Code);
            ItemDictionary.Add(allItemsList[randomIndex].Code, shoppingListItem);
            allItemsList.RemoveAt(randomIndex);
            numberOfItems--;
        }
        
        SerializedShoppingList serializedShoppingList = new SerializedShoppingList(ItemDictionary.Values.ToList());
        ReceiveList_ClientRpc(serializedShoppingList);
        
    }
    
    [ClientRpc]
    //Populate List on Client RPC
    public void ReceiveList_ClientRpc(SerializedShoppingList serializedShoppingList)
    {
        List<ShoppingListItem> itemList = serializedShoppingList.Array.ToList();
        if (!IsServer)
        {
            foreach (ShoppingListItem item in itemList)
            {
                ItemDictionary.Add(item.ItemCode, item);
            }
        }

        if (IsOwner)
        {
            ShoppingListUi.FillUIItems(itemList);
        }
    }

    public void EraseListServer(ulong playerId)
    {
        ItemDictionary = new Dictionary<int, ShoppingListItem>();
    }

    public void EraseListClient(bool wasHost, bool connectionWasLost)
    {
        ItemDictionary = new Dictionary<int, ShoppingListItem>();
        if (IsOwner)
        {
            ShoppingListUi.EraseItems();
        }
    }

    public bool CheckItem(int itemCode)
    {
        if (!IsOnList(itemCode))
        {
            return false;
        }
        ShoppingListItem listItem = ItemDictionary[itemCode];
        if (listItem.Caught)
        {
            return false;
        }

        listItem.Caught = true;
        ItemDictionary.Remove(itemCode);
        ItemDictionary.Add(itemCode, listItem);
        ShoppingListUi.CheckItem(itemCode);
        return true;
    }

    public void UncheckItem(int itemCode)
    {
        ShoppingListItem listItem = ItemDictionary[itemCode];
        listItem.Caught = false;
        ItemDictionary.Remove(itemCode);
        ItemDictionary.Add(itemCode, listItem);
        ShoppingListUi.UncheckItem(itemCode);
    }

    public bool IsOnList(int itemCode)
    {
        return ItemDictionary.ContainsKey(itemCode);
    }




}
