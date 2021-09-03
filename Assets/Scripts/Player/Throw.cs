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

    public float ArcThrow;

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
                Vector3 initialPosition = _player.HoldingItem.transform.position;

                Vector3 target = CalculateTargetPosition();

                // if (InputManager.PressFireButton())
                // {
                ThrowItem(target, initialPosition);
                // }
            }
        }
    }

    //[ServerRpc]
    public void ThrowItem(Vector3 target, Vector3 initialPosition)
    {
        Rigidbody itemRigidbody = _player.HoldingItem.GetComponent<Rigidbody>();
        itemRigidbody.velocity = Vector3.zero;
        _player.HoldingItem.transform.position = initialPosition;
        _player.HoldingItem.SetActive(true);
        _pick.DropItem();
        itemRigidbody.AddForce(target, ForceMode.Impulse);
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
