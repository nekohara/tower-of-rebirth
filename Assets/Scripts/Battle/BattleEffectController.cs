using System.Collections;
using UnityEngine;

public class BattleEffectController : MonoBehaviour
{
    [Header("敵の被ダメージ演出")]
    [SerializeField]
    private float shakeDuration = 0.3f;

    [SerializeField]
    private float shakeStrength = 0.12f;

    [SerializeField]
    private Color damageColor = Color.red;

    public IEnumerator PlayEnemyDamage(
        GameObject enemyObject,
        int damage)
    {
        if (enemyObject == null)
            yield break;

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
    }
}