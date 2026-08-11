using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleEffectController : MonoBehaviour
{
    [Header("敵の被ダメージ演出")]
    [SerializeField]
    private float shakeDuration = 0.3f;

    [SerializeField]
    private float shakeStrength = 0.12f;

    [SerializeField]
    private Color damageColor = Color.red;

    [Header("ダメージ数値")]
    [SerializeField]
    private TMP_Text damageTextPrefab;

    [SerializeField]
    private RectTransform damageTextParent;

    [SerializeField]
    private float damageTextDuration = 0.7f;

    [SerializeField]
    private float damageTextMoveDistance = 50f;

    [Header("回復数値")]
    [SerializeField]
    private RectTransform healTextParent;

    [SerializeField]
    private Color healTextColor = Color.green;

    [Header("プレイヤー被ダメージ")]
    [SerializeField]
    private RectTransform playerDamageTextParent;

    [SerializeField]
    private Color playerDamageTextColor = Color.red;

    [Header("プレイヤー被ダメージ画面")]
    [SerializeField]
    private Image playerDamageFlash;

    [SerializeField]
    private Color playerDamageFlashColor =
        new Color(1f, 0f, 0f, 0.35f);

    [SerializeField]
    private float playerDamageFlashDuration = 0.25f;

    [Header("敵撃破演出")]
    [SerializeField]
    private float enemyDefeatDuration = 0.5f;

    [SerializeField]
    private float enemyDefeatMoveDistance = 0.5f;

    public IEnumerator PlayEnemyDamage(
        GameObject enemyObject,
        int damage)
    {
        if (enemyObject == null)
            yield break;

        Color damageTextColor =
            damageTextPrefab != null
                ? damageTextPrefab.color
                : Color.white;

        Coroutine damageTextCoroutine = StartCoroutine(
                                        PlayFloatingText(
                                            damage,
                                            damageTextParent,
                                            damageTextColor
                                        )
                                    );

        Transform enemyTransform = enemyObject.transform;
        Vector3 originalPosition = enemyTransform.localPosition;

        Renderer[] renderers =
            enemyObject.GetComponentsInChildren<Renderer>();

        Color[] originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i].material;

            if (material.HasProperty("_Color"))
            {
                originalColors[i] = material.color;
                material.color = damageColor;
            }
        }

        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float offsetX =
                Random.Range(-shakeStrength, shakeStrength);

            enemyTransform.localPosition =
                originalPosition + new Vector3(offsetX, 0f, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemyTransform.localPosition = originalPosition;

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = renderers[i].material;

            if (material.HasProperty("_Color"))
            {
                material.color = originalColors[i];
            }
        }

        if (damageTextCoroutine != null)
        {
            yield return damageTextCoroutine;
        }
    }

    public IEnumerator PlayPlayerDamage(int damage)
    {
        Coroutine textCoroutine = StartCoroutine(
            PlayFloatingText(
                damage,
                playerDamageTextParent,
                playerDamageTextColor
            )
        );

        if (playerDamageFlash != null)
        {
            Color startColor = playerDamageFlashColor;
            playerDamageFlash.color = startColor;

            float elapsedTime = 0f;

            while (elapsedTime < playerDamageFlashDuration)
            {
                float rate =
                    elapsedTime / playerDamageFlashDuration;

                Color currentColor = startColor;
                currentColor.a =
                    startColor.a * (1f - rate);

                playerDamageFlash.color = currentColor;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Color endColor = startColor;
            endColor.a = 0f;
            playerDamageFlash.color = endColor;
        }

        if (textCoroutine != null)
        {
            yield return textCoroutine;
        }
    }

    public IEnumerator PlayPlayerHeal(int healAmount)
    {
        yield return PlayFloatingText(
            healAmount,
            healTextParent,
            healTextColor,
            "+"
        );
    }

    private IEnumerator PlayFloatingText(
        int amount,
        RectTransform parent,
        Color textColor,
        string prefix = "")
    {
        if (damageTextPrefab == null ||
            parent == null)
        {
            yield break;
        }

        TMP_Text floatingText = Instantiate(
            damageTextPrefab,
            parent,
            false
        );

        floatingText.text = prefix + amount;
        floatingText.color = textColor;

        RectTransform rectTransform =
            floatingText.rectTransform;

        Vector2 startPosition =
            rectTransform.anchoredPosition;

        float elapsedTime = 0f;

        while (elapsedTime < damageTextDuration)
        {
            float rate =
                elapsedTime / damageTextDuration;

            rectTransform.anchoredPosition =
                startPosition +
                Vector2.up * damageTextMoveDistance * rate;

            Color currentColor = textColor;
            currentColor.a = 1f - rate;
            floatingText.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(floatingText.gameObject);
    }

    public IEnumerator PlayEnemyDefeat(GameObject enemyObject)
    {
        if (enemyObject == null)
            yield break;

        Transform enemyTransform = enemyObject.transform;

        Vector3 startPosition = enemyTransform.localPosition;
        Vector3 startScale = enemyTransform.localScale;

        float elapsedTime = 0f;

        while (elapsedTime < enemyDefeatDuration)
        {
            float rate = elapsedTime / enemyDefeatDuration;

            enemyTransform.localPosition =
                startPosition +
                Vector3.down * enemyDefeatMoveDistance * rate;

            enemyTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    Vector3.zero,
                    rate
                );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemyTransform.localScale = Vector3.zero;
        enemyObject.SetActive(false);
    }
}