using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCanvas : MonoBehaviour
{

    private void Awake()
    {
        GameObject.FindGameObjectsWithTag("ShoppingListUI")[0].GetComponent<ShoppingListUI>().EraseItems();
        if (true)//ScoreController.Instance.IAmWinner)
        {
            ShowWin();
        }
        else
        {
            ShowLose();
        }
        
    }

    public Text WinText;

    public Text LoseText;

    public void ShowWin()
    {
        WinText.enabled = true;
    }
    
    public void HideWin()
    {
        WinText.enabled = false;
    }
    
    public void ShowLose()
    {
        LoseText.enabled = true;
    }
    
    public void HideLose()
    {
        LoseText.enabled = false;
    }
    
}