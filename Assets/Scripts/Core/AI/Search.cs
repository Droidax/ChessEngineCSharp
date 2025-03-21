using Assets.Scripts.Core;
using static MoveGenerator;
using System.Collections.Generic;
using System;

public class Search
{
    private const int Infinity = int.MaxValue;
    private const int NegativeInfinity = int.MinValue;
    private Board board;
    private Eval evaluation;
    public Move BestMove { get; set; }
    private TranspositionTable transpositionTable;
    private int nodesSearched;

    public Search(Board board)
    {
        this.board = board;
        evaluation = new Eval(board);
        transpositionTable = TranspositionTable.Instance;
        nodesSearched = 0;

        // Start a new search - this increments the age in the transposition table
        transpositionTable.NewSearch();
    }

    public int AlphaBetaMax(int alpha, int beta, int depth)
    {
        nodesSearched++;
        ulong hashKey = ZobristHashing.Instance.ComputeFullHash(board);
        TranspositionTable.TranspositionEntry entry = transpositionTable.Retrieve(hashKey);

        // Check if we have a transposition entry
        if (entry != null && entry.Depth >= depth)
        {
            if (entry.Flag == TranspositionTable.EntryFlag.Exact)
                return entry.Score; // Exact score found
            if (entry.Flag == TranspositionTable.EntryFlag.LowerBound)
                alpha = Math.Max(alpha, entry.Score); // Lower bound
            if (entry.Flag == TranspositionTable.EntryFlag.UpperBound)
                beta = Math.Min(beta, entry.Score); // Upper bound
            if (alpha >= beta)
                return entry.Score; // Cutoff
        }

        evaluation = new Eval(board);
        int eval = evaluation.EvaluateCurrentPosition();

        // Handle terminal conditions with depth adjustment
        if (eval == Int32.MaxValue)
        {
            return eval + depth; // Adjusted for shorter mates
        }

        if (eval == Int32.MinValue)
        {
            return eval - depth; // Adjusted for losing positions
        }

        if (eval == 0)
        {
            return 0; // Stalemate
        }

        if (depth == 0)
        {
            return eval;
        }

        var bestMove = new Move();
        var moveGenerator = new MoveGenerator(board);
        List<Move> moves = moveGenerator.GenerateLegalMoves();

        // Try the transposition table move first if available
        if (entry != null && !entry.BestMove.Equals(default(Move)))
        {
            // Re-order moves to try the best move first
            foreach (var move in moves)
            {
                if (move.StartSquare == entry.BestMove.StartSquare &&
                    move.TargetSquare == entry.BestMove.TargetSquare)
                {
                    moves.Remove(move);
                    moves.Insert(0, move);
                    break;
                }
            }
        }

        foreach (Move move in moves)
        {
            board.CopyBoard();
            board.MakeMove(move.StartSquare, move.TargetSquare);

            Search search = new Search(board);
            var score = AlphaBetaMin(alpha, beta, depth - 1);

            board.UnmakeMove();

            if (score >= beta)
            {
                // Store the entry in the transposition table
                transpositionTable.Store(hashKey, score, depth, TranspositionTable.EntryFlag.UpperBound, bestMove);
                return beta;
            }

            if (score > alpha)
            {
                alpha = score;
                bestMove = move;
            }
        }

        BestMove = bestMove;

        // Store the entry in the transposition table
        transpositionTable.Store(hashKey, alpha, depth, TranspositionTable.EntryFlag.Exact, bestMove);
        return alpha;
    }

    public int AlphaBetaMin(int alpha, int beta, int depth)
    {
        nodesSearched++;
        ulong hashKey = ZobristHashing.Instance.ComputeFullHash(board);
        TranspositionTable.TranspositionEntry entry = transpositionTable.Retrieve(hashKey);

        // Check if we have a transposition entry
        if (entry != null && entry.Depth >= depth)
        {
            if (entry.Flag == TranspositionTable.EntryFlag.Exact)
                return entry.Score; // Exact score found
            if (entry.Flag == TranspositionTable.EntryFlag.LowerBound)
                alpha = Math.Max(alpha, entry.Score); // Lower bound
            if (entry.Flag == TranspositionTable.EntryFlag.UpperBound)
                beta = Math.Min(beta, entry.Score); // Upper bound
            if (alpha >= beta)
                return entry.Score; // Cutoff
        }

        evaluation = new Eval(board);
        int eval = evaluation.EvaluateCurrentPosition();

        // Handle terminal conditions with depth adjustment
        if (eval == Int32.MaxValue)
        {
            return -eval - depth; // Adjusted for winning positions
        }

        if (eval == Int32.MinValue)
        {
            return -eval + depth; // Adjusted for losing positions
        }

        if (eval == 0)
        {
            return 0; // Stalemate
        }

        if (depth == 0)
        {
            return -eval;
        }

        var moveGenerator = new MoveGenerator(board);
        List<Move> moves = moveGenerator.GenerateLegalMoves();

        // Try the transposition table move first if available
        if (entry != null && !entry.BestMove.Equals(default(Move)))
        {
            // Re-order moves to try the best move first
            foreach (var move in moves)
            {
                if (move.StartSquare == entry.BestMove.StartSquare &&
                    move.TargetSquare == entry.BestMove.TargetSquare)
                {
                    moves.Remove(move);
                    moves.Insert(0, move);
                    break;
                }
            }
        }

        foreach (Move move in moves)
        {
            board.CopyBoard();
            board.MakeMove(move.StartSquare, move.TargetSquare);

            Search search = new Search(board);
            var score = AlphaBetaMax(alpha, beta, depth - 1);

            board.UnmakeMove();

            if (score <= alpha)
            {
                // Store the entry in the transposition table
                transpositionTable.Store(hashKey, score, depth, TranspositionTable.EntryFlag.LowerBound, move);
                return alpha;
            }

            if (score < beta)
            {
                beta = score;
            }
        }

        // Store the entry in the transposition table
        transpositionTable.Store(hashKey, beta, depth, TranspositionTable.EntryFlag.Exact, new Move()); // Pass a default Move
        return beta;
    }

    // Method to get statistics
    public string GetStats()
    {
        return $"Nodes: {nodesSearched}, TT: {transpositionTable.GetStats()}";
    }
}