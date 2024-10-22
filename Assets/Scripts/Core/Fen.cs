using System;
using System.Collections.Generic;
using Assets.Scripts.UI;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class Fen
    {
        public static string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public static string TestFen = "8/2Rr3p/np2P3/2P3p1/5p2/1k5P/p4bbN/2K5 w - - 0 1";

        private static Dictionary <char, int> _pieceTypeFromChar = new Dictionary<char, int>()
        {
            ['p'] = Pieces.Pawn,
            ['n'] = Pieces.Knight,
            ['b'] = Pieces.Bishop,
            ['r'] = Pieces.Rook,
            ['q'] = Pieces.Queen,
            ['k'] = Pieces.King
        };

        public static LoadedFenInfo LoadPositionFromFen(string fen)
        {
            LoadedFenInfo loadedFenInfo = new LoadedFenInfo();

            var part = fen.Split(' ');

            var file = 0;
            var rank = 7;


            foreach (char character in part[0])
            {
                if (character == '/')
                {
                    file = 0;
                    rank--;
                }

                else
                {
                    if (char.IsDigit(character))
                    {
                        file += (int)char.GetNumericValue(character);
                    }

                    else
                    {
                        int color = char.IsUpper(character) ? Pieces.White : Pieces.Black;
                        int type = _pieceTypeFromChar[char.ToLower(character)];

                        loadedFenInfo.LoadedFenSquares[Board.GetIndexFromPosition(file + 1, rank + 1)] = color | type;
                        file++;
                    }
                }
            }

            loadedFenInfo.WhiteToMove = part[1] == "w";

            if (part[2] != "-")
            {
                var castlingRights = part[2];
                loadedFenInfo.WhiteCastleKingside = castlingRights.Contains("K");
                loadedFenInfo.WhiteCastleQueenside = castlingRights.Contains("Q");
                loadedFenInfo.BlackCastleKingside = castlingRights.Contains("k");
                loadedFenInfo.BlackCastleQueenside = castlingRights.Contains("q");
            }

            if (part[3] != "-")
            {
                loadedFenInfo.EnPassantSquare = part[3][0] switch
                {
                    'a' => Board.GetIndexFromPosition(1, (int)char.GetNumericValue(part[3][1])),
                    'b' => Board.GetIndexFromPosition(2, (int)char.GetNumericValue(part[3][1])),
                    'c' => Board.GetIndexFromPosition(3, (int)char.GetNumericValue(part[3][1])),
                    'd' => Board.GetIndexFromPosition(4, (int)char.GetNumericValue(part[3][1])),
                    'e' => Board.GetIndexFromPosition(5, (int)char.GetNumericValue(part[3][1])),
                    'f' => Board.GetIndexFromPosition(6, (int)char.GetNumericValue(part[3][1])),
                    'g' => Board.GetIndexFromPosition(7, (int)char.GetNumericValue(part[3][1])),
                    'h' => Board.GetIndexFromPosition(8, (int)char.GetNumericValue(part[3][1])),
                    _ => loadedFenInfo.EnPassantSquare
                };
            }
            else
                loadedFenInfo.EnPassantSquare = 65;

            loadedFenInfo.HalfMoveCounter = int.Parse(part[4]);
            loadedFenInfo.FullMoveCounter = int.Parse(part[5]);

            return loadedFenInfo;
        }
        public class LoadedFenInfo
        {
            public int[] LoadedFenSquares = new int[64];
            public bool WhiteCastleKingside;
            public bool WhiteCastleQueenside;
            public bool BlackCastleKingside;
            public bool BlackCastleQueenside;
            public bool WhiteToMove;
            public int EnPassantSquare;
            public int HalfMoveCounter;
            public int FullMoveCounter;
        }
    }
}
