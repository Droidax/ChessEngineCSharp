using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static MoveGenerator;

public class Search
{
    private const int Infinity = int.MaxValue;
    private const int NegativeInfinity = int.MinValue;
    private Board board;
    private Eval evaluation;
    public Move BestMove { get; set; }

    public Search(Board board)
    {
        this.board = board;
        evaluation = new Eval(board);
    }

    public int AlphaBetaMax(int alpha, int beta, int depth)
    {
        evaluation = new Eval(board);
        if (depth == 0) return evaluation.EvaluateCurrentPosition();
        var bestMove = new Move();

        var moveGenerator = new MoveGenerator(board);
        List<Move> moves = moveGenerator.GenerateLegalMoves();
        foreach (Move move in moves)
        {
            board.CopyBoard();
            board.MakeMove(move.StartSquare, move.TargetSquare);
            Search search = new Search(board);
            var score = AlphaBetaMin(alpha, beta, depth - 1);
            board.UnmakeMove();

            if (score >= beta)
            {
                return beta;
            }

            if (score > alpha)
            {
                alpha = score;
                bestMove = move;

            }
        }

        BestMove = bestMove;
        return alpha;
    }

    public int AlphaBetaMin(int alpha, int beta, int depth)
    {
        evaluation = new Eval(board);
        if (depth == 0) return -evaluation.EvaluateCurrentPosition();

        var moveGenerator = new MoveGenerator(board);
        List<Move> moves = moveGenerator.GenerateLegalMoves();
        foreach (Move move in moves)
        {
            board.CopyBoard();
            board.MakeMove(move.StartSquare, move.TargetSquare);
            Search search = new Search(board);
            var score = AlphaBetaMax(alpha, beta, depth - 1);
            board.UnmakeMove();

            if (score <= alpha)
            {
                return alpha;
            }

            if (score < beta)
            {
                beta = score;
            }
        }
        return beta;
    }
}
