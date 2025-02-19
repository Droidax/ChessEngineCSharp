using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Assets.Scripts.Core;
using UnityEngine;

public class HumanPlayer
{
    private Board board;
    private MoveGenerator moveGenerator;
    public HumanPlayer(Board board)
    {
        this.board = board;
        moveGenerator = new MoveGenerator(board);
    }

    public void SetBoard(Board newBoard)
    {
        this.board = newBoard;
    }
    public bool ValidMove(int startSquare, int targetSquare)
    {
        moveGenerator.GenerateLegalMoves();

        foreach (MoveGenerator.Move move in moveGenerator.legalMoves)
        {
            if (move.StartSquare == startSquare && move.TargetSquare == targetSquare)
            {
                return true;
            }
        }

        return false;
    }

}
