using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchMessages : MonoBehaviour
{

    public static MatchMessages Instance;
    public GameObject MessageText;
    //Mensagens de 25 caracteres
    private Text _messageText;
    private bool _messageOnDisplay;
    


    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        _messageText = MessageText.GetComponent<Text>();
        _messageOnDisplay = false;
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


    public void HideMessage()
    {
        MessageText.SetActive(false);
        _messageOnDisplay = false;
    }

    private IEnumerator Fade() 
    {
        yield return new WaitForSeconds(.3f);
        for (float ft = 1f; ft >= 0; ft -= 0.1f) 
        {
            Color c = _messageText.color;
            c.a = ft;
            _messageText.color = c;
            yield return new WaitForSeconds(.1f);
        }
        
        HideMessage();
        Color color = _messageText.color;
        color.a = 1f;
        _messageText.color = color;
        
    }

}
