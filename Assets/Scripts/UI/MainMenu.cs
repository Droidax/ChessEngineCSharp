using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

public class MenuController : MonoBehaviour
{
    private GameObject ButtonReference;
    private GameObject MainMenu;
    private GameObject Settingspanel;
    private GameObject Knight;

    void Awake()
    {
        MainMenu = GameObject.Find("MainMenu");
        GameObject rootObject = GameObject.Find("Canvas");
        Settingspanel = rootObject.transform.Find("SettingsPanel").gameObject;
        Knight = GameObject.Find("Knight");
    }

    private void QuitGame()
    {
            Application.Quit();
    }

    public void SetButtonReference(GameObject button)
    {
        ButtonReference = button;
    }

    public void DoButtonAction()
    {
        switch (ButtonReference.name)
        {
            case "2Players":
                Play2Players();
            break;

            case "PlayerVsAi":
                PlayAgainstAi();
            break;

            case "Settings":
                ToggleSettingsPanel();
            break;

            case "Quit":
                QuitGame();
            break;

            case "CancelButton":
                ToggleSettingsPanel();
            break;
        }

    }

    private void ToggleSettingsPanel()
    {
        MainMenu.SetActive(!MainMenu.activeSelf);
        Settingspanel.SetActive(!Settingspanel.activeSelf);
        foreach (Transform child in MainMenu.transform)
        {
            if (!child.gameObject.name.Contains("Ball"))
            {
                continue;
            }
            child.gameObject.SetActive(true);
            foreach (Transform child2 in child.transform)
            {
                child2.gameObject.SetActive(false);
            }
        }
        ResetKnightPosition();
    }

    private void ResetKnightPosition()
    {
        HorseMenuAnimation horseMenuAnimation = Knight.GetComponent<HorseMenuAnimation>();
        Knight.transform.position = horseMenuAnimation.defaultPosition;
        horseMenuAnimation.StopIdleAnimation();
        horseMenuAnimation.StartIdleAnimation();

    }

    private void Play2Players()
    {
        SettingsManager.Instance.PlayAgainstAi = false;
        SceneManager.LoadScene("ChessBoard");
    }

    private void PlayAgainstAi()
    {
        SettingsManager.Instance.PlayAgainstAi = true;
        SceneManager.LoadScene("ChessBoard");
    }
}
