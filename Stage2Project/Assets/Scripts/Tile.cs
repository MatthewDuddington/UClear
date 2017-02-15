using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isMovable { get; private set; }

    private static int tileSize_ = 7;
    public static int tileSize { get { return tileSize_; } }

    [SerializeField]
    private GameObject[] tilePrefabs_;

    public Tile Init (float x, float z, bool movable)
    {
        transform.position = new Vector3 (x, 0, z);
        isMovable = movable;
        return this;
    }
}
