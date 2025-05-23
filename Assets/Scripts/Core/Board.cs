using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Assets.Scripts.UI;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static GameManager;
using static MoveGenerator;
using Convert = System.Convert;

namespace Assets.Scripts.Core
{
    public class Board
    {
        private static Board instance;
        public List<Board> MoveHistory = new List<Board>();
        public int[] Square = new int[64];
        public Fen.LoadedFenInfo FenInfo;
        public int ColorToMove;
        public int[] Offsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
        public int[] KnightOffsets = { 17, 15, 6, 10, -6, -10, -17, -15 };
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
        public GameManager.PlayerTypes WhitePlayer;
        public GameManager.PlayerTypes BlackPlayer;
        public TranspositionTable TranspositionTable;
        public MoveGenerator MoveGenerator { get; private set; }
        public List<MoveGenerator.Move> LegalMoves { get; private set; }

        public int halfmoveCount { get; private set; }
        public int fullmoveCount { get; private set; }

        private Dictionary<ulong, int> boardPositions = new Dictionary<ulong, int>();


        private Board()
        {
        }

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
            halfmoveCount = FenInfo.FullMoveCounter;
            fullmoveCount = FenInfo.FullMoveCounter;
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
            copiedBoard.halfmoveCount = halfmoveCount;
            copiedBoard.fullmoveCount = fullmoveCount;
            copiedBoard.boardPositions = new Dictionary<ulong, int>(boardPositions);

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

            if (Pieces.IsType(Square[startIndex], Pieces.Pawn) &&
                startIndex + (ColorToMove == Pieces.White ? 16 : -16) == targetIndex)
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
                ResetHalfMoveCounter();
            }

            if (Pieces.IsType(Square[startIndex], Pieces.King) || Pieces.IsType(Square[startIndex], Pieces.Rook))
            {
                UpdateCastlingRights(startIndex);
                ResetHalfMoveCounter();
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
            if (Pieces.IsType(Square[targetIndex], Pieces.Pawn) &&
                squaresToEdge[targetIndex][ColorToMove == Pieces.White ? 0 : 1] == 0)
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

            if (ColorToMove == Pieces.Black)
            {
                fullmoveCount++;
            }

            if (Pieces.GetPieceType(Square[startIndex]) == Pieces.Pawn || Square[targetIndex] != 0)
            {
                ResetHalfMoveCounter();
            }
            else
            {
                IncrementHalfMoveCounter();
            }

            (ColorToMove, opponentColor) = (opponentColor, ColorToMove);
            friendlyColor = ColorToMove;

            UpdateBoardPositionHistory();
        }

        private void MakeMoveAndUpdateVisuals(int startIndex, int targetIndex)
        {
            var copiedBoard = CopyBoard();
            MoveHistory.Add(copiedBoard);

            //en passant
            if (Pieces.IsType(Square[startIndex], Pieces.Pawn) && targetIndex == EnPassantSquare)
            {
                Square[targetIndex + (ColorToMove == Pieces.White ? -8 : 8)] = Pieces.Empty;
                Actions.OnDestroyPiece(targetIndex + (ColorToMove == Pieces.White ? -8 : 8));

            }

            if (Pieces.IsType(Square[startIndex], Pieces.Pawn) &&
                startIndex + (ColorToMove == Pieces.White ? 16 : -16) == targetIndex)
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
                ResetHalfMoveCounter();
            }

            if (Pieces.IsType(Square[startIndex], Pieces.King) || Pieces.IsType(Square[startIndex], Pieces.Rook))
            {
                UpdateCastlingRights(startIndex);
                ResetHalfMoveCounter();
            }

            if (Pieces.IsType(Square[startIndex], Pieces.King) && math.abs(startIndex - targetIndex) == 2)
            {
                if (targetIndex > startIndex)
                {
                    Square[startIndex + 1] = Square[startIndex + 3];
                    Square[startIndex + 3] = Pieces.Empty;

                    Actions.OnPieceMove(startIndex + 3, startIndex + 1);
                }
                else
                {
                    Square[startIndex - 1] = Square[startIndex - 4];
                    Square[startIndex - 4] = Pieces.Empty;

                    Actions.OnPieceMove(startIndex - 4, startIndex - 1);
                }
            }

            Square[targetIndex] = Square[startIndex];
            Square[startIndex] = Pieces.Empty;

            //pawn promotion
            if (Pieces.IsType(Square[targetIndex], Pieces.Pawn) &&
                squaresToEdge[targetIndex][ColorToMove == Pieces.White ? 0 : 1] == 0)
            {
                PromotePawn(targetIndex, promoteTo);
                GameObject.Find("Piece" + startIndex).GetComponent<Piece>()
                    .ChangeSprite(Pieces.GetColor(Square[targetIndex]), promoteTo);

            }

            //king index update
            if (Pieces.IsType(Square[targetIndex], Pieces.King))
            {
                if (Pieces.IsColor(Square[targetIndex], Pieces.White))
                    whiteKingIndex = targetIndex;

                else
                    blackKingIndex = targetIndex;
            }

            if (ColorToMove == Pieces.Black)
            {
                fullmoveCount++;
            }

            if (Pieces.GetPieceType(Square[startIndex]) == Pieces.Pawn || Square[targetIndex] != 0)
            {
                ResetHalfMoveCounter();
            }
            else
            {
                IncrementHalfMoveCounter();
            }

            (ColorToMove, opponentColor) = (opponentColor, ColorToMove);
            friendlyColor = ColorToMove;

            UpdateBoardPositionHistory();
        }


        private void ResetHalfMoveCounter()
        {
            halfmoveCount = 0;
        }

        private void IncrementHalfMoveCounter()
        {
            halfmoveCount++;
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
            halfmoveCount = lastMove.halfmoveCount;
            fullmoveCount = lastMove.fullmoveCount;
            boardPositions = new Dictionary<ulong, int>(lastMove.boardPositions);
        }

        public void MovePiece(GameObject piece, GameObject target)
        {
            int pieceIndex = int.Parse(piece.name.Remove(0, 5));
            int targetIndex = int.Parse(target.name);

            MovePiece(pieceIndex, targetIndex);

        }

        public void MovePiece(MoveGenerator.Move move, bool updateVisuals)
        {
            int pieceIndex = move.StartSquare;
            int targetIndex = move.TargetSquare;

            if (updateVisuals)
            {
                MovePiece(pieceIndex, targetIndex, updateVisuals);
            }
        }

        public void MovePiece(int pieceIndex, int targetIndex)
        {
            if (HumanPlayer.ValidMove(pieceIndex, targetIndex))
            {
                if (Instance.Square[targetIndex] != Pieces.Empty)
                    Actions.OnDestroyPiece(targetIndex);

                MakeMove(pieceIndex, targetIndex);

                Actions.OnPieceMove(pieceIndex, targetIndex);

                GameManager.Instance.MoveWasMade = true;
            }

            ResetSquareColor();
        }

        public void MovePiece(int pieceIndex, int targetIndex, bool updateVisuals)
        {
            if (!updateVisuals)
            {
                MovePiece(pieceIndex, targetIndex);
                return;
            }

            if (HumanPlayer.ValidMove(pieceIndex, targetIndex))
            {
                if (Instance.Square[targetIndex] != Pieces.Empty)
                    Actions.OnDestroyPiece(targetIndex);

                MakeMoveAndUpdateVisuals(pieceIndex, targetIndex);

                Actions.OnPieceMove(pieceIndex, targetIndex);

                GameManager.Instance.MoveWasMade = true;
            }

            ResetSquareColor();

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
                if (board.AttackingSquares[board.ColorToMove == Pieces.White ? 2 : 58] == 1 ||
                    board.AttackingSquares[board.ColorToMove == Pieces.White ? 3 : 59] == 1)
                {
                    return false;
                }

                return true;
            }

            if (board.AttackingSquares[board.ColorToMove == Pieces.White ? 5 : 61] == 1 ||
                board.AttackingSquares[board.ColorToMove == Pieces.White ? 6 : 62] == 1)
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

        public static (int file, int rank) GetPositionFromIndex(int squareIndex) // rank, file
        {
            return (squareIndex % 8 + 1, squareIndex / 8 + 1);
        }

        public static int GetIndexFromPosition(int file, int rank) //file a rank v rozmez� 1 a� 8
        {
            return rank * 8 - (8 - file) - 1;
        }

        public static void HighlightLegalSquare(int index)
        {
            var MoveGenerator = new MoveGenerator(Instance);
            var moves = MoveGenerator.GenerateLegalMoves();

            foreach (MoveGenerator.Move move in moves)
            {
                if (index == move.StartSquare)
                {
                    Actions.OnHighlightedSquare(move.TargetSquare.ToString());
                }
            }

        }

        public static void ResetSquareColor()
        {
            Actions.OnResetSquareColor();
        }

        public void UpdateBoardPositionHistory()
        {
            ulong positionHash = ZobristHashing.Instance.ComputeFullHash(this);

            if (boardPositions.ContainsKey(positionHash))
                boardPositions[positionHash]++;
            else
                boardPositions[positionHash] = 1;
        }

        private bool CheckThreefoldRepetition()
        {
            ulong positionHash = ZobristHashing.Instance.ComputeFullHash(this);
            return boardPositions.ContainsKey(positionHash) && boardPositions[positionHash] >= 3;
        }

        public GameState EvaluateGameCondition()
        {

            if (CheckThreefoldRepetition())
            {
                return GameState.Draw;
            }

            if (halfmoveCount >= 50)
            {
                return GameState.Draw;
            }

            MoveGenerator moveGenerator = new MoveGenerator(this);
            List<Move> legalMoves = moveGenerator.GenerateLegalMoves();

            if (legalMoves.Count == 0)
            {
                if (IsInCheck)
                {
                    // �ach mat
                    if (ColorToMove == Pieces.White)
                        return GameState.BlackWin;

                    return GameState.WhiteWin;
                }

                // Pat
                return GameState.Draw;
            }

            if (ColorToMove == Pieces.White)
            {
                return GameState.WhiteTurn;
            }

            return GameState.BlackTurn;
        }
    }
}
