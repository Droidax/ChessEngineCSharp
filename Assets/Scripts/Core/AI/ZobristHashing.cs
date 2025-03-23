using Assets.Scripts.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Unity.VisualScripting;

public class ZobristHashing
{
    private const int NumPieces = 12;
    private const int BoardSize = 64;
    private readonly ulong[,] zobristTable = new ulong[NumPieces, BoardSize];

    // Random values for additional game states
    private readonly ulong whiteKingsideCastling;
    private readonly ulong whiteQueensideCastling;
    private readonly ulong blackKingsideCastling;
    private readonly ulong blackQueensideCastling;
    private readonly ulong[] enPassantTable = new ulong[8];
    private readonly ulong sideToMoveValue; // black's turn

    private readonly System.Random rnd = new System.Random();

    public static ZobristHashing instance;

    public ZobristHashing()
    {
        InitializeZobristTable();
        whiteKingsideCastling = GetRandomUInt64();
        whiteQueensideCastling = GetRandomUInt64();
        blackKingsideCastling = GetRandomUInt64();
        blackQueensideCastling = GetRandomUInt64();
        for (int file = 0; file < 8; file++)
        {
            enPassantTable[file] = GetRandomUInt64();
        }

        sideToMoveValue = GetRandomUInt64();
    }

    public static ZobristHashing Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ZobristHashing();
            }

            return instance;
        }
    }

    private void InitializeZobristTable()
    {
        for (int piece = 0; piece < NumPieces; piece++)
        {
            for (int square = 0; square < BoardSize; square++)
            {
                zobristTable[piece, square] = GetRandomUInt64();
            }
        }
    }

    private ulong GetRandomUInt64()
    {
        byte[] buffer = new byte[8];
        rnd.NextBytes(buffer);
        return BitConverter.ToUInt64(buffer, 0);
    }

    public ulong ComputeFullHash(Board board)
    {
        ulong hash = 0;

        // Pieces
        for (int square = 0; square < board.Square.Length; square++)
        {
            int piece = board.Square[square];

            if (piece != Pieces.Empty)
            {
                int pieceIndex = GetPieceIndex(piece);
                hash ^= zobristTable[pieceIndex, square];
            }
        }

        // Castling rights (only XOR if the right is available)
        if (board.WhiteCastleKingside)
            hash ^= whiteKingsideCastling;
        if (board.WhiteCastleQueenside)
            hash ^= whiteQueensideCastling;
        if (board.BlackCastleKingside)
            hash ^= blackKingsideCastling;
        if (board.BlackCastleQueenside)
            hash ^= blackQueensideCastling;

        if (board.EnPassantSquare >= 0 && board.EnPassantSquare < 64)
        {
            int file = Board.GetPositionFromIndex(board.EnPassantSquare).file - 1; // if GetPositionFromIndex returns 1-indexed file.
            hash ^= enPassantTable[file];
        }

        // Side to move: XOR if it is Black's turn
        if (board.ColorToMove == Pieces.Black)
            hash ^= sideToMoveValue;

        return hash;
    }

    private int GetPieceIndex(int piece)
    {
        return (Pieces.GetColor(piece) == Pieces.White ? Pieces.GetPieceType(piece) : Pieces.GetPieceType(piece) + 6) - 1 ;
    }

    public ulong UpdateHashForMove(ulong currentHash, Board newBoard, Board oldBoard, MoveGenerator.Move move)
    {
        int fromSquare = move.StartSquare;
        int toSquare = move.TargetSquare;
        int movingPiece = oldBoard.Square[fromSquare];
        int capturedPiece = oldBoard.Square[toSquare];

        int oldEnPassantFile = Board.GetPositionFromIndex(oldBoard.EnPassantSquare).file; 
        int newEnPassantFile = Board.GetPositionFromIndex(newBoard.EnPassantSquare).file;

        // Remove moving piece from its original square and add it to the destination.
        int pieceIndex = GetPieceIndex(movingPiece);
        currentHash ^= zobristTable[pieceIndex, fromSquare];
        currentHash ^= zobristTable[pieceIndex, toSquare];

        // If there's a captured piece, remove it from the destination square.
        if (Pieces.GetPieceType(capturedPiece) != Pieces.Empty)
        {
            int capturedPieceIndex = GetPieceIndex(capturedPiece);
            currentHash ^= zobristTable[capturedPieceIndex, toSquare];
        }

        // Castling rights update
        if (oldBoard.WhiteCastleKingside != newBoard.WhiteCastleKingside)
        {
            if (oldBoard.WhiteCastleKingside) currentHash ^= whiteKingsideCastling;
            if (newBoard.WhiteCastleKingside) currentHash ^= whiteKingsideCastling;
            currentHash ^= zobristTable[3, 7];
            currentHash ^= zobristTable[3, 5];
        }
        if (oldBoard.WhiteCastleQueenside != newBoard.WhiteCastleQueenside)
        {
            if (oldBoard.WhiteCastleQueenside) currentHash ^= whiteQueensideCastling;
            if (newBoard.WhiteCastleQueenside) currentHash ^= whiteQueensideCastling;
            currentHash ^= zobristTable[3, 0];
            currentHash ^= zobristTable[3, 3];
        }
        if (oldBoard.BlackCastleKingside != newBoard.BlackCastleKingside)
        {
            if (oldBoard.BlackCastleKingside) currentHash ^= blackKingsideCastling;
            if (newBoard.BlackCastleKingside) currentHash ^= blackKingsideCastling;
            currentHash ^= zobristTable[8, 63];
            currentHash ^= zobristTable[8, 61];
        }

        if (oldBoard.BlackCastleQueenside != newBoard.BlackCastleQueenside)
        {
            if (oldBoard.BlackCastleQueenside) currentHash ^= blackQueensideCastling;
            if (newBoard.BlackCastleQueenside) currentHash ^= blackQueensideCastling;
            currentHash ^= zobristTable[8, 56];
            currentHash ^= zobristTable[8, 59];
        }

        //En passant capture
        if (movingPiece == Pieces.Pawn && Board.GetPositionFromIndex(toSquare).file == oldEnPassantFile)
        {
            int epCaptureSquare = oldBoard.EnPassantSquare;
            int capturedPawnPiece = oldBoard.Square[epCaptureSquare];
            int capturedPawnIndex = GetPieceIndex(capturedPawnPiece);

            currentHash ^= zobristTable[capturedPawnIndex, epCaptureSquare];
        }

        // En passant update
        if (oldEnPassantFile >= 0 && oldEnPassantFile < 8)
        {
            currentHash ^= enPassantTable[oldEnPassantFile];
        }
        if (newEnPassantFile >= 0 && newEnPassantFile < 8)
        {
            currentHash ^= enPassantTable[newEnPassantFile];
        }

        //Promotion if promoting to queen
        if (oldBoard.ColorToMove == Pieces.White && movingPiece == Pieces.Pawn && toSquare is >= 56 and <= 63)
        {
            currentHash ^= zobristTable[0, toSquare];
            currentHash ^= zobristTable[4, toSquare];
        }
        else if (oldBoard.ColorToMove == Pieces.Black && movingPiece == Pieces.Pawn && toSquare is >= 0 and <= 7)
        {
            currentHash ^= zobristTable[6, toSquare];
            currentHash ^= zobristTable[10, toSquare];
        }

        // Side-to-move update
        currentHash ^= sideToMoveValue;

        return currentHash;
    }
}
