using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Assets.Scripts.Core;
using UnityEngine;

public static class HumanPlayer
{
    public static bool ValidMove(int startSquare, int targetSquare)
    {
        var movegenerator = new MoveGenerator(Board.Instance.CopyBoard());
        var moves = movegenerator.GenerateLegalMoves();

        foreach (MoveGenerator.Move move in moves)
        {
            if (move.StartSquare == startSquare && move.TargetSquare == targetSquare)
            {
                return true;
            }
        }

        return false;
    }

}
