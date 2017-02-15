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
    private Vector2 mapSize;

    // A grid of Z rows * X cols map tiles
    private Tile[,] tileGrid_;

    void Awake()
    {
        tileGrid_ = new Tile[(int) mapSize.x, (int) mapSize.y];
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
        // Corners
        tileGrid_[0, 0] = Instantiate (tilePrefabs_[0].Init(Tile.tileSize * 0, -Tile.tileSize * 0,false));  // NW
        tileGrid_[0, 6] = Instantiate (tilePrefabs_[1].Init(Tile.tileSize * 6, -Tile.tileSize * 0,false));  // NE
        tileGrid_[6, 0] = Instantiate (tilePrefabs_[2].Init(Tile.tileSize * 0, -Tile.tileSize * 6,false));  // SW
        tileGrid_[6, 6] = Instantiate (tilePrefabs_[3].Init(Tile.tileSize * 6, -Tile.tileSize * 6,false));  // SE

        // Edges
        // N
        tileGrid_[0, 2] = Instantiate (tilePrefabs_[4].Init(Tile.tileSize * 2, -Tile.tileSize * 0,false));
        tileGrid_[0, 4] = Instantiate (tilePrefabs_[4].Init(Tile.tileSize * 4, -Tile.tileSize * 0,false));
        // E
        tileGrid_[2, 6] = Instantiate (tilePrefabs_[5].Init(Tile.tileSize * 6, -Tile.tileSize * 2,false));
        tileGrid_[4, 6] = Instantiate (tilePrefabs_[5].Init(Tile.tileSize * 6, -Tile.tileSize * 4,false));
        // S
        tileGrid_[6, 2] = Instantiate (tilePrefabs_[6].Init(Tile.tileSize * 2, -Tile.tileSize * 6,false));
        tileGrid_[6, 4] = Instantiate (tilePrefabs_[6].Init(Tile.tileSize * 4, -Tile.tileSize * 6,false));
        // W
        tileGrid_[2, 0] = Instantiate (tilePrefabs_[7].Init(Tile.tileSize * 0, -Tile.tileSize * 2,false));
        tileGrid_[4, 0] = Instantiate (tilePrefabs_[7].Init(Tile.tileSize * 0, -Tile.tileSize * 4,false));

        // Central
        tileGrid_[2, 2] = Instantiate (tilePrefabs_[4].Init(Tile.tileSize * 2, -Tile.tileSize * 2,false));  // NW
        tileGrid_[2, 4] = Instantiate (tilePrefabs_[5].Init(Tile.tileSize * 4, -Tile.tileSize * 2,false));  // NE
        tileGrid_[4, 2] = Instantiate (tilePrefabs_[6].Init(Tile.tileSize * 2, -Tile.tileSize * 4,false));  // SW
        tileGrid_[4, 4] = Instantiate (tilePrefabs_[7].Init(Tile.tileSize * 4, -Tile.tileSize * 4,false));  // SE
    }

    void RandomiseMoveableTiles()
    {
        for (int row = 0; row < mapSize.y; row++)
        {
            for (int col = 0; col < mapSize.x; col++)
            {
                if (tileGrid_[row, col] == null)
                {
                    int randomTile = Random.Range(0, tilePrefabs_.Length);
                    tileGrid_[row, col] = Instantiate (tilePrefabs_[randomTile].Init(Tile.tileSize * col, -Tile.tileSize * row,false));
                }
            }
        }
    }

    void PositionMap()
    {
        foreach (Tile tile in tileGrid_)
        {
            tile.transform.parent = this.transform;
            float horizontalPos = Tile.tileSize * -mapSize.x * 0.5f + mapSize.x * 0.5f;
            float verticalPos = Tile.tileSize * mapSize.y * 0.5f - mapSize.y * 0.5f;
            tile.transform.Translate(new Vector3(horizontalPos, verticalPos, 0));
        }

        transform.localScale = Vector3.one * 2;
    }

}
