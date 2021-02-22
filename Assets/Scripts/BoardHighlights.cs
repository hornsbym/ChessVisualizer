using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Manages highlighting the squares on the board
public class BoardHighlights : MonoBehaviour
{
    // Makes the highlight manager a singleton
    public static BoardHighlights Instance { set; get; }

    // Maintains a reference to the highlight prefab
    public GameObject highlightPrefab;

    // Will hold all 64 highlight squares on the board
    private GameObject[,] highlights;

    private void Start()
    {   
        // Instantiates the singleton
        Instance = this;
        instantiateHighlightObjects();
    }

    /// <summary>
    /// Places a highlight prefab on each of square of the chess board.
    /// </summary>
    private void instantiateHighlightObjects() {
        // Create an empty 2-d array
        highlights = new GameObject[8,8];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {    
                // Instantiate the new highlight object
                GameObject newHighlight = Instantiate(highlightPrefab);

                // Store the highlight on it's "square"
                highlights[i,j] = newHighlight;

                // Tell the highlight where to draw itself
                newHighlight.transform.position = new Vector3(i + 0.5f, 0.0001f, j + 0.5f);
            }
        }
    }


    /// <summary>
    /// Accepts a 2d array of booleans representing the chessboard.
    /// If a square is set to "true", turns on the highlight at that location.
    /// Manually calculates where the highlight is placed by multiplying the rows
    /// and columns by the width of the highlight tiles.
    /// </summary>
    public void HighLightAllowedMoves(bool[,] moves)
    {

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                highlights[i,j].SetActive(moves[i,j]);

                if (moves[i,j]){
                    var activePair = new List<int>();
                    activePair.Add(i);
                    activePair.Add(j);
                }
            }

        }
    }
}
