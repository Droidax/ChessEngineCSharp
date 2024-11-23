using UnityEngine;

namespace Assets.Scripts.Core
{
    public class Pieces
    {
        public const int Empty = 0;
        public const int Pawn = 1;
        public const int Knight = 2;
        public const int Bishop = 3;
        public const int Rook = 4;
        public const int Queen = 5;
        public const int King = 6;

        public const int White = 8;
        public const int Black = 16;

        const int typeMask = 0b00111;
        const int blackMask = 0b10000;
        const int whiteMask = 0b01000;
        const int colourMask = whiteMask | blackMask;

        public static bool IsColor(int piece, int color)
        {
            return (piece & colourMask) == color;
        }

        public static int GetColor(int piece)
        {
            return piece & colourMask;
        }

        public static bool IsType(int piece, int type)
        {
            return (piece & typeMask ) == type;
        }

        public static int GetPieceType(int piece)
        {
            return piece & typeMask;
        }

        public static bool IsSlidingPiece(int piece)
        {
            return (piece & typeMask) >= 3 && (piece & typeMask) <= 5;
        }

    }
}

