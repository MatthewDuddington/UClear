using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum Direction { Left, Right, Up, Down, Lift }

    public static int Size { get { return 7; } }

    public static Tile ActiveTile;

    private static float slideTime = 1.5f;  // Time for tiles to take when moving during a slide
    private static bool isSliding = false;

    public bool IsMovable  { get; private set; }
    public bool IsEdgeTile { get; private set; }
    public GridIndex Index { get; private set; }

    private MeshRenderer mRenderer;
    private static Material mainMaterial;
    private static Material hoverMaterial;

    void Awake()
    {
        mRenderer = transform.FindChild("Floor").gameObject.GetComponent<MeshRenderer>();   
        mainMaterial = Resources.Load<Material>("TileMainMaterial");
        hoverMaterial = Resources.Load<Material>("TileHoverMaterial");
    }

    public Tile Init (int row, int col, bool isMovable, bool isEdgeTile)
    {
        transform.position = new Vector3 (Size * col, 0, Size * -row);
        Index.SetRow(row);
        Index.SetCol(col);
        IsMovable = isMovable;
        IsEdgeTile = isEdgeTile;
//      print("IsMovable" + isMovable + IsMovable + "IsEdge" + isEdgeTile + IsEdgeTile);
        return this;
    }

    // Simple hover indication for now TODO Use an outline instead
    void OnMouseEnter()
    {
//      print("enter");
        if (IsEdgeTile && IsMovable) { mRenderer.material = hoverMaterial; }
    }

    void OnMouseExit()
    {
//      print("exit");
        if (ActiveTile != this) { mRenderer.material = mainMaterial; }
    }

    // Clicking on moveable tiles slides that row or column by a tile
    void OnMouseDown()
    {
//      print("click");
        print(IsEdgeTile);
        print(IsMovable);

        if (isSliding) { Debug.LogWarning("Already sliding, ignorning click"); return; }  // Ignore clicks when already processign a slide

        if (ActiveTile == this)  // Second click moves the tile
        {
            Map.Get.SlideTiles(Index);
        }
        else if (IsEdgeTile)
        {
//          print("edge tile");
            if (IsMovable)
            {
//              print("moveable");
                ActiveTile = this;  // First click sets the tile as active
                // TODO Sound
            }
        }
    }

    // Public facing interface for the coroutine
    public void Slide(Direction direction)
    {
        StartCoroutine(Co_Slide(direction));
    }

    // Slides the tile in the specified direction
    private IEnumerator Co_Slide(Direction direction)
    {
        switch (direction)
        {
            case Direction.Down:
            {
                transform.Translate(Vector3.down * Size);
                Index.SetRow(Index.Row + 1);
                if (Index.Row > Map.Get.MapSizeVertical - 1) { Index.SetRow(0); }
                break;
            }
            case Direction.Up:
            {
                transform.Translate(Vector3.up * Size);
                Index.SetRow(Index.Row - 1);
                if (Index.Row < 0) { Index.SetRow(Map.Get.MapSizeVertical - 1); }
                break;
            }
            case Direction.Right:
            {
                transform.Translate(Vector3.right * Size);
                Index.SetRow(Index.Col + 1);
                if (Index.Col > Map.Get.MapSizeHorizontal - 1) { Index.SetCol(0); }
                break;
            }
            case Direction.Left:
            {
                transform.Translate(Vector3.left * Size);
                Index.SetRow(Index.Col - 1);
                if (Index.Col < 0) { Index.SetCol(Map.Get.MapSizeHorizontal - 1); }
                break;
            }
            case Direction.Lift:
            {
                isSliding = true;  // Prevent other movement attempts until finished
                transform.Translate(Vector3.back * 3);
                yield return new WaitForSeconds(slideTime / 2);
                transform.Translate(Vector3.forward * 3);
                isSliding = false;
                break;
            }
        }
    }

}
