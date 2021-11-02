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
    private float _lastInteraction;
    private float _timeStuck;

    protected override void Awake()
    {
        base.Awake();

        _freezerDoor = transform.Find(DOOR_NAME).gameObject;

        _timeStuck = DoorStuckDuration;
        _lastInteraction = -_timeStuck;

        IsOpen.OnValueChanged = OpenDoor;
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
        }

        else
        {
            _freezerDoor.transform.Rotate(0, -90, 0, Space.Self);
        }
    }

    private IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(StayOpenDuration);

        // Start Closing animation

        yield return new WaitForSeconds(CloseDoorDuration);

        IsOpen.Value = false;

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
            StartCoroutine(CloseDoor());
        }

        else
        {
            _timeStuck -= TimeSkippedPerClick;

            // UI SIGNAL / DOOR SHAKE
            DoorShake_ClientRpc();
        }
    }

    [ClientRpc]
    private void DoorShake_ClientRpc()
    {

    }
}
