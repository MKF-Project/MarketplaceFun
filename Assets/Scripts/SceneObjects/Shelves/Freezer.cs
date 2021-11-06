using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

[SelectionBase]
public class Freezer : Shelf
{
    private const string ANIM_DOOR_OPEN = "OpenDoor";
    private const string ANIM_DOOR_CLOSE = "CloseDoor";
    private const string ANIM_DOOR_SHAKE = "ShakeDoor";

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

    // These might be runtime changeable in the future, but if so, this would require
    // networking and NetworkVariables. so for the time being these are
    // private, so that they aren't used in other scrripts without the proper
    // networking framework.
    [SerializeField] private float StayOpenDuration;
    [SerializeField] private float CloseDoorDuration;
    [SerializeField] private float DoorStuckDuration;
    [SerializeField] private float TimeSkippedPerClick;
    private Animator _animator;
    private float _lastInteraction;
    private float _timeStuck;
    private bool _isClosing = false;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();

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
            _animator.SetTrigger(ANIM_DOOR_OPEN);
            StartCoroutine(CloseDoor());
        }
    }

    private IEnumerator CloseDoor()
    {
        yield return new WaitForSeconds(StayOpenDuration);

        // Match the animator speed with the desired close duration
        _animator.speed = 1f / CloseDoorDuration;
        _animator.SetTrigger(ANIM_DOOR_CLOSE);

        yield return new WaitForSeconds(CloseDoorDuration);

        // Reset animator speed afterwards
        _animator.speed = 1f;

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
        _animator.SetTrigger(ANIM_DOOR_SHAKE);
    }
}
