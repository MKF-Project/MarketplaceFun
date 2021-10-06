using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class Item : NetworkBehaviour
{
    public const int NO_ITEMTYPE_CODE = int.MinValue;

    private NetworkObject _networkObject;

    [HideInInspector]
    public bool IsOnThrow;

    [HideInInspector]
    public int ItemTypeCode;


    public int EffectType;

    public TakeEffect takeEffect;
    //Object needs to be registered not before NetworkStart, like Awake
    //Because before this the object doesn't have an networkId
    public override void NetworkStart()
    {
        IsOnThrow = false;
        EffectType = 0;
        _networkObject = GetComponent<NetworkObject>();
        RegisterItem();
    }

    private void OnDestroy()
    {
        UnregisterItem();
    }

    private void RegisterItem()
    {
        NetworkItemManager.RegisterItem(_networkObject.PrefabHash, _networkObject.NetworkObjectId, gameObject);
    }

    private void UnregisterItem()
    {
        NetworkItemManager.UnregisterItem(_networkObject.PrefabHash, _networkObject.NetworkObjectId);
    }

    private void Update()
    {
        if (IsOnThrow)
        {
            if (gameObject.GetComponent<Rigidbody>().velocity.sqrMagnitude <= 0.1)
            {
                TriggerDestroyItem();
            }
        }
    }

    public GameObject GetItemVisuals()
    {
        // TODO update this function when we have better structured visuals
        return transform.Find("Cube")?.gameObject;
    }

    public void OnCollisionEnter(Collision other)
    {
        if (IsOnThrow)
        {
            GameObject hitObject = other.gameObject;
            if (hitObject.CompareTag("Player"))
            {
                takeEffect = hitObject.GetComponent<TakeEffect>();
                takeEffect.OnTakeEffect(0);
                TriggerDestroyItem();
            }
        }
    }

    private void TriggerDestroyItem()
    {
        IsOnThrow = false;
        StartCoroutine(nameof(DestroyAfterSeconds), 4f);
    }

    private IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        DestroyItem_ServerRpc();
    }

    [ServerRpc]
    public void DestroyItem_ServerRpc()
    {
        DestroyItem_ClientRpc();
    }

    [ClientRpc]
    public void DestroyItem_ClientRpc()
    {
        Destroy(gameObject);
    }
}
