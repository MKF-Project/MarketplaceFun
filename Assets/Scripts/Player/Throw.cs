using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Throw : NetworkBehaviour
{
    private Player _player;
    private Item _itemToThrow = null;

    
    public float Strength;
    public float Distance;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        if(IsOwner && _itemToThrow != null)
        {
            OnThrowItem();
        }

        _itemToThrow = null;
    }

    public void ThrowItem(Item item)
    {
        if(IsOwner)
        {
            _itemToThrow = item;
        }
    }

    private void OnThrowItem()
    {
        var target = CalculateTargetPosition();

        var itemRigidbody = _itemToThrow.GetComponent<Rigidbody>();
        itemRigidbody.velocity = Vector3.zero;

        _itemToThrow.transform.LookAt(target);
        _itemToThrow.gameObject.SetActive(true);
        _itemToThrow.IsOnThrow = true;
        _itemToThrow.ThrowerId = OwnerClientId;
            
        var direction = target - _itemToThrow.transform.position;
        itemRigidbody.velocity = direction.normalized * Strength;
    }

    private Vector3 CalculateTargetPosition()
    {
        var screenMiddle = new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0);
        var ray = Camera.main.ScreenPointToRay(screenMiddle);

        #if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * Distance, Color.green);
        #endif

        var raycastHit = Physics.Raycast(ray.origin, ray.direction, out var hitInfo, Distance);
        return raycastHit? hitInfo.point : ray.origin + ray.direction * Distance;
    }

}
