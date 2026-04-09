using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float moveDistance = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float encounterRate = 0.2f;
    [SerializeField] private TMP_Text messageText;

    private bool isMoving = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation = Quaternion.identity;



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
        if (Keyboard.current == null) return;

        // 前進
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if (!IsWall(transform.forward))
            {
                targetPosition = transform.position + transform.forward * moveDistance;
                targetRotation = Quaternion.identity;
                isMoving = true;


            }
        }
        // 後退
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (!IsWall(-transform.forward))
            {
                targetPosition = transform.position - transform.forward * moveDistance;
                targetRotation = Quaternion.identity;
                isMoving = true;
            }
        }
        // 左回転
        else if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            targetRotation = transform.rotation * Quaternion.Euler(0, -90, 0);
            isMoving = true;
        }
        // 右回転
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            targetRotation = transform.rotation * Quaternion.Euler(0, 90, 0);
            isMoving = true;
        }

    }

    private void MoveToTarget()
    {
        if (targetRotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                targetRotation = Quaternion.identity;
                isMoving = false;
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

            ShowRandomMessage();
            CheckEncounter();
        }
    }

    private bool IsWall(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, moveDistance))
        {
            Debug.Log("壁あり");
            return true;
        }

        return false;
    }

    private void CheckEncounter()
    {
        if (Random.value < encounterRate)
        {
            Debug.Log("敵と遭遇！");
            SceneManager.LoadScene("Battle");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.dungeonPlayerPosition = transform.position;
                GameManager.Instance.dungeonPlayerRotation = transform.rotation;
            }
        }
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
}