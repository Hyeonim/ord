using UnityEngine;

/// <summary>
/// 적(몬스터) 컨트롤러.
/// 경로를 따라 이동하며, 피격/스턴/이감/방깎 처리를 담당한다.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private WaypointPath path;

    [Header("상태")]
    [SerializeField] private float currentHP;
    [SerializeField] private float maxHP;
    [SerializeField] private float currentArmor;
    [SerializeField] private float moveSpeed;
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private bool isDead = false;

    // 디버프 상태
    private float stunTimer = 0f;
    private float slowAmount = 0f;
    private float slowTimer = 0f;
    private float armorBreakAmount = 0f;
    private float armorBreakTimer = 0f;

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public float CurrentArmor => Mathf.Max(0, currentArmor - armorBreakAmount);
    public bool IsDead => isDead;
    public float HPPercent => maxHP > 0 ? currentHP / maxHP : 0f;

    // HP바 UI용
    public System.Action<float> OnHPChanged;

    public void Initialize(EnemyData data, WaypointPath waypointPath)
    {
        enemyData = data;
        path = waypointPath;
        currentHP = data.maxHP;
        maxHP = data.maxHP;
        currentArmor = data.armor;
        moveSpeed = data.moveSpeed;
        currentWaypointIndex = 0;
        isDead = false;

        if (data.isBoss)
        {
            transform.localScale = Vector3.one * data.bossScale;
        }

        // HP바 생성
        EnemyHPBar hpBar = gameObject.AddComponent<EnemyHPBar>();
        hpBar.Initialize(this);
    }

    private void Update()
    {
        if (isDead) return;

        // 스턴 처리
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        // 이감 처리
        if (slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if (slowTimer <= 0f) slowAmount = 0f;
        }

        // 방깎 타이머
        if (armorBreakTimer > 0f)
        {
            armorBreakTimer -= Time.deltaTime;
            if (armorBreakTimer <= 0f) armorBreakAmount = 0f;
        }

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (path == null) return;
        if (currentWaypointIndex >= path.WaypointCount)
        {
            ReachEnd();
            return;
        }

        Vector3 target = path.GetWaypointPosition(currentWaypointIndex);
        float effectiveSpeed = moveSpeed * (1f - slowAmount);
        transform.position = Vector3.MoveTowards(transform.position, target, effectiveSpeed * Time.deltaTime);

        // 방향 바라보기
        Vector3 dir = (target - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    public void TakeDamage(float damage, AttackType type)
    {
        if (isDead) return;

        currentHP -= damage;
        OnHPChanged?.Invoke(HPPercent);

        // 피격 이펙트 (색상 깜빡임)
        StartCoroutine(FlashDamage());

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) yield break;

        Color original = rend.material.color;
        rend.material.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (rend != null) rend.material.color = original;
    }

    public void ApplyStun(float duration)
    {
        stunTimer = Mathf.Max(stunTimer, duration);
    }

    public void ApplySlow(float amount, float duration)
    {
        slowAmount = Mathf.Max(slowAmount, Mathf.Clamp01(amount / 100f));
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    public void ApplyArmorBreak(float amount, float duration)
    {
        armorBreakAmount = Mathf.Max(armorBreakAmount, amount);
        armorBreakTimer = Mathf.Max(armorBreakTimer, duration);
    }

    private void Die()
    {
        isDead = true;

        // 골드 보상
        if (GameManager.Instance != null && enemyData != null)
        {
            GameManager.Instance.AddGold(enemyData.goldReward);
        }

        // WaveManager에 알림
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyKilled();
        }

        // 사망 이펙트 후 제거
        Destroy(gameObject, 0.1f);
    }

    private void ReachEnd()
    {
        // 끝까지 도달 → 유닛 카운트 증가
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyReachedEnd();
        }

        Destroy(gameObject);
    }
}
