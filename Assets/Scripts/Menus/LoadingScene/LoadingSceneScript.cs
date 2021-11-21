using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.SceneManagement;


public class LoadingSceneScript : MonoBehaviour
{
    private const string LOAD_TEXT_PATH = "LoadingMenu/LoadingText";
    private const string PROGRESS_BAR_PATH = "LoadingMenu/ProgressBar/ProgressAmount";
    private const string PROGRESS_PERCENT_PATH = "LoadingMenu/ProgressBar/ProgressPercent";

    private const string DEFAULT_LOADING_MESSAGE = "Loading...";

    private Text _loadingText;
    private Image _progressBar;
    private Text _progressPercent;

    private void Awake()
    {
        transform.Find(LOAD_TEXT_PATH).TryGetComponent(out _loadingText);
        transform.Find(PROGRESS_BAR_PATH).TryGetComponent(out _progressBar);
        transform.Find(PROGRESS_PERCENT_PATH).TryGetComponent(out _progressPercent);

        _loadingText.text = DEFAULT_LOADING_MESSAGE;
        _progressBar.fillAmount = 0;
        _progressPercent.text = "0%";

        // Whenever we load the LoadingScene itself,
        // we can be sure that the next to scene to be loaded
        // will be the next match scene
        NetworkSceneManager.OnSceneSwitchStarted += LoadSceneInBackground;
    }

    private void LoadSceneInBackground(AsyncOperation loadOperation)
    {
        NetworkSceneManager.OnSceneSwitchStarted -= LoadSceneInBackground;
        StartCoroutine(TrackLoadingProgress(loadOperation));
    }

    private IEnumerator TrackLoadingProgress(AsyncOperation loadOperation)
    {
        // When controlling scene activation manually (allowSceneActivation = false),
        // the AsyncOP progress only goes up to 0.9, meaning we must adjust our limits
        // so that 0.9 now represents 100% of the load progress
        const float MANUAL_ACTIVATION_THRESHOLD = 0.9f;
        const float VISUAL_MULTIPLY_FACTOR = 1f / MANUAL_ACTIVATION_THRESHOLD;

        // Prevent scene from activating immediately when load finishes
        loadOperation.allowSceneActivation = false;

        while(!loadOperation.isDone)
        {
            var progressAmount = loadOperation.progress * VISUAL_MULTIPLY_FACTOR;
            _progressBar.fillAmount = progressAmount;
            _progressPercent.text = $"{(int)(progressAmount * 100)}%";

            if(loadOperation.progress >= MANUAL_ACTIVATION_THRESHOLD)
            {
                loadOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
