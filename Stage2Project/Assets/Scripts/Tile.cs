using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum Direction { North, East, South, West }

    public static int Size { get { return 7; } }
    public static float moveDistance { get { return Size * 2; } }
    public static float LiftDistance { get { return Size * 10; } }

    public static Tile ActiveTile;

    private static float slideTime = 1.5f;  // Time for tiles to take when moving during a slide
    private static bool areSliding = false;

    public GridIndex Index { get; private set; }
    public bool IsMovable  { get; private set; }
    public bool IsEdgeTile { get; private set; }

    private bool IsSpawnTile = false;
    private bool IsCureTile = false;

    private MeshRenderer mRenderer;
    private static Material mainMaterial;
    private static Material slideableMaterial;
    private static Material hoverMaterial;
    private static Material spawnLocationMaterial;
    private static Material cureLocationMaterial;

    private Rigidbody mBody;

    private WaitForFixedUpdate waitForFixedUpdate;
    private WaitForSeconds waitForSlideTime = new WaitForSeconds(slideTime);

    private GameObject [] doors;

    void Awake()
    {
        mRenderer = transform.FindChild("Floor").gameObject.GetComponent<MeshRenderer>();   
        mainMaterial = Resources.Load<Material>("TileMainMaterial");
        slideableMaterial = Resources.Load<Material>("TileSlideableMaterial");
        hoverMaterial = Resources.Load<Material>("TileHoverMaterial");
        spawnLocationMaterial = Resources.Load<Material>("TileSpawnLocationMaterial");
        cureLocationMaterial = Resources.Load<Material>("TileCureLocationMaterial");

        mBody = gameObject.GetComponent<Rigidbody>();

        waitForFixedUpdate = new WaitForFixedUpdate();

        LoadDoors();
    }

    public Tile Init (int row, int col, bool isMovable, bool isEdgeTile)
    {
        transform.position = new Vector3 (Size * col, 0, Size * -row);
        Index = Index.SetRow(row);
        Index = Index.SetCol(col);
        IsMovable = isMovable;
        IsEdgeTile = isEdgeTile;

        if (!IsMovable)
        {
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
        }

        if (IsMovable && IsEdgeTile)
        {
            mRenderer.material = slideableMaterial;
        }

        return this;
    }

    // Simple hover indication for now TODO Use an outline instead
    void OnMouseEnter()
    {
        if (IsEdgeTile && IsMovable)
        { 
            mRenderer.material = hoverMaterial;
        }
    }

    void OnMouseExit()
    {
        if (IsEdgeTile && IsMovable)
        {
            if (mRenderer.material != mainMaterial && ActiveTile != this)
            { 
                mRenderer.material = slideableMaterial;
            }
        }
        else if (!IsSpawnTile && !IsCureTile)
        { 
            mRenderer.material = mainMaterial;
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

    // Set visual for Spawn tile
    public void SetAsSpawnLocation()
    {
        mRenderer.material = spawnLocationMaterial;
        IsSpawnTile = true;
    }

    // Set visual for Decontamination tile
    public void SetAsCureLocation()
    {
        mRenderer.material = cureLocationMaterial;
        IsCureTile = true;
    }

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
        if (  Index.Row == 0
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

        // Fix any transform drift
        float horizontalDrift = transform.position.x % moveDistance;
        if (horizontalDrift >= Size)
        { 
            horizontalDrift = moveDistance - horizontalDrift;
        }
        else if (horizontalDrift <= -Size)
        { 
            horizontalDrift = -(moveDistance + horizontalDrift);
        }

        float verticalDrift = transform.position.z % moveDistance;
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

        // Set slideable colour for new edge tiles (
        if (  Index.Row == 0 
           || Index.Col == 0 
           || Index.Row == Map.Get.MapSizeVertical - 1 
           || Index.Col == Map.Get.MapSizeHorizontal - 1)
        { 
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
        if (  Index.Row == 0 
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

        // Lift and slide tile to start of row / column
        Vector3 liftPart = Vector3.zero;  // Store the Up / Down movement component

        ToggleAllDoors(true);  // Close the doors for takeoff

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

        ToggleAllDoors(false);  // Reopen doors after landing

        // Fix any transform drift
        float horizontalDrift = transform.position.x % moveDistance;
        if (horizontalDrift >= Size)
        {
            horizontalDrift = moveDistance - horizontalDrift;
        }
        else if (horizontalDrift <= -Size)
        {
            horizontalDrift = -(moveDistance + horizontalDrift);
        }

        float verticalDrift = transform.position.z % moveDistance;
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

        areSliding = false;  // Re-enable other movements
    }

    // Check to see which edges have doors and store references 
    private void LoadDoors()
    {
        doors = new GameObject[4];
        Transform checkDoor;

        checkDoor = transform.FindChild("Door_N");
        if (checkDoor != null)
        {
            doors[(int) Direction.North] = checkDoor.gameObject;
        }
        checkDoor = transform.FindChild("Door_E");
        if (checkDoor != null)
        {
            doors[(int) Direction.East] = checkDoor.gameObject;
        }
        checkDoor = transform.FindChild("Door_S");
        if (checkDoor != null)
        {
            doors[(int) Direction.South] = checkDoor.gameObject;
        }
        checkDoor = transform.FindChild("Door_W");
        if (checkDoor != null)
        {
            doors[(int) Direction.West] = checkDoor.gameObject;
        }

        ToggleAllDoors(false);  // Turn off all doors at the start
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
}
