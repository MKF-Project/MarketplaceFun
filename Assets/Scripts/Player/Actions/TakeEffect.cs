using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class TakeEffect : ScorableAction
{
    private Animator _animator;

    private static readonly int ReceiveHit = Animator.StringToHash("ReceiveHit");

    private bool _isTakingEffect;

    private ScoreType _scoreType;

    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _isTakingEffect = false;
    }

    public void OnTakeEffect(int effectCode, ulong throwerId)
    {
        switch (effectCode)
        {
            case  0:
                NormalEffect_ServerRpc(throwerId);
                break;

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NormalEffect_ServerRpc(ulong throwerId)
    {
        if (!_isTakingEffect)
        {
            NetworkManager.ConnectedClients[throwerId].PlayerObject.GetComponent<PlayerScore>().ScoreAction(_scoreType);
            _isTakingEffect = true;
            NormalEffect_ClientRpc();
        }
    }

    [ClientRpc]
    public void NormalEffect_ClientRpc()
    {
        _animator.SetTrigger(ReceiveHit);
        if (IsOwner)
        {
            InputController.FreezePlayerControls();

            StartCoroutine(nameof(ActivateMoves));
        }
    }

    private IEnumerator ActivateMoves()
    {
        yield return new WaitForSeconds(2.5f);
        InputController.UnfreezePlayerControls();
        NoTakingEffect_ServerRpc();
    }

    [ServerRpc]
    public void NoTakingEffect_ServerRpc()
    {
        _isTakingEffect = false;
    }

    
    public override void SetScore(ScoreType scoreType)
    {
        _scoreType = scoreType;
    }
    
    
}
