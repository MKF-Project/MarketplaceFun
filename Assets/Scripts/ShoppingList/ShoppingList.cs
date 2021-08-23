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
    public List<ShoppingListItem> ItemList;
    public ShoppingListUI ShoppingListUi;

    // Start is called before the first frame update
    void Start()
    {
        
        if (IsOwner)
        {
            ShoppingListUi = GameObject.FindGameObjectsWithTag("ShoppingListUI")[0].GetComponent<ShoppingListUI>();
            Debug.Log("Colocou Owner");    
            NetworkController.OnDisconnected += EraseListClient;
        }
        
        ItemList = new List<ShoppingListItem>();
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
            ItemList.Add(shoppingListItem);
            allItemsList.RemoveAt(randomIndex);
            numberOfItems--;
        }
        Debug.Log(ItemList.Count);
        SerializedShoppingList serializedShoppingList = new SerializedShoppingList(ItemList);
        ReceiveList_ClientRpc(serializedShoppingList);
        
    }
    
    [ClientRpc]
    //Populate List on Client RPC
    public void ReceiveList_ClientRpc(SerializedShoppingList serializedShoppingList)
    {
        List<ShoppingListItem> itemList = serializedShoppingList.Array.ToList();
        
        ItemList = itemList;
        Debug.Log(ItemList.Count);
        if (IsOwner)
        {
            ShoppingListUi.FillUIItems(itemList);
        }
    }

    public void EraseListServer(ulong playerId)
    {
        ItemList = new List<ShoppingListItem>();
    }

    public void EraseListClient(bool wasHost, bool connectionWasLost)
    {
        ItemList = new List<ShoppingListItem>();
        if (IsOwner)
        {
            ShoppingListUi.EraseItems();
        }
    }



}
