using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityScene = UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    // Events
    public delegate void OnMainMenuLostConnectionDelegate();
    public static event OnMainMenuLostConnectionDelegate OnMainMenuLostConnection;

    public delegate void OnSceneLoadedDelegate(string sceneName);
    public delegate void OnSceneUnloadedDelegate(string sceneName);
    public static event OnSceneLoadedDelegate OnSceneLoaded;
    public static event OnSceneUnloadedDelegate OnSceneUnloaded;

    public delegate void OnMenuLoadedDelegate();
    public delegate void OnMenuUnloadedDelegate();
    public static event OnMenuLoadedDelegate OnMenuLoaded;
    public static event OnMenuUnloadedDelegate OnMenuUnloaded;

    public delegate void OnLoadingSceneLoadedDelegate();
    public delegate void OnLoadingSceneUnloadedDelegate();
    public static event OnLoadingSceneLoadedDelegate OnLoadingSceneLoaded;
    public static event OnLoadingSceneUnloadedDelegate OnLoadingSceneUnloaded;

    public delegate void OnScoreLoadedDelegate();
    public delegate void OnScoreUnloadedDelegate();
    public static event OnScoreLoadedDelegate OnScoreLoaded;
    public static event OnScoreUnloadedDelegate OnScoreUnloaded;

    public delegate void OnMatchLoadedDelegate(string sceneName);
    public delegate void OnMatchUnloadedDelegate(string sceneName);
    public static event OnMatchLoadedDelegate OnMatchLoaded;
    public static event OnMatchUnloadedDelegate OnMatchUnloaded;

    public String MatchScene;

    public static String MatchSceneTag;

    public const string SELF_TAG = "SceneManager";
    public const string MAIN_MENU_SCENE_NAME = "MainMenu";
    public const string LOADING_SCENE_NAME = "LoadingScene";
    public const string SCORE_SCENE_NAME = "ScoreScene";

    private bool _onMainMenu
    {
        get => UnityScene.SceneManager.GetActiveScene().name == MAIN_MENU_SCENE_NAME;
    }

    private void Awake()
    {
        Object.DontDestroyOnLoad(gameObject);

        MatchSceneTag = MatchScene;
        LobbyMenu.OnStartMatch += loadMatch;

        NetworkController.OnDisconnected += returnToMainMenu;

        UnityScene.SceneManager.sceneLoaded += TriggerSceneLoadEvent;
        UnityScene.SceneManager.sceneUnloaded += TriggerSceneUnloadEvent;

        // If, after moving to DontDestroyOnLoad, we detect more than one
        // SceneManager object, that means we are the duplicate one that came after
        // And so should delete ourselves
        if(GameObject.FindGameObjectsWithTag(SELF_TAG).Length > 1)
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnDestroy()
    {
        LobbyMenu.OnStartMatch -= loadMatch;
        NetworkController.OnDisconnected -= returnToMainMenu;

        UnityScene.SceneManager.sceneLoaded -= TriggerSceneLoadEvent;
        UnityScene.SceneManager.sceneUnloaded -= TriggerSceneUnloadEvent;
    }

    private void returnToMainMenu(bool wasHost, bool connectionWasLost)
    {
        if(_onMainMenu) // No need to go back to main menu if we're already there
        {
            return;
        }

        // Set a self unsubscribing local function to trigger when the mainMenu scene loads
        void deferredMainMenuAction(UnityScene.Scene scene, UnityScene.LoadSceneMode mode)
        {
            UnityScene.SceneManager.sceneLoaded -= deferredMainMenuAction;
            if(!_onMainMenu)
            {
                return;
            }

            // If we disconnected on purpose, no action is required
            // however, if we lost connection, defer event triger until after scene components
            //have been started
            if(connectionWasLost)
            {
                IEnumerator triggerConnectionLost()
                {
                    // Defer event trigger until after Awakes and Starts
                    yield return Utils.EndOfFrameWait;
                    OnMainMenuLostConnection?.Invoke();
                }

                StartCoroutine(triggerConnectionLost());
            }
        }

        // Subscribe the above funtion
        UnityScene.SceneManager.sceneLoaded += deferredMainMenuAction;

        // Finally, load MainMenu
        UnityScene.SceneManager.LoadScene(MAIN_MENU_SCENE_NAME);
    }

    private void loadMatch()
    {
        // TODO get scene name from lobby
        NetworkController.switchNetworkScene(MatchScene);

        // Set the expected number of players that should be moved to spawn
        // areas during the next match
        SpawnController.PlayerSpawnsRequired = NetworkController.NumberOfClients;
    }


    public static void LoadScore()
    {
        NetworkController.switchNetworkScene(SCORE_SCENE_NAME);
    }


    private void TriggerSceneLoadEvent(UnityScene.Scene scene, UnityScene.LoadSceneMode mode)
    {
        OnSceneLoaded?.Invoke(scene.name);

        switch(scene.name)
        {
            case MAIN_MENU_SCENE_NAME:
                OnMenuLoaded?.Invoke();
                break;

            case LOADING_SCENE_NAME:
                OnLoadingSceneLoaded?.Invoke();
                break;

            case SCORE_SCENE_NAME:
                OnScoreLoaded?.Invoke();
                break;

            default:
                OnMatchLoaded?.Invoke(scene.name);
                break;
        }
    }

    private void TriggerSceneUnloadEvent(UnityScene.Scene scene)
    {
        OnSceneUnloaded?.Invoke(scene.name);

        switch(scene.name)
        {
            case MAIN_MENU_SCENE_NAME:
                OnMenuUnloaded?.Invoke();
                break;

            case LOADING_SCENE_NAME:
                OnLoadingSceneUnloaded?.Invoke();
                break;

            case SCORE_SCENE_NAME:
                OnScoreUnloaded?.Invoke();
                break;

            default:
                OnMatchUnloaded?.Invoke(scene.name);
                break;
        }
    }
}
