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
    private Pick _pick;

    public float Strength;

    public float Distance;
    
    public bool _isOn;

    //public Camera Camera;

    //public GameObject InstantiatedBomb;
    // Start is called before the first frame update
    private void Awake()
    {
        _player = GetComponent<Player>();
        _pick = GetComponent<Pick>();
        SceneManager.OnMatchLoaded += TurnOn;
        _isOn = false;
        InputController.OnInteractOrThrow += OnThrow;


    }

    private void OnDestroy()
    {
        SceneManager.OnMatchLoaded -= TurnOn;
        InputController.OnInteractOrThrow -= OnThrow;

    }

    private void TurnOn(string sceneName)
    {
        _isOn = true;
    }

    public void OnThrow()
    {
        if (IsOwner && _isOn)
        {
            if (_player.IsHoldingItem)
            {
                Vector3 initialPosition = _player.HeldPosition.position;

                Vector3 target = CalculateTargetPosition();

             
                ThrowItem(target, initialPosition); 
            }
        }
    }

    //[ServerRpc]
    public void ThrowItem(Vector3 target, Vector3 initialPosition)
    {
        Rigidbody itemRigidbody = _player.HoldingItem.GetComponent<Rigidbody>();
        itemRigidbody.velocity = Vector3.zero;
        _player.HoldingItem.transform.position = initialPosition;
        _player.HoldingItem.transform.LookAt(target);
        _player.HoldingItem.SetActive(true);
        _pick.DropItem();
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
