using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum ShoppingListSFX
{
    ItemCorrect = 0,
    ItemIncorrect = 1,
    ItemOpponent = 2,
    ListComplete = 3,
}

[RequireComponent(typeof(AudioSource))]
public class SceneAudioController : MonoBehaviour
{
    private const float MINIMUM_HIGHPASS_FREQUENCY = 10;
    private const float HIGHPASS_CUTOFF_FREQUENCY = 1000;
    private const string MIXER_HIGHPASS_PARAM_NAME = "Music_Highpass_Cutoff";

    private static SceneAudioController _inst = null;

    [SerializeField] private AudioMixer _mixer;

    private AudioSource _source;

    [Header("Music")]
    [SerializeField] private List<AudioClip> _backgroundSongs = null;

    public static List<AudioClip> BackgroundSongs => _inst?._backgroundSongs;

    [Header("SFX")]
    [SerializeField, Range(0, 1)] private float _sfxVolume = 1;

    [SerializeField] private AudioClip _itemCorrectSFX;
    [SerializeField] private AudioClip _itemIncorrectSFX;
    [SerializeField] private AudioClip _itemOpponentSFX;
    [SerializeField] private AudioClip _listCompleteSFX;

    public static float SFXVolume => _inst != null? _inst._sfxVolume : 0;

    public static AudioClip ItemCorrectSFX   => _inst?._itemCorrectSFX;
    public static AudioClip ItemIncorrectSFX => _inst?._itemIncorrectSFX;
    public static AudioClip ItemOpponentSFX  => _inst?._itemOpponentSFX;
    public static AudioClip ListCompleteSFX  => _inst?._listCompleteSFX;

    private void Awake()
    {
        if(_inst != null)
        {
            Destroy(gameObject);
            return;
        }

        _inst = this;

        TryGetComponent(out _source);
        InitializeAudioSource();

        InputController.OnPause += EnableHighpassFilter;
        InputController.OnUnpause += DisableHighpassFilter;

        ExitMenu.OnStayOnMatch += DisableHighpassFilter;

        SceneManager.OnSceneLoaded += DisableHighpassOnScene;
    }

    private void Start()
    {
        // Bypass the Music highpass filter.
        // We will reset this value again when we pause the game.
        DisableHighpassFilter();
    }

    private void OnDestroy()
    {
        if(_inst == this)
        {
            _inst = null;

            InputController.OnPause -= EnableHighpassFilter;
            InputController.OnUnpause -= DisableHighpassFilter;

            ExitMenu.OnStayOnMatch -= DisableHighpassFilter;

            SceneManager.OnSceneLoaded -= DisableHighpassOnScene;
        }
    }

    private void InitializeAudioSource()
    {
        _source.clip = _backgroundSongs != null && _backgroundSongs.Count > 0? _backgroundSongs[Random.Range(0, _backgroundSongs.Count)] : null;
        _source.Play();
    }

    private void DisableHighpassOnScene(string scene) => DisableHighpassFilter();

    private void EnableHighpassFilter() => _mixer.SetFloat(MIXER_HIGHPASS_PARAM_NAME, HIGHPASS_CUTOFF_FREQUENCY);
    private void DisableHighpassFilter() => _mixer.SetFloat(MIXER_HIGHPASS_PARAM_NAME, MINIMUM_HIGHPASS_FREQUENCY);

    // Static Functions
    public static void PlayItemCorrectSFX()   => _inst._source.PlayOneShot(_inst._itemCorrectSFX,   _inst._sfxVolume / _inst._source.volume);
    public static void PlayItemIncorrectSFX() => _inst._source.PlayOneShot(_inst._itemIncorrectSFX, _inst._sfxVolume / _inst._source.volume);
    public static void PlayItemOpponentSFX()  => _inst._source.PlayOneShot(_inst._itemOpponentSFX,  _inst._sfxVolume / _inst._source.volume);
    public static void PlayListCompleteSFX()  => _inst._source.PlayOneShot(_inst._listCompleteSFX,  _inst._sfxVolume / _inst._source.volume);

    public static void EnableMusicHighpassFilter()  => _inst.EnableHighpassFilter();
    public static void DisableMusicHighpassFilter() => _inst.DisableHighpassFilter();
}
