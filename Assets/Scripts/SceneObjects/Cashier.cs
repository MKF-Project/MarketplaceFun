using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Cashier : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<Player>().IsListComplete)
            {
                MatchMessages.Instance.EditMessage("You Win");
                MatchMessages.Instance.ShowMessage();
                
                //InMatchCanvas.Instance.EndText
            }
            else
            {
                MatchMessages.Instance.EditMessage("List not complete");
                MatchMessages.Instance.ShowMessage();
            }


        }
    }


}
