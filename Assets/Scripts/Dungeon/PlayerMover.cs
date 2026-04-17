using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    private bool isRotating = false;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation = Quaternion.identity;

    int stepsWithoutEncounter = 3;

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
    }
    private void HandleInput()
    {
        if (Keyboard.current == null || dungeonGenerator == null) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            TryMove(transform.forward);
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            TryMove(-transform.forward);
        }
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            targetRotation = transform.rotation * Quaternion.Euler(0, -90, 0);
            isRotating = true;
            isMoving = true;
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            targetRotation = transform.rotation * Quaternion.Euler(0, 90, 0);
            isRotating = true;
            isMoving = true;
        }
    }

    private void TryMove(Vector3 direction)
    {
        Vector3 nextWorldPos = transform.position + direction * moveDistance;

        Vector2Int nextGridPos = WorldToGrid(nextWorldPos);

        if (dungeonGenerator.CanMoveTo(nextGridPos.x, nextGridPos.y))
        {
            Vector3 nextPos = dungeonGenerator.GetWorldPosition(nextGridPos.x, nextGridPos.y);
            nextPos.y = transform.position.y;
            targetPosition = nextPos;

            isRotating = false;
            isMoving = true;
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "壁に阻まれている…";
            }
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
                    audioSource.PlayOneShot(footstepSE);
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

            // ★追加
            if (audioSource != null && footstepSE != null)
            {
                audioSource.PlayOneShot(footstepSE);
                StartCoroutine(StopSE(0.4f));
            }

            ShowRandomMessage();
            CheckEncounter();


            Vector2Int grid = WorldToGrid(transform.position);

            if (dungeonGenerator.GetTileType(grid.x, grid.y) == 2)
            {
                messageText.text = "宝箱を見つけた！";
                GameManager.Instance.potionCount += 1;
                dungeonGenerator.SetTileType(grid.x, grid.y, 0);

                Collider[] hits = Physics.OverlapSphere(transform.position, 0.6f);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Chest"))
                    {
                        Destroy(hit.gameObject);
                        break;
                    }
                }
            }
        }

        
    }

    private void CheckEncounter()
    {
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