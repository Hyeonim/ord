using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 유닛의 상태, 전투, 타겟팅을 관리하는 핵심 컨트롤러.
/// 인벤토리/필드 상태에 따라 행동이 달라진다.
/// </summary>
public class UnitController : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private UnitData unitData;
    public UnitData UnitData => unitData;

    [Header("상태")]
    [SerializeField] private UnitPlacement placement = UnitPlacement.Inventory;
    public UnitPlacement Placement => placement;

    [Header("전투")]
    private Transform currentTarget;
    private float attackTimer;
    private bool isAttacking = false;

    // 드래그 관련
    [HideInInspector] public bool isDragging = false;
    [HideInInspector] public Vector3 originalPosition;

    // 스토리 보스 타겟 (우선순위)
    private StoryBoss storyBossTarget;

    public void Initialize(UnitData data, UnitPlacement startPlacement)
    {
        unitData = data;
        placement = startPlacement;
        gameObject.name = $"Unit_{data.unitName}_{data.grade}";

        // 등급에 따른 색상 설정 (테스트용)
        SetColorByGrade(data.grade);
    }

    private void SetColorByGrade(UnitGrade grade)
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null) return;

        Color color = grade switch
        {
            UnitGrade.Common => Color.white,
            UnitGrade.Uncommon => Color.green,
            UnitGrade.Rare => Color.blue,
            UnitGrade.Epic => new Color(0.6f, 0f, 0.8f), // 보라
            UnitGrade.Legendary => Color.yellow,
            _ => Color.white
        };

        rend.material.color = color;
    }

    private void Update()
    {
        if (isDragging) return;
        if (placement != UnitPlacement.Field) return;

        // 전투 로직: 필드에 배치된 유닛만 실행
        FindTarget();
        AttackTarget();
    }

    /// <summary>
    /// 타겟을 찾는다. 스토리 보스가 사거리 내에 있으면 우선 공격.
    /// 그렇지 않으면 사거리 내 가장 앞서가는 적을 타겟팅.
    /// </summary>
    private void FindTarget()
    {
        // 1. 스토리 보스 우선 체크
        if (storyBossTarget != null && storyBossTarget.IsAlive)
        {
            float distToBoss = Vector3.Distance(transform.position, storyBossTarget.transform.position);
            if (distToBoss <= unitData.attackRange)
            {
                currentTarget = storyBossTarget.transform;
                return;
            }
        }

        // 2. 일반 적 타겟팅 (사거리 내 가장 앞서가는 적)
        EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();
        EnemyController bestTarget = null;
        float bestProgress = -1f;

        foreach (var enemy in allEnemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= unitData.attackRange)
            {
                if (enemy.PathProgress > bestProgress)
                {
                    bestProgress = enemy.PathProgress;
                    bestTarget = enemy;
                }
            }
        }

        currentTarget = bestTarget != null ? bestTarget.transform : null;
    }

    /// <summary>
    /// 타겟을 공격한다.
    /// </summary>
    private void AttackTarget()
    {
        if (currentTarget == null) return;

        attackTimer += Time.deltaTime;
        float attackInterval = 1f / unitData.attackSpeed;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            PerformAttack();
        }

        // 타겟 방향으로 회전
        Vector3 dir = (currentTarget.position - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void PerformAttack()
    {
        if (currentTarget == null) return;

        // 스토리 보스 공격
        StoryBoss boss = currentTarget.GetComponent<StoryBoss>();
        if (boss != null)
        {
            boss.TakeDamage(unitData.attackDamage);
            SpawnAttackEffect(currentTarget.position);
            return;
        }

        // 일반 적 공격
        EnemyController enemy = currentTarget.GetComponent<EnemyController>();
        if (enemy != null && enemy.IsAlive)
        {
            enemy.TakeDamage(unitData.attackDamage);
            SpawnAttackEffect(currentTarget.position);
        }
    }

    /// <summary>
    /// 간단한 공격 이펙트 (라인 렌더러 또는 디버그 라인)
    /// </summary>
    private void SpawnAttackEffect(Vector3 targetPos)
    {
        // 디버그용 라인 표시
        Debug.DrawLine(transform.position + Vector3.up * 0.5f, targetPos + Vector3.up * 0.5f, Color.red, 0.1f);
    }

    public void SetPlacement(UnitPlacement newPlacement)
    {
        placement = newPlacement;
        if (placement == UnitPlacement.Inventory)
        {
            currentTarget = null;
            attackTimer = 0f;
            storyBossTarget = null;
        }
    }

    /// <summary>
    /// 스토리 보스를 타겟으로 설정한다.
    /// 유닛이 보스 근처에 배치되면 호출된다.
    /// </summary>
    public void SetStoryBossTarget(StoryBoss boss)
    {
        storyBossTarget = boss;
    }

    /// <summary>
    /// 사거리를 기즈모로 표시 (에디터 전용)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (unitData == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, unitData.attackRange);
    }
}

public enum UnitPlacement
{
    Inventory,  // 대기석(인벤토리)
    Field       // 전투 필드
}
