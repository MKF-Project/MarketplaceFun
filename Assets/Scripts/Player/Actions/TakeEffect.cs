using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class TakeEffect : NetworkBehaviour
{
    private Animator _animator;

    private PlayerControls _playerControls;
    
    private static readonly int ReceiveHit = Animator.StringToHash("ReceiveHit");
    

    // Start is called before the first frame update
    void Awake()
    {
        //Lembrar de alterar aqui dps que o pedro mexer nos controles
        _playerControls = GetComponent<PlayerControls>();
        _animator = GetComponentInChildren<Animator>();
        InputController.OnPut += OnTakeEffect;
    }

    void OnDestroy()
    {
        InputController.OnPut -= OnTakeEffect;
    }
    

    public void OnTakeEffect()
    {
        OnTakeEffect(0);
    }
    

    public void OnTakeEffect(int effectCode)
    {
        switch (effectCode)
        {
            case  0: 
                NormalEffect_ServerRpc();
                break;
                
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void NormalEffect_ServerRpc()
    {
        NormalEffect_ClientRpc();
    }

    [ClientRpc]
    public void NormalEffect_ClientRpc()
    { 
        _animator.SetTrigger(ReceiveHit);
        if (IsOwner)
        {
            _playerControls.enabled = false;

            StartCoroutine(nameof(ActivateMoves));
        }
    }

    private IEnumerator ActivateMoves()
    {
        yield return new WaitForSeconds(2.5f);
        _playerControls.enabled = true;
    }
}
