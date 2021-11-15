using System.Collections;
using System.Collections.Generic;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCanvas : MonoBehaviour
{
    private const string TEXT_POINTS_TAG = "TextPoints";
    private const string BUTTON_READY_TAG = "ButtonReady";
    private const string BUTTON_START_TAG = "ButtonStart";
    private const string DESCRIPTIVE_POINT_TEXT_TAG = "DescriptivePointText";
    
    private List<Text> _points;
    private List<Text> _descriptiveTexts;

    private GameObject _buttonStart;
    private GameObject _buttonReady;
    

    public void Awake()
    {
        _points = new List<Text>();
        _descriptiveTexts = new List<Text>();

        _buttonStart = gameObject.FindChildWithTag(BUTTON_START_TAG);
        _buttonStart.SetActive(false);

        _buttonReady = gameObject.FindChildWithTag(BUTTON_READY_TAG);
        _buttonReady.SetActive(false);
        
        foreach (GameObject child in gameObject.FindChildrenWithTag(TEXT_POINTS_TAG))
        {
            _points.Add(child.GetComponent<Text>());
        }
        
        foreach (GameObject child in gameObject.FindChildrenWithTag(DESCRIPTIVE_POINT_TEXT_TAG))
        {
            _descriptiveTexts.Add(child.GetComponent<Text>());
        }
        
    }


    public void SetScorePoints(int index, int points)
    {
        _points[index].text = "" + points;
    }
    
    public void SetScoreDescription(int index, int descriptiveText)
    {
        _descriptiveTexts[index].text = "" + descriptiveText;
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
    
}