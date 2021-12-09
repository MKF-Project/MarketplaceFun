using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerAudio : NetworkBehaviour
{
    private const float MUTE_TIMEOUT = 0.5f;

    [Header("Sounds")]
    public List<AudioClip> Steps;

    private AudioSource _playerSource;
    private WaitForSeconds _muteTimeout = new WaitForSeconds(MUTE_TIMEOUT);

    private void Awake()
    {
        _playerSource = GetComponentInChildren<AudioSource>();

        StartCoroutine(UnmuteAfterSeconds());

        SceneManager.OnSceneLoaded += StartMuted;
    }

    private void OnDestroy()
    {
        SceneManager.OnSceneLoaded -= StartMuted;
    }

    private void Start()
    {
        if(IsOwner)
        {
            _playerSource.spatialBlend = 0;
        }
    }

    private void StartMuted(string scene) => StartCoroutine(UnmuteAfterSeconds());

    private IEnumerator UnmuteAfterSeconds()
    {
        _playerSource.mute = true;

        yield return _muteTimeout;

        _playerSource.mute = false;
    }

    // Call Sound Functions
    public void AnimationStepForward(AnimationEvent caller)
    {
        if(caller.animatorClipInfo.weight > 0.5f)
        {
            PlayStep();
        }
    }

    public void AnimationStepSideways(AnimationEvent caller)
    {
        if(caller.animatorClipInfo.weight >= 0.5f)
        {
            PlayStep();
        }
    }

    public void PlayStep() => _playerSource.PlayOneShot(Steps[Random.Range(0, Steps.Count)]);
}
