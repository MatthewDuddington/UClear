﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GridIndex
{
    public int Row { get; private set; }
    public int Col { get; private set; }

    public void SetRow(int rowIndex) { Row = rowIndex; }
    public void SetCol(int colIndex) { Col = colIndex; }
}

public class Map : MonoBehaviour
{
    // Easy accessor for the class instance
    private static Map map_;
    public static Map Get
    { 
        get
        {
            if (map_ == null) { Debug.LogError("No Map present in scene"); }
            return map_;
        }
        private set { map_ = value; }
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
    private Tile[] TilePrefabs;

    // A grid of Z rows * X cols map tiles
    private Tile[,] tileGrid;

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

        tileGrid = new Tile[(int) MapSizeVertical, (int) MapSizeHorizontal];
    }

    void Start()
    {
        AddDefaultTiles();
        RandomiseMoveableTiles();
        PositionMap();
    }

    // Add the fixed corner, edge and central tiles
    void AddDefaultTiles()
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
    void RandomiseMoveableTiles()
    {
        for (int row = 0; row < MapSizeVertical; row++)
        {
            for (int col = 0; col < MapSizeHorizontal; col++)
            {
                if (tileGrid[row, col] == null)
                {
                    int randomTile = Random.Range(0, TilePrefabs.Length);
                    bool isEdgeTile = false;
                    if (row == 0 || col == 0 || row == MapSizeVertical - 1 || col == MapSizeHorizontal - 1) { isEdgeTile = true; print("true edge"); }
                    tileGrid[row, col] = Instantiate (TilePrefabs[randomTile]).Init(row, col, true, isEdgeTile);
                }
            }
        }
    }

    // Centre the tiles within the Map GameObject and scale to fit camera
    void PositionMap()
    {
        foreach (Tile tile in tileGrid)
        {
            // Parent the tile into the Map GameObject
            tile.transform.parent = this.transform;

            // Shift the tile by half the map's height and width plus adjust for the tile axis being at it's centre
            float horizontalPos = (Tile.Size * -MapSizeHorizontal * 0.5f) + (Tile.Size * 0.5f);
            float verticalPos = (Tile.Size * MapSizeVertical * 0.5f) - (Tile.Size * 0.5f);
            tile.transform.Translate(new Vector3(horizontalPos, verticalPos, 0));
        }

        // Scale the map TODO Avoid hardcoded value here
        transform.localScale = Vector3.one * 2;
    }

    public void SlideTiles(GridIndex initiatingTileIndex)
    {
        Tile[] tiles;
        Tile.Direction direction;

        // Add in reverse order, references to the tiles to be moved
        if (initiatingTileIndex.Row == 0)  // Top edge
        {
            tiles = new Tile[MapSizeVertical];
            for (int i = 0; i < MapSizeVertical; i++)
            {
                tiles[i] = tileGrid[0, (MapSizeVertical - 1) - i];
            }
            direction = Tile.Direction.Down;
        }
        else if (initiatingTileIndex.Row == MapSizeVertical - 1)  // Bottom edge
        {
            tiles = new Tile[MapSizeVertical];
            for (int i = 0; i < MapSizeVertical; i++)
            {
                tiles[i] = tileGrid[0, i];
            }
            direction = Tile.Direction.Up;
        }
        else if (initiatingTileIndex.Col == 0) // Left edge
        {
            tiles = new Tile[MapSizeHorizontal];
            for (int i = 0; i < MapSizeHorizontal; i++)
            {
                tiles[i] = tileGrid[(MapSizeHorizontal - 1) - i, 0];
            }
            direction = Tile.Direction.Right;
        }
        else  // Right edge (Assumes this function will only be called on valid edge tiles)
        {
            tiles = new Tile[MapSizeHorizontal];
            for (int i = 0; i < MapSizeHorizontal; i++)
            {
                tiles[i] = tileGrid[i, 0];
            }
            direction = Tile.Direction.Left;
        }

        tiles[0].Slide(Tile.Direction.Lift);  // Lift up the tile to move over to the end
        for (int i = 1; i < tiles.Length; i++)
        {
            tiles[i].Slide(direction);  // Updates the tile's row and col index...
            tileGrid[tiles[i].Index.Row, tiles[i].Index.Col] = tiles[i];  // ...so this will update accordingly.
        }
    }
}
