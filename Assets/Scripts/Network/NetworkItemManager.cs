using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using UnityEngine;

public class NetworkItemManager : MonoBehaviour
{
    private static NetworkItemManager _instance = null;
    private static Dictionary<string, GameObject> SpawnedItemList = null;

    private void Awake()
    {
        if(_instance != null)
        {
            // NetworkItemManager is NOT the primary script on this GameObject,
            // so it only Destroys itself when it
            // detects a singleton already present
            Destroy(this);

            return;
        }

        _instance = this;
        SpawnedItemList = new Dictionary<string, GameObject>();

        gameObject.EnsureObjectDontDestroy();
    }

    private void OnDestroy()
    {
        if(_instance == this)
        {
            _instance = null;
            SpawnedItemList = null;
        }
    }

    public static void RegisterItem(ulong prefabHash, ulong id, GameObject item)
    {


        string stringifiedKey = StringifyKey(prefabHash, id);
        if (!SpawnedItemList.ContainsKey(stringifiedKey))
        {
            SpawnedItemList.Add(stringifiedKey, item);
        }
        #if UNITY_EDITOR
            else
            {
                Debug.LogError($"Add same Item {prefabHash} - {id}" );
            }
        #endif
    }

    public static void UnregisterItem(ulong prefabHash, ulong id)
    {
        string stringifiedKey = StringifyKey(prefabHash, id);
        if (!SpawnedItemList.Remove(stringifiedKey))
        {
            #if UNITY_EDITOR
                Debug.LogError($"Item is not registered {prefabHash} - {id}" );
            #endif
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
