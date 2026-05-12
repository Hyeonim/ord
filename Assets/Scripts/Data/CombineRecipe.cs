using UnityEngine;
using System;

/// <summary>
/// 조합법 데이터를 정의하는 ScriptableObject.
/// 히든, 전설, 초월 등 특수 조합법을 관리한다.
/// </summary>
[CreateAssetMenu(fileName = "NewCombineRecipe", menuName = "ORD/Combine Recipe")]
public class CombineRecipe : ScriptableObject
{
    [Header("결과 유닛")]
    public UnitData resultUnit;         // 조합 결과 유닛

    [Header("재료")]
    public RecipeMaterial[] materials;  // 필요 재료 목록
    public int woodRequired = 0;        // 필요 목재 수

    [Header("조합 조건")]
    public bool isUnique = false;       // 1회만 제작 가능 (초월 등)
    public UnitGrade minimumGrade = UnitGrade.Common; // 최소 등급 조건

    [Header("표시")]
    public string recipeDescription;    // 조합법 설명
}

/// <summary>
/// 조합 재료 항목
/// </summary>
[Serializable]
public class RecipeMaterial
{
    public UnitData unitData;       // 필요 유닛
    public int count = 1;           // 필요 수량
}
