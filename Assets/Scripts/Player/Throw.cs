using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Throw : NetworkBehaviour
{
    public GameObject BombPrefab;
    private GameObject _bomb;
    private Rigidbody _bombRigidbody;
    public float Strength;

    public float ArcThrow;

    private bool _isOn;
    
    //public Camera Camera;

    //public GameObject InstantiatedBomb;
    // Start is called before the first frame update
    private void Awake()
    {
        SceneManager.OnMatchLoaded += SpawnBomb;
        _isOn = false;
    }

    private void OnDestroy()
    {
        SceneManager.OnMatchLoaded -= SpawnBomb;
    }

    private void SpawnBomb(string sceneName)
    {
        if (IsServer)
        {
            _bomb = Instantiate(BombPrefab, Vector3.zero, Quaternion.identity);
            _bomb.GetComponent<NetworkObject>().Spawn();
            _bombRigidbody = _bomb.GetComponent<Rigidbody>();
            _bomb.SetActive(false);
        }

        _isOn = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsOwner && _isOn)
        {
            Vector3 initialPosition =  new Vector3(transform.position.x, transform.position.y + 3, transform.position.z);
            Vector3 target = CalculateTargetPosition();

            if (InputManager.PressFireButton())
            {
                ThrowServerRpc(target, initialPosition);
            }
        }
    }

    [ServerRpc]
    public void ThrowServerRpc(Vector3 target, Vector3 initialPosition)
    {
        _bombRigidbody.velocity = Vector3.zero;
        _bomb.transform.position = initialPosition;
        _bomb.SetActive(true);
        _bombRigidbody.AddForce(target, ForceMode.Impulse);
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
