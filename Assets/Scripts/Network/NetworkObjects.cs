using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;

public static class NetworkObjects
{
    private static IEnumerable<GameObject> _internalFindNetworkObject(ulong networkID, bool includeInactive = false, GameObject root = null)
    {
        NetworkObject[] netObjects;

        if(root == null)
        {
            netObjects = GameObject.FindObjectsOfType<NetworkObject>();
        }
        else
        {
            netObjects = root.transform.GetComponentsInChildren<NetworkObject>(includeInactive);
        }

        return netObjects
            .Where(obj => obj.NetworkObjectId == networkID)
            .Select(obj => obj.gameObject);
    }

    public static GameObject GetNetworkObject(ulong networkID, bool includeInactive = false, GameObject root = null)
    {
        return _internalFindNetworkObject(networkID, includeInactive, root).FirstOrDefault();
    }

    public static T GetNetworkObjectComponent<T>(ulong networkID, bool includeInactive = false, GameObject root = null)
    {
        return _internalFindNetworkObject(networkID, includeInactive, root).Select(obj => obj.GetComponent<T>()).FirstOrDefault();
    }
}
