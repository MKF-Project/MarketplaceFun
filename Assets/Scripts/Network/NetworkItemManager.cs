using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class NetworkItemManager : NetworkBehaviour
{
    
    private static Dictionary<string, GameObject> SpawnedItemList = null;
    // Start is called before the first frame update
    void Awake()
    {
        SpawnedItemList = SpawnedItemList ?? new Dictionary<string, GameObject>();
    }

    // Update is called once per frame
    void OnDestroy()
    {
        SpawnedItemList = null;
    }

    
    public static void RegisterItem(ulong prefabHash, ulong id, GameObject item)
    {

        string stringifiedKey = StringifyKey(prefabHash, id);
        if (!SpawnedItemList.ContainsKey(stringifiedKey))
        {
            SpawnedItemList.Add(stringifiedKey, item);
        }

        else
        {
            Debug.LogError($"Add same Item {prefabHash} - {id}" );
        }

    }


    public static void UnregisterItem(ulong prefabHash, ulong id)
    {
        string stringifiedKey = StringifyKey(prefabHash, id);
        if (!SpawnedItemList.Remove(stringifiedKey))
        {

            Debug.LogError($"Item is not registered {prefabHash} - {id}" );

        }

    }

    private static string StringifyKey(ulong prefabHash, ulong id)
    {
        return $"{prefabHash}{id}";
    }

    public static GameObject GetNetworkItem(ulong prefabHash, ulong id)
    {
        print("S " + prefabHash + " - " + id);

        return SpawnedItemList[StringifyKey(prefabHash, id)];
    }


}
