using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.VFX;
using static MoveGenerator;

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;
    [SerializeField] private UI _spawnerPrefab;
    private UI Spawner;
    private Coroutine moveCoroutine;
    public bool WaitingForMove { get; private set; }
    public bool MoveWasMade { get; set;}
    public GameState State { get; private set; }
    private Computer computer;
    private Transform EndGameScreen;
    private Transform Settings;
    private Transform SettingsButton;
    private PlayerTypes whitePlayerType; 
    private PlayerTypes blackPlayerType;

    void Awake()
    {
        EndGameScreen = GameObject.Find("Canvas").transform.Find("EndGameScreen");
        Settings = GameObject.Find("Canvas").transform.Find("SettingsPanel");
        SettingsButton = GameObject.Find("Canvas").transform.Find("SettingsButton");
        whitePlayerType = PlayerTypes.Human;
        if (SettingsManager.Instance.PlayAgainstAi)
        {
            blackPlayerType = PlayerTypes.Computer;
        }
        else
        {
            blackPlayerType = PlayerTypes.Human;
        }
    }

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
                moveCoroutine = StartCoroutine(WhiteToMove());
                break;

            case GameState.BlackTurn:
                moveCoroutine = StartCoroutine(BlackToMove());
                break;

            case GameState.WhiteWin:
                WhiteWin();
                break;

            case GameState.BlackWin:
                BlackWin();
                break;

            case GameState.Draw:
                Draw();
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

        SetPlayers(whitePlayerType, blackPlayerType);

        Debug.Log($"White Player: {Board.Instance.WhitePlayer}");
        Debug.Log($"Black Player: {Board.Instance.BlackPlayer}");
        WaitingForMove = false;
        MoveWasMade = false;


        if (Board.Instance.ColorToMove == Pieces.White)
        {
            ChangeState(GameState.WhiteTurn);
        }
        else
        {
            ChangeState(GameState.BlackTurn);
        }
    }
    IEnumerator WhiteToMove()
    {
        MoveGenerator moveGenerator = new MoveGenerator(Board.Instance);
        moveGenerator.GenerateLegalMoves(Pieces.White);

        if (Board.Instance.WhitePlayer == PlayerTypes.Human)
        {
            WaitingForMove = true;
            yield return new WaitUntil(PlayerMadeMove);

        }
        else
        {
            computer.SetBoard(Board.Instance);
            Board.Instance.MovePiece(computer.ChooseBestMove(), true);
        }
        MoveWasMade = false;
        WaitingForMove = false;

        Board.Instance.UpdateOpponentsAttackingSquares(Board.Instance);
        yield return null;

        ChangeState(Board.Instance.EvaluateGameCondition());
        Debug.Log(ZobristHashing.Instance.ComputeFullHash(Board.Instance));

    }
    IEnumerator BlackToMove()
    {
        MoveGenerator moveGenerator = new MoveGenerator(Board.Instance);
        moveGenerator.GenerateLegalMoves(Pieces.Black);

        if (Board.Instance.BlackPlayer == PlayerTypes.Human)
        {
            WaitingForMove = true;
            yield return new WaitUntil(PlayerMadeMove);
        }
        else
        {
            Board.Instance.MovePiece(computer.ChooseBestMove(), true);
            Board.Instance.ColorToMove = Pieces.White;
            Board.Instance.friendlyColor = Pieces.White;
            Board.Instance.opponentColor = Pieces.Black;
        }
        MoveWasMade = false;
        WaitingForMove = false;

        Board.Instance.UpdateOpponentsAttackingSquares(Board.Instance);
        yield return null;
        ChangeState(Board.Instance.EvaluateGameCondition());
        Debug.Log(ZobristHashing.Instance.ComputeFullHash(Board.Instance));

    }

    private void WhiteWin()
    {
        EndGameScreen.gameObject.SetActive(true);
        foreach (Transform child in EndGameScreen.transform)
        {
            if (child.name == "WhiteWinText")
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private void BlackWin()
    {
        EndGameScreen.gameObject.SetActive(true);
        foreach (Transform child in EndGameScreen.transform)
        {
            if (child.name == "BlackWinText")
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private void Draw()
    {
        EndGameScreen.gameObject.SetActive(true);
        foreach (Transform child in EndGameScreen.transform)
        {
            if (child.name == "DrawText")
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private bool PlayerMadeMove()
    { 
        return MoveWasMade;
    }

    private void SetPlayers(PlayerTypes white, PlayerTypes black)
    {
        Board.Instance.WhitePlayer = white;
        Board.Instance.BlackPlayer = black;

        if (white == PlayerTypes.Computer || black == PlayerTypes.Computer)
        {
            computer = new Computer(Board.Instance);
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleSettings()
    {
        Settings.gameObject.SetActive(!Settings.gameObject.activeSelf);
        SettingsButton.gameObject.SetActive(!SettingsButton.gameObject.activeSelf);
    }

    public enum GameState
    {
        Starting = 0,
        WhiteTurn = 1,
        BlackTurn = 2,
        WhiteWin = 3,
        BlackWin = 4,
        Draw = 5

    }

    public enum PlayerTypes
    {
        Human = 0,
        Computer = 1
    }
}
