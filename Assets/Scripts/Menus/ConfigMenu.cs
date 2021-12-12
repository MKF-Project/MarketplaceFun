using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigMenu : MonoBehaviour
{

    public static float Sensitivity = 1;

    // Events
    public delegate void OnPressOKDelegate();
    public static event OnPressOKDelegate OnPressOK;

    [SerializeField] private Slider SensitivitySlider = null;
    [SerializeField] private Text SensitivityValue = null;

    private void Awake()
    {
        onChangeSliderValue();

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
    }

    // Button Actions
    public void pressOK() => OnPressOK?.Invoke();
}
