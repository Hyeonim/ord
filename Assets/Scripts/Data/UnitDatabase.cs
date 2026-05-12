using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임 내 모든 유닛 데이터를 관리하는 데이터베이스.
/// 소환 시 등급별 확률 테이블로 사용된다.
/// </summary>
[CreateAssetMenu(fileName = "UnitDatabase", menuName = "RandomDefense/UnitDatabase")]
public class UnitDatabase : ScriptableObject
{
    public UnitData[] allUnits;

    [Header("등급별 소환 확률 (%)")]
    public float commonRate = 50f;
    public float uncommonRate = 30f;
    public float rareRate = 15f;
    public float epicRate = 4f;
    public float legendaryRate = 1f;

    /// <summary>
    /// 확률 기반으로 랜덤 유닛 데이터를 반환한다.
    /// </summary>
    public UnitData GetRandomUnit()
    {
        float roll = Random.Range(0f, 100f);
        UnitGrade selectedGrade;

        if (roll < legendaryRate)
            selectedGrade = UnitGrade.Legendary;
        else if (roll < legendaryRate + epicRate)
            selectedGrade = UnitGrade.Epic;
        else if (roll < legendaryRate + epicRate + rareRate)
            selectedGrade = UnitGrade.Rare;
        else if (roll < legendaryRate + epicRate + rareRate + uncommonRate)
            selectedGrade = UnitGrade.Uncommon;
        else
            selectedGrade = UnitGrade.Common;

        UnitData[] candidates = allUnits.Where(u => u.grade == selectedGrade).ToArray();
        if (candidates.Length == 0)
        {
            // 해당 등급 유닛이 없으면 Common에서 뽑기
            candidates = allUnits.Where(u => u.grade == UnitGrade.Common).ToArray();
        }

        return candidates[Random.Range(0, candidates.Length)];
    }

    /// <summary>
    /// 특정 등급의 유닛 목록을 반환한다.
    /// </summary>
    public UnitData[] GetUnitsByGrade(UnitGrade grade)
    {
        return allUnits.Where(u => u.grade == grade).ToArray();
    }
}
