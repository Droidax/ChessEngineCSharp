using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Slider engineSearchDepthSlider;
    public Toggle fullscreenToggle;

    private void Start()
    {
        engineSearchDepthSlider.value = SettingsManager.Instance.engineSearchDepth;
        fullscreenToggle.isOn = SettingsManager.Instance.fullscreen;
    }
    public void OnSearchDepthChanged(float value)
    {
        SettingsManager.Instance.SetEngineSearchDepth((int)value);
    }
    public void OnFullscreenToggled(bool value)
    {
        SettingsManager.Instance.SetFullscreen(value);
    }
    public void OnApplyButtonPressed()
    {
        SettingsManager.Instance.SaveSettings();
    }
}
