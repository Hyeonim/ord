using UnityEngine;

/// <summary>
/// 모바일 터치 기반 드래그 앤 드롭 시스템.
/// 유닛을 인벤토리에서 필드로 배치하거나, 필드 내 위치를 교환한다.
/// </summary>
public class DragDropManager : MonoBehaviour
{
    public static DragDropManager Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private float dragThreshold = 0.3f;

    private UnitController draggedUnit;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private int activeTouchId = -1;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (Input.touchCount > 0)
            HandleTouchInput();
        else
            HandleMouseInput();
    }

    private void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                if (!isDragging) { activeTouchId = touch.fingerId; TryStartDrag(touch.position); }
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (isDragging && touch.fingerId == activeTouchId) UpdateDrag(touch.position);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging && touch.fingerId == activeTouchId) { EndDrag(touch.position); activeTouchId = -1; }
                break;
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) TryStartDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0) && isDragging) UpdateDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0) && isDragging) EndDrag(Input.mousePosition);
    }

    private void TryStartDrag(Vector2 screenPos)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            UnitController unit = hit.collider.GetComponent<UnitController>();
            if (unit != null)
            {
                draggedUnit = unit;
                originalPosition = unit.transform.position;
                isDragging = true;
                draggedUnit.transform.position += Vector3.up * 0.5f;
            }
        }
    }

    private void UpdateDrag(Vector2 screenPos)
    {
        if (draggedUnit == null || mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * 0.5f);
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            draggedUnit.transform.position = worldPos + Vector3.up * 0.3f;
        }
    }

    private void EndDrag(Vector2 screenPos)
    {
        if (draggedUnit == null) { isDragging = false; return; }
        if (mainCamera == null) mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        bool placed = false;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            FieldSlot slot = hit.collider.GetComponent<FieldSlot>();
            if (slot != null)
            {
                if (!slot.isOccupied)
                {
                    slot.PlaceUnit(draggedUnit);
                    placed = true;
                }
                else if (slot.placedUnit != null && slot.placedUnit != draggedUnit)
                {
                    SwapUnits(draggedUnit, slot);
                    placed = true;
                }
            }
        }

        if (!placed) draggedUnit.transform.position = originalPosition;
        draggedUnit = null;
        isDragging = false;
    }

    private void SwapUnits(UnitController unit, FieldSlot targetSlot)
    {
        UnitController existingUnit = targetSlot.placedUnit;
        Vector3 existingPos = existingUnit.transform.position;
        targetSlot.placedUnit = unit;
        unit.transform.position = existingPos;
        existingUnit.transform.position = originalPosition;
    }
}
