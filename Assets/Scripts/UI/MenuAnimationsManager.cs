using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MenuAnimationsManager : MonoBehaviour
{
    private Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();

    public void ShowDots(GameObject gameObject)
    {
        string objectName = GetDotsName(gameObject.name);

        StopCoroutineIfExists(objectName, "deactivation");

        Coroutine coroutine = StartCoroutine(ActivateDots(objectName));
        activeCoroutines[objectName + "_activation"] = coroutine;
    }

    public void HideDots(GameObject gameObject)
    {
        string objectName = GetDotsName(gameObject.name);

        StopCoroutineIfExists(objectName, "activation");

        Coroutine coroutine = StartCoroutine(DisableDots(objectName));
        activeCoroutines[objectName + "_deactivation"] = coroutine;
    }

    private void StopCoroutineIfExists(string objectName, string type)
    {
        string key = objectName + "_" + type;
        if (activeCoroutines.ContainsKey(key))
        {
            StopCoroutine(activeCoroutines[key]);
            activeCoroutines.Remove(key);
        }
    }

    private string GetDotsName(string buttonName)
    {
        switch (buttonName)
        {
            case "PlayerVsAi": return "BallsTopRight";
            case "2Players": return "BallsTopLeft";
            case "Settings": return "BallsBottomLeft";
            case "Quit": return "BallsBottomRight";
            default: return null;
        }
    }

    IEnumerator ActivateDots(string name)
    {
        GameObject gameObject = GameObject.Find(name);

        foreach (Transform eachChild in gameObject.transform)
        {
            eachChild.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.15f);
        }

        activeCoroutines.Remove(name + "_activation");
    }

    IEnumerator DisableDots(string name)
    {
        GameObject gameObject = GameObject.Find(name);

        if (gameObject == null)
        {
            yield break;
        }

        for (int childIndex = gameObject.transform.childCount - 1; childIndex >= 0; childIndex--)
        {
            gameObject.transform.GetChild(childIndex).gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(0.15f);
        }

        activeCoroutines.Remove(name + "_deactivation");
    }

    public void StartHorseMovement(Transform targetPosition)
    {
        HorseMenuAnimation horseMenuAnimation = GameObject.Find("Knight").GetComponent<HorseMenuAnimation>();
        horseMenuAnimation.MoveHorse(targetPosition, OnHorseMovementComplete);
    }


    private void OnHorseMovementComplete()
    {
        GameObject mCamera = GameObject.Find("Main Camera");
        mCamera.GetComponent<MenuController>().DoButtonAction();
        activeCoroutines.Clear();
    }
}
