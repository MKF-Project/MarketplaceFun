using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAPI;

public static class NetworkObjects
{
    /** External **/
    public static GameObject GetNetworkObject(ulong networkID, bool includeInactive = false, GameObject root = null)
    {
        return _internalFindNetworkObject(networkID, includeInactive, root).FirstOrDefault();
    }

    public static T GetNetworkObjectComponent<T>(ulong networkID, bool includeInactive = false, GameObject root = null)
    {
        return _internalFindNetworkObject(networkID, includeInactive, root).Select(obj => obj.GetComponent<T>()).FirstOrDefault();
    }

    public static GameObject GetNetworkObjectInSet(ulong networkID, IEnumerable<GameObject> set, bool includeInactive = false)
    {
        return _internalFindNetworkObjectInSet(networkID, set).Where(obj => obj.activeSelf != includeInactive).FirstOrDefault();
    }

    public static T GetNetworkObjectComponentInSet<T>(ulong networkID, IEnumerable<GameObject> set, bool includeInactive = false)
    {
        return _internalFindNetworkObjectInSet(networkID, set).Where(obj => obj.activeSelf != includeInactive).Select(obj => obj.GetComponent<T>()).FirstOrDefault();
    }

    /** Internal **/
    private static IEnumerable<GameObject> _internalFindNetworkObject(ulong networkID, bool includeInactive = false, GameObject root = null)
    {
        IEnumerable<NetworkObject> netObjects;

        if(root == null)
        {
            netObjects = GameObject.FindObjectsOfType<NetworkObject>().Where(netObj => netObj.gameObject.activeSelf != includeInactive);
        }
        else
        {
            netObjects = root.transform.GetComponentsInChildren<NetworkObject>(includeInactive).AsEnumerable();
        }

        return _internalFindNetworkObjectInSet(networkID, netObjects);
    }

    private static IEnumerable<GameObject> _internalFindNetworkObjectInSet(ulong networkID, IEnumerable<GameObject> set)
    {
        return _internalFindNetworkObjectInSet(networkID, set.Select(obj => obj.GetComponent<NetworkObject>()).Where(obj => obj != null));
    }

    private static IEnumerable<GameObject> _internalFindNetworkObjectInSet(ulong networkID, IEnumerable<NetworkObject> set)
    {
        return set.Where(obj => obj.NetworkObjectId == networkID).Select(obj => obj.gameObject);
    }
}
