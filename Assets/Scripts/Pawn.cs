using System.Collections;
using UnityEngine;

public class Pawn : Piece
{

    public override bool[,] PossibleMoves()
    {
        bool[,] r = new bool[8, 8];

        Piece c, c2;

        int[] e = BoardManager.Instance.EnPassantMove;

        if (isWhite)
        {
            ////// White team move //////

            // Diagonal left
            if (CurrentX != 0 && CurrentY != 7)
            {
                if(e[0] == CurrentX -1 && e[1] == CurrentY + 1)
                    r[CurrentX - 1, CurrentY + 1] = true;

                c = BoardManager.Instance.pieces[CurrentX - 1, CurrentY + 1];
                if (c != null && !c.isWhite)
                    r[CurrentX - 1, CurrentY + 1] = true;
            }

            // Diagonal right
            if (CurrentX != 7 && CurrentY != 7)
            {
                if (e[0] == CurrentX + 1 && e[1] == CurrentY + 1)
                    r[CurrentX + 1, CurrentY + 1] = true;

                c = BoardManager.Instance.pieces[CurrentX + 1, CurrentY + 1];
                if (c != null && !c.isWhite)
                    r[CurrentX + 1, CurrentY + 1] = true;
            }

            // Middle
            if (CurrentY != 7)
            {
                c = BoardManager.Instance.pieces[CurrentX, CurrentY + 1];
                if (c == null)
                    r[CurrentX, CurrentY + 1] = true;
            }

            // Middle on first move
            if (CurrentY == 1)
            {
                c = BoardManager.Instance.pieces[CurrentX, CurrentY + 1];
                c2 = BoardManager.Instance.pieces[CurrentX, CurrentY + 2];
                if (c == null && c2 == null)
                    r[CurrentX, CurrentY + 2] = true;
            }
        }
        else
        {
            ////// Black team move //////

            // Diagonal left
            if (CurrentX != 0 && CurrentY != 0)
            {
                if (e[0] == CurrentX - 1 && e[1] == CurrentY - 1)
                    r[CurrentX - 1, CurrentY - 1] = true;

                c = BoardManager.Instance.pieces[CurrentX - 1, CurrentY - 1];
                if (c != null && c.isWhite)
                    r[CurrentX - 1, CurrentY - 1] = true;
            }

            // Diagonal right
            if (CurrentX != 7 && CurrentY != 0)
            {
                if (e[0] == CurrentX + 1 && e[1] == CurrentY - 1)
                    r[CurrentX + 1, CurrentY - 1] = true;

                c = BoardManager.Instance.pieces[CurrentX + 1, CurrentY - 1];
                if (c != null && c.isWhite)
                    r[CurrentX + 1, CurrentY - 1] = true;
            }

            // Middle
            if (CurrentY != 0)
            {
                c = BoardManager.Instance.pieces[CurrentX, CurrentY - 1];
                if (c == null)
                    r[CurrentX, CurrentY - 1] = true;
            }

            // Middle on first move
            if (CurrentY == 6)
            {
                c = BoardManager.Instance.pieces[CurrentX, CurrentY - 1];
                c2 = BoardManager.Instance.pieces[CurrentX, CurrentY - 2];
                if (c == null && c2 == null)
                    r[CurrentX, CurrentY - 2] = true;
            }
        }

        return r;
    }

    /// <summary>
    /// Returns the diagonal squares for pawns.
    /// </summary>
    public bool[,] AttackingMoves(){
        bool[,] r = new bool[8,8];

        Piece c;

        if (isWhite)
        {
            // Diagonal left
            if (CurrentX != 0 && CurrentY != 7)
            {
                c = BoardManager.Instance.pieces[CurrentX - 1, CurrentY + 1];
                r[CurrentX - 1, CurrentY + 1] = true;
            }

            // Diagonal right
            if (CurrentX != 7 && CurrentY != 7)
            {
                c = BoardManager.Instance.pieces[CurrentX + 1, CurrentY + 1];
                r[CurrentX + 1, CurrentY + 1] = true;
            }
        } else {
            // Diagonal left
            if (CurrentX != 0 && CurrentY != 0)
            {
                c = BoardManager.Instance.pieces[CurrentX - 1, CurrentY - 1];
                r[CurrentX - 1, CurrentY - 1] = true;
            }

            // Diagonal right
            if (CurrentX != 7 && CurrentY != 0)
            {
                c = BoardManager.Instance.pieces[CurrentX + 1, CurrentY - 1];
                r[CurrentX + 1, CurrentY - 1] = true;
            }
        }

        return r;
    }
}
