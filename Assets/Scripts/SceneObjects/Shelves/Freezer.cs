using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public enum FreezerDoorState
{
    Closed,
    Open,
    Stuck
}

[SelectionBase]
public class Freezer : Shelf
{
    // Door Movement
    private const string ANIM_DOOR_OPEN = "OpenDoor";
    private const string ANIM_DOOR_CLOSE = "CloseDoor";
    private const string ANIM_DOOR_SHAKE = "ShakeDoor";

    // UI
    private const string ANIM_DOOR_STUCK = "DoorStuck";

    [HideInInspector]
    public NetworkVariable<FreezerDoorState> DoorState = new NetworkVariable<FreezerDoorState>
    (
        new NetworkVariableSettings
        {
            ReadPermission = NetworkVariablePermission.Everyone,
            WritePermission = NetworkVariablePermission.ServerOnly
        },
        FreezerDoorState.Closed
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
    private float _stuckTimeLeft;
    private bool _isClosing = false;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();

        _stuckTimeLeft = DoorStuckDuration;
        _lastInteraction = -_stuckTimeLeft;

        DoorState.OnValueChanged = HandleDoorState;
    }

    private void Update()
    {
        // Unset stuck from freezer door if it has waited enough time
        if(IsServer && DoorState.Value == FreezerDoorState.Stuck && Time.time - _lastInteraction > _stuckTimeLeft)
        {
            _stuckTimeLeft = DoorStuckDuration;
            DoorState.Value = FreezerDoorState.Closed;
        }
    }

    protected override void ShowButtonPrompt(GameObject player)
    {
        // If the door is open, we defer to the default Shelf script
        // which decides wether or not to show UI based on
        // item availability in the generator
        if(DoorState.Value == FreezerDoorState.Open)
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
        // Defer to base shelf script when the time comes
        // to request a generated item
        if(DoorState.Value == FreezerDoorState.Open)
        {
            base.InteractWithShelf(player);
        }

        // Otherwise, if the door is closed
        // we first request it to be opened
        else if(player.TryGetComponent<Player>(out _playerBuffer) && _playerBuffer.CanInteract)
        {
            RequestDoorOpen_ServerRpc();
        }
    }

    private void HandleDoorState(FreezerDoorState before, FreezerDoorState after)
    {
        switch(after)
        {
            case FreezerDoorState.Closed:
                _animator.SetBool(ANIM_DOOR_STUCK, false);
                break;

            case FreezerDoorState.Open:
                _animator.SetBool(ANIM_DOOR_STUCK, false);
                _animator.SetTrigger(ANIM_DOOR_OPEN);
                StartCoroutine(CloseDoor());
                break;

            case FreezerDoorState.Stuck:
                _animator.SetBool(ANIM_DOOR_STUCK, true);
                break;
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
            DoorState.Value = FreezerDoorState.Stuck;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDoorOpen_ServerRpc()
    {
        if(DoorState.Value == FreezerDoorState.Closed)
        {
            _lastInteraction = Time.time;

            DoorState.Value = FreezerDoorState.Open;
        }

        else if(DoorState.Value == FreezerDoorState.Stuck)
        {
            _stuckTimeLeft -= TimeSkippedPerClick;

            print($"Open attempt. time = {_stuckTimeLeft}");

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
