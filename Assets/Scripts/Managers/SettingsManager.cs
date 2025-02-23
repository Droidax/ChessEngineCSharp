using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    public bool PlayAgainstAi;


    public const int defaultEngineSearchDepth = 1;
    public const bool defaultFullscreen = true;

    public int engineSearchDepth;
    public bool fullscreen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSettings();
    }

    public void LoadSettings()
    {
        engineSearchDepth = PlayerPrefs.HasKey("EngineSearchDepth") ? PlayerPrefs.GetInt("EngineSearchDepth") : defaultEngineSearchDepth;
        fullscreen = PlayerPrefs.HasKey("Fullscreen") ? PlayerPrefs.GetInt("Fullscreen") == 1 : defaultFullscreen;

        Screen.fullScreen = fullscreen;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("EngineSearchDepth", engineSearchDepth);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    public void SetFullscreen(bool isFullscreen)
    {
        fullscreen = isFullscreen;
        Screen.fullScreen = isFullscreen;
    }

    public void SetEngineSearchDepth(int engineDepth)
    {
        engineSearchDepth = engineDepth;
    }
}
