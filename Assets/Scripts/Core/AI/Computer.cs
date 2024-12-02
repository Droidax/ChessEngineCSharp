using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using Random = System.Random;


public class Computer
{
    private Board board;
    private MoveGenerator moveGenerator;
    private static Random rnd = new Random();

    public Computer(Board board)
    {
        this.board = board;
        moveGenerator = new MoveGenerator(board.CopyBoard());
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
        var search = new Search(board);
        search.AlphaBetaMax(int.MinValue, int.MaxValue, 5);

        return search.BestMove;
    }



    
}
