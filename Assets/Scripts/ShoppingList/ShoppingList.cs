using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class ShoppingList : NetworkBehaviour
{
    public Dictionary<ulong, ShoppingListItem> ItemDictionary;
    //public List<ShoppingListItem> ItemList;
    public ShoppingListUI ShoppingListUi;

    private int _quantityChecked;

    private int _randomSeed;

    // Start is called before the first frame update
    void Start()
    {
        _quantityChecked = 0;
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();

        if (IsOwner)
        {
            ShoppingListUi = GameObject.FindGameObjectsWithTag("ShoppingListUI")[0].GetComponent<ShoppingListUI>();
            Debug.Log("Colocou Owner");
            NetworkController.OnDisconnected += EraseListClient;

        }

        if (IsServer)
        {
            Debug.Log("Colocou Server");
            _randomSeed = (int) Time.time;
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
        GenerateList(5, ItemTypeList.ItemList.Keys.ToList());
    }


    //Only on server
    public void GenerateList(int numberOfItems, List<ulong> itemListKeys)
    {
        var random = new Random(_randomSeed);

        while (numberOfItems > 0)
        {
            int randomIndex = random.Next(0, itemListKeys.Count);

            ShoppingListItem shoppingListItem = new ShoppingListItem(itemListKeys[randomIndex]);

            //Debug.Log(GetComponent<NetworkObject>().OwnerClientId + ": " + allItemsList[randomIndex].Name);

            ItemDictionary.Add(itemListKeys[randomIndex], shoppingListItem);
            itemListKeys.RemoveAt(randomIndex);
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
            Debug.Log("Im owner: " + GetComponent<NetworkObject>().OwnerClientId);
            ShoppingListUi.FillUIItems(itemList);
        }
    }

    public void EraseListServer(ulong playerId)
    {
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();
    }

    public void EraseListClient(bool wasHost, bool connectionWasLost)
    {
        ItemDictionary = new Dictionary<ulong, ShoppingListItem>();
        if (IsOwner)
        {
            ShoppingListUi.EraseItems();
        }
    }

    public bool CheckItem(ulong itemCode)
    {
        if(!ItemDictionary.ContainsKey(itemCode))
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
        _quantityChecked++;
        return true;
    }

    public void UncheckItem(ulong itemCode)
    {
        if(!ItemDictionary.ContainsKey(itemCode))
        {
            return;
        }

        ShoppingListItem listItem = ItemDictionary[itemCode];
        listItem.Caught = false;
        ItemDictionary.Remove(itemCode);
        ItemDictionary.Add(itemCode, listItem);
        ShoppingListUi.UncheckItem(itemCode);
        _quantityChecked--;
    }


    public bool IsListChecked()
    {
        if (_quantityChecked == ItemDictionary.Count)
        {
            return true;
        }

        return false;
    }

    // public void PrintList()
    // {

    //     String msg = "";
    //     foreach (ShoppingListItem item in ItemDictionary.Values)
    //     {
    //         msg +=  ", " + ItemTypeList.ItemList[item.ItemCode].Name;
    //     }
    //     MatchMessages.Instance.EditMessage(msg, 10f);
    //     MatchMessages.Instance.ShowMessage();
    // }

}
