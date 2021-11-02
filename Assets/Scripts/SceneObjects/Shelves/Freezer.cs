using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class Freezer : Shelf
{
    private const string DOOR_NAME = "Door";

    [HideInInspector]
    public NetworkVariableBool IsOpen = new NetworkVariableBool
    (
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        },
        false
    );

    public float StayOpenDuration;
    public float CloseDoorDuration;
    public float DoorStuckDuration;
    public float TimeSkippedPerClick;

    private GameObject _freezerDoor;
    private Quaternion _startingRotation;
    private float _lastInteraction;
    private float _timeStuck;
    private bool _isClosing = false;

    protected override void Awake()
    {
        base.Awake();

        _freezerDoor = transform.Find(DOOR_NAME).gameObject;
        _startingRotation = _freezerDoor.transform.localRotation;

        _timeStuck = DoorStuckDuration;
        _lastInteraction = -_timeStuck;

        IsOpen.OnValueChanged = OpenDoor;
    }

    private void Update()
    {
        if(_isClosing)
        {
            _freezerDoor.transform.Rotate(0, (-90 * Time.deltaTime) / CloseDoorDuration, 0, Space.Self);
        }
    }

    protected override void ShowButtonPrompt(GameObject player)
    {
        if(IsOpen.Value)
        {
            base.ShowButtonPrompt(player);
        }

        else if(player.TryGetComponent<Player>(out _playerBuffer) && _playerBuffer.CanInteract)
        {
            _interactScript.InteractUI.SetActive(true);
        }
    }

    protected override void InteractWithShelf(GameObject player)
    {
        if(IsOpen.Value)
        {
            base.InteractWithShelf(player);
        }

        else if(player.TryGetComponent<Player>(out _playerBuffer) && _playerBuffer.CanInteract)
        {
            RequestDoorOpen_ServerRpc();
        }
    }

    private void OpenDoor(bool before, bool after)
    {
        if(after)
        {
            _freezerDoor.transform.Rotate(0, 90, 0, Space.Self);
            // VFXs...

            StartCoroutine(CloseDoor());
        }

        else
        {
            _freezerDoor.transform.localRotation = _startingRotation;
        }
    }

    private IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(StayOpenDuration);

        _isClosing = true;
        yield return new WaitForSeconds(CloseDoorDuration);

        _isClosing = false;
        if(IsServer)
        {
            IsOpen.Value = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDoorOpen_ServerRpc()
    {
        // Can open freezer without waiting
        if(Time.time - _lastInteraction > _timeStuck)
        {
            _timeStuck = DoorStuckDuration;
            _lastInteraction = Time.time;

            // OPEN
            IsOpen.Value = true;
        }

        else
        {
            _timeStuck -= TimeSkippedPerClick;
            print($"Open attempt. time = {_timeStuck}");

            // UI SIGNAL / DOOR SHAKE
            DoorShake_ClientRpc();
        }
    }

    [ClientRpc]
    private void DoorShake_ClientRpc()
    {
        print("Shake Door");
    }
}
