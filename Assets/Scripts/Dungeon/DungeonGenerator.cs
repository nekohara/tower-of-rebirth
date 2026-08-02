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

    [Header("Room Generation")]
    [SerializeField] private int minRoomCount = 4;
    [SerializeField] private int maxRoomCount = 7;
    [SerializeField] private int minRoomSize = 3;
    [SerializeField] private int maxRoomSize = 6;
    [SerializeField] private int roomPlacementAttempts = 200;

    private readonly List<RectInt> rooms = new List<RectInt>();

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

    private void GenerateRandomMap()
    {
        map = new int[height, width];
        rooms.Clear();

        // 全マスを壁で初期化
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                map[z, x] = 1;
            }
        }

        int targetRoomCount =
            Random.Range(minRoomCount, maxRoomCount + 1);

        int attempts = 0;

        while (rooms.Count < targetRoomCount &&
               attempts < roomPlacementAttempts)
        {
            attempts++;

            int roomWidth =
                Random.Range(minRoomSize, maxRoomSize + 1);

            int roomHeight =
                Random.Range(minRoomSize, maxRoomSize + 1);

            // 外周1マスを壁として残す
            int roomX = Random.Range(1, width - roomWidth);
            int roomZ = Random.Range(1, height - roomHeight);

            RectInt newRoom =
                new RectInt(roomX, roomZ, roomWidth, roomHeight);

            if (DoesRoomOverlap(newRoom))
            {
                continue;
            }

            CarveRoom(newRoom);

            if (rooms.Count > 0)
            {
                Vector2Int previousCenter =
                    GetRoomCenter(rooms[rooms.Count - 1]);

                Vector2Int newCenter =
                    GetRoomCenter(newRoom);

                ConnectRooms(previousCenter, newCenter);
            }

            rooms.Add(newRoom);
        }

        if (rooms.Count == 0)
        {
            Debug.LogError("部屋を生成できませんでした");
            return;
        }

        if (rooms.Count < minRoomCount)
        {
            Debug.LogWarning(
                $"部屋数が不足しています: {rooms.Count}部屋");
        }

        // 最初の部屋の中央を開始地点にする
        startPosition = GetRoomCenter(rooms[0]);
        map[startPosition.y, startPosition.x] = 0;

        PlaceExit();
        PlaceTreasureBoxes();

        Debug.Log(
            $"ダンジョンを生成しました: {rooms.Count}部屋");
    }

    private bool DoesRoomOverlap(RectInt newRoom)
    {
        foreach (RectInt room in rooms)
        {
            // 部屋同士の間に壁1マス分の余白を確保
            RectInt expandedRoom = new RectInt(
                room.xMin - 1,
                room.yMin - 1,
                room.width + 2,
                room.height + 2);

            if (expandedRoom.Overlaps(newRoom))
            {
                return true;
            }
        }

        return false;
    }

    private void CarveRoom(RectInt room)
    {
        for (int z = room.yMin; z < room.yMax; z++)
        {
            for (int x = room.xMin; x < room.xMax; x++)
            {
                map[z, x] = 0;
            }
        }
    }

    private Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(
            room.xMin + room.width / 2,
            room.yMin + room.height / 2);
    }

    private void ConnectRooms(
        Vector2Int first,
        Vector2Int second)
    {
        // 接続方向をランダムに変えて単調さを抑える
        if (Random.value < 0.5f)
        {
            CarveHorizontalCorridor(
                first.x,
                second.x,
                first.y);

            CarveVerticalCorridor(
                first.y,
                second.y,
                second.x);
        }
        else
        {
            CarveVerticalCorridor(
                first.y,
                second.y,
                first.x);

            CarveHorizontalCorridor(
                first.x,
                second.x,
                second.y);
        }
    }

    private void CarveHorizontalCorridor(
        int startX,
        int endX,
        int z)
    {
        int minX = Mathf.Min(startX, endX);
        int maxX = Mathf.Max(startX, endX);

        for (int x = minX; x <= maxX; x++)
        {
            map[z, x] = 0;
        }
    }

    private void CarveVerticalCorridor(
        int startZ,
        int endZ,
        int x)
    {
        int minZ = Mathf.Min(startZ, endZ);
        int maxZ = Mathf.Max(startZ, endZ);

        for (int z = minZ; z <= maxZ; z++)
        {
            map[z, x] = 0;
        }
    }

    private void PlaceTreasureBoxes()
    {
        int treasureCount = Random.Range(1, 3);
        int placed = 0;
        int attempts = 0;

        RectInt startRoom = rooms[0];

        while (placed < treasureCount && attempts < 1000)
        {
            attempts++;

            int chestX = Random.Range(1, width - 1);
            int chestZ = Random.Range(1, height - 1);

            Vector2Int chestPosition =
                new Vector2Int(chestX, chestZ);

            // 通路・部屋の床以外、出口、既存宝箱を除外
            if (map[chestZ, chestX] != 0)
            {
                continue;
            }

            // 開始部屋には配置しない
            if (startRoom.Contains(chestPosition))
            {
                continue;
            }

            map[chestZ, chestX] = 2;
            placed++;
        }

        if (placed < treasureCount)
        {
            Debug.LogWarning(
                $"宝箱を{treasureCount}個中{placed}個しか配置できませんでした");
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
                    Vector3 position = basePos + Vector3.up * 0.5f;
                    Quaternion rotation = GetExitRotation(x, z);

                    Instantiate(exitPrefab, position, rotation, dungeonRoot);
                }
            }
        }
    }

    private Quaternion GetExitRotation(int x, int z)
    {
        // 出口Prefabの正面がローカル+Z方向である前提

        if (IsWall(x, z + 1)) // 北側が壁
        {
            return Quaternion.Euler(0f, 0f, 0f);
        }

        if (IsWall(x, z - 1)) // 南側が壁
        {
            return Quaternion.Euler(0f, 180f, 0f);
        }

        if (IsWall(x + 1, z)) // 東側が壁
        {
            return Quaternion.Euler(0f, -90f, 0f);
        }

        if (IsWall(x - 1, z)) // 西側が壁
        {
            return Quaternion.Euler(0f, 90f, 0f);
        }

        return Quaternion.identity;
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
        int farthestDistance = -1;

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

            // BFSの探索とは分けて、出口候補だけを絞り込む
            bool isExitCandidate =
                IsInsideRoom(current.position) &&
                !rooms[0].Contains(current.position) &&
                CountAdjacentWalls(
                    current.position.x,
                    current.position.y) == 1;

            if (isExitCandidate &&
                current.distance > farthestDistance)
            {
                farthestDistance = current.distance;
                farthestPosition = current.position;
            }

            // 通路を含む、すべての床を探索する
            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current.position + direction;

                if (next.x < 0 || next.x >= mapWidth ||
                    next.y < 0 || next.y >= mapHeight)
                {
                    continue;
                }

                if (visited[next.y, next.x])
                {
                    continue;
                }

                if (map[next.y, next.x] != 0)
                {
                    continue;
                }

                visited[next.y, next.x] = true;
                queue.Enqueue((next, current.distance + 1));
            }
        }

        if (farthestDistance < 0)
        {
            Debug.LogError("出口を配置できる部屋内の壁際がありません");
            return;
        }

        map[farthestPosition.y, farthestPosition.x] = 3;

        Debug.Log(
            $"出口を配置しました: {farthestPosition}, 距離: {farthestDistance}");
    }


    private int CountAdjacentWalls(int x, int z)
    {
        int count = 0;

        if (IsWall(x, z + 1)) count++;
        if (IsWall(x, z - 1)) count++;
        if (IsWall(x + 1, z)) count++;
        if (IsWall(x - 1, z)) count++;

        return count;
    }


    private bool IsInsideRoom(Vector2Int position)
    {
        foreach (RectInt room in rooms)
        {
            if (room.Contains(position))
            {
                return true;
            }
        }

        return false;
    }


}