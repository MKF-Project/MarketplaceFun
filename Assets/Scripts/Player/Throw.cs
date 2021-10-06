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
    private bool _shouldThrow = false;

    public float Strength;
    public float Distance;

    private void Awake()
    {
        _player = GetComponent<Player>();

    }

    private void FixedUpdate()
    {
        if(_shouldThrow && IsOwner && _player.IsHoldingItem)
        {
            var initialPosition = _player.HoldingItem.transform.position;
            var target = CalculateTargetPosition();

            ThrowItem(target, initialPosition);
        }

        _shouldThrow = false;
    }

    public void OnThrow()
    {
        if(IsOwner && _player.IsHoldingItem)
        {
            _shouldThrow = true;
        }
    }

    private void ThrowItem(Vector3 target, Vector3 initialPosition)
    {
        GameObject holdingItem = _player.HoldingItem;
        var itemRigidbody = holdingItem.GetComponent<Rigidbody>();
        itemRigidbody.velocity = Vector3.zero;
        
        holdingItem.transform.position = initialPosition;
        holdingItem.transform.LookAt(target);
        holdingItem.SetActive(true);
        holdingItem.GetComponent<Item>().IsOnThrow = true;    
        _player.DropItem();

        Vector3 direction = target - initialPosition;
        itemRigidbody.velocity = direction.normalized * Strength;

    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 screenMiddle = new Vector3();
        screenMiddle.x = Screen.width / 2f;
        screenMiddle.y = Screen.height / 2f;
        Ray ray = Camera.main.ScreenPointToRay(screenMiddle);

        Vector3 target;
        #if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * Distance, Color.green);
        #endif
        
        bool raycastHit = Physics.Raycast(ray.origin, ray.direction, out var hitInfo, Distance);
        if (raycastHit)
        {
            
            target = hitInfo.point;
        }
        else
        {
            target = ray.origin + ray.direction * Distance;    
        }
        
        return target;
    }

}
