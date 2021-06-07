using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerController : NetworkBehaviour
{
    private Camera _playerCamera;

    private void Awake()
    {
        _playerCamera = gameObject.GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        if(IsOwner) {
            _playerCamera.enabled = true;
            ObjectsManager.OverviewCamera?.SetActive(false);
        }
    }

    private void OnDestroy() {
        if(IsOwner) {
            ObjectsManager.OverviewCamera?.SetActive(true);
        }
    }
}
