using System;
using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCanvas : MonoBehaviour
{
    public GameObject ButtonStart;
    public GameObject ButtonReady;
    public GameObject ScorePanel;
    private Text _scoreText;
    public GameObject ExitButton;
    public Image CoinRight;
    public Image CoinLeft;

    public void Awake()
    {
        ButtonStart.SetActive(false);

        ButtonReady.SetActive(false);

        _scoreText = ScorePanel.GetComponentInChildren<Text>();
    }

    public void ShowButtonStart()
    {
        ButtonStart.SetActive(true);
    }

    public void ShowButtonReady()
    {
        ButtonReady.SetActive(true);

    }
    
    public void ShowButtonExit()
    {
        ExitButton.SetActive(true);

    }
    
    public void HideButtonStart()
    {
        ButtonStart.SetActive(false);
    }

    public void HideButtonReady()
    {
        ButtonReady.SetActive(false);
    }
    
    public void HideScoreText()
    {
        ScorePanel.SetActive(false);
    }
    
    public void HideButtonExit()
    {
        ExitButton.SetActive(false);

    }

    public void HideCoins()
    {
        CoinRight.enabled = false;
        CoinLeft.enabled = false;;
    }


    public void HideUI()
    {
        HideButtonStart();
        HideButtonReady();
        HideScoreText();
        HideCoins();

    }


    public void ActivateButtonStart()
    {
        ButtonStart.GetComponent<Button>().interactable = true;
    }

    public void ActivateButtonReady()
    {
        ButtonReady.GetComponent<Button>().interactable = true;
    }
    
    public void InactivateButtonReady()
    {
        ButtonReady.GetComponent<Button>().interactable = false;
    }

    public void ShowScoreText(String text)
    {
        _scoreText.text = text;
    }
    
    public void ShowScoreText(String text, Color color)
    {
        _scoreText.text = text;
        _scoreText.color = color;
        CoinRight.color = color;
        CoinLeft.color = color;
    }
}