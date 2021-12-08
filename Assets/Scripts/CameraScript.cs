using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private const string CAMERA_POSITION_NAME = "CameraPosition";

    private Vector3 _initalPosition = Vector3.zero;
    private Quaternion _initialRotation = Quaternion.identity;

    /* Player related variables */
    private Player _currentPlayer = null;

    public bool CameraOnPlayer;

    private void Awake()
    {
        _initalPosition = transform.position;
        _initialRotation = transform.rotation;
        MatchManager.OnMatchExit += SetCameraOnScene;

    }

    private void OnDestroy()
    {
        MatchManager.OnMatchExit -= SetCameraOnScene;
    }


    private void Start()
    {
        if (CameraOnPlayer)
        {
            SetCameraOnPlayer();
        }

    }

    //
    //private void LateUpdate()
    private void SetCameraOnPlayer()
    {
        if(_currentPlayer == NetworkController.SelfPlayer)
        {
            return;
        }

        // Current player is not the same as Local player
        if(PlayerManager.AllowPlayerControls && NetworkController.SelfPlayer != null)
        {
            _currentPlayer = NetworkController.SelfPlayer;
            var playerCameraObject = _currentPlayer.transform.Find(CAMERA_POSITION_NAME).gameObject;

            if(playerCameraObject != null)
            {
                // Clear older cameras that might've been present on the player
                playerCameraObject.DestroyAllChildren();

                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.SetParent(playerCameraObject.transform, false);

                HidePlayerHead(_currentPlayer);
                return;
            }

            #if UNITY_EDITOR
                else
                {
                    Debug.LogError($"[{gameObject.name}]: Player ({_currentPlayer.name}) has no Camera placeholder object");
                }
            #endif
        }

        // Reset Player variables if no valid Local player was found
        _currentPlayer = null;

        transform.position = _initalPosition;
        transform.rotation = _initialRotation;
    }

    public void SetCameraOnScene()
    {
        ShowPlayerHead(_currentPlayer);
        _currentPlayer = null;

        transform.SetParent(null);
        transform.position = _initalPosition;
        transform.rotation = _initialRotation;
    }



    public static void HidePlayerHead(Player player)
    {
        if(player.TryGetComponent(out PlayerDisplay display))
        {
            display.PlayerHeadComponents.ForEach(mesh => mesh.enabled = false);
        }
    }

    public static void ShowPlayerHead(Player player)
    {
        if(player.TryGetComponent(out PlayerDisplay display))
        {
            display.PlayerHeadComponents.ForEach(mesh => mesh.enabled = true);
        }
    }
}
