using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 유닛 전투 컨트롤러.
/// 사거리 내 적 탐색 → 공격 → 능력 적용을 처리한다.
/// </summary>
public class UnitController : MonoBehaviour
{
    [Header("데이터")]
    [SerializeField] private UnitData unitData;
    [SerializeField] private UnitPlacement placement = UnitPlacement.Inventory;

    [Header("전투 상태")]
    [SerializeField] private float attackTimer = 0f;
    [SerializeField] private Transform currentTarget;
    [SerializeField] private bool isAttacking = false;

    public UnitData UnitData => unitData;
    public UnitPlacement Placement => placement;

    private Renderer unitRenderer;

    public void Initialize(UnitData data, UnitPlacement place)
    {
        unitData = data;
        placement = place;
        attackTimer = 0f;
        currentTarget = null;

        unitRenderer = GetComponent<Renderer>();
        if (unitRenderer != null && data != null)
        {
            unitRenderer.material.color = data.GetGradeColor();
        }

        // 인벤토리에 있으면 전투 비활성
        isAttacking = (placement == UnitPlacement.Field);
    }

    private void Update()
    {
        if (unitData == null) return;
        if (placement != UnitPlacement.Field) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        attackTimer += Time.deltaTime;

        if (currentTarget == null || !IsTargetValid())
        {
            FindTarget();
        }

        if (currentTarget != null && attackTimer >= 1f / unitData.attackSpeed)
        {
            Attack();
            attackTimer = 0f;
        }

        // 타겟 방향 바라보기
        if (currentTarget != null)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }
    }

    private void FindTarget()
    {
        if (unitData == null) return;

        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        float closestDist = unitData.attackRange;
        Transform closest = null;

        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= closestDist)
            {
                closestDist = dist;
                closest = enemy.transform;
            }
        }

        currentTarget = closest;
    }

    private bool IsTargetValid()
    {
        if (currentTarget == null) return false;
        EnemyController enemy = currentTarget.GetComponent<EnemyController>();
        if (enemy == null || enemy.IsDead) return false;
        float dist = Vector3.Distance(transform.position, currentTarget.position);
        return dist <= unitData.attackRange;
    }

    private void Attack()
    {
        if (currentTarget == null) return;
        EnemyController enemy = currentTarget.GetComponent<EnemyController>();
        if (enemy == null || enemy.IsDead) return;

        float damage = CalculateDamage(enemy);
        enemy.TakeDamage(damage, unitData.attackType);

        // 능력 적용
        ApplyAbility(enemy);

        // 투사체 생성 (시각적)
        if (unitData.projectilePrefab != null)
        {
            GameObject proj = Instantiate(unitData.projectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null) p.Initialize(currentTarget, unitData.projectileSpeed);
        }
    }

    private float CalculateDamage(EnemyController enemy)
    {
        float damage = unitData.attackDamage;

        switch (unitData.attackType)
        {
            case AttackType.Physical:
                // 물리: 방어력에 의해 감소
                float reduction = enemy.CurrentArmor / (enemy.CurrentArmor + 100f);
                damage *= (1f - reduction);
                break;
            case AttackType.Magical:
                // 마법: 방어력 무시
                break;
            case AttackType.Percent:
                // %: 현재 체력 기반
                if (unitData.abilityType == AbilityType.SingleTarget)
                    damage = enemy.CurrentHP * (unitData.abilityValue / 100f);
                else if (unitData.abilityType == AbilityType.FinishDamage)
                    damage = enemy.MaxHP * (unitData.abilityValue / 100f);
                break;
        }

        return damage;
    }

    private void ApplyAbility(EnemyController enemy)
    {
        if (unitData.abilityType == AbilityType.None) return;

        switch (unitData.abilityType)
        {
            case AbilityType.Stun:
                enemy.ApplyStun(unitData.abilityDuration);
                break;
            case AbilityType.Slow:
                enemy.ApplySlow(unitData.abilityValue, unitData.abilityDuration);
                break;
            case AbilityType.ArmorBreak:
                enemy.ApplyArmorBreak(unitData.abilityValue, unitData.abilityDuration);
                break;
            case AbilityType.AttackBuff:
                BuffNearbyUnits(unitData.abilityValue, false);
                break;
            case AbilityType.AttackSpeedBuff:
                BuffNearbyUnits(unitData.abilityValue, true);
                break;
        }
    }

    private void BuffNearbyUnits(float value, bool isSpeedBuff)
    {
        // 주변 아군 유닛 버프 (범위 5)
        UnitController[] allies = FindObjectsOfType<UnitController>();
        foreach (var ally in allies)
        {
            if (ally == this || ally.Placement != UnitPlacement.Field) continue;
            if (Vector3.Distance(transform.position, ally.transform.position) <= 5f)
            {
                // 버프 적용 (간단 구현)
            }
        }
    }

    /// <summary>
    /// 필드에 배치
    /// </summary>
    public void PlaceOnField(Vector3 position)
    {
        placement = UnitPlacement.Field;
        transform.position = position;
        isAttacking = true;

        if (SummonManager.Instance != null)
        {
            SummonManager.Instance.RemoveFromInventory(this);
            SummonManager.Instance.AddToField(this);
        }
    }

    /// <summary>
    /// 인벤토리로 회수
    /// </summary>
    public void ReturnToInventory(Vector3 slotPosition)
    {
        placement = UnitPlacement.Inventory;
        transform.position = slotPosition;
        isAttacking = false;
        currentTarget = null;

        if (SummonManager.Instance != null)
        {
            SummonManager.Instance.RemoveFromField(this);
            SummonManager.Instance.InventoryUnits.Add(this);
        }
    }
}
