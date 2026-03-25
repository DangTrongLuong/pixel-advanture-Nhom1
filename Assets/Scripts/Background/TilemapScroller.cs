using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapScroller : MonoBehaviour
{
    public float scrollSpeed = 2f;

    private float tileHeight;
    private Transform[] tiles;
    private float[] startYPositions;

    void Start()
    {
        tiles = new Transform[transform.childCount];
        startYPositions = new float[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            tiles[i] = transform.GetChild(i);
            startYPositions[i] = tiles[i].position.y;
        }

        var tilemap = tiles[0].GetComponent<Tilemap>();
        tilemap.CompressBounds();
        tileHeight = tilemap.localBounds.size.y * tiles[0].localScale.y;

    }

    void Update()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].position += Vector3.down * scrollSpeed * Time.deltaTime;
            if (tiles[i].position.y <= startYPositions[i] - tileHeight)
            {
                tiles[i].position = new Vector3(
                    tiles[i].position.x,
                    startYPositions[i],
                    tiles[i].position.z
                );
            }
        }
    }
}