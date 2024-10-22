using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Assets.Scripts.UI;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Convert = System.Convert;

namespace Assets.Scripts.Core
{
    public class Board
    {
        private static Board instance;
        public List<Board> MoveHistory = new List<Board>();
        public int[] Square = new int[64];
        public  Fen.LoadedFenInfo FenInfo;
        public int ColorToMove;
        public int[] Offsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
        public int[] KnightOffsets = { 17, 15, 6, 10, -6, -10, -17, -15};
        public static readonly int[][] squaresToEdge = new int[64][];
        public int opponentColor;
        public int friendlyColor;
        public int whiteKingIndex;
        public int blackKingIndex;
        public int promoteTo = Pieces.Queen;
        private GameObject BoardManager = GameObject.Find("BoardManager");
        public int[] AttackingSquares = new int[64];
        public bool IsInCheck;
        public int EnPassantSquare;
        public bool WhiteCastleKingside;
        public bool WhiteCastleQueenside;
        public bool BlackCastleKingside;
        public bool BlackCastleQueenside;

        private Board(){ }
        public static Board Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Board();
                }
                return instance;
            }
        }
        public void SetNewBoard()
        {
            FenInfo = Fen.LoadPositionFromFen(Fen.StartingFen);
            EnPassantSquare = FenInfo.EnPassantSquare;
            WhiteCastleKingside = FenInfo.WhiteCastleKingside;
            WhiteCastleQueenside = FenInfo.BlackCastleQueenside;
            BlackCastleKingside = FenInfo.BlackCastleKingside;
            BlackCastleQueenside = FenInfo.BlackCastleQueenside;
            Square = FenInfo.LoadedFenSquares;
            ColorToMove = FenInfo.WhiteToMove ? Pieces.White : Pieces.Black;
            opponentColor = FenInfo.WhiteToMove ? Pieces.Black : Pieces.White;
            friendlyColor = opponentColor == Pieces.White ? Pieces.Black : Pieces.White;


            for (int index = 0; index < 64; index++)
            {
                if (Pieces.IsType(Square[index], Pieces.King))
                {
                    if (Pieces.IsColor(Square[index], Pieces.White))
                    {
                        whiteKingIndex = index;
                        continue;
                    }
                    blackKingIndex = index;
                }
            }

            ComputeSquaresToEdge();
        }

        public Board CopyBoard()
        {
            var copiedBoard = new Board();
            Array.Copy(Square, copiedBoard.Square, Square.Length);
            copiedBoard.ColorToMove = ColorToMove;
            copiedBoard.whiteKingIndex = whiteKingIndex;
            copiedBoard.blackKingIndex = blackKingIndex;
            Array.Copy(AttackingSquares, copiedBoard.AttackingSquares, AttackingSquares.Length);
            copiedBoard.IsInCheck = IsInCheck;
            copiedBoard.ColorToMove = ColorToMove;
            copiedBoard.friendlyColor = friendlyColor;
            copiedBoard.opponentColor = opponentColor;
            copiedBoard.EnPassantSquare = EnPassantSquare;
            copiedBoard.WhiteCastleKingside = WhiteCastleKingside;
            copiedBoard.WhiteCastleQueenside = WhiteCastleQueenside;
            copiedBoard.BlackCastleKingside = BlackCastleKingside;
            copiedBoard.BlackCastleQueenside = BlackCastleQueenside;

            return copiedBoard;
        }

        public void MakeMove(int startIndex, int targetIndex)
        {
            var copiedBoard = CopyBoard();
            MoveHistory.Add(copiedBoard);

            //en passant
            if (Pieces.IsType(Square[startIndex], Pieces.Pawn) && targetIndex == EnPassantSquare)
            {
                Square[targetIndex + (ColorToMove == Pieces.White ? -8 : 8)] = Pieces.Empty;

            }
            if (Pieces.IsType(Square[startIndex], Pieces.Pawn) && startIndex + (ColorToMove == Pieces.White ? 16 : -16) == targetIndex)
            {
                EnPassantSquare = startIndex + (ColorToMove == Pieces.White ? 8 : -8);
            }
            else
            {
                EnPassantSquare = 65;
            }

            //castling
            if (Pieces.IsType(Square[targetIndex], Pieces.Rook))
            {
                UpdateCastlingRights(targetIndex);
            }
            if (Pieces.IsType(Square[startIndex], Pieces.King) || Pieces.IsType(Square[startIndex], Pieces.Rook))
            {
                UpdateCastlingRights(startIndex);
            }
            if (Pieces.IsType(Square[startIndex], Pieces.King) && math.abs(startIndex - targetIndex) == 2)
            {
                if (targetIndex > startIndex)
                {
                    Square[startIndex + 1] = Square[startIndex + 3];
                    Square[startIndex + 3] = Pieces.Empty;
                }
                else
                {
                    Square[startIndex - 1] = Square[startIndex - 4];
                    Square[startIndex - 4] = Pieces.Empty;
                }
            }

            Square[targetIndex] = Square[startIndex];
            Square[startIndex] = Pieces.Empty;

            //pawn promotion
            if (Pieces.IsType(Square[targetIndex], Pieces.Pawn) && squaresToEdge[targetIndex][ColorToMove == Pieces.White ? 0 : 1] == 0)
            {
                PromotePawn(targetIndex, promoteTo);
            }

            //king index update
            if (Pieces.IsType(Square[targetIndex], Pieces.King))
            {
                if (Pieces.IsColor(Square[targetIndex], Pieces.White))
                    whiteKingIndex = targetIndex;
                
                else
                    blackKingIndex = targetIndex;
            }

            ColorToMove = opponentColor;
            opponentColor = friendlyColor;
            friendlyColor = ColorToMove;
        }

        public void UnmakeMove()
        {
            var lastMove = MoveHistory.Last();
            MoveHistory.RemoveAt(MoveHistory.Count - 1);

            Array.Copy(lastMove.Square, Square, lastMove.Square.Length);
            Array.Copy(lastMove.AttackingSquares, AttackingSquares, lastMove.AttackingSquares.Length);

            ColorToMove = lastMove.ColorToMove;
            FenInfo = lastMove.FenInfo;
            whiteKingIndex = lastMove.whiteKingIndex;
            blackKingIndex = lastMove.blackKingIndex;
            IsInCheck = lastMove.IsInCheck;
            ColorToMove = lastMove.ColorToMove;
            friendlyColor = lastMove.friendlyColor;
            opponentColor = lastMove.opponentColor;
            EnPassantSquare = lastMove.EnPassantSquare;
            WhiteCastleKingside = lastMove.WhiteCastleKingside;
            WhiteCastleQueenside = lastMove.WhiteCastleQueenside;
            BlackCastleKingside = lastMove.BlackCastleKingside;
            BlackCastleQueenside = lastMove.BlackCastleQueenside;
        }
        public void MovePiece(GameObject piece, GameObject target)
        {
            int pieceIndex = int.Parse(piece.name.Remove(0, 5));    
            int targetIndex = int.Parse(target.name);
            MakeMove(pieceIndex, targetIndex);

            BoardUi.DeleteAllPieces();
            BoardManager.GetComponent<BoardUi>().SpawnAllPieces();
        }

        public void UpdateOpponentsAttackingSquares(Board board)
        {
            MoveGenerator moveGenerator = new MoveGenerator(board);
            List<MoveGenerator.Move> moves = moveGenerator.GeneratePseudoMoves(board.opponentColor);
            AttackingSquares = new int[64];
            if (MoveGenerator.pseudoMoves == null)
                return;

            foreach (MoveGenerator.Move move in moves)
            {
                board.AttackingSquares[move.TargetSquare] = 1;
            }
            
            if (board.ColorToMove == Pieces.Black && board.AttackingSquares[board.blackKingIndex] == 1)
            {
                board.IsInCheck = true;
                return;
            }

            if (board.ColorToMove == Pieces.White && board.AttackingSquares[board.whiteKingIndex] == 1)
            {
                board.IsInCheck = true;
                return;
            }

            board.IsInCheck = false;
            
        }

        public bool CanCastle(bool queenSide, Board board)
        {
            if (board.ColorToMove == Pieces.White)
            {
                if (board.AttackingSquares[whiteKingIndex] == 1)
                    return false;
            }
            else
            {
                if (board.AttackingSquares[blackKingIndex] == 1)
                    return false;
            }

            if (queenSide)
            {
                if (board.AttackingSquares[board.ColorToMove == Pieces.White ? 2 : 58] == 1 || board.AttackingSquares[board.ColorToMove == Pieces.White ? 3 : 59] == 1)
                {
                        return false;
                }

                return true;
            }

            if (board.AttackingSquares[board.ColorToMove == Pieces.White ? 5 : 61] == 1 || board.AttackingSquares[board.ColorToMove == Pieces.White ? 6 : 62] == 1)
            {
                return false;
            }

            return true;
        }

        public void PromotePawn(int squareIndex, int pieceType)
        {
            Square[squareIndex] = Pieces.GetColor(Square[squareIndex]) | pieceType;
        }

        public void UpdateCastlingRights(int pieceIndex)
        {
            int piece = Square[pieceIndex];

            if (Pieces.IsType(piece, Pieces.King))
            {
                if (Pieces.IsColor(piece, Pieces.White))
                {
                    WhiteCastleKingside = false;
                    WhiteCastleQueenside = false;
                }
                else
                {
                    BlackCastleKingside = false;
                    BlackCastleQueenside = false;
                }
            }

            else
            {
                if (Pieces.IsColor(piece, Pieces.White))
                {
                    if (GetPositionFromIndex(pieceIndex).Item1 != 1)
                    {
                        WhiteCastleKingside = false;
                    }
                    else
                    {
                        WhiteCastleQueenside = false;
                    }
                }
                else
                {
                    if (GetPositionFromIndex(pieceIndex).Item1 != 1)
                    {
                        BlackCastleKingside = false;
                    }
                    else
                    {
                        BlackCastleQueenside = false;
                    }
                }
            }
        }


        static void ComputeSquaresToEdge()
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    int numUp = 7 - rank;
                    int numDown = rank;
                    int numLeft = file;
                    int numRight = 7 - file;


                    squaresToEdge[GetIndexFromPosition(file + 1, rank + 1)] = new[]
                    {
                        numUp, numDown, numLeft, numRight, math.min(numUp, numLeft), math.min(numDown, numRight),
                        math.min(numUp, numRight), math.min(numDown, numLeft)
                    };
                }
            }
        }

        public static (int, int) GetPositionFromIndex(int squareIndex)// rank, file
        {
            return (squareIndex % 8 + 1, squareIndex / 8 + 1);
        }
        public static int GetIndexFromPosition(int file, int rank)
        {
            return rank * 8 - (8 - file) - 1;
        }

        public static void HighlightLegalSquare(int index)
        {
            var movegenerator = new MoveGenerator(Instance.CopyBoard());
            var moves = movegenerator.GenerateLegalMoves();

            foreach (MoveGenerator.Move move in moves)
            {
                if (move.StartSquare == index)
                {
                    GameObject square = GameObject.Find(Convert.ToString(move.TargetSquare));
                    square.GetComponent<Tile>().ChangeColorLegal();
                }
            }
        }

        public static void ResetSquareColor()
        {
            for (int i = 0; i < 64; i++)
            {
                GameObject square = GameObject.Find(Convert.ToString(i));
                square.GetComponent<Tile>().ChangeColorDefault();

            }
        }
    }
}
