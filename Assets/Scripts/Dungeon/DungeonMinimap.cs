using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMinimap : MonoBehaviour
{
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private PlayerMover playerMover;
    [SerializeField] private RawImage mapImage;

    [Header("Display")]
    [SerializeField] private int pixelsPerTile = 6;

    [Header("Colors")]
    [SerializeField]
    private Color wallColor =
        new Color(0.08f, 0.08f, 0.08f);

    [SerializeField]
    private Color floorColor =
        new Color(0.65f, 0.65f, 0.65f);

    [SerializeField]
    private Color treasureColor =
        new Color(1f, 0.75f, 0f);

    [SerializeField]
    private Color exitColor =
        new Color(0.1f, 0.8f, 0.2f);

    [SerializeField]
    private Color playerColor =
        new Color(0.9f, 0.1f, 0.1f);

    [SerializeField] private Color directionColor = Color.white;

    private Texture2D mapTexture;

    private Vector2Int lastPlayerPosition;
    private Vector2Int lastPlayerDirection;
    private int lastMapHash;

    private bool initialized;

    [SerializeField]
    private Color unexploredColor = Color.black;

    private bool[,] exploredTiles;
    private int lastGenerationVersion;

    private IEnumerator Start()
    {
        if (dungeonGenerator == null ||
            playerMover == null ||
            mapImage == null)
        {
            Debug.LogError(
                "DungeonMinimapの参照が設定されていません");
            yield break;
        }

        yield return new WaitUntil(
            () => dungeonGenerator.HasMap);

        CreateTexture();
        ResetExploration();
        RefreshMinimap();

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        // 新しいフロアが生成された
        if (dungeonGenerator.GenerationVersion !=
            lastGenerationVersion)
        {
            ResetExploration();
            RefreshMinimap();
            return;
        }

        Vector2Int playerPosition =
            dungeonGenerator.WorldToGrid(
                playerMover.transform.position);

        Vector2Int playerDirection =
            GetPlayerDirection();

        int mapHash = CalculateMapHash();

        if (playerPosition != lastPlayerPosition ||
            playerDirection != lastPlayerDirection ||
            mapHash != lastMapHash)
        {
            RefreshMinimap();
        }
    }


    private void ResetExploration()
    {
        exploredTiles = new bool[
            dungeonGenerator.MapHeight,
            dungeonGenerator.MapWidth];

        lastGenerationVersion =
            dungeonGenerator.GenerationVersion;
    }

    private void RevealAroundPlayer()
    {
        Vector2Int playerPosition =
            dungeonGenerator.WorldToGrid(
                playerMover.transform.position);

        for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
        {
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                int x = playerPosition.x + offsetX;
                int z = playerPosition.y + offsetZ;

                if (x < 0 || x >= dungeonGenerator.MapWidth ||
                    z < 0 || z >= dungeonGenerator.MapHeight)
                {
                    continue;
                }

                exploredTiles[z, x] = true;
            }
        }
    }


    private void CreateTexture()
    {
        int textureWidth =
            dungeonGenerator.MapWidth * pixelsPerTile;

        int textureHeight =
            dungeonGenerator.MapHeight * pixelsPerTile;

        mapTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            false);

        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;

        mapImage.texture = mapTexture;
    }

    public void RefreshMinimap()
    {
        RevealAroundPlayer();

        DrawDungeon();
        DrawPlayer();

        mapTexture.Apply();

        lastPlayerPosition =
            dungeonGenerator.WorldToGrid(
                playerMover.transform.position);

        lastPlayerDirection = GetPlayerDirection();
        lastMapHash = CalculateMapHash();
    }

    private void DrawDungeon()
    {
        for (int z = 0;
             z < dungeonGenerator.MapHeight;
             z++)
        {
            for (int x = 0;
                 x < dungeonGenerator.MapWidth;
                 x++)
            {
                if (!exploredTiles[z, x])
                {
                    FillTile(x, z, unexploredColor);
                    continue;
                }

                int tileType =
                    dungeonGenerator.GetTileType(x, z);

                Color color = GetTileColor(tileType);
                FillTile(x, z, color);
            }
        }
    }


    private Color GetTileColor(int tileType)
    {
        switch (tileType)
        {
            case 0:
                return floorColor;

            case 2:
                return treasureColor;

            case 3:
                return exitColor;

            default:
                return wallColor;
        }
    }

    private void FillTile(
        int tileX,
        int tileZ,
        Color color)
    {
        int startX = tileX * pixelsPerTile;
        int startY = tileZ * pixelsPerTile;

        for (int y = 0; y < pixelsPerTile; y++)
        {
            for (int x = 0; x < pixelsPerTile; x++)
            {
                mapTexture.SetPixel(
                    startX + x,
                    startY + y,
                    color);
            }
        }
    }

    private void DrawPlayer()
    {
        Vector2Int playerPosition =
            dungeonGenerator.WorldToGrid(
                playerMover.transform.position);

        Vector2Int direction = GetPlayerDirection();

        int startX =
            playerPosition.x * pixelsPerTile;

        int startY =
            playerPosition.y * pixelsPerTile;

        int centerX =
            startX + pixelsPerTile / 2;

        int centerY =
            startY + pixelsPerTile / 2;

        // プレイヤー本体
        int bodyRadius =
            Mathf.Max(1, pixelsPerTile / 3);

        for (int y = -bodyRadius;
             y <= bodyRadius;
             y++)
        {
            for (int x = -bodyRadius;
                 x <= bodyRadius;
                 x++)
            {
                SetPixelSafe(
                    centerX + x,
                    centerY + y,
                    playerColor);
            }
        }

        // 向きを示す先端
        int tipLength =
            Mathf.Max(2, pixelsPerTile / 2);

        for (int i = 1; i <= tipLength; i++)
        {
            SetPixelSafe(
                centerX + direction.x * i,
                centerY + direction.y * i,
                directionColor);
        }
    }

    private Vector2Int GetPlayerDirection()
    {
        Vector3 forward =
            playerMover.transform.forward;

        if (Mathf.Abs(forward.x) >
            Mathf.Abs(forward.z))
        {
            return new Vector2Int(
                forward.x >= 0f ? 1 : -1,
                0);
        }

        return new Vector2Int(
            0,
            forward.z >= 0f ? 1 : -1);
    }

    private int CalculateMapHash()
    {
        unchecked
        {
            int hash = 17;

            for (int z = 0;
                 z < dungeonGenerator.MapHeight;
                 z++)
            {
                for (int x = 0;
                     x < dungeonGenerator.MapWidth;
                     x++)
                {
                    hash = hash * 31 +
                        dungeonGenerator.GetTileType(x, z);
                }
            }

            return hash;
        }
    }

    private void SetPixelSafe(
        int x,
        int y,
        Color color)
    {
        if (x < 0 || x >= mapTexture.width ||
            y < 0 || y >= mapTexture.height)
        {
            return;
        }

        mapTexture.SetPixel(x, y, color);
    }

    private void OnDestroy()
    {
        if (mapTexture != null)
        {
            Destroy(mapTexture);
        }
    }
}