using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        ulong positionHash = ZobristHashing.Instance.ComputeFullHash(board);
        var ttEntry = TranspositionTable.Instance.Retrieve(positionHash);
        if (ttEntry != null && ttEntry.Depth >= depth)
        {
            if (ttEntry.Flag == TranspositionTable.EntryFlag.Exact)
                return ttEntry.Score;
            else if (ttEntry.Flag == TranspositionTable.EntryFlag.LowerBound)
                alpha = Math.Max(alpha, ttEntry.Score);
            else if (ttEntry.Flag == TranspositionTable.EntryFlag.UpperBound)
                beta = Math.Min(beta, ttEntry.Score);

            if (alpha >= beta)
                return beta;
        }

        evaluation = new Eval(board);
        if (depth == 0) return evaluation.EvaluateCurrentPosition();
        var bestMove = new Move();

        var moveGenerator = new MoveGenerator(board);
        List<Move> moves = moveGenerator.GenerateLegalMoves();

        if (ttEntry != null && !ttEntry.BestMove.Equals(new Move()))
        {
            int index = moves.FindIndex(m =>
                m.StartSquare == ttEntry.BestMove.StartSquare &&
                m.TargetSquare == ttEntry.BestMove.TargetSquare);
            if (index > 0)
            {
                // Swap the TT move with the first move.
                Move temp = moves[0];
                moves[0] = moves[index];
                moves[index] = temp;
            }
        }

        int originalAlpha = alpha;

        foreach (Move move in moves)
        {
            board.CopyBoard();
            board.MakeMove(move.StartSquare, move.TargetSquare);
            Search search = new Search(board);
            var score = AlphaBetaMin(alpha, beta, depth - 1);
            board.UnmakeMove();

            if (score >= beta)
            {
                TranspositionTable.Instance.Store(positionHash, beta, depth, TranspositionTable.EntryFlag.LowerBound, move);
                return beta;
            }

            if (score > alpha)
            {
                alpha = score;
                bestMove = move;

            }
        }

        BestMove = bestMove;
        TranspositionTable.EntryFlag flag;
        if (alpha <= originalAlpha)
            flag = TranspositionTable.EntryFlag.UpperBound;
        else if (alpha >= beta)
            flag = TranspositionTable.EntryFlag.LowerBound;
        else
            flag = TranspositionTable.EntryFlag.Exact;
        TranspositionTable.Instance.Store(positionHash, alpha, depth, flag, bestMove);
        return alpha;
    }

    public int AlphaBetaMin(int alpha, int beta, int depth)
    {
        var bestMove = new Move();

        ulong positionHash = ZobristHashing.Instance.ComputeFullHash(board);
        var ttEntry = TranspositionTable.Instance.Retrieve(positionHash);
        if (ttEntry != null && ttEntry.Depth >= depth)
        {
            if (ttEntry.Flag == TranspositionTable.EntryFlag.Exact)
                return ttEntry.Score;
            else if (ttEntry.Flag == TranspositionTable.EntryFlag.LowerBound)
                alpha = Math.Max(alpha, ttEntry.Score);
            else if (ttEntry.Flag == TranspositionTable.EntryFlag.UpperBound)
                beta = Math.Min(beta, ttEntry.Score);

            if (alpha >= beta)
                return beta;
        }

        evaluation = new Eval(board);
        if (depth == 0) return -evaluation.EvaluateCurrentPosition();

        var moveGenerator = new MoveGenerator(board);
        List<Move> moves = moveGenerator.GenerateLegalMoves();

        if (ttEntry != null && !ttEntry.BestMove.Equals(new Move()))
        {
            int index = moves.FindIndex(m =>
                m.StartSquare == ttEntry.BestMove.StartSquare &&
                m.TargetSquare == ttEntry.BestMove.TargetSquare);
            if (index > 0)
            {
                Move temp = moves[0];
                moves[0] = moves[index];
                moves[index] = temp;
            }
        }
        int originalBeta = beta;

        foreach (Move move in moves)
        {
            board.CopyBoard();
            board.MakeMove(move.StartSquare, move.TargetSquare);
            Search search = new Search(board);
            var score = AlphaBetaMax(alpha, beta, depth - 1);
            board.UnmakeMove();

            if (score <= alpha)
            {
                TranspositionTable.Instance.Store(positionHash, alpha, depth, TranspositionTable.EntryFlag.UpperBound,
                    move);
                return alpha;
            }

            if (score < beta)
            {
                beta = score;
                bestMove = move;
            }
        }
        TranspositionTable.EntryFlag flag;
        if (beta >= originalBeta)
            flag = TranspositionTable.EntryFlag.LowerBound;
        else if (beta <= alpha)
            flag = TranspositionTable.EntryFlag.UpperBound;
        else
            flag = TranspositionTable.EntryFlag.Exact;
        TranspositionTable.Instance.Store(positionHash, beta, depth, flag, bestMove);
        return beta;
    }
}
