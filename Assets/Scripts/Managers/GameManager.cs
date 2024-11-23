using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.VFX;

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;
    [SerializeField] private UI _spawnerPrefab;
    private UI Spawner;
    public bool WaitingForPlayerToPlay { get; private set;}

    public GameState State { get; private set; }


    void Start() => ChangeState(GameState.Starting);

    public void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.Starting:
                StartGame();
                break;

            case GameState.WhiteTurn:
                WhiteToMove();
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

        Debug.Log($"New state: {newState}");
    }

    private void StartGame()
    {
        Board.Instance.SetNewBoard();
        Spawner = Instantiate(_spawnerPrefab);

        Spawner.SpawnBoard();
        Spawner.SpawnPieces();  
        State = GameState.Starting;

        Board.Instance.WhitePlayer = PlayerTypes.Human;
        Board.Instance.BlackPlayer = PlayerTypes.Computer;

        Debug.Log($"White Player: {Board.Instance.WhitePlayer}");
        Debug.Log($"Black Player: {Board.Instance.BlackPlayer}");

        ChangeState(GameState.WhiteTurn);
    }

    private void WhiteToMove()
    {
        MoveGenerator moveGenerator = new MoveGenerator(Board.Instance);
        moveGenerator.GenerateLegalMoves(Pieces.White);

        if (Board.Instance.WhitePlayer == PlayerTypes.Human)
        {
            WaitingForPlayerToPlay = true;
        }
        else
        {
            Computer computer = new Computer(Board.Instance);
            computer.ChooseBestMove();
            //play move
        }
        
    }


    public enum GameState
    {
        Starting = 0,
        WhiteTurn = 1,
        BlackTurn = 2,
        WhiteWin = 3,
        BlackWin = 4

    }

    public enum PlayerTypes
    {
        Human = 0,
        Computer = 1
    }
}
