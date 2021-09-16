﻿using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Serialization;
using UnityEngine;

public class Item : NetworkBehaviour
{
    private Transform _heldPosition;
    private bool _isHeld;
    private NetworkObject _networkObject;
    public int ItemTypeCode;

    
    //Object needs to be registered not before NetworkStart, like Awake
    //Because before this the object doesn't have an networkId
    public override void NetworkStart()
    {
        _networkObject = GetComponent<NetworkObject>();
        RegisterItem();
    }

    void Start()
    {
        _isHeld = false;
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

    void Update()
    {
        if (_isHeld)
        {
            transform.position = _heldPosition.position;
            transform.forward = _heldPosition.forward;
            Debug.DrawRay(_heldPosition.position, _heldPosition.forward* 10, Color.red);
        }
    }

    public void BeHeld(Transform holderPosition)
    {
        _heldPosition = holderPosition;
        _isHeld = true;
    }

    public void BeDropped()
    {
        _heldPosition = null;
        _isHeld = false;
    }
    


}
