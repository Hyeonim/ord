using UnityEngine;

/// <summary>
/// 적 머리 위에 표시되는 체력바.
/// 월드 스페이스에서 빌보드 방식으로 카메라를 향한다.
/// </summary>
public class EnemyHPBar : MonoBehaviour
{
    [SerializeField] private Transform fillBar;
    [SerializeField] private float yOffset = 1.2f;

    private EnemyController enemy;
    private Camera mainCamera;
    private Transform barBackground;

    public void Initialize(EnemyController enemyController)
    {
        enemy = enemyController;
        mainCamera = Camera.main;

        // HP바 생성 (간단한 큐브 기반)
        CreateHPBar();

        if (enemy != null)
        {
            enemy.OnHPChanged += UpdateBar;
        }
    }

    private void CreateHPBar()
    {
        // 배경 (빨간색)
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bg.name = "HPBar_BG";
        bg.transform.SetParent(transform);
        bg.transform.localPosition = new Vector3(0, yOffset, 0);
        bg.transform.localScale = new Vector3(1f, 0.1f, 0.1f);
        Renderer bgRend = bg.GetComponent<Renderer>();
        if (bgRend != null) bgRend.material.color = Color.red;
        Destroy(bg.GetComponent<Collider>());
        barBackground = bg.transform;

        // 전경 (녹색)
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fill.name = "HPBar_Fill";
        fill.transform.SetParent(bg.transform);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale = Vector3.one;
        Renderer fillRend = fill.GetComponent<Renderer>();
        if (fillRend != null) fillRend.material.color = Color.green;
        Destroy(fill.GetComponent<Collider>());
        fillBar = fill.transform;
    }

    private void LateUpdate()
    {
        if (enemy == null || enemy.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        // 빌보드: 항상 카메라를 향함
        if (mainCamera != null && barBackground != null)
        {
            barBackground.rotation = mainCamera.transform.rotation;
        }
    }

    private void UpdateBar(float hpPercent)
    {
        if (fillBar != null)
        {
            fillBar.localScale = new Vector3(hpPercent, 1f, 1f);
            fillBar.localPosition = new Vector3((hpPercent - 1f) / 2f, 0, 0);

            // 색상 변화
            Renderer rend = fillBar.GetComponent<Renderer>();
            if (rend != null)
            {
                if (hpPercent > 0.5f) rend.material.color = Color.green;
                else if (hpPercent > 0.25f) rend.material.color = Color.yellow;
                else rend.material.color = Color.red;
            }
        }
    }

    private void OnDestroy()
    {
        if (enemy != null) enemy.OnHPChanged -= UpdateBar;
    }
}
