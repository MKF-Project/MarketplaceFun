using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private const string PLAYER_CAMERA_NAME = "CameraPosition";
    private const string PLAYER_OVERVIEW_CAMERA_NAME = "OverviewCameraPosition";

    private Vector3 _initalPosition = Vector3.zero;
    private Quaternion _initialRotation = Quaternion.identity;

    /* Player related variables */
    private Player _currentPlayer = null;

    public bool CameraOnPlayer;
    public LayerMask OverviewCollision;

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
        if(CameraOnPlayer)
        {
            SetCameraOnPlayer();
        }

    }

    public void SetCameraOnPlayer()
    {
        // Current player is not the same as Local player
        if(PlayerManager.AllowPlayerControls && NetworkController.SelfPlayer != null)
        {
            _currentPlayer = NetworkController.SelfPlayer;
            var playerCameraObject = _currentPlayer.transform.Find(PLAYER_CAMERA_NAME).gameObject;

            if(playerCameraObject != null)
            {
                // Clear older cameras that might've been present on the player
                playerCameraObject.DestroyAllChildren();

                transform.SetParent(playerCameraObject.transform, false);

                transform.localRotation = Quaternion.identity;
                transform.localPosition = Vector3.zero;

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

    public void SetCameraOnPlayerOverview()
    {
        if(NetworkController.SelfPlayer == null || _currentPlayer != NetworkController.SelfPlayer)
        {
            return;
        }

        var playerOverviewObject = _currentPlayer.transform.Find(PLAYER_OVERVIEW_CAMERA_NAME).gameObject;
        if(playerOverviewObject != null)
        {
            ShowPlayerHead(_currentPlayer);

            transform.SetParent(playerOverviewObject.transform, false);

            transform.localPosition = Vector3.zero;
            transform.LookAt(_currentPlayer.transform);
        }

        #if UNITY_EDITOR
            else
            {
                Debug.LogError($"[{gameObject.name}]: Player ({_currentPlayer.name}) has no Camera Overview placeholder object");
            }
        #endif
    }

    public void SetCameraOnScene()
    {
        if (_currentPlayer != null)
        {
            ShowPlayerHead(_currentPlayer);
            _currentPlayer = null;
        }

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
