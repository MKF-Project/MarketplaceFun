using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchMessages : MonoBehaviour
{

    public static MatchMessages Instance;
    public GameObject MessageText;
    public GameObject PanelBlue;
    public GameObject PanelGreen;
    public GameObject PanelRed;
    public GameObject PanelYellow;
    public GameObject PanelGray;
    
    private Text _messageText;  //Mensagens de 25 caracteres
    private bool _messageOnDisplay;
    private float _waitToDisappear;

    private Color _defaultColor;


    public void Awake()
    {
        if (MatchMessages.Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            _messageText = MessageText.GetComponent<Text>();
            _messageOnDisplay = false;
            _waitToDisappear = 3f;
            _defaultColor = _messageText.color;
        }
        else
        {
            DestroyImmediate(gameObject);
        }

    }

    public void ShowMessage()
    {
        if (!_messageOnDisplay)
        { 
            _messageOnDisplay = true;
            MessageText.SetActive(true);
            StartCoroutine(nameof(Fade));
        }
        

    }

    public void EditMessage(String text)
    {
        _messageText.text = text;
    }
    public void EditMessage(String text, float waitToDisappear)
    {
        _messageText.text = text;
        _waitToDisappear = waitToDisappear;
    }

    public void EditColorMessage(int playerIndex)
    {
        switch (playerIndex)
        {
            case 1:
                PanelBlue.SetActive(true);
                break;
            case 2:
                PanelGreen.SetActive(true);
                break;
            case 3:
                PanelRed.SetActive(true);
                break;
            case 4:
                PanelYellow.SetActive(true);
                break;
            case 5:
                PanelGray.SetActive(true);
                break;
        }
    }
    

    public void HideMessage()
    {
        MessageText.SetActive(false);
        _messageOnDisplay = false;
        PanelBlue.SetActive(false);
        PanelGreen.SetActive(false);
        PanelRed.SetActive(false);
        PanelYellow.SetActive(false);
        PanelGray.SetActive(false);
    }

    private IEnumerator Fade() 
    {
        yield return new WaitForSeconds(_waitToDisappear);
        /*
        for (float ft = 1f; ft >= 0; ft -= 0.1f) 
        {
            Color c = _messageText.color;
            c.a = ft;
            _messageText.color = c;
            yield return new WaitForSeconds(.1f);
        }
        */
        HideMessage();
        //Color color = _messageText.color;
        //color.a = 1f;
        //_messageText.color = _defaultColor;
        
    }

}
