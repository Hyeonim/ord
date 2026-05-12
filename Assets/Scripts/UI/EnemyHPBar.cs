using UnityEngine;

/// <summary>
/// 적 머리 위에 표시되는 HP 바.
/// 월드 스페이스 Canvas 또는 간단한 스프라이트로 구현.
/// </summary>
public class EnemyHPBar : MonoBehaviour
{
    [Header("참조")]
    public Transform fillBar;
    public EnemyController enemy;

    private Vector3 originalScale;
    private Camera mainCamera;

    private void Start()
    {
        if (fillBar != null)
            originalScale = fillBar.localScale;
        mainCamera = Camera.main;

        if (enemy == null)
            enemy = GetComponentInParent<EnemyController>();
    }

    private void Update()
    {
        if (enemy == null) return;

        // HP 비율에 따라 바 크기 조절
        if (fillBar != null)
        {
            float ratio = enemy.GetHPRatio();
            Vector3 scale = originalScale;
            scale.x = originalScale.x * ratio;
            fillBar.localScale = scale;
        }

        // 카메라를 향하도록 회전
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
