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
        else Destroy(gameObject);
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

        if (GameManager.Instance.Gold < summonCost)
        {
            Debug.Log("[SummonManager] 골드가 부족합니다!");
            return false;
        }

        GameManager.Instance.SpendGold(summonCost);

        UnitData data = unitDatabase.GetRandomUnit();
        GameObject unitObj = Instantiate(data.prefab, GetEmptySlotPosition(), Quaternion.identity, inventoryParent);
        UnitController unit = unitObj.GetComponent<UnitController>();
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
        // 동일 유닛 ID를 가진 유닛들을 찾는다
        var sameUnits = inventoryUnits.Where(u => u.UnitData.unitID == data.unitID).ToList();

        if (sameUnits.Count >= data.combineCount)
        {
            PerformCombine(sameUnits, data);
        }
    }

    /// <summary>
    /// 수동 조합 트리거 (UI 버튼 등에서 호출)
    /// </summary>
    public void TryManualCombine(UnitData data)
    {
        var sameUnits = inventoryUnits.Where(u => u.UnitData.unitID == data.unitID).ToList();
        if (sameUnits.Count >= data.combineCount)
        {
            PerformCombine(sameUnits, data);
        }
        else
        {
            Debug.Log($"[SummonManager] 조합 재료 부족: {sameUnits.Count}/{data.combineCount}");
        }
    }

    private void PerformCombine(List<UnitController> materials, UnitData sourceData)
    {
        // 상위 등급 유닛 데이터 찾기
        UnitGrade nextGrade = (UnitGrade)((int)sourceData.grade + 1);
        UnitData[] nextGradeUnits = unitDatabase.GetUnitsByGrade(nextGrade);

        if (nextGradeUnits.Length == 0)
        {
            Debug.Log("[SummonManager] 더 이상 상위 등급이 없습니다!");
            return;
        }

        // 재료 유닛 제거 (combineCount 만큼만)
        for (int i = 0; i < sourceData.combineCount; i++)
        {
            UnitController unit = materials[i];
            inventoryUnits.Remove(unit);
            Destroy(unit.gameObject);
        }

        // 상위 유닛 생성
        UnitData resultData = nextGradeUnits[Random.Range(0, nextGradeUnits.Length)];
        GameObject resultObj = Instantiate(resultData.prefab, GetEmptySlotPosition(), Quaternion.identity, inventoryParent);
        UnitController resultUnit = resultObj.GetComponent<UnitController>();
        resultUnit.Initialize(resultData, UnitPlacement.Inventory);
        inventoryUnits.Add(resultUnit);

        Debug.Log($"[SummonManager] 조합 성공! {sourceData.unitName} x{sourceData.combineCount} → {resultData.unitName} (★{(int)resultData.grade})");

        // 연쇄 조합 체크
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
        // 슬롯이 없으면 기본 위치
        return inventoryParent.position + Vector3.right * inventoryUnits.Count * 1.5f;
    }

    /// <summary>
    /// 인벤토리에서 유닛을 제거한다 (필드 배치 시 호출).
    /// </summary>
    public void RemoveFromInventory(UnitController unit)
    {
        inventoryUnits.Remove(unit);
    }

    /// <summary>
    /// 유닛을 인벤토리로 되돌린다.
    /// </summary>
    public void ReturnToInventory(UnitController unit)
    {
        if (inventoryUnits.Count >= maxInventorySize) return;
        unit.transform.position = GetEmptySlotPosition();
        unit.SetPlacement(UnitPlacement.Inventory);
        inventoryUnits.Add(unit);
    }

    public List<UnitController> GetInventoryUnits() => inventoryUnits;
}
