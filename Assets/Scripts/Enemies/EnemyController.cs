using UnityEngine;

/// <summary>
/// 적 유닛의 이동, 체력, 사망 처리를 담당하는 컨트롤러.
/// 웨이포인트 경로를 따라 이동하며, 경로를 완주하면 플레이어 라이프를 감소시킨다.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private EnemyData enemyData;
    public EnemyData EnemyData => enemyData;

    [Header("상태")]
    private float currentHP;
    private int currentWaypointIndex = 0;
    private int lapsCompleted = 0;
    private float totalDistanceTraveled = 0f;

    // 경로 참조
    private WaypointPath path;

    // 경로 진행도 (타겟팅 우선순위에 사용)
    public float PathProgress => totalDistanceTraveled;
    public bool IsAlive => currentHP > 0f;

    [Header("순환 설정")]
    public int maxLaps = 3; // 최대 순환 횟수 (이후 라이프 감소)

    public void Initialize(EnemyData data, WaypointPath waypointPath, int startIndex = 0)
    {
        enemyData = data;
        path = waypointPath;
        currentHP = data.maxHP;
        currentWaypointIndex = startIndex;
        totalDistanceTraveled = 0f;
        lapsCompleted = 0;

        gameObject.name = $"Enemy_{data.enemyName}";
        transform.position = path.GetWaypointPosition(startIndex);

        // 적 색상 설정 (테스트용)
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material.color = data.isBoss ? Color.red : new Color(1f, 0.5f, 0f); // 보스=빨강, 일반=주황
        }
    }

    private void Update()
    {
        if (!IsAlive) return;
        MoveAlongPath();
    }

    /// <summary>
    /// 웨이포인트 경로를 따라 이동한다.
    /// </summary>
    private void MoveAlongPath()
    {
        if (path == null || path.waypoints.Length == 0) return;

        Vector3 targetPos = path.GetWaypointPosition(currentWaypointIndex);
        Vector3 direction = (targetPos - transform.position).normalized;
        float step = enemyData.moveSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
        totalDistanceTraveled += step;

        // 타겟 방향으로 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 웨이포인트 도달 체크
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            int nextIndex = path.GetNextIndex(currentWaypointIndex);

            // 순환 완료 체크
            if (nextIndex == 0 && currentWaypointIndex == path.waypoints.Length - 1)
            {
                lapsCompleted++;
                if (lapsCompleted >= maxLaps)
                {
                    // 최대 순환 완료 → 플레이어 라이프 감소
                    OnReachEnd();
                    return;
                }
            }

            currentWaypointIndex = nextIndex;
        }
    }

    /// <summary>
    /// 경로 완주 시 호출. 플레이어 라이프를 감소시키고 자신을 제거한다.
    /// </summary>
    private void OnReachEnd()
    {
        GameManager.Instance.LoseLife(1);
        Debug.Log($"[Enemy] {enemyData.enemyName}이(가) 경로를 완주! 라이프 -1");
        Die(false);
    }

    /// <summary>
    /// 데미지를 받는다.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHP -= damage;
        
        // 피격 이펙트 (깜빡임)
        StartCoroutine(FlashEffect());

        if (currentHP <= 0f)
        {
            Die(true);
        }
    }

    /// <summary>
    /// 사망 처리.
    /// </summary>
    private void Die(bool giveReward)
    {
        if (giveReward)
        {
            GameManager.Instance.AddGold(enemyData.goldReward);
            WaveManager.Instance.OnEnemyKilled();
        }

        // 사망 이펙트 (스케일 축소)
        Destroy(gameObject, 0.1f);
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            rend.material.color = original;
        }
    }

    /// <summary>
    /// 체력 비율을 반환한다 (HP 바 등에 사용).
    /// </summary>
    public float GetHPRatio()
    {
        if (enemyData == null) return 0f;
        return currentHP / enemyData.maxHP;
    }
}
