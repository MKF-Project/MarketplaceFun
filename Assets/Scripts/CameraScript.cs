using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private const string CAMERA_POSITION_NAME = "CameraPosition";

    private Vector3 _initalPosition = Vector3.zero;
    private Quaternion _initialRotation = Quaternion.identity;

    /* Player related variables */
    private GameObject _currentPlayer = null;

    private void Awake()
    {
        _initalPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if(MatchManager.Instance.MainPlayer == _currentPlayer)
        {
            return;
        }

        // Current player is not the same as Local player
        if(PlayerManager.AllowPlayerControls && MatchManager.Instance.MainPlayer != null)
        {
            _currentPlayer = MatchManager.Instance.MainPlayer;
            var playerCameraObject = _currentPlayer.transform.Find(CAMERA_POSITION_NAME).gameObject;
            #if UNITY_EDITOR
                if(playerCameraObject == null)
                {
                    Debug.LogError($"[{gameObject.name}]: Player ({_currentPlayer.name}) has no Camera placeholder object");
                }
            #endif

            if(playerCameraObject != null)
            {
                transform.position = Vector3.zero;
                transform.rotation = Quaternion.identity;
                transform.SetParent(playerCameraObject.transform, false);
                return;
            }
        }

        // Reset Player variables if no valid Local player was found
        _currentPlayer = null;

        transform.position = _initalPosition;
        transform.rotation = _initialRotation;
    }


}
