using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class ShoppingListGenerator : NetworkBehaviour
{

    //Penasr em fazer Shopping List Client e um ShoppingListServer
    public Dictionary<ulong, ShoppingList> ClientsShoppingLists;

    public static ShoppingListGenerator Instance;
    //This class only works on the server side
    void Awake()
    {
        if (IsClient)
        {
            Destroy(this);
        }

        Instance = this;
    }
    
    
    [ServerRpc]
    public void RequestList_ServerRpc(ulong clientId)
    {
        
    }
}
