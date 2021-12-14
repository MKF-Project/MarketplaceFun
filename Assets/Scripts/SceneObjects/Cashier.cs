using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Cashier : MonoBehaviour
{
    private const string SHOPPING_CART_TAG = "ShoppingCart";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(SHOPPING_CART_TAG))
        {
            Player player = other.GetComponent<ShoppingCartItem>().Owner;
            if (player != null && player == NetworkController.SelfPlayer)
            {
                ShoppingList playerShoppingList = player.gameObject.GetComponent<ShoppingList>();
                if (playerShoppingList.IsListChecked())
                {
                    CheckOut checkOut = player.gameObject.GetComponent<CheckOut>();
                    checkOut.PlayerCheckOut();
                    //MatchMessages.Instance.EditMessage("You Win");
                    //MatchMessages.Instance.ShowMessage();
                }
                else
                {
                    MatchMessages.Instance.EditColorMessage(5);
                    MatchMessages.Instance.EditMessage("Lista incompleta!");
                    MatchMessages.Instance.ShowMessage();
                }
            }

        }
    }


}
