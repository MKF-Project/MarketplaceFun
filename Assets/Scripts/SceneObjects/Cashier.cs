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
            Player player = other.GetComponent<Player>();
            if (MatchManager.Instance.IsMainPlayer(player.gameObject))
            {
                if (player.IsListComplete)
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


}
