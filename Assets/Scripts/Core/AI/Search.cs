using Assets.Scripts.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MoveGenerator;

public class Search
{
    private const int Infinity = int.MaxValue;
    private const int NegativeInfinity = int.MinValue;
    private Board board;
    private Eval evaluation;
    private ZobristHashing zobristHashing;
    public Move BestMove { get; private set; }

    // Debug statistics
    private int nodesExamined;
    private int ttHits;

    public Search(Board board)
    {
        this.board = board;
        evaluation = new Eval(board);
        zobristHashing = ZobristHashing.Instance;
        nodesExamined = 0;
        ttHits = 0;
    }

    /// <summary>
    /// Negamax search with alpha-beta pruning and transposition table.
    /// </summary>
    /// <param name="depth">Remaining search depth.</param>
    /// <param name="alpha">Lower bound.</param>
    /// <param name="beta">Upper bound.</param>
    /// <param name="color">1 if maximizing for current player, -1 otherwise.</param>
    /// <returns>The evaluation score.</returns>
    public int Negamax(int depth, int alpha, int beta, int color)
    {
        nodesExamined++;

        // Compute the Zobrist hash for the current board position.
        ulong positionHash = zobristHashing.ComputeFullHash(board);

        // Transposition Table Lookup.
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
            {
                ttHits++;
                return ttEntry.Score;
            }
        }

        // Terminal condition: if depth is 0 or board is in a terminal state.
        if (depth == 0 || board.EvaluateGameCondition() == GameManager.GameState.Draw || board.EvaluateGameCondition() == GameManager.GameState.BlackWin || board.EvaluateGameCondition() == GameManager.GameState.WhiteWin)
        {
            // Evaluation is multiplied by color so that the perspective is consistent.
            return color * evaluation.EvaluateCurrentPosition();
        }

        // Generate all legal moves.
        var moveGenerator = new MoveGenerator(board);
        List<Move> moves = moveGenerator.GenerateLegalMoves();

        // If no moves exist, treat as terminal.
        if (moves.Count == 0)
        {
            return color * evaluation.EvaluateCurrentPosition();
        }
        
        // Optional: If a TT move exists, bring it to the front.
        if (ttEntry != null && !ttEntry.BestMove.Equals(default(Move)))
        {
            int ttMoveIndex = moves.FindIndex(m =>
                m.StartSquare == ttEntry.BestMove.StartSquare &&
                m.TargetSquare == ttEntry.BestMove.TargetSquare);
            if (ttMoveIndex != -1)
            {
                Move ttMove = moves[ttMoveIndex];
                moves.RemoveAt(ttMoveIndex);
                moves.Insert(0, ttMove);
            }
        }

        int originalAlpha = alpha;
        int bestScore = NegativeInfinity;
        // Initialize bestMoveLocal with the first move to ensure a legal move is available.
        Move bestMoveLocal = moves[0];

        // Negamax recursion over all moves.
        foreach (Move move in moves)
        {
            board.MakeMove(move.StartSquare, move.TargetSquare);
            int score = -Negamax(depth - 1, -beta, -alpha, -color);
            board.UnmakeMove();

            if (score > bestScore)
            {
                bestScore = score;
                bestMoveLocal = move;
                // Optionally output debugging info.
                Debug.Log($"New best move at depth {depth}: {move.StartSquare} -> {move.TargetSquare} with score {score}");
            }

            alpha = Math.Max(alpha, score);

            // Beta cutoff.
            if (alpha >= beta)
            {
                // Store TT entry with LOWERBOUND flag.
                TranspositionTable.Instance.Store(positionHash, bestScore, depth, TranspositionTable.EntryFlag.LowerBound, move);
                BestMove = move;
                return bestScore;
            }
        }

        // Determine the flag to store in the transposition table.
        TranspositionTable.EntryFlag flag;
        if (bestScore <= originalAlpha)
            flag = TranspositionTable.EntryFlag.UpperBound;
        else if (bestScore >= beta)
            flag = TranspositionTable.EntryFlag.LowerBound;
        else
            flag = TranspositionTable.EntryFlag.Exact;

        TranspositionTable.Instance.Store(positionHash, bestScore, depth, flag, bestMoveLocal);
        BestMove = bestMoveLocal;
        Debug.Log($"Final best move at depth {depth}: {bestMoveLocal.StartSquare} -> {bestMoveLocal.TargetSquare}");
        return bestScore;
    }

    /// <summary>
    /// Initial call for the Negamax search.
    /// Assumes the current board is to move with "color" = 1.
    /// </summary>
    public int GetBestMoveValue(int depth)
    {
        // Starting with 1 since we assume the current player is the maximizer.
        return Negamax(depth, NegativeInfinity, Infinity, 1);
    }

    public void PrintStatistics()
    {
        Debug.Log($"Search Stats: {nodesExamined} nodes examined, {ttHits} TT hits");
        Debug.Log($"TT hit rate: {(nodesExamined > 0 ? (float)ttHits / nodesExamined * 100 : 0):F2}%");
        Debug.Log($"TT size: {TranspositionTable.Instance.GetSize()} entries");
    }
}
