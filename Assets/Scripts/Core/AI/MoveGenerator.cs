using Assets.Scripts.Core;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.ObjectModel;
using System.Linq;

public class MoveGenerator
{
    public static List<Move> pseudoMoves;
    public static List<Move> legalMoves;
    public static int initialRank;
    private int opponentColor;
    public Board board { private get; set; }

    public MoveGenerator(Board board)
    {
        this.board = board;
    }
    public struct Move
    {
        public Move(int startSquare, int targetSquare)
        {
            this.StartSquare = startSquare;
            this.TargetSquare = targetSquare;
        }
        public int StartSquare { get; set; }
        public int TargetSquare { get; set; }
    }
    
    public List<Move> GeneratePseudoMoves()
    {
        return GeneratePseudoMoves(board.friendlyColor);
    }

    public List<Move> GeneratePseudoMoves(int color)
    {
        initialRank = color == Pieces.White ? 2 : 7;
        opponentColor = color == Pieces.White ? Pieces.Black : Pieces.White;
        pseudoMoves = new List<Move>();


        for (int startSquare = 0; startSquare < 64; startSquare++)
        {
            int piece = board.Square[startSquare];


            if (Pieces.IsColor(piece, color))
            {
                if (Pieces.IsSlidingPiece(piece))
                {
                    GenerateSlidingMoves(startSquare, piece, color);
                }

                if (Pieces.IsType(piece, Pieces.King))
                {
                    GenerateKingMoves(startSquare, color);
                }

                if (Pieces.IsType(piece, Pieces.Knight))
                {
                    GenerateKnightMoves(startSquare, color);
                }

                if (Pieces.IsType(piece, Pieces.Pawn))
                {
                    GeneratePawnMoves(startSquare, color);
                }

 

            }
        }
        GenerateCastling(color);
        return pseudoMoves;
    }

    public List<Move> GenerateLegalMoves()
    {
        return GenerateLegalMoves(board.friendlyColor);
    }

    public List<Move> GenerateLegalMoves(int color)
    {
        legalMoves = new List<Move>();
        board.UpdateOpponentsAttackingSquares(board);
        List<Move> pseudoLegalMoves = GeneratePseudoMoves();

        foreach (Move pseudoLegalMove in pseudoLegalMoves)
        {
            board.MakeMove(pseudoLegalMove.StartSquare, pseudoLegalMove.TargetSquare);

            MoveGenerator moveGenerator = new MoveGenerator(board);
            List<Move> responses = moveGenerator.GeneratePseudoMoves();
            if (responses.Any( move => move.TargetSquare == (color == Pieces.White ? board.whiteKingIndex : board.blackKingIndex)))
            {
                //pokud by pohl zabrat krale, tak se pocita jako nelegalni tah
            }
            else
            {
                legalMoves.Add(pseudoLegalMove);
            }

            board.UnmakeMove();
        }
        return legalMoves;
    }

    public void GenerateSlidingMoves(int starSquare, int piece, int color)
    {
        int starDirection = (Pieces.IsType(piece, Pieces.Bishop)) ? 4 : 0;
        int endDirection = (Pieces.IsType(piece, Pieces.Rook)) ? 4 : 8;

        for (int direction = starDirection; direction < endDirection; direction++)
        {
            for (int x = 0; x < Board.squaresToEdge[starSquare][direction]; x++)
            {
                int targetSquare = starSquare + board.Offsets[direction] * (x + 1);
                int targetSquarePiece = board.Square[targetSquare];

                //Blokovanz vlasnimi figurkami
                if (Pieces.IsColor(targetSquarePiece, color))
                    break;

                pseudoMoves.Add(new Move(starSquare, targetSquare));

                //Nelze pokracovat, prtoze vzal protivnikovu figuru
                if (Pieces.IsColor(targetSquarePiece, opponentColor))
                    break;
            }
        }
    }

    public void GenerateKingMoves(int kingSquare, int color)
    {
        for (int direction = 0; direction < 8; direction++)
        {
            //pokud je na okraji sachovnice
            if (Board.squaresToEdge[kingSquare][direction] == 0)
                continue;
            int targetSquare = kingSquare + board.Offsets[direction];

            //blokovan vlastnimi figurkami
            if (Pieces.IsColor(board.Square[targetSquare], color))
                continue;
            pseudoMoves.Add(new Move(kingSquare, targetSquare));
        }
    }

    public void GenerateKnightMoves(int starSquare, int color)
    {
        foreach (int offset in board.KnightOffsets)
        {
            int targetSquare = starSquare + offset;
            (int, int) startSquareTuple = Board.GetPositionFromIndex(starSquare);
            (int, int) targetSquareTuple = Board.GetPositionFromIndex(targetSquare);

            if (targetSquare is > 63 or < 0)
                continue;
            //preskakovani z jednoho okraje na druhy
            if (math.abs(startSquareTuple.Item1 - targetSquareTuple.Item1) > 2)
                continue;
            if (math.abs(startSquareTuple.Item2 - targetSquareTuple.Item2) > 2)
                continue;
            //blokovano vlasnimi figurkami
            if (Pieces.IsColor(board.Square[targetSquare], color))
                continue;

            pseudoMoves.Add(new Move(starSquare, targetSquare));
        }
    }

    public void GeneratePawnMoves(int starSquare, int color)
    {
        if (Board.squaresToEdge[starSquare][color == Pieces.White ? 0 : 1] == 0)
            return;

        //capture
        if (Board.squaresToEdge[starSquare][2] != 0)
        {
            if (Pieces.IsColor(board.Square[starSquare + (color == Pieces.White ? 7 : -9)], opponentColor)) pseudoMoves.Add(new Move(starSquare, starSquare + (color == Pieces.White ? 7 : -9)));
        }

        if (Board.squaresToEdge[starSquare][3] != 0)
        {
            if (Pieces.IsColor(board.Square[starSquare + (color == Pieces.White ? 9 : -7)], opponentColor)) pseudoMoves.Add(new Move(starSquare, starSquare + (color == Pieces.White ? 9 : -7)));
        }

        //en passant
        if (starSquare + (color == Pieces.White ? 7 : -9) == board.EnPassantSquare && (Board.squaresToEdge[starSquare][2] != 0))
        {
            pseudoMoves.Add(new Move(starSquare, starSquare + (color == Pieces.White ? 7 : -9)));
        }
        if (starSquare + (color == Pieces.White ? 9 : -7) == board.EnPassantSquare && Board.squaresToEdge[starSquare][3] != 0)
        {
            pseudoMoves.Add(new Move(starSquare, starSquare + (color == Pieces.White ? 9 : -7)));
        }


        if (board.Square[starSquare + (color == Pieces.White ? 8 : -8)] != Pieces.Empty)
            return;

        //push
        pseudoMoves.Add(new Move(starSquare, starSquare + (color == Pieces.White ? 8 : -8)));

        //double push
        if (Board.GetPositionFromIndex(starSquare).file == initialRank && board.Square[starSquare + (color == Pieces.White ? 16 : -16)] == Pieces.Empty)
        {
            pseudoMoves.Add(new Move(starSquare, starSquare + (color == Pieces.White ? 16 : -16)));
        }
    }

    public void GenerateCastling(int color)
    {
        if (color == Pieces.White)
        {
            if (board.WhiteCastleKingside && board.CanCastle(false, board))
            {
                if (board.Square[5] == Pieces.Empty && board.Square[6] == Pieces.Empty)
                    pseudoMoves.Add(new Move(4, 6));
            }

            if (board.WhiteCastleQueenside && board.CanCastle(true, board))
            {
                if (board.Square[3] == Pieces.Empty && board.Square[2] == Pieces.Empty && board.Square[1] == Pieces.Empty)
                    pseudoMoves.Add(new Move(4, 2));
            }
        }
        else
        {
            if (board.BlackCastleKingside && board.CanCastle(false, board))
            {
                if (board.Square[61] == Pieces.Empty && board.Square[62] == Pieces.Empty)
                    pseudoMoves.Add(new Move(60, 62));
            }

            if (board.BlackCastleQueenside && board.CanCastle(true, board))
            {
                if (board.Square[59] == Pieces.Empty && board.Square[58] == Pieces.Empty && board.Square[57] == Pieces.Empty)
                    pseudoMoves.Add(new Move(60, 58));
            }
        }
    }
}
