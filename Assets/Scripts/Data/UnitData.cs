using UnityEngine;

/// <summary>
/// 유닛 데이터를 정의하는 ScriptableObject.
/// 각 원피스 캐릭터의 능력치, 등급, 스킬 정보를 담는다.
/// </summary>
[CreateAssetMenu(fileName = "NewUnitData", menuName = "ORD/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("기본 정보")]
    public int unitID;
    public string unitName;         // 캐릭터 이름 (예: 루피, 조로)
    public string displayName;      // 표시 이름
    public UnitGrade grade;         // 등급
    public Sprite unitIcon;         // 아이콘
    public GameObject prefab;       // 프리팹

    [Header("전투 스탯")]
    public float attackDamage = 10f;
    public float attackSpeed = 1f;      // 초당 공격 횟수
    public float attackRange = 3f;
    public AttackType attackType = AttackType.Physical;

    [Header("능력")]
    public AbilityType abilityType = AbilityType.None;
    public float abilityValue = 0f;     // 능력 수치 (스턴 시간, 이감 %, 방깎 수치 등)
    public float abilityDuration = 0f;  // 능력 지속 시간

    [Header("조합 정보")]
    public int combineCount = 3;        // 조합에 필요한 같은 유닛 수
    public UnitData evolvedUnit;        // 같은 유닛 3개 조합 시 진화 유닛

    [Header("투사체")]
    public float projectileSpeed = 10f;
    public GameObject projectilePrefab;

    [Header("설명")]
    [TextArea(3, 5)]
    public string description;

    /// <summary>
    /// 등급에 따른 색상 반환
    /// </summary>
    public Color GetGradeColor()
    {
        return grade switch
        {
            UnitGrade.Common => Color.white,
            UnitGrade.Uncommon => Color.green,
            UnitGrade.Special => new Color(0.2f, 0.6f, 1f),
            UnitGrade.Rare => new Color(0.6f, 0f, 0.8f),
            UnitGrade.Legendary => new Color(1f, 0.84f, 0f),
            UnitGrade.Hidden => new Color(1f, 0.4f, 0f),
            UnitGrade.Transcendent => Color.red,
            UnitGrade.Immortal => new Color(0.8f, 0f, 0.2f),
            UnitGrade.Eternal => new Color(0f, 0.8f, 0.8f),
            UnitGrade.Limited => new Color(1f, 0f, 1f),
            _ => Color.white
        };
    }
}
