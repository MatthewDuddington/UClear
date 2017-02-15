using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{

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
    private Tile[] tilePrefabs_;

    [SerializeField]
    private int mapSizeHorizontal;
    [SerializeField]
    private int mapSizeVertical;

    // A grid of Z rows * X cols map tiles
    private Tile[,] tileGrid_;

    void Awake()
    {
        // Check for map size having non-odd proportions and adjust if needed
        if (((int) mapSizeHorizontal) % 2 == 0)
        {
            Debug.LogWarning("Map Size horizontal proportion is non-odd, adjusting to compensate");
            mapSizeHorizontal ++;
        }
        if (((int) mapSizeVertical) % 2 == 0)
        {
            Debug.LogWarning("Map Size vertical proportion is non-odd, adjusting to compensate");
            mapSizeVertical ++;
        }

        tileGrid_ = new Tile[(int) mapSizeVertical, (int) mapSizeHorizontal];
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
        int rightIndex = mapSizeHorizontal - 1;
        int bottomIndex = mapSizeVertical - 1;

        // Corners
        tileGrid_[0          , 0         ] = Instantiate (tilePrefabs_[0].Init(Tile.tileSize * 0         , -Tile.tileSize * 0          ,false));  // NW
        tileGrid_[0          , rightIndex] = Instantiate (tilePrefabs_[1].Init(Tile.tileSize * rightIndex, -Tile.tileSize * 0          ,false));  // NE
        tileGrid_[bottomIndex, 0         ] = Instantiate (tilePrefabs_[2].Init(Tile.tileSize * 0         , -Tile.tileSize * bottomIndex,false));  // SW
        tileGrid_[bottomIndex, rightIndex] = Instantiate (tilePrefabs_[3].Init(Tile.tileSize * rightIndex, -Tile.tileSize * bottomIndex,false));  // SE

        // Edges
        // North & South
        for (int col = 2; col < rightIndex; col += 2)
        {
            tileGrid_[0          , col] = Instantiate (tilePrefabs_[4].Init(Tile.tileSize * col, -Tile.tileSize * 0          ,false));  // N
            tileGrid_[bottomIndex, col] = Instantiate (tilePrefabs_[6].Init(Tile.tileSize * col, -Tile.tileSize * bottomIndex,false));  // S
        }
        // East & West
        for (int row = 2; row < bottomIndex; row += 2)
        {
            tileGrid_[row, rightIndex] = Instantiate (tilePrefabs_[5].Init(Tile.tileSize * rightIndex, -Tile.tileSize * row,false));  // E
            tileGrid_[row, 0         ] = Instantiate (tilePrefabs_[7].Init(Tile.tileSize * 0         , -Tile.tileSize * row,false));  // W
        }

        // Central pieces
        for (int row = 2; row < bottomIndex; row += 2) 
        {
            for (int col = 2; col < rightIndex; col += 2)
            {
                int randomTJunt = Random.Range(4, 8);
                tileGrid_[row, col] = Instantiate (tilePrefabs_[randomTJunt].Init(Tile.tileSize * col, -Tile.tileSize * row,false));
            }
        }
    }

    // Populate the remaining tile spaces with a random choice of tiles
    void RandomiseMoveableTiles()
    {
        for (int row = 0; row < mapSizeVertical; row++)
        {
            for (int col = 0; col < mapSizeHorizontal; col++)
            {
                if (tileGrid_[row, col] == null)
                {
                    int randomTile = Random.Range(0, tilePrefabs_.Length);
                    tileGrid_[row, col] = Instantiate (tilePrefabs_[randomTile].Init(Tile.tileSize * col, -Tile.tileSize * row,false));
                }
            }
        }
    }

    // Centre the tiles within the Map GameObject and scale to fit camera
    void PositionMap()
    {
        foreach (Tile tile in tileGrid_)
        {
            // Parent the tile into the Map GameObject
            tile.transform.parent = this.transform;

            // Shift the tile by half the map's height and width plus adjust for the tile axis being at it's centre
            float horizontalPos = (Tile.tileSize * -mapSizeHorizontal * 0.5f) + (Tile.tileSize * 0.5f);
            float verticalPos = (Tile.tileSize * mapSizeVertical * 0.5f) - (Tile.tileSize * 0.5f);
            tile.transform.Translate(new Vector3(horizontalPos, verticalPos, 0));
        }

        // Scale the map TODO Avoid hardcoded value here
        transform.localScale = Vector3.one * 2;
    }

}
