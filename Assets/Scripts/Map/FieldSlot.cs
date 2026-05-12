using UnityEngine;

/// <summary>
/// 필드의 개별 배치 슬롯을 나타낸다.
/// 유닛이 이 슬롯 위에 배치될 수 있다.
/// </summary>
public class FieldSlot : MonoBehaviour
{
    [Header("상태")]
    public bool isOccupied = false;
    public UnitController occupiedUnit;

    [Header("시각 설정")]
    public Color emptyColor = new Color(0.3f, 0.8f, 0.3f, 0.5f);
    public Color occupiedColor = new Color(0.8f, 0.3f, 0.3f, 0.5f);
    public Color highlightColor = new Color(1f, 1f, 0.3f, 0.7f);

    private Renderer slotRenderer;
    private bool isHighlighted = false;

    private void Awake()
    {
        slotRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    /// <summary>
    /// 유닛을 이 슬롯에 배치한다.
    /// </summary>
    public void PlaceUnit(UnitController unit)
    {
        occupiedUnit = unit;
        isOccupied = true;
        UpdateVisual();
    }

    /// <summary>
    /// 슬롯에서 유닛을 제거한다.
    /// </summary>
    public void RemoveUnit()
    {
        occupiedUnit = null;
        isOccupied = false;
        UpdateVisual();
    }

    /// <summary>
    /// 하이라이트 표시 (드래그 중 호버 시).
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (slotRenderer == null) return;

        if (isHighlighted)
            slotRenderer.material.color = highlightColor;
        else if (isOccupied)
            slotRenderer.material.color = occupiedColor;
        else
            slotRenderer.material.color = emptyColor;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.1f, 1f));
    }
}
