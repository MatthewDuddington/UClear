using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GridIndex
{
    public int Row { get; private set; }
    public int Col { get; private set; }

    /// <summary>
    /// Returns a copy of the GridIndex with an updated row.
    /// You MUST assign the returned copy back to the original variable to avoid pass by value nullification.
    /// </summary>
    public GridIndex SetRow(int rowIndex)
    {
        Row = rowIndex;
        return this;
    }
    /// <summary>
    /// Returns a copy of the GridIndex with an updated column.
    /// You MUST assign the returned copy back to the original variable to avoid pass by value nullification.
    /// </summary>
    public GridIndex SetCol(int colIndex)
    {
        Col = colIndex;
        return this;
    }

    public GridIndex(int row, int col)
    {
        Row = row;
        Col = col;
    }
}

public class Map : MonoBehaviour
{
    // Easy accessor for the class instance
    private static Map This;
    public static Map Get
    { 
        get
        {
            if (This == null)
            {
                Debug.LogError("No Map present in scene");
            }
            return This;
        }
        private set { This = value; }
    } 

    public enum TileType
    {
        Corner_SE,
        Corner_SW,
        Corner_NE,
        Corner_NW,
        TJunc_SEW,
        TJunc_NSW,
        TJunc_NEW,
        TJunc_NSE,
        FourWay,
        Straight_NS,
        Straight_EW
    }

    [SerializeField]
    private int mapSizeHorizontal;
    public int MapSizeHorizontal { get { return mapSizeHorizontal; } }
    [SerializeField]
    private int mapSizeVertical;
    public int MapSizeVertical   { get { return mapSizeVertical; } }

    [SerializeField]
    private Tile [] TilePrefabs;

    // A grid of Z rows * X cols map tiles
    private Tile [,] tileGrid;

    public Vector3 AgentSpawnLocation { get; private set; }
    public Tile AgentSpawnTile { get; private set; }

    void Awake()
    {
        // Set easy accessor for the class
        Get = this;

        // Check for map size having non-odd proportions and adjust if needed
        if ((mapSizeHorizontal) % 2 == 0)
        {
            Debug.LogWarning("Map Size horizontal proportion is non-odd, adjusting to compensate");
            mapSizeHorizontal ++;
        }
        if ((mapSizeVertical) % 2 == 0)
        {
            Debug.LogWarning("Map Size vertical proportion is non-odd, adjusting to compensate");
            mapSizeVertical ++;
        }

        // Setup size of tile grid 2D array
        tileGrid = new Tile[(int) MapSizeVertical, (int) MapSizeHorizontal];
    }

    public void GenerateNewMap()
    {
        foreach (Tile tile in tileGrid)
        {
            if (tile != null)
            {
                GameObject.DestroyImmediate(tile.gameObject);  // TODO Consider disabling and reusing tiles from a pool rather than destroying and reinstantiating each time.
            }
        }

        // Reset scale fix of the map. TODO Avoid having to do this
        transform.localScale = Vector3.one;

        AddDefaultTiles();
        RandomiseMoveableTiles();
        PositionMap();
        RandomiseGameLocations();
    }

    // Add the fixed corner, edge and central tiles
    private void AddDefaultTiles()
    {
        int cMax = MapSizeHorizontal - 1;  // Maximum col index
        int rMax = MapSizeVertical - 1;    // Maximum row index

        // Corners
        tileGrid[0   , 0   ] = Instantiate (TilePrefabs[0]).Init(0   , 0   , false, true);  // NW
        tileGrid[0   , cMax] = Instantiate (TilePrefabs[1]).Init(0   , cMax, false, true);  // NE
        tileGrid[rMax, 0   ] = Instantiate (TilePrefabs[2]).Init(rMax, 0   , false, true);  // SW
        tileGrid[rMax, cMax] = Instantiate (TilePrefabs[3]).Init(rMax, cMax, false, true);  // SE

        // Edges
        // North & South
        for (int col = 2; col < cMax; col += 2)
        {
            tileGrid[0   , col] = Instantiate (TilePrefabs[4]).Init(0   , col, false, true);  // N
            tileGrid[rMax, col] = Instantiate (TilePrefabs[6]).Init(rMax, col, false, true);  // S
        }
        // East & West
        for (int row = 2; row < rMax; row += 2)
        {
            tileGrid[row, cMax] = Instantiate (TilePrefabs[5]).Init(row, cMax, false, true);  // E
            tileGrid[row, 0   ] = Instantiate (TilePrefabs[7]).Init(row, 0   , false, true);  // W
        }

        // Central pieces
        for (int row = 2; row < rMax; row += 2) 
        {
            for (int col = 2; col < cMax; col += 2)
            {
                int randomTJunt = Random.Range(4, 8);
                tileGrid[row, col] = Instantiate (TilePrefabs[randomTJunt]).Init(row, col, false, false);
            }
        }
    }

    // Populate the remaining tile spaces with a random choice of tiles
    private void RandomiseMoveableTiles()
    {
        for (int row = 0; row < MapSizeVertical; row++)
        {
            for (int col = 0; col < MapSizeHorizontal; col++)
            {
                if (tileGrid[row, col] == null)
                {
                    int randomTile = Random.Range(0, TilePrefabs.Length);
                    if (row == 0)  // N
                    {
                        tileGrid[row, col] = Instantiate (TilePrefabs[randomTile]).Init(row, col, true, true);
                        tileGrid[row, col].mFloor.transform.Rotate(Vector3.up, 180);
                    }
                    else if (col == 0)  // W
                    {
                        tileGrid[row, col] = Instantiate (TilePrefabs[randomTile]).Init(row, col, true, true);
                        tileGrid[row, col].mFloor.transform.Rotate(Vector3.up, 90);
                    }
                    else if (row == MapSizeVertical - 1)  // S
                    {
                        tileGrid[row, col] = Instantiate (TilePrefabs[randomTile]).Init(row, col, true, true);
                    }
                    else if (col == MapSizeHorizontal - 1)  // E
                    {
                        tileGrid[row, col] = Instantiate (TilePrefabs[randomTile]).Init(row, col, true, true);
                        tileGrid[row, col].mFloor.transform.Rotate(Vector3.up, 270);
                    }
                    else
                    {
                        tileGrid[row, col] = Instantiate (TilePrefabs[randomTile]).Init(row, col, true, false);
                    }
                }
            }
        }
    }

    // Centre the tiles within the Map GameObject and scale to fit camera
    private void PositionMap()
    {
        foreach (Tile tile in tileGrid)
        {
            // Parent the tile into the Map GameObject
            tile.transform.parent = this.transform;

            // Shift the tile by half the map's height and width plus adjust for the tile axis being at it's centre
            float horizontalPos = (Tile.Size * -MapSizeHorizontal * 0.5f) + (Tile.Size * 0.5f);
            float verticalPos = (Tile.Size * MapSizeVertical * 0.5f) - (Tile.Size * 0.5f);
            tile.transform.Translate(new Vector3(horizontalPos, 0, verticalPos));

            // Setup initial exit relations
            tile.UpdateExitTiles();
        }

        // Scale the map TODO Avoid hardcoded value here
        transform.localScale = Vector3.one * 2;
    }

    // Randomise location of Agent spawn and Decontamination in oposite corners
    private void RandomiseGameLocations()
    {
        int randomCornerIndex = Random.Range(0, 4);
        Tile decontamTile;
        switch (randomCornerIndex)
        {
            case 0:
            {
                AgentSpawnTile = tileGrid[0, 0];
                decontamTile = tileGrid[MapSizeVertical - 1, MapSizeHorizontal - 1];
                break;
            }
            case 1:
            {
                AgentSpawnTile = tileGrid[0, MapSizeHorizontal - 1];
                decontamTile = tileGrid[MapSizeVertical - 1, 0];
                break;
            }
            case 2:
            {
                AgentSpawnTile = tileGrid[MapSizeVertical - 1, 0];
                decontamTile = tileGrid[0, MapSizeHorizontal - 1];
                break;
            }
            case 3:
            {
                AgentSpawnTile = tileGrid[MapSizeVertical - 1, MapSizeHorizontal - 1];
                decontamTile = tileGrid[0, 0];
                break;
            }
            default:  // Should never reach here but compiler complains if cureTile is unassigned
            {
                decontamTile = new Tile();
                break;
            }
        }

        AgentSpawnLocation = AgentSpawnTile.transform.position + (Vector3.up * 1.2f);
        AgentSpawnTile.SetAsSpawnLocation();
        decontamTile.SetAsDecontaminationLocation();
    }

    public void SlideTiles(GridIndex initiatingTileIndex)
    {
        Tile[] tiles;
        Tile.Direction direction;
        
        // Add in reverse order, references to the tiles to be moved. Also call for safety doors to raise.
        if (initiatingTileIndex.Row == 0)  // Top edge
        {
            tiles = new Tile[MapSizeVertical];
            for (int i = 0; i < MapSizeVertical; i++)
            {
                tiles[i] = tileGrid[(MapSizeVertical - 1) - i, initiatingTileIndex.Col];
            }
            direction = Tile.Direction.South;
        }
        else if (initiatingTileIndex.Row == MapSizeVertical - 1)  // Bottom edge
        {
            tiles = new Tile[MapSizeVertical];
            for (int i = 0; i < MapSizeVertical; i++)
            {
                tiles[i] = tileGrid[i, initiatingTileIndex.Col];
            }
            direction = Tile.Direction.North;
        }
        else if (initiatingTileIndex.Col == 0) // Left edge
        {
            tiles = new Tile[MapSizeHorizontal];
            for (int i = 0; i < MapSizeHorizontal; i++)
            {
                tiles[i] = tileGrid[initiatingTileIndex.Row, (MapSizeHorizontal - 1) - i];
            }
            direction = Tile.Direction.East;
        }
        else  // Right edge (Assumes this function will only be called on valid edge tiles)
        {
            tiles = new Tile[MapSizeHorizontal];
            for (int i = 0; i < MapSizeHorizontal; i++)
            {
                tiles[i] = tileGrid[initiatingTileIndex.Row, i];
            }
            direction = Tile.Direction.West;
        }

        // Lock facing door on tiles adjacent to the one being lifted and to the gap at the end of the row:
        // Tile North of lifted and South of gap
        if ( direction == Tile.Direction.South
          || direction == Tile.Direction.East
          || direction == Tile.Direction.West)
        {
            tileGrid[tiles[0].Index.Row - 1, tiles[0].Index.Col].TriggerSafetyDoor(Tile.Direction.South);
            if (direction != Tile.Direction.South)
            {
                tileGrid[tiles[tiles.Length - 1].Index.Row + 1, tiles[tiles.Length - 1].Index.Col].TriggerSafetyDoor(Tile.Direction.North);
            }
        }
        // Tile South of lifted and North of gap
        if ( direction == Tile.Direction.North
          || direction == Tile.Direction.East
          || direction == Tile.Direction.West)
        {
            tileGrid[tiles[0].Index.Row + 1, tiles[0].Index.Col].TriggerSafetyDoor(Tile.Direction.North);
            if (direction != Tile.Direction.North)
            {
                tileGrid[tiles[tiles.Length - 1].Index.Row - 1, tiles[tiles.Length - 1].Index.Col].TriggerSafetyDoor(Tile.Direction.South);
            }
        }
        // Tile West of lifted and East of gap
        if ( direction == Tile.Direction.East
          || direction == Tile.Direction.North
          || direction == Tile.Direction.South)
        {
            tileGrid[tiles[0].Index.Row, tiles[0].Index.Col - 1].TriggerSafetyDoor(Tile.Direction.West);
            if (direction != Tile.Direction.East)
            {
                tileGrid[tiles[tiles.Length - 1].Index.Row, tiles[tiles.Length - 1].Index.Col + 1].TriggerSafetyDoor(Tile.Direction.East);
            }
        }
        // Tile East of lifted and West of gap
        if ( direction == Tile.Direction.West
          || direction == Tile.Direction.North
          || direction == Tile.Direction.South)
        {
            tileGrid[tiles[0].Index.Row, tiles[0].Index.Col + 1].TriggerSafetyDoor(Tile.Direction.East);
            if (direction != Tile.Direction.West)
            {
                tileGrid[tiles[tiles.Length - 1].Index.Row, tiles[tiles.Length - 1].Index.Col - 1].TriggerSafetyDoor(Tile.Direction.West);
            }
        }

        // Lift up the tile to move over to the end
        tiles[0].Slide(direction, true);  // Updates the tile's row and col index...
        tileGrid[tiles[0].Index.Row, tiles[0].Index.Col] = tiles[0];  // ...so this will update accordingly

        // Slide along the other tiles
        for (int i = 1; i < tiles.Length; i++)
        {
            tiles[i].Slide(direction);
            tileGrid[tiles[i].Index.Row, tiles[i].Index.Col] = tiles[i];
        }

        // Update tile exit paths records
        foreach (Tile tile in tileGrid)
        {
            tile.UpdateExitTiles();
        }
    }

    // Return a reference to the tile which neighbours the home tile in the given direction
    public Tile TileNeighbour(Tile homeTile, Tile.Direction neighbourDirection)
    {
        switch (neighbourDirection)
        {
            case Tile.Direction.North:
            {
                if (homeTile.Index.Row != 0)
                {
                    return tileGrid[homeTile.Index.Row - 1, homeTile.Index.Col    ];
                }
                break;
            }
            case Tile.Direction.East:
            {
                if (homeTile.Index.Col != Map.Get.MapSizeHorizontal - 1)
                {
                    return tileGrid[homeTile.Index.Row    , homeTile.Index.Col + 1];
                }
                break;
            }
            case Tile.Direction.South:
            {
                if (homeTile.Index.Row != Map.Get.MapSizeVertical - 1)
                {
                    return tileGrid[homeTile.Index.Row + 1, homeTile.Index.Col    ];
                }
                break;
            }
            case Tile.Direction.West:
            {
                if (homeTile.Index.Col != 0)
                {
                    return tileGrid[homeTile.Index.Row    , homeTile.Index.Col - 1];
                }
                break;
            }
        }

        return null;  // Should never reach here but compiler requires complete return code path
    }
}
