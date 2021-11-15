using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    private const string FILL_CIRCLE_PATH = "InteractObject/InteractCanvas/FillCircle";

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
    // private, so that they aren't used in other scripts without the proper
    // networking framework.
    [SerializeField] private float StayOpenDuration;
    [SerializeField] private float CloseDoorDuration;
    [SerializeField] private float DoorStuckDuration;
    [SerializeField] private float TimeSkippedPerClick;
    private Animator _animator;
    private Image _fillCircle;
    // private float _lastInteraction;
    private float _stuckTimeLeft;
    private float _setStuckTime;
    private bool _isClosing = false;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();

        _fillCircle = transform.Find(FILL_CIRCLE_PATH).GetComponent<Image>();
        _fillCircle.enabled = false;
        _fillCircle.fillAmount = 0;

        _stuckTimeLeft = DoorStuckDuration;
        _setStuckTime = -DoorStuckDuration;
        // _lastInteraction = -_stuckTimeLeft;

        DoorState.OnValueChanged = HandleDoorState;
    }

    private void Update()
    {
        // Unset stuck from freezer door if it has waited enough time
        if(DoorState.Value == FreezerDoorState.Stuck)
        {
            var secondsStuck = Time.time - _setStuckTime;
            _fillCircle.fillAmount = secondsStuck / _stuckTimeLeft;

            if(IsServer && secondsStuck > _stuckTimeLeft)
            {
                DoorState.Value = FreezerDoorState.Closed;
            }
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
        _fillCircle.fillAmount = 0;

        switch(after)
        {
            case FreezerDoorState.Closed:
                _stuckTimeLeft = DoorStuckDuration;

                _animator.SetBool(ANIM_DOOR_STUCK, false);

                _fillCircle.enabled = false;
                break;

            case FreezerDoorState.Open:
                _stuckTimeLeft = DoorStuckDuration;

                _animator.SetBool(ANIM_DOOR_STUCK, false);
                _animator.SetTrigger(ANIM_DOOR_OPEN);

                _fillCircle.enabled = false;

                StartCoroutine(CloseDoor());
                break;

            case FreezerDoorState.Stuck:
                _animator.SetBool(ANIM_DOOR_STUCK, true);

                _fillCircle.enabled = true;
                _setStuckTime = Time.time;
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
            _setStuckTime = Time.time;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDoorOpen_ServerRpc()
    {
        if(DoorState.Value == FreezerDoorState.Closed)
        {
            DoorState.Value = FreezerDoorState.Open;
        }

        else if(DoorState.Value == FreezerDoorState.Stuck)
        {
            FailedAttemptDoorOpen_ClientRpc();
        }
    }

    [ClientRpc]
    private void FailedAttemptDoorOpen_ClientRpc()
    {
        _stuckTimeLeft -= TimeSkippedPerClick;
        print($"Open attempt. time = {_stuckTimeLeft}");

        // UI SIGNAL / DOOR SHAKE
        _animator.SetTrigger(ANIM_DOOR_SHAKE);
    }
}
