using System;
using UnityEngine;
using UnityEngine.UI;

public class WinCanvas : MonoBehaviour
{
    public Text WinText;

    public void ShowWinText(String winnerText)
    {
        
        WinText.text = winnerText;
    }
}
