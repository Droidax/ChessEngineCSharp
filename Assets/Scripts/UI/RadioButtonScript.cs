using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadioButtonScript : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    [SerializeField] private Sprite _Selected;
    [SerializeField] private Sprite _unselected;

    void Awake()
    {
        if (this.gameObject.name == "Fullscreen")
        {
            _isActive = SettingsManager.Instance.fullscreen;

            if (_isActive)
            {
                gameObject.GetComponent<Image>().sprite = _Selected;
                gameObject.GetComponent<Button>().interactable = false;
            }
            else
            {
                gameObject.GetComponent<Image>().sprite = _unselected;
                gameObject.GetComponent<Button>().interactable = true;

            }
        }

        if (this.gameObject.name == "Windowed")
        {
            _isActive = !SettingsManager.Instance.fullscreen;

            if (_isActive)
            {
                gameObject.GetComponent<Image>().sprite = _Selected;
                gameObject.GetComponent<Button>().interactable = false;
            }
            else
            {
                gameObject.GetComponent<Image>().sprite = _unselected;
                gameObject.GetComponent<Button>().interactable = true;

            }
        }
    }

    public void ToggleButton()
    {
        _isActive = !_isActive;

        if (_isActive)
        {
            gameObject.GetComponent<Image>().sprite = _Selected;
            gameObject.GetComponent<Button>().interactable = false;
        }
        else
        {
            gameObject.GetComponent<Image>().sprite = _unselected;

        }
    }
}
