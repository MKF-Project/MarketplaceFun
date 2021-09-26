using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Cashier : MonoBehaviour
{
    private const string SHOPPING_CART_TAG = "ShoppingCart";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == SHOPPING_CART_TAG)
        {
            Player player = other.GetComponent<ShoppingCart>().Owner;
            if (MatchManager.Instance.IsMainPlayer(player.gameObject))
            {
                if (player.IsListComplete)
                {
                    MatchMessages.Instance.EditMessage("You Win");
                    MatchMessages.Instance.ShowMessage();
                    ScoreController.Instance.IWin();
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
