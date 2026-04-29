using UnityEngine;
using static TreasureBox;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform dungeonRoot;
    [SerializeField] private float tileSize = 2f;
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private Vector2Int startPosition = new Vector2Int(1, 1);
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 15;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private int treasureCount = 5;

    public Vector2Int StartPosition => startPosition;

    private int[,] map;

    public float TileSize => tileSize;

    private void Start()
    {
        if (dungeonRoot == null)
        {
            Debug.LogError("dungeonRoot が設定されていません");
            return;
        }

        Debug.Log($"DungeonRoot Scale: {dungeonRoot.localScale}");

        var gm = GameManager.Instance;

        if (gm != null && gm.hasDungeonMap && gm.currentDungeonMap != null)
        {
            map = gm.currentDungeonMap;
        }
        else
        {
            GenerateRandomMap();

            if (gm != null)
            {
                gm.currentDungeonMap = map;
                gm.hasDungeonMap = true;
            }
        }

        BuildDungeon();
    }

    //private void GenerateDungeon()
    //{
    //    ClearDungeon();

    //    map = new int[height, width];

    //    // 全部壁で初期化
    //    for (int z = 0; z < height; z++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            map[z, x] = 1;
    //        }
    //    }

    //    // ランダムウォーク開始
    //    int xPos = width / 2;
    //    int zPos = height / 2;

    //    for (int i = 0; i < 200; i++)
    //    {
    //        map[zPos, xPos] = 0;

    //        int dir = Random.Range(0, 4);

    //        switch (dir)
    //        {
    //            case 0: xPos++; break;
    //            case 1: xPos--; break;
    //            case 2: zPos++; break;
    //            case 3: zPos--; break;
    //        }

    //        // 範囲制限
    //        xPos = Mathf.Clamp(xPos, 1, width - 2);
    //        zPos = Mathf.Clamp(zPos, 1, height - 2);
    //    }


    //    map[startPosition.y, startPosition.x] = 0;


    //    // 生成処理（今のままでOK）
    //    for (int z = 0; z < height; z++)
    //    {
    //        for (int x = 0; x < width; x++)
    //        {
    //            Vector3 basePos = new Vector3(x * tileSize, 0, z * tileSize);

    //            if (map[z, x] == 0)
    //            {
    //                Instantiate(floorPrefab, basePos, Quaternion.identity, dungeonRoot);

    //                CreateWallIfNeeded(x, z, 0, -1, basePos, Vector3.back);
    //                CreateWallIfNeeded(x, z, 0, 1, basePos, Vector3.forward);
    //                CreateWallIfNeeded(x, z, -1, 0, basePos, Vector3.left);
    //                CreateWallIfNeeded(x, z, 1, 0, basePos, Vector3.right);
    //            }
    //        }
    //    }
    //}

    private void GenerateRandomMap()
    {
        map = new int[height, width];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                map[z, x] = 1;
            }
        }

        int xPos = startPosition.x;
        int zPos = startPosition.y;

        for (int i = 0; i < 100; i++)
        {
            map[zPos, xPos] = 0;

            int dir = Random.Range(0, 4);

            switch (dir)
            {
                case 0: xPos++; break;
                case 1: xPos--; break;
                case 2: zPos++; break;
                case 3: zPos--; break;
            }

            xPos = Mathf.Clamp(xPos, 1, width - 2);
            zPos = Mathf.Clamp(zPos, 1, height - 2);
        }

        map[startPosition.y, startPosition.x] = 0;
        map[startPosition.y, startPosition.x + 1] = 0;

        int placed = 0;
        int safety = 0;

        while (placed < treasureCount && safety < 1000)
        {
            safety++;

            int chestX = Random.Range(1, width - 1);
            int chestZ = Random.Range(1, height - 1);

            if (map[chestZ, chestX] != 0)
                continue;

            if (chestX == startPosition.x && chestZ == startPosition.y)
                continue;

            map[chestZ, chestX] = 2;
            placed++;
        }
    }

    private void BuildDungeon()
    {
        ClearDungeon();

        int width = map.GetLength(1);
        int height = map.GetLength(0);

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 basePos = new Vector3(x * tileSize, 0, z * tileSize);

                if (map[z, x] == 0 || map[z, x] == 2)
                {
                    Instantiate(floorPrefab, basePos, Quaternion.identity, dungeonRoot);

                    CreateWallIfNeeded(x, z, 0, -1, basePos, Vector3.back);
                    CreateWallIfNeeded(x, z, 0, 1, basePos, Vector3.forward);
                    CreateWallIfNeeded(x, z, -1, 0, basePos, Vector3.left);
                    CreateWallIfNeeded(x, z, 1, 0, basePos, Vector3.right);

                    if (map[z, x] == 2)
                    {
                        GameObject chest = Instantiate(chestPrefab, basePos + Vector3.up * 0.5f, Quaternion.identity, dungeonRoot);

                        TreasureBox box = chest.GetComponent<TreasureBox>();

                        int rand = Random.Range(0, 100);

                        if (rand < 60)
                        {
                            box.SetTreasureType(TreasureType.Potion);
                        }
                        else if (rand < 90)
                        {
                            box.SetTreasureType(TreasureType.Gold);
                        }
                        else
                        {
                            box.SetTreasureType(TreasureType.Trap);
                        }
                    }
                }
            }
        }
    }

    private void CreateWallIfNeeded(int x, int z, int offsetX, int offsetZ, Vector3 basePos, Vector3 dir)
    {
        int checkX = x + offsetX;
        int checkZ = z + offsetZ;

        if (IsWall(checkX, checkZ))
        {
            Vector3 wallPos = basePos + dir * (tileSize * 0.5f);
            wallPos.y = wallHeight * 0.5f;

            Quaternion rot = Quaternion.identity;

            if (dir == Vector3.forward || dir == Vector3.back)
            {
                rot = Quaternion.identity;
            }
            else
            {
                rot = Quaternion.Euler(0, 90, 0);
            }

            Instantiate(wallPrefab, wallPos, rot, dungeonRoot);
        }
    }

    private bool IsWall(int x, int z)
    {
        int width = map.GetLength(1);
        int height = map.GetLength(0);

        if (x < 0 || x >= width || z < 0 || z >= height)
        {
            return true;
        }

        return map[z, x] == 1;
    }

    private void ClearDungeon()
    {
        if (dungeonRoot == null) return;

        for (int i = dungeonRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(dungeonRoot.GetChild(i).gameObject);
        }
    }

    public bool CanMoveTo(int x, int z)
    {
        int width = map.GetLength(1);
        int height = map.GetLength(0);

        if (x < 0 || x >= width || z < 0 || z >= height)
        {
            return false;
        }

        return map[z, x] == 0 || map[z, x] == 2;
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x * tileSize, 0f, z * tileSize);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x + tileSize * 0.5f) / tileSize);
        int z = Mathf.FloorToInt((worldPos.z + tileSize * 0.5f) / tileSize);

        return new Vector2Int(x, z);
    }

    public int GetTileType(int x, int z)
    {
        return map[z, x];
    }

    public void SetTileType(int x, int z, int value)
    {
        map[z, x] = value;
    }
}