using UnityEngine;

/// <summary>
/// 유닛의 드래그 앤 드롭 이동을 관리하는 시스템.
/// - 인벤토리 → 필드 배치
/// - 필드 → 필드 위치 이동
/// - 유닛 간 위치 교환(Swap)
/// </summary>
public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance { get; private set; }

    [Header("설정")]
    public LayerMask fieldLayer;        // 필드 영역 레이어
    public LayerMask unitLayer;         // 유닛 레이어
    public LayerMask inventoryLayer;    // 인벤토리 영역 레이어
    public float dragHeight = 1.5f;     // 드래그 중 유닛 높이

    [Header("상태")]
    private UnitController draggedUnit;
    private Vector3 dragOffset;
    private bool isDragging = false;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPickUp();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            DragUnit();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            DropUnit();
        }
    }

    /// <summary>
    /// 마우스 클릭 시 유닛을 집어올리려 시도한다.
    /// </summary>
    private void TryPickUp()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, unitLayer))
        {
            UnitController unit = hit.collider.GetComponent<UnitController>();
            if (unit == null) unit = hit.collider.GetComponentInParent<UnitController>();

            if (unit != null)
            {
                draggedUnit = unit;
                draggedUnit.isDragging = true;
                draggedUnit.originalPosition = draggedUnit.transform.position;
                isDragging = true;

                // 드래그 시작 시 약간 위로 올림
                Vector3 pos = draggedUnit.transform.position;
                pos.y = dragHeight;
                draggedUnit.transform.position = pos;

                Debug.Log($"[DragDrop] {unit.UnitData.unitName} 집어올림");
            }
        }
    }

    /// <summary>
    /// 유닛을 마우스 위치에 따라 이동시킨다.
    /// </summary>
    private void DragUnit()
    {
        if (draggedUnit == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.y = dragHeight;
            draggedUnit.transform.position = worldPos;
        }
    }

    /// <summary>
    /// 유닛을 놓을 때의 처리.
    /// - 필드 위에 놓으면 배치
    /// - 다른 유닛 위에 놓으면 위치 교환(Swap)
    /// - 인벤토리 위에 놓으면 되돌리기
    /// </summary>
    private void DropUnit()
    {
        if (draggedUnit == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 1. 다른 유닛 위에 놓았는지 체크 (Swap)
        if (Physics.Raycast(ray, out hit, 100f, unitLayer))
        {
            UnitController otherUnit = hit.collider.GetComponent<UnitController>();
            if (otherUnit == null) otherUnit = hit.collider.GetComponentInParent<UnitController>();

            if (otherUnit != null && otherUnit != draggedUnit)
            {
                SwapUnits(draggedUnit, otherUnit);
                FinishDrag();
                return;
            }
        }

        // 2. 필드 위에 놓았는지 체크
        if (Physics.Raycast(ray, out hit, 100f, fieldLayer))
        {
            PlaceOnField(hit.point);
            FinishDrag();
            return;
        }

        // 3. 그 외 → 원래 위치로 복귀
        ReturnToOriginal();
        FinishDrag();
    }

    /// <summary>
    /// 두 유닛의 위치를 교환한다.
    /// </summary>
    private void SwapUnits(UnitController unitA, UnitController unitB)
    {
        Vector3 posA = unitA.originalPosition;
        Vector3 posB = unitB.transform.position;

        unitA.transform.position = new Vector3(posB.x, 0f, posB.z);
        unitB.transform.position = new Vector3(posA.x, 0f, posA.z);

        // 배치 상태도 교환
        UnitPlacement placementA = unitA.Placement;
        UnitPlacement placementB = unitB.Placement;
        unitA.SetPlacement(placementB);
        unitB.SetPlacement(placementA);

        // 인벤토리 목록 업데이트
        if (placementA == UnitPlacement.Inventory && placementB == UnitPlacement.Field)
        {
            SummonManager.Instance.RemoveFromInventory(unitA);
            SummonManager.Instance.ReturnToInventory(unitB);
        }
        else if (placementA == UnitPlacement.Field && placementB == UnitPlacement.Inventory)
        {
            SummonManager.Instance.RemoveFromInventory(unitB);
            SummonManager.Instance.ReturnToInventory(unitA);
        }

        // 스토리 보스 근처 배치 체크
        CheckStoryBossProximity(unitA);
        CheckStoryBossProximity(unitB);

        Debug.Log($"[DragDrop] {unitA.UnitData.unitName} ↔ {unitB.UnitData.unitName} 위치 교환!");
    }

    /// <summary>
    /// 유닛을 필드에 배치한다.
    /// </summary>
    private void PlaceOnField(Vector3 position)
    {
        position.y = 0f;
        draggedUnit.transform.position = position;

        if (draggedUnit.Placement == UnitPlacement.Inventory)
        {
            SummonManager.Instance.RemoveFromInventory(draggedUnit);
            draggedUnit.SetPlacement(UnitPlacement.Field);
            Debug.Log($"[DragDrop] {draggedUnit.UnitData.unitName} 필드에 배치!");
        }

        // 스토리 보스 근처 배치 체크
        CheckStoryBossProximity(draggedUnit);
    }

    /// <summary>
    /// 유닛을 원래 위치로 되돌린다.
    /// </summary>
    private void ReturnToOriginal()
    {
        draggedUnit.transform.position = draggedUnit.originalPosition;
        Debug.Log($"[DragDrop] {draggedUnit.UnitData.unitName} 원래 위치로 복귀");
    }

    /// <summary>
    /// 드래그 종료 처리.
    /// </summary>
    private void FinishDrag()
    {
        if (draggedUnit != null)
        {
            draggedUnit.isDragging = false;
        }
        draggedUnit = null;
        isDragging = false;
    }

    /// <summary>
    /// 유닛이 스토리 보스 근처에 배치되었는지 체크한다.
    /// </summary>
    private void CheckStoryBossProximity(UnitController unit)
    {
        if (unit.Placement != UnitPlacement.Field) return;

        StoryBoss boss = FindObjectOfType<StoryBoss>();
        if (boss != null && boss.IsAlive)
        {
            float dist = Vector3.Distance(unit.transform.position, boss.transform.position);
            if (dist <= unit.UnitData.attackRange)
            {
                unit.SetStoryBossTarget(boss);
                Debug.Log($"[DragDrop] {unit.UnitData.unitName}이(가) 스토리 보스를 타겟으로 설정!");
            }
            else
            {
                unit.SetStoryBossTarget(null);
            }
        }
    }
}
