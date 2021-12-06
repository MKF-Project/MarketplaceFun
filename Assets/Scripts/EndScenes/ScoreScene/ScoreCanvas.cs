using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCanvas : MonoBehaviour
{
    private const string BUTTON_READY_TAG = "ButtonReady";
    private const string BUTTON_START_TAG = "ButtonStart";
    

    private GameObject _buttonStart;
    private GameObject _buttonReady;
    public GameObject ScorePanel;
    private Text _scoreText;
    

    public void Awake()
    {
        _buttonStart = gameObject.FindChildWithTag(BUTTON_START_TAG);
        _buttonStart.SetActive(false);

        _buttonReady = gameObject.FindChildWithTag(BUTTON_READY_TAG);
        _buttonReady.SetActive(false);

        _scoreText = ScorePanel.GetComponentInChildren<Text>();
    }

    public void ShowButtonStart()
    {
        _buttonStart.SetActive(true);
    }

    public void ShowButtonReady()
    {
        _buttonReady.SetActive(true);

    }

    public void ActivateButtonStart()
    {
        _buttonStart.GetComponent<Button>().interactable = true;
    }

    public void InactivateButtonReady()
    {
        _buttonReady.GetComponent<Button>().interactable = false;
    }

    public void ShowScoreText(String text)
    {
        _scoreText.text = text;
    }
    
    public void ShowScoreText(String text, Color color)
    {
        _scoreText.text = text;
        _scoreText.color = color;
    }
}