using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 유닛 소환 및 조합 시스템을 관리하는 매니저.
/// - 골드를 소비하여 랜덤 유닛 소환
/// - 동일 유닛 3개를 모아 상위 유닛으로 조합
/// </summary>
public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [Header("참조")]
    public UnitDatabase unitDatabase;
    public Transform inventoryParent;    // 인벤토리(대기석) 부모 오브젝트
    public Transform[] inventorySlots;   // 대기석 슬롯 위치 배열

    [Header("설정")]
    public int summonCost = 50;
    public int maxInventorySize = 12;

    // 현재 인벤토리에 있는 유닛 리스트
    private List<UnitController> inventoryUnits = new List<UnitController>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 인스펙터에서 연결 안 된 경우 Resources 폴더에서 자동 탐색
        if (unitDatabase == null)
        {
            unitDatabase = Resources.Load<UnitDatabase>("UnitDatabase");
            if (unitDatabase == null)
            {
                // Resources에도 없으면 씬 내 모든 UnitData를 찾아 임시 DB 생성
                AutoBuildDatabase();
            }
        }

        // inventoryParent가 없으면 자신의 Transform 사용
        if (inventoryParent == null)
            inventoryParent = transform;
    }

    /// <summary>
    /// 씬 내 UnitData ScriptableObject를 찾아 임시 UnitDatabase를 자동 구성한다.
    /// </summary>
    private void AutoBuildDatabase()
    {
        UnitData[] allFound = Resources.FindObjectsOfTypeAll<UnitData>();
        if (allFound != null && allFound.Length > 0)
        {
            unitDatabase = ScriptableObject.CreateInstance<UnitDatabase>();
            unitDatabase.allUnits = allFound;
            unitDatabase.commonRate = 50f;
            unitDatabase.uncommonRate = 30f;
            unitDatabase.rareRate = 15f;
            unitDatabase.epicRate = 4f;
            unitDatabase.legendaryRate = 1f;
            Debug.Log($"[SummonManager] UnitDatabase 자동 구성 완료: {allFound.Length}개 유닛 등록");
        }
        else
        {
            Debug.LogWarning("[SummonManager] UnitData를 찾을 수 없습니다. 인스펙터에서 UnitDatabase를 직접 할당해 주세요.");
        }
    }

    /// <summary>
    /// 랜덤 유닛을 소환하여 인벤토리에 배치한다.
    /// </summary>
    public bool SummonRandomUnit()
    {
        if (inventoryUnits.Count >= maxInventorySize)
        {
            Debug.Log("[SummonManager] 인벤토리가 가득 찼습니다!");
            return false;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[SummonManager] GameManager.Instance가 null입니다.");
            return false;
        }

        if (GameManager.Instance.Gold < summonCost)
        {
            Debug.Log("[SummonManager] 골드가 부족합니다!");
            return false;
        }

        if (unitDatabase == null)
        {
            Debug.LogError("[SummonManager] unitDatabase가 없습니다. 인스펙터에서 UnitDatabase를 할당해 주세요.");
            return false;
        }

        if (unitDatabase.allUnits == null || unitDatabase.allUnits.Length == 0)
        {
            Debug.LogError("[SummonManager] UnitDatabase에 유닛이 없습니다. UnitData를 추가해 주세요.");
            return false;
        }

        UnitData data = unitDatabase.GetRandomUnit();
        if (data == null)
        {
            Debug.LogError("[SummonManager] GetRandomUnit()이 null을 반환했습니다.");
            return false;
        }

        GameManager.Instance.SpendGold(summonCost);

        GameObject unitObj;
        if (data.prefab != null)
        {
            unitObj = Instantiate(data.prefab, GetEmptySlotPosition(), Quaternion.identity, inventoryParent);
            unitObj.SetActive(true); // 프리팩 비활성 대비
        }
        else
        {
            // prefab이 없으면 UnitFactory로 동적 생성
            unitObj = UnitFactory.CreateTempUnitPrefab(data.grade);
            unitObj.transform.position = GetEmptySlotPosition();
            unitObj.transform.SetParent(inventoryParent);
        }
        UnitController unit = unitObj.GetComponent<UnitController>();

        if (unit == null)
        {
            Debug.LogError($"[SummonManager] '{data.unitName}' 프리팹에 UnitController 컴포넌트가 없습니다.");
            Destroy(unitObj);
            return false;
        }

        unit.Initialize(data, UnitPlacement.Inventory);
        inventoryUnits.Add(unit);
        CheckCombine(data);

        Debug.Log($"[SummonManager] {data.unitName} (★{(int)data.grade}) 소환!");
        return true;
    }

    /// <summary>
    /// 조합 가능 여부를 체크하고, 가능하면 자동 조합을 수행한다.
    /// </summary>
    public void CheckCombine(UnitData data)
    {
        if (data == null) return;
        var sameUnits = inventoryUnits
            .Where(u => u != null && u.UnitData != null && u.UnitData.unitID == data.unitID)
            .ToList();

        if (sameUnits.Count >= data.combineCount)
            PerformCombine(sameUnits, data);
    }

    /// <summary>
    /// 수동 조합 트리거 (UI 버튼 등에서 호출)
    /// </summary>
    public void TryManualCombine(UnitData data)
    {
        if (data == null) return;
        var sameUnits = inventoryUnits
            .Where(u => u != null && u.UnitData != null && u.UnitData.unitID == data.unitID)
            .ToList();

        if (sameUnits.Count >= data.combineCount)
            PerformCombine(sameUnits, data);
        else
            Debug.Log($"[SummonManager] 조합 재료 부족: {sameUnits.Count}/{data.combineCount}");
    }

    private void PerformCombine(List<UnitController> materials, UnitData sourceData)
    {
        if (unitDatabase == null) return;

        UnitGrade nextGrade = (UnitGrade)((int)sourceData.grade + 1);
        UnitData[] nextGradeUnits = unitDatabase.GetUnitsByGrade(nextGrade);

        if (nextGradeUnits == null || nextGradeUnits.Length == 0)
        {
            Debug.Log("[SummonManager] 더 이상 상위 등급이 없습니다!");
            return;
        }

        for (int i = 0; i < sourceData.combineCount; i++)
        {
            UnitController unit = materials[i];
            inventoryUnits.Remove(unit);
            Destroy(unit.gameObject);
        }

        UnitData resultData = nextGradeUnits[Random.Range(0, nextGradeUnits.Length)];
        if (resultData.prefab == null)
        {
            Debug.LogError($"[SummonManager] 조합 결과 '{resultData.unitName}'의 prefab이 null입니다.");
            return;
        }

        GameObject resultObj = Instantiate(resultData.prefab, GetEmptySlotPosition(), Quaternion.identity, inventoryParent);
        UnitController resultUnit = resultObj.GetComponent<UnitController>();
        if (resultUnit == null) { Destroy(resultObj); return; }

        resultUnit.Initialize(resultData, UnitPlacement.Inventory);
        inventoryUnits.Add(resultUnit);

        Debug.Log($"[SummonManager] 조합 성공! {sourceData.unitName} x{sourceData.combineCount} → {resultData.unitName} (★{(int)resultData.grade})");
        CheckCombine(resultData);
    }

    /// <summary>
    /// 빈 인벤토리 슬롯 위치를 반환한다.
    /// </summary>
    private Vector3 GetEmptySlotPosition()
    {
        if (inventorySlots != null && inventorySlots.Length > 0)
        {
            int index = Mathf.Min(inventoryUnits.Count, inventorySlots.Length - 1);
            return inventorySlots[index].position;
        }
        if (inventoryParent != null)
            return inventoryParent.position + Vector3.right * inventoryUnits.Count * 1.5f;
        return Vector3.zero;
    }

    public void RemoveFromInventory(UnitController unit) => inventoryUnits.Remove(unit);

    public void ReturnToInventory(UnitController unit)
    {
        if (inventoryUnits.Count >= maxInventorySize) return;
        unit.transform.position = GetEmptySlotPosition();
        unit.SetPlacement(UnitPlacement.Inventory);
        inventoryUnits.Add(unit);
    }

    public List<UnitController> GetInventoryUnits() => inventoryUnits;

    /// <summary>
    /// 조합법 정보를 반환한다. (UI에서 조합법 패널 표시용)
    /// </summary>
    public List<CombineRecipe> GetAllRecipes()
    {
        var recipes = new List<CombineRecipe>();
        if (unitDatabase == null) return recipes;

        foreach (var unit in unitDatabase.allUnits)
        {
            if ((int)unit.grade < (int)UnitGrade.Legendary)
            {
                UnitGrade nextGrade = (UnitGrade)((int)unit.grade + 1);
                UnitData[] results = unitDatabase.GetUnitsByGrade(nextGrade);
                recipes.Add(new CombineRecipe
                {
                    material = unit,
                    count = unit.combineCount,
                    possibleResults = results
                });
            }
        }
        return recipes;
    }
}

[System.Serializable]
public class CombineRecipe
{
    public UnitData material;
    public int count;
    public UnitData[] possibleResults;
}
