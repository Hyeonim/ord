using UnityEngine;

/// <summary>
/// 모바일 전용 입력 관리.
/// 핀치 줌, 더블탭 소환, 롱프레스 유닛 정보 등을 처리한다.
/// </summary>
public class MobileInputManager : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float doubleTapTime = 0.3f;
    [SerializeField] private float longPressTime = 0.5f;
    [SerializeField] private float pinchZoomSpeed = 0.05f;
    [SerializeField] private float minZoom = 20f;
    [SerializeField] private float maxZoom = 60f;

    private float lastTapTime = 0f;
    private float touchStartTime = 0f;
    private bool isLongPress = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        // 모바일 프레임레이트 설정
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Update()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // 핀치 줌 (2손가락)
        if (Input.touchCount == 2)
        {
            HandlePinchZoom();
        }
        // 단일 터치
        else if (Input.touchCount == 1)
        {
            HandleSingleTouch();
        }
    }

    private void HandlePinchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 prevPos0 = touch0.position - touch0.deltaPosition;
        Vector2 prevPos1 = touch1.position - touch1.deltaPosition;

        float prevDist = (prevPos0 - prevPos1).magnitude;
        float currentDist = (touch0.position - touch1.position).magnitude;
        float diff = prevDist - currentDist;

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Clamp(
                mainCamera.fieldOfView + diff * pinchZoomSpeed,
                minZoom, maxZoom
            );
        }
    }

    private void HandleSingleTouch()
    {
        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartTime = Time.time;
                isLongPress = false;
                break;

            case TouchPhase.Stationary:
                if (!isLongPress && Time.time - touchStartTime >= longPressTime)
                {
                    isLongPress = true;
                    OnLongPress(touch.position);
                }
                break;

            case TouchPhase.Ended:
                if (!isLongPress)
                {
                    float timeSinceLastTap = Time.time - lastTapTime;
                    if (timeSinceLastTap <= doubleTapTime)
                    {
                        OnDoubleTap(touch.position);
                    }
                    lastTapTime = Time.time;
                }
                break;
        }
    }

    private void OnDoubleTap(Vector2 screenPos)
    {
        // 더블탭: 빈 공간이면 소환, 유닛이면 정보 표시
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            UnitController unit = hit.collider.GetComponent<UnitController>();
            if (unit != null)
            {
                // 유닛 정보 표시
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification($"{unit.UnitData.unitName} ({unit.UnitData.grade}) ATK:{unit.UnitData.attackDamage}");
            }
        }
    }

    private void OnLongPress(Vector2 screenPos)
    {
        // 롱프레스: 유닛 상세 정보
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            UnitController unit = hit.collider.GetComponent<UnitController>();
            if (unit != null && unit.UnitData != null)
            {
                string info = $"{unit.UnitData.displayName}\n등급: {unit.UnitData.grade}\n공격력: {unit.UnitData.attackDamage}\n공속: {unit.UnitData.attackSpeed}\n사거리: {unit.UnitData.attackRange}";
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowNotification(info, 3f);
            }
        }
    }
}
