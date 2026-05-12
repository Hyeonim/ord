using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 전체 유닛 데이터베이스.
/// 소환 확률, 유닛 검색, 등급별 필터링을 담당한다.
/// </summary>
[CreateAssetMenu(fileName = "UnitDatabase", menuName = "ORD/Unit Database")]
public class UnitDatabase : ScriptableObject
{
    [Header("유닛 목록")]
    public UnitData[] allUnits;

    [Header("조합법 목록")]
    public CombineRecipe[] allRecipes;

    [Header("흔함 유닛 소환 풀 (위스프 소환용)")]
    public UnitData[] commonPool;

    /// <summary>
    /// 위스프 소환: 흔함 등급 유닛 중 랜덤 반환
    /// </summary>
    public UnitData GetRandomCommonUnit()
    {
        if (commonPool == null || commonPool.Length == 0) return null;
        return commonPool[Random.Range(0, commonPool.Length)];
    }

    /// <summary>
    /// 등급별 유닛 목록 반환
    /// </summary>
    public UnitData[] GetUnitsByGrade(UnitGrade grade)
    {
        if (allUnits == null) return new UnitData[0];
        return allUnits.Where(u => u != null && u.grade == grade).ToArray();
    }

    /// <summary>
    /// 이름으로 유닛 검색
    /// </summary>
    public UnitData FindUnitByName(string unitName)
    {
        if (allUnits == null) return null;
        return allUnits.FirstOrDefault(u => u != null && u.unitName == unitName);
    }

    /// <summary>
    /// ID로 유닛 검색
    /// </summary>
    public UnitData FindUnitByID(int id)
    {
        if (allUnits == null) return null;
        return allUnits.FirstOrDefault(u => u != null && u.unitID == id);
    }

    /// <summary>
    /// 특정 유닛의 조합법 검색
    /// </summary>
    public CombineRecipe FindRecipeForUnit(UnitData unit)
    {
        if (allRecipes == null) return null;
        return allRecipes.FirstOrDefault(r => r != null && r.resultUnit == unit);
    }

    /// <summary>
    /// 특정 유닛이 재료로 사용되는 모든 조합법 반환
    /// </summary>
    public CombineRecipe[] FindRecipesUsingUnit(UnitData unit)
    {
        if (allRecipes == null) return new CombineRecipe[0];
        return allRecipes.Where(r => r != null && r.materials != null &&
            r.materials.Any(m => m.unitData == unit)).ToArray();
    }
}
