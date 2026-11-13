
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoShooter : MonoBehaviour
{
    [Header("弾の設定")]
    public GameObject bulletPrefab;
    public float fireInterval = 0.5f;
    public int baseDamage = 10;

    private float nextFireTime;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // ゲームオーバー中は撃てない
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (Mouse.current == null)
            return;

        // 左クリックしたときだけ発射
        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextFireTime)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        // マウスのスクリーン座標を取得
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        // スクリーン座標をワールド座標に変換
        float cameraDistance = Mathf.Abs(
            transform.position.z -
            mainCamera.transform.position.z
        );

        Vector3 screenPosition = new Vector3(
            mousePosition.x,
            mousePosition.y,
            cameraDistance
        );

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        // プレイヤーからマウス方向へのベクトル
        Vector2 direction =
            (Vector2)(mouseWorldPosition - transform.position);

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        // 弾を生成
        GameObject bullet = Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.identity
        );

        // 発射方向とダメージを設定
        BulletController controller =
            bullet.GetComponent<BulletController>();

        if (controller != null)
        {
            controller.Launch(direction, baseDamage);
        }
        else
        {
            Debug.LogError(
                "弾Prefabに BulletController がありません！"
            );

            Destroy(bullet);
            return;
        }

        nextFireTime = Time.time + fireInterval;
    }

    // レベルアップ機能との互換性を維持
    public void UpgradeDamage()
    {
        baseDamage += 10;

        Debug.Log(
            "攻撃力アップ！ 現在の攻撃力：" + baseDamage
        );
    }

    public void UpgradeAttackSpeed()
    {
        fireInterval = Mathf.Max(
            0.1f,
            fireInterval * 0.8f
        );

        Debug.Log(
            "攻撃速度アップ！ 発射間隔：" + fireInterval
        );
    }
}


