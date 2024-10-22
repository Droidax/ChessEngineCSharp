using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eval
{
    private const int PawnValue = 100;
    private const int KnightValue = 300;
    private const int BishopValue = 310;
    private const int RookValue = 500;
    private const int QueenValue = 900;
    private Board board;

    public Eval(Board board)
    {
        this.board = board;
    }

    public int EvaluateCurrentPosition()
    {
        int whiteMaterial = CountMaterial(Pieces.White);
        int blackMaterial = CountMaterial(Pieces.Black);

    
        int evaluation = whiteMaterial - blackMaterial;

        int side = board.ColorToMove == Pieces.White ? 1 : -1;

        return evaluation * side;
    }

    public int CountMaterial(int ColorToCount)
    {
        int materialScore = 0;

        for (int index = 0; index < 64; index++)
        {
            if (!Pieces.IsColor(board.Square[index], ColorToCount))
                continue;

            switch (Pieces.GetPieceType(board.Square[index]))
            {
                case Pieces.Pawn:
                    materialScore += PawnValue;
                    break;

                case Pieces.Knight:
                    materialScore += KnightValue;
                    break;

                case Pieces.Bishop:
                    materialScore += BishopValue;
                    break;

                case Pieces.Rook:
                    materialScore += RookValue;
                    break;

                case Pieces.Queen:
                    materialScore += QueenValue;
                    break;
            }
        }

        return materialScore;
    }
}
