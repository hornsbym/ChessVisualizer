using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    // Makes the BoardManager a singleton?
    public static BoardManager Instance { get; set; }

    // Multidimensional array for recording which moves are valid 
    // for a particular piece
    // [,] is for multidimensional arrays
    private bool[,] allowedMoves { get; set; }

    // Sets baseline tile size and offset for laying
    // out pieces on the board
    public const float TILE_SIZE = 1.0f;
    public const float TILE_OFFSET = 0.5f;

    [SerializeField]
    public float SECONDARY_TILE_SIZE;
    [SerializeField]
    public float SECONDARY_TILE_OFFSET;

    // Keeps track of which square is selected.
    // The selected square is changed by hovering over the square.
    private int selectionX = -1;
    private int selectionY = -1;

    /// I think the difference between "activeChessman"
    /// and "selectedChessman" is as follows:
    ///    - activeChessman = Unity game object representing the
    ///      selected chess piece
    ///    - selectedChessman = abstraction of a game piece; doesn't
    ///      have any direct appearence in the game

    // Keeps track of the chess piece prefabs?
    // The "piecePrefabs" property contains the blueprints for 
    // instantiateing chess pieces. They do not represent the pieces
    // that appear on the board
    //
    // Why is the "pieceGameObject" property a list?
    // These represent the pieces that are actually instantiated into
    // the game. These are the pieces that will be manipulated by the player.
    public List<GameObject> piecePrefabs;
    private List<GameObject> pieceGameObjects;
    public List<GameObject> secondaryPiecePrefabs;
    private List<GameObject> secondaryPieceGameObjects;

    // Used for instantiating pieces facing the right direction
    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    /// Multidimensional array of chess pieces
    public Piece[,] pieces { get; set; }
    public Piece[,] secondaryPieces {get; set; }
    private Piece selectedPiece;

    // Boolean for maintaining which turn is which
    public bool isWhiteTurn = true;

    // When pieces are captured, they go to one of these spots
    public GameObject whiteDiscardZone;
    public GameObject blackDiscardZone;

    // Maintains references to the various materials that will
    // appear on the board
    private Material previousMat;
    public Material selectedMat;

    // I have not clue what this is for
    public int[] EnPassantMove { set; get; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
        SpawnAllPieces();
        EnPassantMove = new int[2] { -1, -1 };
    }

    /// <summary> 
    /// Core game loop.
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        UpdateSelection();

        // Loop over entire game board and hightlight the potential moves
        // only for the active color.
        // Disabled when there's a selected piece.
        if (selectedPiece == null)
        {
            bool[,] allPossibleMoves = new bool[8,8];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece piece = pieces[x, y];
                    if (piece != null && piece.isWhite == isWhiteTurn)
                    {
                        // Gets all the possible moves for each piece
                        bool[,] piecePossibleMoves = piece.PossibleMoves();

                        // Combines all pieces' possible moves into one board 
                        allPossibleMoves = combinePossibleMoves(allPossibleMoves, piecePossibleMoves);
                    }
                }
            }
            // Highlight all possible moves for the entire team
            BoardHighlights.Instance.HighLightAllowedMoves(allPossibleMoves);
        } else {
            // Draws highlights with only the selected piece's possible
            // moves highlighted
            allowedMoves = selectedPiece.PossibleMoves();
            BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
        }

        // Handles mouse click event
        if (Input.GetMouseButtonDown(0))
        {
            // Selects or moves a piece.
            if (selectionX >= 0 && selectionY >= 0)
            {   
                // If there's no selected piece, selects the piece
                // at the selected square
                if (selectedPiece == null)
                {
                    // Select the piece
                    SelectPiece(selectionX, selectionY);
                }
                // If there is a selected piece, try to move it.
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }

        // Handles shift key input 
        if(Input.GetKeyDown("left shift"))
        {   
            CameraManager.Instance.toggleActiveCameras();
        }

        if(Input.GetKeyUp("left shift")) 
        {
            CameraManager.Instance.toggleActiveCameras();
        }
        

        // Handles the  escape key pressed event
        if (Input.GetKey("escape"))
            Application.Quit();

    }

    /// <summary>
    /// Takes in two boolean nested arrays representing game boards.
    /// Combines them into a single board where if a square on either
    /// board is "true", the corresponding square on the output board
    /// will also be true.
    /// </summary>
    private bool[,] combinePossibleMoves(bool[,] board1, bool[,] board2){
        bool[,] combinedBoards = new bool[8, 8];

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                // Either one of the boards has the square as true,
                // so set the new combined board to true as well.
                if (board1[i,j] || board2[i,j]){
                    combinedBoards[i,j] = true;
                }
                // Neither board has the square set to true, so set 
                // the combined board square to false
                else {
                    combinedBoards[i,j] = false;
                }
            }
        }

        return combinedBoards;
    }

    ///<summary>
    /// Handles logic dealing with selecting a chess piece.
    ///</summary>
    private void SelectPiece(int x, int y)
    {
        // Doesn't let you select empty squares
        // When an empty square or an enemy square is clicked, 
        // deselect the current piece.
        if (pieces[x, y] == null) {
            selectedPiece = null;
            return;
        }

        // Doesn't let you select a piece that isn't the correct color
        if (pieces[x, y].isWhite != isWhiteTurn) {
            selectedPiece = null;
            return;
        }

        // Gets all the allowed moves for the selected piece.
        // This will be different for each piece type.
        // This is stored at the class level, so it can later be
        // accessed by other methods.
        allowedMoves = pieces[x, y].PossibleMoves();

        // Selects the abstract piece
        selectedPiece = pieces[x, y];

        // Applies a texture to the selected piece.
        previousMat = selectedPiece.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedPiece.GetComponent<MeshRenderer>().material = selectedMat;

        // Highlights the allowed moves for the selected piece.
        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    /// <summary>
    /// Removes a piece from the board and places it  
    /// in an empty discard space.
    /// </summary>
    private void capture(Piece capturedPiece) {
        // Capture a piece
        // If the King is captured, immediately end the game.
        if (capturedPiece.GetType() == typeof(King))
        {
            // End the game
            EndGame();
            return;
        }

        // Removes the captured piece's game object from the board
        pieceGameObjects.Remove(capturedPiece.gameObject);
        secondaryPieceGameObjects.Remove(capturedPiece.gameObject);

        // Move to a random spot on the discard zones
        if (capturedPiece.isWhite) {
            Renderer whiteDiscardZoneRenderer = whiteDiscardZone.GetComponent<Renderer>();
            Vector3 whiteDiscardZoneMins = whiteDiscardZoneRenderer.bounds.min;
            Vector3 whiteDiscardZoneMaxs = whiteDiscardZoneRenderer.bounds.max;

            Vector3 randomWhiteDiscardSpot = new Vector3(
                UnityEngine.Random.Range(whiteDiscardZoneMins.x, whiteDiscardZoneMaxs.x),
                0,
                UnityEngine.Random.Range(whiteDiscardZoneMins.z, whiteDiscardZoneMaxs.z)
            );

            capturedPiece.gameObject.transform.position = randomWhiteDiscardSpot;
        } else {
            Renderer blackDiscardZoneRenderer = blackDiscardZone.GetComponent<Renderer>();

            Vector3 blackDiscardZoneMins = blackDiscardZoneRenderer.bounds.min;
            Vector3 blackDiscardZoneMaxs = blackDiscardZoneRenderer.bounds.max;

            Vector3 randomBlackDiscardSpot = new Vector3(
                UnityEngine.Random.Range(blackDiscardZoneMins.x, blackDiscardZoneMaxs.x),
                0,
                UnityEngine.Random.Range(blackDiscardZoneMins.z, blackDiscardZoneMaxs.z)
            );

            capturedPiece.gameObject.transform.position = randomBlackDiscardSpot;
        }
    }

    // Handles move selection logic for the provided coordinates
    private void MoveChessman(int x, int y)
    {   
        // Handles when the selected piece is in the set of allowed moves
        if (allowedMoves[x, y])
        {   
            // Gets whatever piece is on selected coordinates
            Piece c = pieces[x, y];

            // Handles logic for enemy pieces
            if (c != null && c.isWhite != isWhiteTurn)
            {
                capture(c);
            }

            // Handles en passant special case
            // TODO: Figure this en passant logic later
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = pieces[x, y - 1];
                else
                    c = pieces[x, y + 1];

                pieceGameObjects.Remove(c.gameObject);
                
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            // Pawn specific logic (promotions, en passant)
            if (selectedPiece.GetType() == typeof(Pawn))
            {
                // White Promotion
                if(y == 7) 
                {
                    pieceGameObjects.Remove(selectedPiece.gameObject);
                    Destroy(selectedPiece.gameObject);
                    SpawnPiece(1, x, y, true);
                    selectedPiece = pieces[x, y];
                }
                 // Black Promotion
                else if (y == 0)
                {
                    pieceGameObjects.Remove(selectedPiece.gameObject);
                    Destroy(selectedPiece.gameObject);
                    SpawnPiece(7, x, y, false);
                    selectedPiece = pieces[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedPiece.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedPiece.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            // Nullifies the current position
            pieces[selectedPiece.CurrentX, selectedPiece.CurrentY] = null;
            
            // Changes the selected piece's position to the new square in the game
            selectedPiece.transform.position = GetTileCenter(x, y);
            selectedPiece.SetPosition(x, y);

            // Changes the selected piece's position in the board manager
            pieces[x, y] = selectedPiece;

            // Changes to the other color's turn
            isWhiteTurn = !isWhiteTurn;

            // Removes the piece highlight effect
            selectedPiece.GetComponent<MeshRenderer>().material = previousMat;

            // Deselects the piece after completing the move
            selectedPiece = null;

            // Turns the camera to the next person's view
            CameraManager.Instance.revolve();
        }
        // Allows players to switch their selected pieces
        else if (pieces[x,y] != null)  {
            // Removes the piece highlight effect
            selectedPiece.GetComponent<MeshRenderer>().material = previousMat;

            SelectPiece(x, y);
        } else {
            // Removes the piece highlight effect
            selectedPiece.GetComponent<MeshRenderer>().material = previousMat;

            // Deselects the piece
            selectedPiece = null;
        }
    }

    /// <summary> 
    /// Updates the selected square coordinates
    /// </summary>
    private void UpdateSelection()
    {
        if (!Camera.main) return;

        // I think this is for determining where the mouse is 
        // aimed in relation to how the camera is positioned.
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }
 
    /// <summary>
    /// Puts a new chess piece on the board at the specified location.
    /// The "index" determines which type of piece should be spawned.
    /// </summary>
    private void SpawnPiece(int index, int x, int y, bool isWhite)
    {
        // Finds the position the 3d chess piece should be placed
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        // Finds the position the 2d chess piece should be placed
        Vector3 secondaryPosition = Get2dTileCenter(x, y);
        GameObject secondaryGo;

        if (isWhite)
        {
            // Place the 3d piece with the appropriate orientation
            go = (GameObject) Instantiate(piecePrefabs[index], position, whiteOrientation);
        }
        else
        {   
            // Place the 3d piece with the appropriate orientation
            go = (GameObject) Instantiate(piecePrefabs[index], position, blackOrientation);
        }

        // Place the 2d piece with no color-specific orientation
        secondaryGo = (GameObject) Instantiate(secondaryPiecePrefabs[index],
            secondaryPosition, Quaternion.Euler(90, 0, 0));

        // Not sure why this is necessary
        go.transform.SetParent(transform);
        secondaryGo.transform.SetParent(transform);

        // Creates and tracks the piece models
        pieces[x, y] = go.GetComponent<Piece>();
        pieces[x, y].SetPosition(x, y);

        // Keeps references to the piece views
        pieceGameObjects.Add(go);
        secondaryPieceGameObjects.Add(secondaryGo);
    }

    /// <summary>
    /// Finds squares on the 3d board.
    /// </summary>
    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

    /// <summary>
    /// Finds squares on the 2d board.
    /// </summary>
    private Vector3 Get2dTileCenter(int x, int y) {
        Vector3 secondaryOrigin = new Vector3(45, 0, 0);

        secondaryOrigin.x += (SECONDARY_TILE_SIZE * x) + SECONDARY_TILE_OFFSET;
        secondaryOrigin.z += (SECONDARY_TILE_SIZE * y) + SECONDARY_TILE_OFFSET;

        return secondaryOrigin;
    }

    /// <summary>
    /// Initializes the game board.
    /// <remarks>
    /// Piece types are indexed like this:
    /// White King = 0, Black King = 6
    /// White Queen = 1, Black Queen = 7
    /// White Rook = 2, Black Rook = 8
    /// White Bishop = 3, Black Bishop = 9
    /// White Knight = 4, Black Knight = 10
    /// White Pawns = 5, Black Pawn = 11
    /// </remarks>
    /// </summary>
    private void SpawnAllPieces()
    {
        // Resets the active chess piece to a new blank list (why a list?)
        pieceGameObjects = new List<GameObject>();
        secondaryPieceGameObjects = new List<GameObject>();

        // Resets all pieces in the game to a blank chess board
        pieces = new Piece[8, 8];

        /////// White ///////

        // King
        SpawnPiece(0, 4, 0, true);

        // Queen
        SpawnPiece(1, 3, 0, true);

        // Rooks
        SpawnPiece(2, 0, 0, true);
        SpawnPiece(2, 7, 0, true);

        // Bishops
        SpawnPiece(3, 2, 0, true);
        SpawnPiece(3, 5, 0, true);

        // Knights
        SpawnPiece(4, 1, 0, true);
        SpawnPiece(4, 6, 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnPiece(5, i, 1, true);
        }


        /////// Black ///////

        // King
        SpawnPiece(6, 4, 7, false);

        // Queen
        SpawnPiece(7, 3, 7, false);

        // Rooks
        SpawnPiece(8, 0, 7, false);
        SpawnPiece(8, 7, 7, false);

        // Bishops
        SpawnPiece(9, 2, 7, false);
        SpawnPiece(9, 5, 7, false);

        // Knights
        SpawnPiece(10, 1, 7, false);
        SpawnPiece(10, 6, 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnPiece(11, i, 6, false);
        }
    }
    
    /// Declares a winner and resets the game
    private void EndGame()
    {
        // Declare the winner
        if (isWhiteTurn)
            Debug.Log("White wins");
        else
            Debug.Log("Black wins");


        // Remove all current pieces
        foreach (GameObject go in pieceGameObjects)
        {
            Destroy(go);
        }

        // Reset the game state
        isWhiteTurn = true;
        // BoardHighlights.Instance.HideHighlights();
        SpawnAllPieces();
    }
}


