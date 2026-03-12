using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public RectTransform hexPrefab;
    public int hexGridRadius;
    private float hexNodeSize;

    Vector2Int[] directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1)
    };

    void Start()
    {
        hexNodeSize = hexPrefab.rect.height / 2f;
        GenerateGrid();
    }

    void GenerateGrid()
    {
        CreateHex(0, 0);

        for (int layer = 1; layer <= hexGridRadius; layer++)
        {
            GenerateRing(layer);
        }
    }

    void GenerateRing(int layer)
    {
        Vector2Int hex = directions[4] * layer;

        for (int side = 0; side < 6; side++)
        {
            for (int step = 0; step < layer; step++)
            {
                CreateHex(hex.x, hex.y);
                hex += directions[side];
            }
        }
    }

    void CreateHex(int q, int r)
    {
        Vector2 pos = HexToPixel(q, r);
        RectTransform hex = Instantiate(hexPrefab, transform);
        hex.anchoredPosition = pos;
    }

    Vector2 HexToPixel(int q, int r)
    {
        float width = Mathf.Sqrt(3) * hexNodeSize;
        float height = 2f * hexNodeSize;
        float x = width * (q + r * 0.5f);
        float y = height * 0.75f * r;
        return new Vector2(x, y);
    }
}
