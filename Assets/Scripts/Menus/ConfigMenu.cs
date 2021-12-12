using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigMenu : MonoBehaviour
{
    // Events
    public delegate void OnPressOKDelegate();
    public static event OnPressOKDelegate OnPressOK;

    public static float Sensitivity = 1;
    private static bool SensitivityConfigured = false;

    [SerializeField] private Slider SensitivitySlider = null;
    [SerializeField] private Text SensitivityValue = null;

    private void Awake()
    {
        if(SensitivityConfigured)
        {
            SensitivitySlider.value = Sensitivity;
        }
        else
        {
            Sensitivity = SensitivitySlider.value;
        }

        SensitivityValue.text = Sensitivity.ToString("0.00");

        GameMenu.OnPressConfigurations += this.toggleMenu;
    }

    private void OnDestroy()
    {
        GameMenu.OnPressConfigurations -= this.toggleMenu;
    }

    public void onChangeSliderValue()
    {
        Sensitivity = SensitivitySlider.value;
        SensitivityValue.text = Sensitivity.ToString("0.00");
        SensitivityConfigured = true;
    }

    // Button Actions
    public void pressOK() => OnPressOK?.Invoke();
}
