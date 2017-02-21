using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum Direction { Left, Right, Up, Down }

    public static int Size { get { return 7; } }
    public static float moveDistance { get { return Size * 2; } }
    public static float LiftDistance { get { return Size * 0.7f; } }

    public static Tile ActiveTile;

    private static float slideTime = 0.5f;  // Time for tiles to take when moving during a slide
    private static bool areSliding = false;

    public bool IsMovable  { get; private set; }
    public bool IsEdgeTile { get; private set; }

    public GridIndex Index { get; private set; }

    private MeshRenderer mRenderer;
    private static Material mainMaterial;
    private static Material hoverMaterial;

    private Rigidbody mBody;

    void Awake()
    {
        mRenderer = transform.FindChild("Floor").gameObject.GetComponent<MeshRenderer>();   
        mainMaterial = Resources.Load<Material>("TileMainMaterial");
        hoverMaterial = Resources.Load<Material>("TileHoverMaterial");
        mBody = gameObject.GetComponent<Rigidbody>();
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
//            GameObject.Destroy(gameObject.GetComponent<BoxCollider>());
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
        }
        return this;
    }

    // Simple hover indication for now TODO Use an outline instead
    void OnMouseEnter()
    {
        if (IsEdgeTile && IsMovable) { mRenderer.material = hoverMaterial; }
    }

    void OnMouseExit()
    {
        if (mRenderer.material != mainMaterial && ActiveTile != this) { mRenderer.material = mainMaterial; }
    }

    // Clicking on moveable tiles slides that row or column by a tile
    void OnMouseDown()
    {
        if (areSliding) { Debug.LogWarning("Already sliding, ignorning click"); return; }  // Ignore clicks when already processign a slide

        if (ActiveTile == this)  // Second click moves the tile
        {
            Map.Get.SlideTiles(Index);
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

    // Public facing interface for the coroutine
    public void Slide(Direction direction, bool shouldLift = false)
    {
        if (shouldLift) { StartCoroutine(Co_Lift(direction)); }
        else { StartCoroutine(Co_Slide(direction)); }
    }

    // Slides the tile in the specified direction
    private IEnumerator Co_Slide(Direction direction)
    {
        float endTime = Time.fixedTime + slideTime;

        switch (direction)
        {
            case Direction.Down:
            {
                while (Time.fixedTime < endTime)
                {
                    mBody.MovePosition(transform.position + ((Vector3.back * moveDistance * 2) * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }
//                transform.Translate(Vector3.down * moveDistance);
                Index = Index.SetRow(Index.Row + 1);
                break;
            }
            case Direction.Up:
            {
                while (Time.fixedTime < endTime)
                {
                    mBody.MovePosition(transform.position + ((Vector3.forward * moveDistance * 2) * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }
//                transform.Translate(Vector3.up * moveDistance);
                Index = Index.SetRow(Index.Row - 1);
                break;
            }
            case Direction.Right:
            {
                while (Time.fixedTime < endTime)
                {
                    mBody.MovePosition(transform.position + ((Vector3.right * moveDistance * 2) * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }
//                transform.Translate(Vector3.right * moveDistance);
                Index = Index.SetCol(Index.Col + 1);
                break;
            }
            case Direction.Left:
            {
                while (Time.fixedTime < endTime)
                {
                    mBody.MovePosition(transform.position + ((Vector3.left * moveDistance * 2) * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }
//                transform.Translate(Vector3.left * moveDistance);
                Index = Index.SetCol(Index.Col - 1);
                break;
            }
        }

        if (Index.Row == 0 || Index.Col == 0 || Index.Row == Map.Get.MapSizeVertical - 1 || Index.Col == Map.Get.MapSizeHorizontal - 1) { IsEdgeTile = true; }
        else { IsEdgeTile = false; }

        yield return new WaitForSeconds(slideTime);  // TODO Lerp these movements
    }

    private IEnumerator Co_Lift(Direction direction)
    {
        areSliding = true;  // Prevent other movement attempts until finished

        float endTime = Time.fixedTime + (slideTime * 0.5f);

        switch (direction)
        {
            case Direction.Down:
            {
                while (Time.fixedTime < endTime)
                {
                    mBody.MovePosition(transform.position + ((Vector3.up * LiftDistance * 2) * Time.fixedDeltaTime));
                    mBody.MovePosition(transform.position + ((Vector3.forward * (Map.Get.MapSizeVertical - 1) * 2) * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }

                endTime = Time.fixedTime + (slideTime * 0.5f);

                while (Time.fixedTime < endTime)
                {
                    mBody.MovePosition(transform.position + ((Vector3.down * LiftDistance * 2) * Time.fixedDeltaTime));
                    mBody.MovePosition(transform.position + ((Vector3.forward * (Map.Get.MapSizeVertical - 1) * 2) * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }

//                transform.Translate(Vector3.back * LiftDistance);
//                transform.Translate(Vector3.up * moveDistance * (Map.Get.MapSizeVertical - 1));
                Index = Index.SetRow(0);
                break;
            }
            case Direction.Up:
            {
                transform.Translate(Vector3.back * LiftDistance);
                transform.Translate(Vector3.down * moveDistance * (Map.Get.MapSizeVertical - 1));
                Index = Index.SetRow(Map.Get.MapSizeVertical - 1);
                break;
            }
            case Direction.Right:
            {
                transform.Translate(Vector3.back * LiftDistance);
                transform.Translate(Vector3.left * moveDistance * (Map.Get.MapSizeHorizontal - 1));
                Index = Index.SetCol(0);
                break;
            }
            case Direction.Left:
            {
                transform.Translate(Vector3.back * LiftDistance);
                transform.Translate(Vector3.right * moveDistance * (Map.Get.MapSizeHorizontal - 1));
                Index = Index.SetCol(Map.Get.MapSizeHorizontal - 1);
                break;
            }
        }

        areSliding = false;
    }
}
