
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShockwaveSkill : MonoBehaviour
{
    [Header("衝撃波の設定")]
    public float radius = 4f;
    public int damage = 40;
    public float cooldown = 5f;
    public float effectDuration = 0.35f;

    private float nextUseTime = 0f;

    public float CooldownRemaining =>
    Mathf.Max(0f, nextUseTime - Time.time);

    public float CooldownProgress =>
        cooldown <= 0f ? 1f :
        Mathf.Clamp01(1f - CooldownRemaining / cooldown);

    public bool IsReady => CooldownRemaining <= 0f;

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame &&
            Time.time >= nextUseTime)
        {
            UseShockwave();
        }
    }

    void UseShockwave()
    {
        nextUseTime = Time.time + cooldown;

        // 衝撃波の範囲内にあるコライダーを取得
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, radius);

        // 同じ敵に複数回ダメージを与えない
        HashSet<GameObject> damagedObjects = new HashSet<GameObject>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.transform == transform)
                continue;

            EnemyController enemy =
                hit.GetComponentInParent<EnemyController>();

            if (enemy != null)
            {
                if (damagedObjects.Add(enemy.gameObject))
                    enemy.TakeDamage(damage);

                continue;
            }

            BossController boss =
                hit.GetComponentInParent<BossController>();

            if (boss != null)
            {
                if (damagedObjects.Add(boss.gameObject))
                    boss.TakeDamage(damage);
            }
        }

        // 円形の衝撃波エフェクトを表示
        StartCoroutine(ShowShockwave());

        Debug.Log("衝撃波発動！");
    }

    IEnumerator ShowShockwave()
    {
        GameObject effect = new GameObject("ShockwaveEffect");

        LineRenderer line = effect.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = 65;
        line.startWidth = 0.10f;
        line.endWidth = 0.10f;
        line.startColor = Color.cyan;
        line.endColor = Color.white;
        line.sortingOrder = 30;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
            line.material = new Material(shader);

        Vector3 center = transform.position;
        float elapsed = 0f;

        while (elapsed < effectDuration)
        {
            if (effect == null)
                yield break;

            elapsed += Time.deltaTime;

            float progress =
                Mathf.Clamp01(elapsed / effectDuration);

            float currentRadius = radius * progress;

            for (int i = 0; i < line.positionCount; i++)
            {
                float angle =
                    i * Mathf.PI * 2f / (line.positionCount - 1);

                Vector3 point = center + new Vector3(
                    Mathf.Cos(angle) * currentRadius,
                    Mathf.Sin(angle) * currentRadius,
                    0f
                );

                line.SetPosition(i, point);
            }

            line.startColor = new Color(
                0.2f, 0.9f, 1f, 1f - progress);

            line.endColor = new Color(
                1f, 1f, 1f, 1f - progress);

            yield return null;
        }

        Destroy(effect);
    }
}
