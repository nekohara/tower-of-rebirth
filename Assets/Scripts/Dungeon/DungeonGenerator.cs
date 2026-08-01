using UnityEngine;
using System.Collections.Generic;
using static TreasureBox;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject exitPrefab;
    [SerializeField] private Transform dungeonRoot;
    [SerializeField] private float tileSize = 2f;
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private Vector2Int startPosition = new Vector2Int(1, 1);
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 15;

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

        // 出口を配置してから宝箱を配置する
        PlaceExit();

        int treasureCount = Random.Range(1, 3);
        int placed = 0;
        int safety = 0;

        while (placed < treasureCount && safety < 1000)
        {
            safety++;

            int chestX = Random.Range(1, width - 1);
            int chestZ = Random.Range(1, height - 1);

            // 通路以外には配置しない
            // これにより、開始地点・出口・ほかの宝箱も除外される
            if (map[chestZ, chestX] != 0)
                continue;

            if (chestX == startPosition.x &&
                chestZ == startPosition.y)
            {
                continue;
            }

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
                int tileType = map[z, x];

                if (tileType != 0 &&
                    tileType != 2 &&
                    tileType != 3)
                {
                    continue;
                }

                Vector3 basePos =
                    new Vector3(x * tileSize, 0, z * tileSize);

                Instantiate(
                    floorPrefab,
                    basePos,
                    Quaternion.identity,
                    dungeonRoot);

                CreateWallIfNeeded(x, z, 0, -1, basePos, Vector3.back);
                CreateWallIfNeeded(x, z, 0, 1, basePos, Vector3.forward);
                CreateWallIfNeeded(x, z, -1, 0, basePos, Vector3.left);
                CreateWallIfNeeded(x, z, 1, 0, basePos, Vector3.right);

                if (tileType == 2)
                {
                    CreateTreasureBox(basePos);
                }
                else if (tileType == 3)
                {
                    Instantiate(
                        exitPrefab,
                        basePos + Vector3.up * 0.5f,
                        Quaternion.identity,
                        dungeonRoot);
                }
            }
        }
    }

    private void CreateTreasureBox(Vector3 basePos)
    {
        GameObject chest = Instantiate(
            chestPrefab,
            basePos,
            Quaternion.identity,
            dungeonRoot);

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
                rot = Quaternion.Euler(0,0,0);
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

        return map[z, x] == 0 || map[z, x] == 2 || map[z, x] == 3;
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x * tileSize, 0.4f, z * tileSize);
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

    private void PlaceExit()
    {
        int mapWidth = map.GetLength(1);
        int mapHeight = map.GetLength(0);

        var visited = new bool[mapHeight, mapWidth];
        var queue = new Queue<(Vector2Int position, int distance)>();

        queue.Enqueue((startPosition, 0));
        visited[startPosition.y, startPosition.x] = true;

        Vector2Int farthestPosition = startPosition;
        int farthestDistance = 0;

        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.distance > farthestDistance)
            {
                farthestDistance = current.distance;
                farthestPosition = current.position;
            }

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current.position + direction;

                if (next.x < 0 || next.x >= mapWidth ||
                    next.y < 0 || next.y >= mapHeight)
                {
                    continue;
                }

                if (visited[next.y, next.x])
                    continue;

                if (map[next.y, next.x] != 0)
                    continue;

                visited[next.y, next.x] = true;
                queue.Enqueue((next, current.distance + 1));
            }
        }

        if (farthestPosition == startPosition)
        {
            Debug.LogError("出口を配置できる通路がありません");
            return;
        }

        map[farthestPosition.y, farthestPosition.x] = 3;

        Debug.Log(
            $"出口を配置しました: {farthestPosition}, 距離: {farthestDistance}");
    }
}