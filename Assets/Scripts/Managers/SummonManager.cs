using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 위스프 소환 시스템 및 인벤토리 관리.
/// 위스프를 사용하여 랜덤 흔함 유닛을 소환하고, 인벤토리를 관리한다.
/// </summary>
public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance { get; private set; }

    [Header("데이터베이스")]
    [SerializeField] private UnitDatabase unitDatabase;

    [Header("인벤토리 설정")]
    [SerializeField] private int maxInventorySize = 12;
    [SerializeField] private Transform inventoryParent;
    [SerializeField] private Transform[] inventorySlots;

    /// <summary>
    /// 외부에서 인벤토리 슬롯 설정 (MapGenerator 등)
    /// </summary>
    public void SetInventorySlots(Transform[] slots)
    {
        inventorySlots = slots;
        if (inventoryParent == null && slots != null && slots.Length > 0)
            inventoryParent = slots[0].parent;
    }

    [Header("필드 설정")]
    [SerializeField] private Transform fieldParent;

    // 인벤토리 유닛 목록
    private List<UnitController> inventoryUnits = new List<UnitController>();
    private List<UnitController> fieldUnits = new List<UnitController>();

    public List<UnitController> InventoryUnits => inventoryUnits;
    public List<UnitController> FieldUnits => fieldUnits;
    public int InventoryCount => inventoryUnits.Count;
    public int MaxInventory => maxInventorySize;
    public UnitDatabase Database => unitDatabase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 자동 DB 탐색
        if (unitDatabase == null)
        {
            unitDatabase = Resources.Load<UnitDatabase>("UnitDatabase");
        }

        // DB가 있어도 commonPool이 비어있으면 자동 구성
        if (unitDatabase != null && (unitDatabase.commonPool == null || unitDatabase.commonPool.Length == 0))
        {
            Debug.Log("[SummonManager] UnitDatabase의 commonPool이 비어있어 allUnits에서 자동 구성합니다.");
            // allUnits에서 흔함 등급 또는 전체를 commonPool로 사용
            if (unitDatabase.allUnits != null && unitDatabase.allUnits.Length > 0)
            {
                unitDatabase.commonPool = unitDatabase.allUnits;
            }
        }

        if (unitDatabase == null)
        {
            Debug.LogWarning("[SummonManager] UnitDatabase를 찾을 수 없습니다. 런타임 자동 생성합니다.");
            AutoBuildDatabase();
        }

        // 인벤토리 슬롯 자동 탐색
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            AutoFindInventorySlots();
        }
    }

    private void AutoFindInventorySlots()
    {
        MapGenerator map = FindObjectOfType<MapGenerator>();
        if (map != null && map.generatedInventorySlots != null && map.generatedInventorySlots.Length > 0)
        {
            inventorySlots = map.generatedInventorySlots;
            if (inventoryParent == null) inventoryParent = transform;
            Debug.Log($"[SummonManager] MapGenerator에서 인벤토리 슬롯 {inventorySlots.Length}개 자동 연결");
        }
    }

    /// <summary>
    /// 위스프를 사용하여 랜덤 흔함 유닛 소환
    /// </summary>
    public bool SummonWithWisp()
    {
        if (GameManager.Instance == null) return false;
        if (!GameManager.Instance.UseWisp()) 
        {
            Debug.Log("[SummonManager] 위스프가 부족합니다!");
            return false;
        }

        return SummonRandomCommonUnit();
    }

    /// <summary>
    /// 랜덤 흔함 유닛 소환 (내부)
    /// </summary>
    private bool SummonRandomCommonUnit()
    {
        if (inventoryUnits.Count >= maxInventorySize)
        {
            Debug.Log("[SummonManager] 인벤토리가 가득 찼습니다!");
            // 위스프 반환
            if (GameManager.Instance != null) GameManager.Instance.AddWisps(1);
            return false;
        }

        if (unitDatabase == null)
        {
            Debug.LogError("[SummonManager] UnitDatabase가 없습니다.");
            return false;
        }

        UnitData data = unitDatabase.GetRandomCommonUnit();
        if (data == null)
        {
            Debug.LogError("[SummonManager] 소환 가능한 흔함 유닛이 없습니다.");
            return false;
        }

        // 유닛 생성
        GameObject unitObj = CreateUnitObject(data);
        if (unitObj == null) return false;

        // 인벤토리 슬롯에 배치
        Vector3 slotPos = GetEmptySlotPosition();
        unitObj.transform.position = slotPos;

        UnitController unit = unitObj.GetComponent<UnitController>();
        if (unit == null) unit = unitObj.AddComponent<UnitController>();
        unit.Initialize(data, UnitPlacement.Inventory);

        inventoryUnits.Add(unit);

        Debug.Log($"[SummonManager] '{data.unitName}' 소환! ({data.grade}) 인벤토리: {inventoryUnits.Count}/{maxInventorySize}");

        // 자동 조합 체크
        if (CombineManager.Instance != null)
            CombineManager.Instance.CheckAutoCombine(unit);

        return true;
    }

    /// <summary>
    /// 유닛 오브젝트 생성
    /// </summary>
    private GameObject CreateUnitObject(UnitData data)
    {
        GameObject unitObj;

        if (data.prefab != null)
        {
            unitObj = Instantiate(data.prefab, Vector3.zero, Quaternion.identity, inventoryParent);
            unitObj.SetActive(true);
        }
        else
        {
            // 프리팹 없으면 동적 생성 (등급별 형태)
            unitObj = CreateTempUnitPrefab(data.grade);
            unitObj.transform.SetParent(inventoryParent);
        }

        unitObj.name = $"Unit_{data.unitName}";
        return unitObj;
    }

    /// <summary>
    /// 임시 유닛 프리팹 생성 (등급별 형태/색상)
    /// </summary>
    private GameObject CreateTempUnitPrefab(UnitGrade grade)
    {
        PrimitiveType shape;
        float scale;

        switch (grade)
        {
            case UnitGrade.Common:
                shape = PrimitiveType.Cube; scale = 0.5f; break;
            case UnitGrade.Uncommon:
                shape = PrimitiveType.Cube; scale = 0.6f; break;
            case UnitGrade.Special:
                shape = PrimitiveType.Cylinder; scale = 0.5f; break;
            case UnitGrade.Rare:
                shape = PrimitiveType.Capsule; scale = 0.5f; break;
            case UnitGrade.Legendary:
            case UnitGrade.Hidden:
                shape = PrimitiveType.Capsule; scale = 0.7f; break;
            default:
                shape = PrimitiveType.Capsule; scale = 0.8f; break;
        }

        GameObject obj = GameObject.CreatePrimitive(shape);
        obj.transform.localScale = Vector3.one * scale;

        // UnitController 추가
        if (obj.GetComponent<UnitController>() == null)
            obj.AddComponent<UnitController>();

        return obj;
    }

    /// <summary>
    /// 빈 인벤토리 슬롯 위치 반환
    /// </summary>
    private Vector3 GetEmptySlotPosition()
    {
        if (inventorySlots != null && inventorySlots.Length > 0)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                bool occupied = inventoryUnits.Any(u => u != null &&
                    Vector3.Distance(u.transform.position, inventorySlots[i].position) < 0.5f);
                if (!occupied) return inventorySlots[i].position;
            }
        }

        // 슬롯이 없으면 그리드 배치
        int index = inventoryUnits.Count;
        int col = index % 4;
        int row = index / 4;
        return new Vector3(col * 1.2f - 1.8f, 0.5f, row * 1.2f - 8f);
    }

    /// <summary>
    /// 유닛을 인벤토리에서 제거
    /// </summary>
    public void RemoveFromInventory(UnitController unit)
    {
        inventoryUnits.Remove(unit);
    }

    /// <summary>
    /// 유닛을 필드에 추가
    /// </summary>
    public void AddToField(UnitController unit)
    {
        if (!fieldUnits.Contains(unit))
            fieldUnits.Add(unit);
    }

    /// <summary>
    /// 유닛을 필드에서 제거
    /// </summary>
    public void RemoveFromField(UnitController unit)
    {
        fieldUnits.Remove(unit);
    }

    /// <summary>
    /// 인벤토리 초기화
    /// </summary>
    public void ResetInventory()
    {
        foreach (var unit in inventoryUnits)
        {
            if (unit != null) Destroy(unit.gameObject);
        }
        foreach (var unit in fieldUnits)
        {
            if (unit != null) Destroy(unit.gameObject);
        }
        inventoryUnits.Clear();
        fieldUnits.Clear();
    }

    /// <summary>
    /// 같은 유닛 목록 반환 (조합용)
    /// </summary>
    public List<UnitController> GetSameUnits(UnitData data)
    {
        return inventoryUnits.Where(u => u != null && u.UnitData != null &&
            u.UnitData.unitName == data.unitName && u.UnitData.grade == data.grade).ToList();
    }

    /// <summary>
    /// DB 자동 구성 (Resources에 없을 때)
    /// </summary>
    private void AutoBuildDatabase()
    {
        unitDatabase = ScriptableObject.CreateInstance<UnitDatabase>();

        // 원피스 흔함 유닛 14종
        string[] commonNames = {
            "루피", "조로", "나미", "우솝", "상디",
            "쵸파", "로빈", "프랑키", "브룩", "에이스",
            "마르코", "킨에몬", "로우", "키드"
        };

        List<UnitData> allUnits = new List<UnitData>();
        List<UnitData> commonPool = new List<UnitData>();

        for (int i = 0; i < commonNames.Length; i++)
        {
            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.unitID = i + 1;
            data.unitName = commonNames[i];
            data.displayName = commonNames[i];
            data.grade = UnitGrade.Common;
            data.attackDamage = 8f + i * 0.5f;
            data.attackSpeed = 1f;
            data.attackRange = 3f;
            data.combineCount = 3;
            allUnits.Add(data);
            commonPool.Add(data);
        }

        unitDatabase.allUnits = allUnits.ToArray();
        unitDatabase.commonPool = commonPool.ToArray();

        Debug.Log($"[SummonManager] UnitDatabase 자동 구성 완료: {allUnits.Count}개 유닛 등록");
    }
}
