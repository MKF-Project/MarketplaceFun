using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private const string CAMERA_TAG = "MainCamera";

    private Vector3 _initalPosition = Vector3.zero;
    private Quaternion _initialRotation = Quaternion.identity;

    private GameObject _currentPlayer = null;

    private GameObject _playerCameraObject = null;


    private void Awake()
    {
        _initalPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    private void Update()
    {
        if(_playerCameraObject != null)
        {
            transform.position = _playerCameraObject.transform.position;
            transform.rotation = _playerCameraObject.transform.rotation;
        }

        if(MatchManager.Instance.MainPlayer == _currentPlayer)
        {
            return;
        }

        if(PlayerController.playerBehaviourEnabled && MatchManager.Instance.MainPlayer != null)
        {
            _currentPlayer = MatchManager.Instance.MainPlayer;
            _playerCameraObject = _currentPlayer.FindChildWithTag(CAMERA_TAG, false);
            if(_playerCameraObject != null)
            {
                transform.position = _playerCameraObject.transform.position;
                transform.rotation = _playerCameraObject.transform.rotation;

                return;
            }
        }

        _currentPlayer = null;
        _playerCameraObject = null;
        transform.position = _initalPosition;
        transform.rotation = _initialRotation;
    }
}
