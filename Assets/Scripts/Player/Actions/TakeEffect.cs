using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class TakeEffect : ScorableAction
{
    private Animator _animator;
    private Player _playerScript;

    private static readonly int RECEIVE_HIT = Animator.StringToHash("P_Recebeu_Golpe");

    private bool _isTakingEffect;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponentInChildren<Animator>();

        if(!TryGetComponent(out _playerScript))
        {
            #if UNITY_EDITOR
                Debug.LogError($"[{name}/TakeEffect]: Player Script not found!");
            #endif
        }

        _isTakingEffect = false;
    }

    public void OnTakeEffect(int effectCode, ulong throwerId)
    {
        switch(effectCode)
        {
            case  0:
                NormalEffect_ServerRpc(throwerId);
                break;

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NormalEffect_ServerRpc(ulong throwerId)
    {
        if(!_isTakingEffect)
        {
            NetworkManager.ConnectedClients[throwerId].PlayerObject.GetComponent<PlayerScore>().ScoreAction(_scoreType);
            _isTakingEffect = true;
            NormalEffect_ClientRpc();

        }
    }

    [ClientRpc]
    public void NormalEffect_ClientRpc()
    {
        // Only freeze the controls for the player who actually got hit
        if(IsOwner)
        {
            InputController.FreezePlayerControls();

            _playerScript.DropItem();
            _playerScript.ReleaseCart();
        }
        _animator.SetTrigger(RECEIVE_HIT);
    }

    private void EnableMovement()
    {
        if(IsOwner)
        {
            InputController.UnfreezePlayerControls();
            NoTakingEffect_ServerRpc();
        }
    }

    [ServerRpc]
    public void NoTakingEffect_ServerRpc()
    {
        _isTakingEffect = false;
    }
}
