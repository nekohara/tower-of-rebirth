using System.Collections;
using UnityEngine;
using TMPro;

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

    public IEnumerator PlayEnemyDamage(
        GameObject enemyObject,
        int damage)
    {
        if (enemyObject == null)
            yield break;

        Coroutine damageTextCoroutine =  StartCoroutine(PlayDamageText(damage));

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

    private IEnumerator PlayDamageText(int damage)
    {
        if (damageTextPrefab == null ||
            damageTextParent == null)
        {
            yield break;
        }

        TMP_Text damageText = Instantiate(
            damageTextPrefab,
            damageTextParent
        );

        damageText.text = damage.ToString();

        RectTransform rectTransform =
            damageText.rectTransform;

        Vector2 startPosition =
            rectTransform.anchoredPosition;

        Color startColor = damageText.color;
        float elapsedTime = 0f;

        while (elapsedTime < damageTextDuration)
        {
            float rate =
                elapsedTime / damageTextDuration;

            rectTransform.anchoredPosition =
                startPosition +
                Vector2.up * damageTextMoveDistance * rate;

            Color currentColor = startColor;
            currentColor.a = 1f - rate;
            damageText.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(damageText.gameObject);
    }
}