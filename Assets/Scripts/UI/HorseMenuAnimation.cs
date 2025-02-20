using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HorseMenuAnimation : MonoBehaviour
{
    private float moveDuration = 0.5f;
    private float idleHeight = 8f;
    private float idleDuration = 1f;
    private Sequence idleSequence;
    public Vector3 defaultPosition { get; private set; }

    void Awake()
    {
        defaultPosition = gameObject.transform.position;
    }
    private void Start()
    {
        StartIdleAnimation();
    }

    public void StartIdleAnimation()
    {
        idleSequence = DOTween.Sequence();

        idleSequence.Append(transform.DOMoveY(transform.position.y + idleHeight, idleDuration)
            .SetEase(Ease.InOutSine));
        idleSequence.Append(transform.DOMoveY(transform.position.y, idleDuration)
            .SetEase(Ease.InOutSine));


        idleSequence.SetLoops(-1);
    }
    public void StopIdleAnimation()
    {
        if (idleSequence != null)
        {
            idleSequence.Kill();
            idleSequence = null;
        }
    }
    public void MoveHorse(Transform targetPosition, System.Action onComplete = null)
    {
        StopIdleAnimation();

        Sequence moveSequence = DOTween.Sequence();

        moveSequence.Append(transform.DOMove(new Vector3(transform.position.x, targetPosition.position.y), moveDuration)
            .SetEase(Ease.InOutSine));

        moveSequence.Append(transform.DOMove(targetPosition.position, moveDuration)
            .SetEase(Ease.InOutSine));

        moveSequence.OnComplete(() => onComplete?.Invoke());

        moveSequence.Play();

    }

}

