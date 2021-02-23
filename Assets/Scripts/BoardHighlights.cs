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
    public GameObject secondaryHighlightPrefab;

    [SerializeField]
    public Vector3 secondaryBoardOrigin;
    [SerializeField]
    public Vector3 secondaryHighlightOffsets;

    // Will hold all 64 highlight squares on the board
    private GameObject[,] highlights;
    private GameObject[,] secondaryHighlights;

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
        secondaryHighlights = new GameObject[8,8];

        // Get the height and width of the highlight objects
        Vector3 highlightExtents = highlightPrefab.GetComponent<Renderer>().bounds.extents;
        Vector3 secondaryHighlightsExtents = secondaryHighlightPrefab.GetComponent<Renderer>().bounds.extents;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {    
                // Instantiate the new highlight objects
                GameObject newHighlight = Instantiate(highlightPrefab);
                GameObject newSecondaryHighlight = Instantiate(secondaryHighlightPrefab);

                // Store the highlight on it's "square"
                highlights[i,j] = newHighlight;
                secondaryHighlights[i,j] = newSecondaryHighlight;

                // Tell the highlights where to draw themselves
                newHighlight.transform.position = new Vector3(i + highlightExtents.x, 0.0001f, j + highlightExtents.z);
                newSecondaryHighlight.transform.position = new Vector3(
                    secondaryBoardOrigin.x + (secondaryHighlightsExtents.x * i * 2),
                    -.001f,
                    secondaryBoardOrigin.z + (secondaryHighlightsExtents.z * j * 2)
                );
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
                secondaryHighlights[i,j].SetActive(moves[i,j]);
            }
        }
    }
}
