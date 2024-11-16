using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; }

    void Start() => ChangeState(GameState.Starting);

    public void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Starting:
                //StartGame();
                break;

            case GameState.WhiteTurn:
                //WhiteToMove();
                break;

            case GameState.BlackTurn:
                //BlackToMove();
                break;

            case GameState.WhiteWin:
                break;

            case GameState.BlackWin:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            
        }

        OnAfterStateChanged?.Invoke(newState);
    }


    [SerializeField]
    public enum GameState
    {
        Starting = 0,
        WhiteTurn = 1,
        BlackTurn = 2,
        WhiteWin = 3,
        BlackWin = 4

    }
}
