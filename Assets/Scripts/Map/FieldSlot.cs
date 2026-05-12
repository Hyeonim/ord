using UnityEngine;

/// <summary>
/// 유닛을 배치할 수 있는 필드 슬롯.
/// 터치/클릭으로 유닛을 배치하거나 교환한다.
/// </summary>
public class FieldSlot : MonoBehaviour
{
    [Header("설정")]
    public int slotIndex;
    public bool isOccupied = false;

    [Header("배치된 유닛")]
    public UnitController placedUnit;

    [Header("시각")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.8f, 0.3f, 0.5f);
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.7f);
    [SerializeField] private Color occupiedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private Renderer slotRenderer;

    private void Awake()
    {
        slotRenderer = GetComponent<Renderer>();
        UpdateVisual();
    }

    public bool PlaceUnit(UnitController unit)
    {
        if (isOccupied) return false;

        placedUnit = unit;
        isOccupied = true;
        unit.PlaceOnField(transform.position + Vector3.up * 0.5f);
        UpdateVisual();
        return true;
    }

    public UnitController RemoveUnit()
    {
        if (!isOccupied || placedUnit == null) return null;

        UnitController unit = placedUnit;
        placedUnit = null;
        isOccupied = false;
        UpdateVisual();
        return unit;
    }

    public void SwapUnit(FieldSlot otherSlot)
    {
        if (otherSlot == null) return;

        UnitController temp = placedUnit;
        Vector3 tempPos = transform.position + Vector3.up * 0.5f;
        Vector3 otherPos = otherSlot.transform.position + Vector3.up * 0.5f;

        placedUnit = otherSlot.placedUnit;
        otherSlot.placedUnit = temp;

        if (placedUnit != null) placedUnit.transform.position = tempPos;
        if (otherSlot.placedUnit != null) otherSlot.placedUnit.transform.position = otherPos;

        isOccupied = placedUnit != null;
        otherSlot.isOccupied = otherSlot.placedUnit != null;

        UpdateVisual();
        otherSlot.UpdateVisual();
    }

    public void Highlight(bool on)
    {
        if (slotRenderer == null) return;
        slotRenderer.material.color = on ? highlightColor : (isOccupied ? occupiedColor : normalColor);
    }

    private void UpdateVisual()
    {
        if (slotRenderer == null) return;
        slotRenderer.material.color = isOccupied ? occupiedColor : normalColor;
    }
}
