using UnityEngine;

/// <summary>
/// 유닛의 드래그 앤 드롭 이동을 관리하는 시스템.
/// - 인벤토리 → 필드 배치
/// - 필드 → 필드 위치 이동
/// - 유닛 간 위치 교환(Swap)
/// - PC(마우스) + 모바일(터치) 동시 지원
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
    private bool isDragging = false;
    private Camera mainCamera;
    private int activeTouchId = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // 모바일 터치 우선 처리
        if (Input.touchCount > 0)
            HandleTouch();
        else
            HandleMouse();
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            TryPickUp(Input.mousePosition);
        else if (Input.GetMouseButton(0) && isDragging)
            DragUnit(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0) && isDragging)
            DropUnit(Input.mousePosition);
    }

    private void HandleTouch()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (!isDragging)
                    {
                        TryPickUp(touch.position);
                        if (isDragging) activeTouchId = touch.fingerId;
                    }
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDragging && touch.fingerId == activeTouchId)
                        DragUnit(touch.position);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (isDragging && touch.fingerId == activeTouchId)
                    {
                        DropUnit(touch.position);
                        activeTouchId = -1;
                    }
                    break;
            }
        }
    }

    private void TryPickUp(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
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

                Vector3 pos = draggedUnit.transform.position;
                pos.y = dragHeight;
                draggedUnit.transform.position = pos;

                Debug.Log($"[DragDrop] {unit.UnitData.unitName} 집어올림");
            }
        }
    }

    private void DragUnit(Vector2 screenPos)
    {
        if (draggedUnit == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            worldPos.y = dragHeight;
            draggedUnit.transform.position = worldPos;
        }
    }

    private void DropUnit(Vector2 screenPos)
    {
        if (draggedUnit == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
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
        if (SummonManager.Instance != null)
        {
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
            if (SummonManager.Instance != null)
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
