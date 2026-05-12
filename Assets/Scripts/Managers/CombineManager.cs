using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 조합 시스템 관리.
/// 같은 유닛 3개 자동 조합 + 히든/전설/초월 특수 조합을 처리한다.
/// </summary>
public class CombineManager : MonoBehaviour
{
    public static CombineManager Instance { get; private set; }

    [Header("조합법")]
    [SerializeField] private CombineRecipe[] specialRecipes;

    [Header("설정")]
    [SerializeField] private bool autoCombineEnabled = true;

    private HashSet<string> craftedUniqueUnits = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CheckAutoCombine(UnitController newUnit)
    {
        if (!autoCombineEnabled) return;
        if (newUnit == null || newUnit.UnitData == null) return;

        SummonManager sm = SummonManager.Instance;
        if (sm == null) return;

        List<UnitController> sameUnits = sm.GetSameUnits(newUnit.UnitData);
        if (sameUnits.Count >= newUnit.UnitData.combineCount)
        {
            PerformBasicCombine(sameUnits, newUnit.UnitData);
        }
    }

    private void PerformBasicCombine(List<UnitController> units, UnitData sourceData)
    {
        if (units.Count < sourceData.combineCount) return;
        SummonManager sm = SummonManager.Instance;
        if (sm == null) return;

        List<UnitController> toConsume = units.Take(sourceData.combineCount).ToList();
        Vector3 combinePos = toConsume[0].transform.position;

        foreach (var unit in toConsume)
        {
            sm.RemoveFromInventory(unit);
            Destroy(unit.gameObject);
        }

        UnitData resultData = sourceData.evolvedUnit != null ? sourceData.evolvedUnit : CreateEvolvedUnit(sourceData);
        SpawnCombinedUnit(resultData, combinePos);
        Debug.Log($"[CombineManager] 조합! {sourceData.unitName}({sourceData.grade}) x{sourceData.combineCount} -> {resultData.unitName}({resultData.grade})");
    }

    public bool TrySpecialCombine(CombineRecipe recipe)
    {
        if (recipe == null) return false;
        if (recipe.isUnique && craftedUniqueUnits.Contains(recipe.resultUnit.unitName))
        {
            Debug.Log($"[CombineManager] '{recipe.resultUnit.unitName}' 1회 제한 초과");
            return false;
        }

        SummonManager sm = SummonManager.Instance;
        GameManager gm = GameManager.Instance;
        if (sm == null || gm == null) return false;

        if (recipe.woodRequired > 0 && gm.Wood < recipe.woodRequired) return false;

        List<UnitController> materialsToConsume = new List<UnitController>();
        foreach (var material in recipe.materials)
        {
            if (material.unitData == null) continue;
            var available = sm.InventoryUnits.Where(u =>
                u != null && u.UnitData != null &&
                u.UnitData.unitName == material.unitData.unitName &&
                !materialsToConsume.Contains(u)).ToList();
            if (available.Count < material.count) return false;
            materialsToConsume.AddRange(available.Take(material.count));
        }

        Vector3 combinePos = materialsToConsume.Count > 0 ? materialsToConsume[0].transform.position : Vector3.zero;
        foreach (var unit in materialsToConsume)
        {
            sm.RemoveFromInventory(unit);
            Destroy(unit.gameObject);
        }
        if (recipe.woodRequired > 0) gm.SpendWood(recipe.woodRequired);

        SpawnCombinedUnit(recipe.resultUnit, combinePos);
        if (recipe.isUnique) craftedUniqueUnits.Add(recipe.resultUnit.unitName);
        Debug.Log($"[CombineManager] 특수 조합! -> {recipe.resultUnit.unitName}({recipe.resultUnit.grade})");
        return true;
    }

    private UnitData CreateEvolvedUnit(UnitData source)
    {
        UnitData evolved = ScriptableObject.CreateInstance<UnitData>();
        evolved.unitName = source.unitName;
        evolved.displayName = source.displayName;
        evolved.unitID = source.unitID + 100;
        evolved.grade = GetNextGrade(source.grade);
        evolved.attackDamage = source.attackDamage * 3f;
        evolved.attackSpeed = source.attackSpeed * 1.2f;
        evolved.attackRange = source.attackRange + 0.5f;
        evolved.attackType = source.attackType;
        evolved.combineCount = 3;
        return evolved;
    }

    private UnitGrade GetNextGrade(UnitGrade current)
    {
        return current switch
        {
            UnitGrade.Common => UnitGrade.Uncommon,
            UnitGrade.Uncommon => UnitGrade.Special,
            UnitGrade.Special => UnitGrade.Rare,
            UnitGrade.Rare => UnitGrade.Legendary,
            _ => UnitGrade.Legendary
        };
    }

    private void SpawnCombinedUnit(UnitData data, Vector3 position)
    {
        SummonManager sm = SummonManager.Instance;
        if (sm == null) return;

        GameObject unitObj;
        if (data.prefab != null)
        {
            unitObj = Instantiate(data.prefab, position, Quaternion.identity);
            unitObj.SetActive(true);
        }
        else
        {
            PrimitiveType shape = data.grade >= UnitGrade.Legendary ? PrimitiveType.Capsule : PrimitiveType.Cylinder;
            unitObj = GameObject.CreatePrimitive(shape);
            unitObj.transform.position = position;
            float scale = 0.5f + ((int)data.grade) * 0.1f;
            unitObj.transform.localScale = Vector3.one * scale;
        }

        UnitController unit = unitObj.GetComponent<UnitController>();
        if (unit == null) unit = unitObj.AddComponent<UnitController>();
        unit.Initialize(data, UnitPlacement.Inventory);

        Renderer rend = unitObj.GetComponent<Renderer>();
        if (rend != null) rend.material.color = data.GetGradeColor();

        sm.InventoryUnits.Add(unit);
        unitObj.name = $"Unit_{data.unitName}_{data.grade}";
    }

    public CombineRecipe[] GetAllSpecialRecipes() => specialRecipes;
    public void ToggleAutoCombine() { autoCombineEnabled = !autoCombineEnabled; }
    public bool IsAutoCombineEnabled => autoCombineEnabled;
}
