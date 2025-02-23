using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class SettingsMenu : MonoBehaviour
{
    public Slider engineSearchDepthSlider;
    public Toggle fullscreenToggle;
    public TextMeshProUGUI depthText;

    private void Start()
    {
        engineSearchDepthSlider.value = SettingsManager.Instance.engineSearchDepth;
        Debug.Log(SettingsManager.Instance.engineSearchDepth.ToString());
        depthText.text = "Search depth of engine: " + SettingsManager.Instance.engineSearchDepth + " \n(Higher numbers may cause more lag in between moves)";
        fullscreenToggle.isOn = SettingsManager.Instance.fullscreen;
    }

    public void OnSearchDepthChanged(float value)
    {
        SettingsManager.Instance.SetEngineSearchDepth((int)value);
        depthText.text = "Search depth of engine: " + SettingsManager.Instance.engineSearchDepth + " \n(Higher numbers may cause more lag in between moves)";
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
