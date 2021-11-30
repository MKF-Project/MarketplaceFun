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

    private void Awake()
    {
        _initalPosition = transform.position;
        _initialRotation = transform.rotation;

        SceneManager.OnSceneLoaded += DetachFromPlayer;
    }

    private void OnDestroy()
    {
        SceneManager.OnSceneLoaded -= DetachFromPlayer;
    }

    private void LateUpdate()
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
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.SetParent(playerCameraObject.transform, false);
                ShowPlayerHead(_currentPlayer);
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

    private void DetachFromPlayer(string sceneName)
    {
        if(_currentPlayer == NetworkController.SelfPlayer)
        {
            ShowPlayerHead(_currentPlayer);
            Destroy(gameObject);
        }
    }

    public static void HidePlayerHead(Player player)
    {
        // TODO: Implement
    }

    public static void ShowPlayerHead(Player player)
    {
        // TODO: Implement
    }
}
