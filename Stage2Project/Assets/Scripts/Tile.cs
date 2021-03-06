﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum Direction { North, East, South, West }

    public static int Size { get { return 7; } }
    public static float moveDistance { get { return Size * 2; } }
    public static float LiftDistance { get { return Size * 5; } }

    public static Tile ActiveTile;

    private static float slideTime = 1.5f;  // Time for tiles to take when moving during a slide
    private static bool areSliding = false;

    public GridIndex Index     { get; private set; }
    public bool IsMovable      { get; private set; }
    public bool IsEdgeTile     { get; private set; }

    public enum TileType { Normal, Decontam, Spawn, Escape }
    public TileType myTileType;

    public GameObject mFloor { get; private set; }

    public bool [] ExitDirections { get; private set; }  // Directions that have exits on this tile only (For valid exits use ExitTiles)
    public Tile [] ExitTiles  { get; private set; }  // Neighbouring tiles where both tiles share a matching exit.
    public bool [] exitDirectionsVisible;
    public Tile [] exitTilesVisible;


    private MeshRenderer mRenderer;
    private static Material mainMaterial;
    private static Material slideableMaterial;
    private static Material hoverMaterial;
    private static Material decontamLocationMaterial;
    private static Material decontamLocationWallMaterial;

    private Rigidbody mBody;

    private WaitForFixedUpdate waitForFixedUpdate;
    private WaitForSeconds waitForSlideTime = new WaitForSeconds(slideTime);

    private GameObject [] doors;

    public GridIndex VIndex; // TODO remove temp visualisation of index
    void Update()
    {
        VIndex = Index;
    }

    void Awake()
    {
        mainMaterial = Resources.Load<Material>("TileMainMaterial");
        slideableMaterial = Resources.Load<Material>("TileSlideableMaterial");
        hoverMaterial = Resources.Load<Material>("TileHoverMaterial");

        mBody = GetComponent<Rigidbody>();
        mFloor = transform.GetChild(0).gameObject;

        waitForFixedUpdate = new WaitForFixedUpdate();

        ExitDirections = new bool[4] { false, false, false, false };
    }

    // Initialisation for tiles inside map grid
    public Tile Init (int row, int col, bool isMovable, bool isEdgeTile)
    {
        transform.position = (new Vector3 (Size * col, 0, Size * -row));
        Index = Index.SetRow(row);
        Index = Index.SetCol(col);
        IsMovable = isMovable;
        IsEdgeTile = isEdgeTile;

        myTileType = TileType.Normal;

        mRenderer = transform.FindChild("Floor").gameObject.GetComponent<MeshRenderer>();

        if (!IsMovable)
        {
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
        }

        if ( IsMovable
          && IsEdgeTile)
        {
            mRenderer.material = slideableMaterial;
        }

        LoadDoorsAndExits();

        return this;
    }

    // Initialisation for escape tiles
    public Tile Init (int row, int col)
    {
        transform.position = (new Vector3 (Size * col, 0, Size * -row));
        Index = Index.SetRow(row);
        Index = Index.SetCol(col);
        myTileType = TileType.Escape;

        return this;
    }

    // Simple hover indication for now
    void OnMouseEnter()
    {
        if(GameManager.Get.GameState == GameManager.State.Playing)
        {
            if ( IsEdgeTile
              && IsMovable)
            { 
                mRenderer.material = hoverMaterial;
            }
        }
    }

    void OnMouseExit()
    {
        if(GameManager.Get.GameState == GameManager.State.Playing)
        {
            if ( IsEdgeTile
              && IsMovable)
            {
                if (mRenderer.material != mainMaterial && ActiveTile != this)
                { 
                    mRenderer.material = slideableMaterial;
                }
            }
            else if (myTileType == TileType.Normal)
            { 
                mRenderer.material = mainMaterial;
            }
        }
    }

    // Clicking on moveable tiles slides that row or column by a tile
    void OnMouseDown()
    {
        // Ignore clicks when already processign a slide
        if (areSliding)
        {
            Debug.LogWarning("Already sliding, ignorning click");
            return;
        }

        if (ActiveTile == this)  // Second click moves the tile
        {
            Map.Get.SlideTiles(Index);
            ActiveTile = null;
        }
        else if (IsEdgeTile)
        {
            if (IsMovable)
            {
                ActiveTile = this;  // First click sets the tile as active
                // TODO Sound
            }
        }
    }

    // Set visual and reference for Spawn tile
    public void SetAsSpawnLocation()
    {
        mRenderer.material = Resources.Load<Material>("TileSpawnLocationMaterial");
        Material radiationWallMaterial = Resources.Load<Material>("WallRadiationMaterial");
        for (int i = 1; i <= 3; i++)
        {
            transform.GetChild(i).GetComponent<Renderer>().material = radiationWallMaterial;
        }
//        IsSpawnTile = true;
        myTileType = TileType.Spawn;
    }

    // Set visual and reference for Decontamination tile
    public void SetAsDecontaminationLocation()
    {
        mRenderer.material = Resources.Load<Material>("TileCureLocationMaterial");
        Material decontamLocationWallMaterial = Resources.Load<Material>("WallDecontaminationMaterial");
        for (int i = 1; i <= 3; i++)
        {
            transform.GetChild(i).GetComponent<Renderer>().material = decontamLocationWallMaterial;
        }
//        IsDecontamTile = true;
        myTileType = TileType.Decontam;
    }
    
    //----------------------------------------------------------------------------//
    //                             TILE MOVEMENT                                  //
    //----------------------------------------------------------------------------//

    // Public facing interface for the slide coroutines
    public void Slide(Direction direction, bool shouldLift = false)
    {
        if (shouldLift)
        {
            StartCoroutine(Co_Lift(direction));
        }
        else
        {
            StartCoroutine(Co_Slide(direction));
        }
    }

    // Slides the tile in the specified direction
    private IEnumerator Co_Slide(Direction direction)
    {
        // Check which direction to move tile in and set new index values BEFORE any yielding
        Vector3 directionPart = Vector3.zero;  // Store the Vertical / Horizontal movement component

        switch (direction)
        {
            case Direction.South:
            {
                Index = Index.SetRow(Index.Row + 1);  // Must do these before any yeild statements
                directionPart = Vector3.back * moveDistance;
                break;
            }
            case Direction.North:
            {
                Index = Index.SetRow(Index.Row - 1);
                directionPart = Vector3.forward * moveDistance;
                break;
            }
            case Direction.East:
            {
                Index = Index.SetCol(Index.Col + 1);
                directionPart = Vector3.right * moveDistance;
                break;
            }
            case Direction.West:
            {
                Index = Index.SetCol(Index.Col - 1);
                directionPart = Vector3.left * moveDistance;
                break;
            }
        }

        // Recheck edge tiles
        if ( Index.Row == 0
          || Index.Col == 0
          || Index.Row == Map.Get.MapSizeVertical - 1
          || Index.Col == Map.Get.MapSizeHorizontal - 1)
        {
            IsEdgeTile = true;
        }
        else
        {
            IsEdgeTile = false;
        }
        
        // Slide tile along row / column
        float endTime = Time.fixedTime + slideTime;
        while (Time.fixedTime < endTime)
        {
            mBody.MovePosition(transform.position + ((directionPart / slideTime) * Time.fixedDeltaTime));
            yield return waitForFixedUpdate;
        }

        FixDrift();  // Fix any transform drift

        // Set slideable colour for new edge tiles (
        if (Index.Row == 0)
        {
            mFloor.transform.rotation = Quaternion.Euler(Vector3.up * 180);
            mRenderer.material = slideableMaterial;
        }
        else if (Index.Col == 0)
        {
            mFloor.transform.rotation = Quaternion.Euler(Vector3.up * 90);
            mRenderer.material = slideableMaterial;
        } 
        else if (Index.Row == Map.Get.MapSizeVertical - 1)
        {
            mRenderer.material = slideableMaterial;
        }
        else if (Index.Col == Map.Get.MapSizeHorizontal - 1)
        { 
            mFloor.transform.rotation = Quaternion.Euler(Vector3.up * 270);
            mRenderer.material = slideableMaterial;
        }
    }

    // Lifts the end tile that will be pushed off the map and places it at the start of the row / column
    private IEnumerator Co_Lift(Direction direction)
    {
        areSliding = true;  // Prevent other movement attempts until finished

        // Check which movement direction to store and set new index values BEFORE any yielding
        Vector3 directionPart = Vector3.zero;  // Store the Vertical / Horizontal movement component

        switch (direction)
        {
            case Direction.South:
            {
                Index = Index.SetRow(0);  // Must do these before any yeild statements
                directionPart = Vector3.forward * (moveDistance * (Map.Get.MapSizeVertical - 1));
                break;
            }
            case Direction.North:
            {
                Index = Index.SetRow(Map.Get.MapSizeVertical - 1);
                directionPart = Vector3.back * (moveDistance * (Map.Get.MapSizeVertical - 1));
                break;
            }
            case Direction.East:
            {
                Index = Index.SetCol(0);
                directionPart = Vector3.left * (moveDistance * (Map.Get.MapSizeHorizontal - 1));
                break;
            }
            case Direction.West:
            {
                Index = Index.SetCol(Map.Get.MapSizeHorizontal - 1);
                directionPart = Vector3.right * (moveDistance * (Map.Get.MapSizeHorizontal - 1));
                break;
            }
        }

        // Recheck edge tiles
        if ( Index.Row == 0 
          || Index.Col == 0
          || Index.Row == Map.Get.MapSizeVertical - 1 
          || Index.Col == Map.Get.MapSizeHorizontal - 1)
        {
            IsEdgeTile = true;
            // Rotate texture to face correct direction
            mFloor.transform.Rotate(Vector3.up, 180);
        }
        else
        {
            IsEdgeTile = false;
        }

        // Lift and slide tile to start of row / column
        Vector3 liftPart = Vector3.zero;  // Store the Up / Down movement component

        ToggleAllDoors(true);  // Close the doors for takeoff

        // Curved tile movment
        //*/
        float startTime = Time.fixedTime;
        float endTime = startTime + slideTime;
        while (Time.fixedTime < endTime)
        {
            float t = (Time.fixedTime - startTime) / slideTime;
            liftPart = Vector3.up * ((LiftDistance * GameManager.Get.liftCurve.Evaluate(t)) - transform.position.y);
            mBody.MovePosition((transform.position + ((directionPart / slideTime) * Time.fixedDeltaTime)) + liftPart);

            // Check for pausing
            while (GameManager.Get.GameState == GameManager.State.Paused)
            {
                
            }

            yield return waitForFixedUpdate;
        }
        /*/
        // Trianglular tile movement
        float endTime = Time.fixedTime + (slideTime * 0.5f);  // Set up half the time for the first movement
        while (Time.fixedTime < endTime)
        {
            liftPart = Vector3.up * LiftDistance;
            mBody.MovePosition(transform.position + (((directionPart + liftPart) / slideTime) * Time.fixedDeltaTime));
            yield return waitForFixedUpdate;
        }

        endTime = Time.fixedTime + (slideTime * 0.5f);  // Set remaining time for the second movement
        while (Time.fixedTime < endTime)
        {
            liftPart = Vector3.down * LiftDistance;
            mBody.MovePosition(transform.position + (((directionPart + liftPart) / slideTime) * Time.fixedDeltaTime));
            yield return waitForFixedUpdate;
        }
        //*/

        ToggleAllDoors(false);  // Reopen doors after landing

        FixDrift();  // Fix any transform drift

        areSliding = false;  // Re-enable other movements
    }

    // Fix small transform drifts by moving back by any remainer after dividing by move distances.
    // TODO Rarely a small dift still occours and this fix just amplifies it?
    private void FixDrift()
    {
        float horizontalDrift = transform.position.x % moveDistance;
//        print(horizontalDrift);
        if (horizontalDrift >= Size)
        {
            horizontalDrift = moveDistance - horizontalDrift;
        }
        else if (horizontalDrift <= -Size)
        {
            horizontalDrift = -(moveDistance + horizontalDrift);
        }

        float verticalDrift = transform.position.z % moveDistance;
//        print(verticalDrift);
        if (verticalDrift >= Size)
        {
            verticalDrift = moveDistance - verticalDrift;
        }
        else if (verticalDrift <= -Size)
        {
            verticalDrift = -(moveDistance + verticalDrift);
        }

        Vector3 transformDrift = new Vector3(horizontalDrift, transform.position.y, verticalDrift);
        mBody.MovePosition(transform.position - transformDrift);
    }

    //----------------------------------------------------------------------------//
    //                             DOORS & EXITS                                  //
    //----------------------------------------------------------------------------//

    // Check to see which edges have doors and store references 
    private void LoadDoorsAndExits()
    {
        doors = new GameObject[4];
        Transform checkDoor;

        checkDoor = transform.FindChild("Door_N");
        if (checkDoor != null)
        {
            doors[(int) Direction.North] = checkDoor.gameObject;
            ExitDirections[(int) Direction.North] = true;
        }
        checkDoor = transform.FindChild("Door_E");
        if (checkDoor != null)
        {
            doors[(int) Direction.East] = checkDoor.gameObject;
            ExitDirections[(int) Direction.East] = true;
        }
        checkDoor = transform.FindChild("Door_S");
        if (checkDoor != null)
        {
            doors[(int) Direction.South] = checkDoor.gameObject;
            ExitDirections[(int) Direction.South] = true;
        }
        checkDoor = transform.FindChild("Door_W");
        if (checkDoor != null)
        {
            doors[(int) Direction.West] = checkDoor.gameObject;
            ExitDirections[(int) Direction.West] = true;
        }

        ToggleAllDoors(false);  // Turn off all doors at the start
        exitDirectionsVisible = ExitDirections;
    }

    // Close / open a door on a specific tile edge direction
    private void ToggleDoor(Direction direction, bool shouldBeClosed)
    {
        if (doors[(int) direction] != null)
        {
            doors[(int) direction].SetActive(shouldBeClosed);
        }
    }

    private void ToggleDoor(Direction direction)
    {
        if (doors[(int) direction] != null)
        {
            doors[(int) direction].SetActive(!doors[(int) direction].activeSelf);
        }
    }

    // Shortcut for toggling all potential doors on a tile
    private void ToggleAllDoors(bool shouldBeClosed)
    {
        ToggleDoor(Direction.North, shouldBeClosed);
        ToggleDoor(Direction.East,  shouldBeClosed);
        ToggleDoor(Direction.South, shouldBeClosed);
        ToggleDoor(Direction.West,  shouldBeClosed);
    }

    private void ToggleAllDoors()
    {
        ToggleDoor(Direction.North, !doors[(int) Direction.North].activeSelf);
        ToggleDoor(Direction.East,  !doors[(int) Direction.East].activeSelf);
        ToggleDoor(Direction.South, !doors[(int) Direction.South].activeSelf);
        ToggleDoor(Direction.West,  !doors[(int) Direction.West].activeSelf);
    }

    // Public facing interface for the door coroutine
    public void TriggerSafetyDoor(Direction direction)
    {
        StartCoroutine(Co_TriggerSafetyDoor(direction));
    }

    // Closes the door facing the gap created by a lifted tile (called by Map) then reopens it after the slide has finished
    private IEnumerator Co_TriggerSafetyDoor(Direction direction)
    {
        ToggleDoor(direction, true);
        yield return waitForSlideTime;
        ToggleDoor(direction, false);
    }

    public void UpdateExitTiles()
    {
        int numberOfExitTiles = 0;
        Tile[] temp = new Tile[4];
        Tile neighbour;
        for (int i = 0; i < 4; i++)
        {
            if (ExitDirections[i])
            {
                switch (i)
                {
                    case (int) Direction.North:
                    {
                        neighbour = Map.Get.TileNeighbour(this, Direction.North);
                        if (neighbour && neighbour.ExitDirections[(int) Direction.South])
                        {
                            temp[numberOfExitTiles] = neighbour;
                            numberOfExitTiles++;
                        }
                        break;
                    }
                    case (int) Direction.East:
                    {
                        neighbour = Map.Get.TileNeighbour(this, Direction.East);
                        if (neighbour && neighbour.ExitDirections[(int) Direction.West])
                        {
                            temp[numberOfExitTiles] = neighbour;
                            numberOfExitTiles++;
                        }
                        break;
                    }
                    case (int) Direction.South:
                    {
                        neighbour = Map.Get.TileNeighbour(this, Direction.South);
                        if (neighbour && neighbour.ExitDirections[(int) Direction.North])
                        {
                            temp[numberOfExitTiles] = neighbour;
                            numberOfExitTiles++;
                        }
                        break;
                    }
                    case (int) Direction.West:
                    {
                        neighbour = Map.Get.TileNeighbour(this, Direction.West);
                        if (neighbour && neighbour.ExitDirections[(int) Direction.East])
                        {
                            temp[numberOfExitTiles] = neighbour;
                            numberOfExitTiles++;
                        }
                        break;
                    }
                }
            }
        }

        Tile[] exitTiles = new Tile[numberOfExitTiles];
        for (int i = 0; i < numberOfExitTiles; i++)
        {
            exitTiles[i] = temp[i];
        }

        ExitTiles = exitTiles;
        exitTilesVisible = ExitTiles;
    }
}
