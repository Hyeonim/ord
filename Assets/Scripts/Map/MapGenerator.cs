using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 맵 자동 생성.
/// 디펜스 라인(경로), 유닛 배치 슬롯, 인벤토리 영역을 자동으로 구성한다.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("맵 크기")]
    [SerializeField] private float mapWidth = 20f;
    [SerializeField] private float mapHeight = 30f;

    [Header("경로 설정")]
    [SerializeField] private int waypointCount = 6;

    [Header("필드 슬롯 설정")]
    [SerializeField] private int fieldSlotRows = 3;
    [SerializeField] private int fieldSlotCols = 4;
    [SerializeField] private float slotSpacing = 2f;

    [Header("인벤토리 슬롯 설정")]
    [SerializeField] private int inventorySlotRows = 3;
    [SerializeField] private int inventoryCols = 4;

    [Header("생성된 오브젝트")]
    public WaypointPath generatedPath;
    public FieldSlot[] generatedFieldSlots;
    public Transform[] generatedInventorySlots;
    public Transform enemyParent;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // 바닥 생성
        CreateGround();

        // 적 경로 생성
        CreateEnemyPath();

        // 필드 슬롯 생성 (유닛 배치 영역)
        CreateFieldSlots();

        // 인벤토리 슬롯 생성
        CreateInventorySlots();

        // EnemyParent 생성
        GameObject ep = new GameObject("EnemyParent");
        ep.transform.SetParent(transform);
        enemyParent = ep.transform;

        // WaveManager에 연결
        ConnectToManagers();

        Debug.Log("[MapGenerator] 맵 생성 완료!");
    }

    private void CreateGround()
    {
        // 메인 필드 바닥
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground_Field";
        ground.transform.SetParent(transform);
        ground.transform.position = new Vector3(0, -0.5f, 0);
        ground.transform.localScale = new Vector3(mapWidth, 1f, mapHeight);
        ground.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.5f, 0.2f);

        // 인벤토리 영역 바닥
        GameObject invGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
        invGround.name = "Ground_Inventory";
        invGround.transform.SetParent(transform);
        invGround.transform.position = new Vector3(0, -0.5f, -mapHeight / 2 - 5f);
        invGround.transform.localScale = new Vector3(mapWidth, 1f, 8f);
        invGround.GetComponent<Renderer>().sharedMaterial.color = new Color(0.3f, 0.3f, 0.4f);
    }

    private void CreateEnemyPath()
    {
        GameObject pathObj = new GameObject("EnemyPath");
        pathObj.transform.SetParent(transform);
        WaypointPath wp = pathObj.AddComponent<WaypointPath>();

        List<Transform> waypoints = new List<Transform>();

        // S자 경로 생성
        float pathStartZ = mapHeight / 2 - 2f;
        float pathEndZ = -mapHeight / 2 + 2f;
        float zigzagWidth = mapWidth * 0.3f;

        for (int i = 0; i < waypointCount; i++)
        {
            GameObject point = new GameObject($"Waypoint_{i}");
            point.transform.SetParent(pathObj.transform);

            float t = (float)i / (waypointCount - 1);
            float z = Mathf.Lerp(pathStartZ, pathEndZ, t);
            float x = (i % 2 == 0) ? -zigzagWidth : zigzagWidth;
            if (i == 0 || i == waypointCount - 1) x = 0; // 시작/끝은 중앙

            point.transform.position = new Vector3(x, 0.1f, z);
            waypoints.Add(point.transform);
        }

        wp.waypoints = waypoints.ToArray();
        generatedPath = wp;
    }

    private void CreateFieldSlots()
    {
        List<FieldSlot> slots = new List<FieldSlot>();
        float startX = -(fieldSlotCols - 1) * slotSpacing / 2f;
        float startZ = 2f;

        for (int row = 0; row < fieldSlotRows; row++)
        {
            for (int col = 0; col < fieldSlotCols; col++)
            {
                GameObject slotObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                slotObj.name = $"FieldSlot_{row}_{col}";
                slotObj.transform.SetParent(transform);
                slotObj.transform.position = new Vector3(
                    startX + col * slotSpacing,
                    0.01f,
                    startZ + row * slotSpacing
                );
                slotObj.transform.rotation = Quaternion.Euler(90, 0, 0);
                slotObj.transform.localScale = Vector3.one * (slotSpacing * 0.8f);

                FieldSlot slot = slotObj.AddComponent<FieldSlot>();
                slot.slotIndex = row * fieldSlotCols + col;

                Renderer rend = slotObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.3f, 0.8f, 0.3f, 0.5f);
                    rend.material = mat;
                }

                slots.Add(slot);
            }
        }

        generatedFieldSlots = slots.ToArray();
    }

    private void CreateInventorySlots()
    {
        List<Transform> slots = new List<Transform>();
        float startX = -(inventoryCols - 1) * 1.5f / 2f;
        float startZ = -mapHeight / 2 - 3f;

        for (int row = 0; row < inventorySlotRows; row++)
        {
            for (int col = 0; col < inventoryCols; col++)
            {
                GameObject slotObj = new GameObject($"InvSlot_{row}_{col}");
                slotObj.transform.SetParent(transform);
                slotObj.transform.position = new Vector3(
                    startX + col * 1.5f,
                    0.5f,
                    startZ + row * 1.5f
                );
                slots.Add(slotObj.transform);
            }
        }

        generatedInventorySlots = slots.ToArray();
    }

    private void ConnectToManagers()
    {
        // WaveManager 연결
        WaveManager wm = FindObjectOfType<WaveManager>();
        if (wm != null)
        {
            wm.enemyPath = generatedPath;
            wm.enemyParent = enemyParent;
        }

        // SummonManager 연결
        SummonManager sm = FindObjectOfType<SummonManager>();
        if (sm != null)
        {
            sm.SetInventorySlots(generatedInventorySlots);
        }
    }
}
