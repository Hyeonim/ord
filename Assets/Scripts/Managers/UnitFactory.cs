using UnityEngine;

/// <summary>
/// 테스트용 유닛 프리팹을 런타임에 동적으로 생성하는 팩토리.
/// ScriptableObject에 프리팹이 할당되지 않았을 때 사용된다.
/// </summary>
public class UnitFactory : MonoBehaviour
{
    public static UnitFactory Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 등급에 따른 임시 유닛 프리팹을 생성한다.
    /// </summary>
    public static GameObject CreateTempUnitPrefab(UnitGrade grade)
    {
        GameObject unit;

        switch (grade)
        {
            case UnitGrade.Common:
                unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                unit.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                break;
            case UnitGrade.Uncommon:
                unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                unit.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                break;
            case UnitGrade.Rare:
                unit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                unit.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
                break;
            case UnitGrade.Epic:
                unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                unit.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                break;
            case UnitGrade.Legendary:
                unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                unit.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                break;
            default:
                unit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
        }

        // UnitController 추가
        if (unit.GetComponent<UnitController>() == null)
        {
            unit.AddComponent<UnitController>();
        }

        // 레이어 설정 (드래그용)
        unit.layer = LayerMask.NameToLayer("Default");

        return unit;
    }

    /// <summary>
    /// 임시 적 프리팹을 생성한다.
    /// </summary>
    public static GameObject CreateTempEnemyPrefab()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemy.transform.localScale = Vector3.one * 0.8f;
        enemy.GetComponent<Renderer>().sharedMaterial.color = new Color(1f, 0.5f, 0f);

        if (enemy.GetComponent<EnemyController>() == null)
        {
            enemy.AddComponent<EnemyController>();
        }

        return enemy;
    }
}
