using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomStartPosition : MonoBehaviour
{
    // Not used in this game

	void Start()
    {
        float hw = Arena.Width * 0.5f;
        float hh = Arena.Height * 0.5f;
        float x = Random.Range( -hw, hw );
        float z = Random.Range(-hh, hh);
        transform.position = new Vector3(x, 0.5f, z);
    }
}
