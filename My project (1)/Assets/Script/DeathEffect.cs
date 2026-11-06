
using UnityEngine;
using System.Collections;

public class DeathEffect : MonoBehaviour
{
    public static void Play(Sprite sprite, Vector3 position, Color color)
    {
        if (sprite == null) return;

        GameObject effect = new GameObject("DeathEffect");
        DeathEffect controller = effect.AddComponent<DeathEffect>();

        controller.StartCoroutine(
            controller.CreatePieces(sprite, position, color)
        );
    }

    IEnumerator CreatePieces(
        Sprite sprite,
        Vector3 position,
        Color color)
    {
        const int pieceCount = 12;

        for (int i = 0; i < pieceCount; i++)
        {
            GameObject piece = new GameObject("EffectPiece");
            piece.transform.position = position;
            piece.transform.localScale = Vector3.one * Random.Range(0.4f, 0.6f);

            SpriteRenderer sr = piece.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 20;

            Vector2 direction = Random.insideUnitCircle.normalized;

            StartCoroutine(AnimatePiece(
                piece, sr, direction * Random.Range(2f, 5f)
            ));
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    IEnumerator AnimatePiece(
        GameObject piece,
        SpriteRenderer sr,
        Vector2 velocity)
    {
        float duration = 0.35f;
        float elapsed = 0f;
        Vector3 startScale = piece.transform.localScale;

        while (elapsed < duration && piece != null)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            piece.transform.position +=
                (Vector3)(velocity * dt);

            velocity *= 0.94f;

            float alpha = 1f - elapsed / duration;

            if (sr != null)
                sr.color = new Color(
                    sr.color.r,
                    sr.color.g,
                    sr.color.b,
                    alpha
                );

            if (piece != null)
                piece.transform.localScale =
                    startScale * alpha;

            yield return null;
        }

        if (piece != null)
            Destroy(piece);
    }
}

