using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.Scripts.Core;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using static MoveGenerator;
using Random = System.Random;


public class Computer
{
    private Board board;
    private MoveGenerator moveGenerator;
    private static Random rnd = new Random();
    private PolyglotBook openingBook;

    public Computer(Board board)
    {
        this.board = board;
        moveGenerator = new MoveGenerator(board.CopyBoard());
        openingBook = new PolyglotBook();
        openingBook.LoadFromFile("Assets/Resources/Perfect2021.bin");
    }

    public void SetBoard(Board board)
    {
        this.board = board;
    }

    public MoveGenerator.Move ChooseRandomMove()
    {
        List<MoveGenerator.Move> moves = moveGenerator.GenerateLegalMoves();

        return moves[rnd.Next(0, moves.Count)];
    }

    public MoveGenerator.Move ChooseBestMove()
    {
        ulong zobristKey = ZobristHashing.Instance.ComputeFullHash(board);
        if (openingBook.TryGetMove(zobristKey, out Move bookMove, out int weight))
        {
            Console.WriteLine(bookMove.StartSquare);
            Console.WriteLine(bookMove.TargetSquare);
            return bookMove;
        }
        Search search = new Search(board);
        //int score = search.Negamax(SettingsManager.Instance.engineSearchDepth, int.MinValue, int.MaxValue, 1);
        //Move bestMove = search.BestMove;
        //search.PrintStatistics();
        search.AlphaBetaMax(int.MinValue, int.MaxValue, 4);
        return search.BestMove;
    }



    
}
