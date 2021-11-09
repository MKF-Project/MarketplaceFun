using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class TakeEffect : NetworkBehaviour
{
    private Animator _animator;

    private static readonly int RECEIVE_HIT = Animator.StringToHash("Recebeu_Golpe");

    private bool _isTakingEffect;


    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _isTakingEffect = false;
    }

    public void OnTakeEffect(int effectCode)
    {
        switch(effectCode)
        {
            case 0:
                NormalEffect_ServerRpc();
                break;

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NormalEffect_ServerRpc()
    {
        if(!_isTakingEffect)
        {
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
