using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadioButtonScript : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    [SerializeField] private Sprite _Selected;
    [SerializeField] private Sprite _unselected;

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
