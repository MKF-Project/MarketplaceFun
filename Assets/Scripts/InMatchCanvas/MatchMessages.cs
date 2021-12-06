using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchMessages : MonoBehaviour
{

    public static MatchMessages Instance;
    public GameObject MessageText;
    
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
            _waitToDisappear = .3f;
        }
        else
        {
            DestroyImmediate(gameObject);
        }

        _defaultColor = _messageText.color;



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

    public void EditColorMessage(Color color)
    {
        _messageText.color = color;
    }

    public void HideMessage()
    {
        MessageText.SetActive(false);
        _messageOnDisplay = false;
    }

    private IEnumerator Fade() 
    {
        yield return new WaitForSeconds(_waitToDisappear);
        for (float ft = 1f; ft >= 0; ft -= 0.1f) 
        {
            Color c = _messageText.color;
            c.a = ft;
            _messageText.color = c;
            yield return new WaitForSeconds(.1f);
        }
        
        HideMessage();
        //Color color = _messageText.color;
        //color.a = 1f;
        _messageText.color = _defaultColor;
        
    }

}
