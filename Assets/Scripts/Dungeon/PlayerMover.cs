using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float moveDistance = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float encounterRate = 0.2f;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private FadeController fadeController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip footstepSE;

    [SerializeField]
    private bool disableEncounterForDebug;

    [SerializeField] private int demoClearFloor = 3;

    private bool isRotating = false;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation = Quaternion.identity;

    int stepsWithoutEncounter = 3;

    private bool isFloorTransitioning;

    private string[] dungeonMessages =
{
    "前に進んだ…",
    "冷たい風が吹いた…",
    "奥から物音が聞こえる…",
    "何かの気配がする…",
    "足元がぬかるんでいる…",
    "壁が湿っている…",
    "静寂が広がっている…"
};

    private void Start()
    {
        stepsWithoutEncounter = 3;

        var gm = GameManager.Instance;


        if (fadeController != null)
        {
            StartCoroutine(fadeController.FadeIn());
        }

        if (dungeonGenerator != null)
        {
            moveDistance = dungeonGenerator.TileSize;

            //if (gm != null && gm.hasDungeonPosition)
            //{
            //    transform.position = gm.dungeonPlayerPosition;
            //    transform.rotation = gm.dungeonPlayerRotation;
            //}
            if (gm != null && gm.hasDungeonPosition)
            {
                Vector2Int grid = gm.dungeonPlayerGridPos;

                Vector3 pos = dungeonGenerator.GetWorldPosition(grid.x, grid.y);
                pos.y = transform.position.y;

                transform.position = pos;
                transform.rotation = gm.dungeonPlayerRotation;
            }
            else
            {
                Vector2Int start = dungeonGenerator.StartPosition;
                transform.position = dungeonGenerator.GetWorldPosition(start.x, start.y);
                transform.rotation = Quaternion.identity;
               

            }
        }

        targetPosition = transform.position;
        targetRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (isMoving)
        {
            MoveToTarget();
            return;
        }

        HandleInput();

        CheckFrontObject();
    }
    private void HandleInput()
    {
        if (dungeonGenerator == null)
        {
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame ||
                 Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                TryMove(transform.forward);
                return;
            }


            if (Keyboard.current.sKey.wasPressedThisFrame ||
                Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                TryMove(-transform.forward);
                return;
            }

            if (Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                StartRotation(-90f);
                return;
            }

            if (Keyboard.current.dKey.wasPressedThisFrame ||
                Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                StartRotation(90f);
                return;
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                InteractWithFrontObject();
                return;
            }
        }

        if (Mouse.current != null)
        {
            bool isPointerOverUI =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();

            if (Mouse.current.leftButton.wasPressedThisFrame &&
                !isPointerOverUI)
            {
                TryMove(transform.forward);
                return;
            }

            float scrollY = Mouse.current.scroll.ReadValue().y;

            if (scrollY > 0f)
            {
                StartRotation(90f);
                return;
            }

            if (scrollY < 0f)
            {
                StartRotation(-90f);
                return;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                InteractWithFrontObject();
            }
        }
    }

    private void StartRotation(float angle)
    {
        targetRotation =
            transform.rotation * Quaternion.Euler(0f, angle, 0f);

        isRotating = true;
        isMoving = true;
    }

    private void InteractWithFrontObject()
    {
        Vector3 forwardDir = new Vector3(
            transform.forward.x,
            0f,
            transform.forward.z
        ).normalized;

        Vector3 checkPos =
            transform.position + forwardDir * moveDistance;

        Collider[] hits = Physics.OverlapSphere(checkPos, 0.4f);

        foreach (Collider hit in hits)
        {
            TreasureBox box = hit.GetComponentInParent<TreasureBox>();

            if (box != null)
            {
                box.Open();
                return;
            }
        }
    }


private TreasureBox GetTreasureBoxAt(Vector3 position)
{
    Collider[] hits = Physics.OverlapSphere(position, 0.6f);

    foreach (Collider hit in hits)
    {
        TreasureBox box =
            hit.GetComponentInParent<TreasureBox>();

        if (box != null)
        {
            return box;
        }
    }

    return null;
}

private void TryMove(Vector3 direction)
    {
        Vector3 nextWorldPos =
            transform.position + direction * moveDistance;

        Vector2Int nextGridPos = WorldToGrid(nextWorldPos);

        int tileType = dungeonGenerator.GetTileType(
            nextGridPos.x,
            nextGridPos.y);

        if (tileType == 2)
        {
            TreasureBox box = GetTreasureBoxAt(nextWorldPos);

            if (messageText != null)
            {
                if (box != null && box.isOpened)
                {
                    messageText.text = "空の宝箱がある……";
                }
                else
                {
                    messageText.text =
                        "宝箱を見つけた。右クリックかSpaceで開けられそうだ。";
                }
            }

            return;
        }

        if (dungeonGenerator.CanMoveTo(
            nextGridPos.x,
            nextGridPos.y))
        {
            Vector3 nextPos = dungeonGenerator.GetWorldPosition(
                nextGridPos.x,
                nextGridPos.y);

            nextPos.y = transform.position.y;
            targetPosition = nextPos;

            isRotating = false;
            isMoving = true;
        }
        else if (messageText != null)
        {
            messageText.text = "壁に阻まれている……";
        }
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return dungeonGenerator.WorldToGrid(worldPos);
    }

    private void MoveToTarget()
    {
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
                isMoving = false;

                if (audioSource != null && footstepSE != null)
                {
                    //audioSource.PlayOneShot(footstepSE);
                    StartCoroutine(StopSE(0.2f));
                }
            }
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            transform.position = targetPosition;
            isMoving = false;

            if (audioSource != null && footstepSE != null)
            {
                audioSource.PlayOneShot(footstepSE);
                StartCoroutine(StopSE(0.4f));
            }

            // 出口なら通常のメッセージ・エンカウント処理を行わない
            if (CheckDungeonExit())
            {
                return;
            }

            ShowRandomMessage();
            CheckEncounter();
        }
    }

    private void CheckEncounter()
    {

#if UNITY_EDITOR
        if (disableEncounterForDebug)
        {
            return;
        }
#endif

        if (stepsWithoutEncounter > 0)
        {
            stepsWithoutEncounter--;
            return;
        }

        if (Random.value < encounterRate)
        {
            StartCoroutine(EncounterRoutine());
        }
    }
    private bool CheckDungeonExit()
    {
        if (isFloorTransitioning)
        {
            return true;
        }

        Vector2Int gridPosition =
            dungeonGenerator.WorldToGrid(transform.position);

        int tileType = dungeonGenerator.GetTileType(
            gridPosition.x,
            gridPosition.y);

        if (tileType != 3)
        {
            return false;
        }

        isFloorTransitioning = true;

        GameManager gm = GameManager.Instance;

        // 体験版の最終階をクリア
        if (gm != null && gm.currentDungeonFloor >= demoClearFloor)
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadToBeContinued();
            }
            else
            {
                SceneManager.LoadScene("ToBeContinued");
            }

            return true;
        }

        // 次のフロアへ進む
        if (gm != null)
        {
            gm.currentDungeonFloor++;

            // 次のフロアでは新しいマップを生成する
            gm.currentDungeonMap = null;
            gm.hasDungeonMap = false;

            // 前フロアの位置を復元しない
            gm.hasDungeonPosition = false;
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadDungeon();
        }
        else
        {
            SceneManager.LoadScene("Dungeon");
        }

        return true;
    }

    void CheckFrontObject()
    {
        Vector3 forwardDir = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 checkPos = transform.position + forwardDir * moveDistance;

        Collider[] hits = Physics.OverlapSphere(checkPos, 0.4f);

        foreach (var hit in hits)
        {
            TreasureBox box = hit.GetComponentInParent<TreasureBox>();
            if (box != null && !box.isOpened)
            {
                if (messageText != null)
                {
                    messageText.text = "宝箱がある……";
                }

                return;
            }
        }
    }

    private IEnumerator EncounterRoutine()
    {
        Debug.Log("敵と遭遇！");

        var gm = GameManager.Instance;

        if (gm == null)
        {
            var go = new GameObject("GameManager");
            gm = go.AddComponent<GameManager>();
        }

        if (gm != null)
        {
            Vector2Int gridPos = dungeonGenerator.WorldToGrid(transform.position);

            gm.dungeonPlayerGridPos = gridPos;
            gm.dungeonPlayerRotation = transform.rotation;
            gm.hasDungeonPosition = true;
        }

        yield return fadeController.FadeOut();

        SceneManager.LoadScene("Battle");
    }

    private void ShowRandomMessage()
    {
        if (Random.value < 0.1f)
        {
            messageText.text = "強い敵の気配がする…";
            return;
        }

        int index = Random.Range(0, dungeonMessages.Length);
        messageText.text = dungeonMessages[index];
    }

    private IEnumerator StopSE(float time)
    {
        yield return new WaitForSeconds(time);
        audioSource.Stop();
    }
}