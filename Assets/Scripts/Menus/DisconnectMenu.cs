using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectMenu : MonoBehaviour
{
    // Events
    public delegate void OnPressOKDelegate();
    public static event OnPressOKDelegate OnPressOK;

    private void Awake()
    {
        SceneManager.OnMainMenuLostConnection += this.toggleMenuDelayed;
    }

    private void OnDestroy()
    {
        SceneManager.OnMainMenuLostConnection -= this.toggleMenuDelayed;
    }

    // Buttton Actions
    public void pressOK() => OnPressOK?.Invoke();
}
