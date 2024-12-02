using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Actions
{
    public static Action<string> OnHighlightedSquare;
    public static Action OnResetSquareColor;
    public static Action<int, int> OnPieceMove;
    public static Action<int> OnDestroyPiece;
}
