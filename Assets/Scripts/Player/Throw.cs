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
    public float ArcThrow;

    //public Camera Camera;

    //public GameObject InstantiatedBomb;
    // Start is called before the first frame update
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

            PerformThrow(target, initialPosition);
        }

        _shouldThrow = false;
    }

    private void PerformThrow(Vector3 target, Vector3 initialPosition)
    {
        var itemRigidbody = _player.HoldingItem.GetComponent<Rigidbody>();
        itemRigidbody.velocity = Vector3.zero;
        _player.HoldingItem.transform.position = initialPosition;
        _player.HoldingItem.SetActive(true);

        _player.DropItem();

        itemRigidbody.AddForce(target, ForceMode.Impulse);
    }

    public void ThrowItem()
    {
        if(IsOwner && _player.IsHoldingItem)
        {
            _shouldThrow = true;
        }
    }

    private Vector3 CalculateTargetPosition()
    {
        Vector3 screenMiddle = new Vector3();
        screenMiddle.x = Screen.width / 2;
        screenMiddle.y = Screen.height / 2;
        Ray ray = Camera.main.ScreenPointToRay(screenMiddle);

        Vector3 target = ray.direction * Strength;
        target += Vector3.up * ArcThrow;
        return target;
    }

}
