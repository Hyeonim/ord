using UnityEngine;

/// <summary>
/// 유닛의 기본 데이터를 정의하는 ScriptableObject.
/// 등급, 스탯, 조합 재료 정보를 포함한다.
/// </summary>
[CreateAssetMenu(fileName = "NewUnitData", menuName = "RandomDefense/UnitData")]
public class UnitData : ScriptableObject
{
    [Header("기본 정보")]
    public string unitName;
    public int unitID;
    public UnitGrade grade;
    public Sprite icon;
    public GameObject prefab;

    [Header("전투 스탯")]
    public float attackDamage = 10f;
    public float attackSpeed = 1f;   // 초당 공격 횟수
    public float attackRange = 3f;   // 사거리
    public float projectileSpeed = 10f;

    [Header("조합 정보")]
    public UnitData[] combineMaterials; // 이 유닛을 만들기 위해 필요한 하위 유닛들
    public int combineCount = 3;       // 필요한 동일 유닛 수 (기본 3합)
}

public enum UnitGrade
{
    Common = 1,    // ★1
    Uncommon = 2,  // ★2
    Rare = 3,      // ★3
    Epic = 4,      // ★4
    Legendary = 5  // ★5
}
