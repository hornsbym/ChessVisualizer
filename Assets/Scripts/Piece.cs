using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Represents a game piece
public abstract class Piece : MonoBehaviour
{
    // Describes which square the piece should occupy
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }

    // Whether or not the piece is white
    public bool isWhite;

    // Allows a piece to change its position on the board
    public void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

    /// "Virtual" allows the piece to overwritten by subclasses
    /// Each unique piece type will overwrite this
    public virtual bool[,] PossibleMoves()
    {
        return new bool[8, 8];
    }

    /// (?) I think this checks the piece can move to the provided coordinates 
    public bool Move(int x, int y, ref bool[,] r)
    {
        // Makes sure the projected square is within the bounds of the board
        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            // Tries to get the piece at the specified square
            Piece c = BoardManager.Instance.pieces[x, y];

            // If the square is empty, it can move there
            if (c == null)
                r[x, y] = true;

            // If the has a piece of the opposite color, it can move there
            // (by capturing the piece)
            else
            {
                if (isWhite != c.isWhite)
                    r[x, y] = true;
                return true;
            }
        }
        return false;
    }
}
