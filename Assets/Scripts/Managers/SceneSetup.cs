using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 씬을 자동으로 세팅하는 에디터 유틸리티.
/// 메뉴에서 실행하면 테스트용 씬이 자동 구성된다.
/// 
/// 사용법: Unity 메뉴 → RandomDefense → Setup Test Scene
/// </summary>
public class SceneSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("RandomDefense/Setup Test Scene")]
    public static void SetupTestScene()
    {
        // === 1. 매니저 오브젝트 생성 ===
        GameObject managers = CreateOrFind("--- MANAGERS ---");
        
        // GameManager
        GameObject gmObj = CreateChild(managers, "GameManager");
        gmObj.AddComponent<GameManager>();

        // WaveManager
        GameObject wmObj = CreateChild(managers, "WaveManager");
        wmObj.AddComponent<WaveManager>();

        // SummonManager
        GameObject smObj = CreateChild(managers, "SummonManager");
        smObj.AddComponent<SummonManager>();

        // DragDropManager
        GameObject ddObj = CreateChild(managers, "DragDropManager");
        ddObj.AddComponent<DragDropManager>();

        // StoryModeManager
        GameObject stObj = CreateChild(managers, "StoryModeManager");
        stObj.AddComponent<StoryModeManager>();

        // === 2. 맵 구성 ===
        GameObject map = CreateOrFind("--- MAP ---");

        // 바닥 (필드)
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Field";
        ground.transform.parent = map.transform;
        ground.transform.localScale = new Vector3(3f, 1f, 3f);
        ground.transform.position = Vector3.zero;
        ground.layer = LayerMask.NameToLayer("Default");
        ground.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.4f, 0.2f);

        // 웨이포인트 경로 (사각형 루프)
        GameObject pathObj = CreateChild(map, "WaypointPath");
        WaypointPath path = pathObj.AddComponent<WaypointPath>();

        // 사각형 모서리 웨이포인트 생성 (11시→7시→5시→1시 순환)
        Vector3[] waypointPositions = new Vector3[]
        {
            new Vector3(-12f, 0f, 12f),   // 11시 (좌상)
            new Vector3(-12f, 0f, -12f),  // 7시 (좌하)
            new Vector3(12f, 0f, -12f),   // 5시 (우하)
            new Vector3(12f, 0f, 12f),    // 1시 (우상)
        };

        Transform[] waypoints = new Transform[waypointPositions.Length];
        for (int i = 0; i < waypointPositions.Length; i++)
        {
            GameObject wp = new GameObject($"Waypoint_{i}");
            wp.transform.parent = pathObj.transform;
            wp.transform.position = waypointPositions[i];
            waypoints[i] = wp.transform;

            // 시각적 마커 (작은 구)
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Marker";
            marker.transform.parent = wp.transform;
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * 0.5f;
            marker.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }
        path.waypoints = waypoints;
        path.isLoop = true;

        // WaveManager에 경로 연결
        WaveManager wm = wmObj.GetComponent<WaveManager>();
        wm.enemyPath = path;
        GameObject enemyParent = CreateChild(map, "EnemyParent");
        wm.enemyParent = enemyParent.transform;

        // === 3. 인벤토리(대기석) 영역 ===
        GameObject inventory = CreateOrFind("--- INVENTORY ---");
        inventory.transform.position = new Vector3(0f, 0f, -18f);

        // 인벤토리 바닥
        GameObject invGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
        invGround.name = "InventoryGround";
        invGround.transform.parent = inventory.transform;
        invGround.transform.localScale = new Vector3(20f, 0.2f, 4f);
        invGround.transform.localPosition = Vector3.zero;
        invGround.GetComponent<Renderer>().sharedMaterial.color = new Color(0.3f, 0.3f, 0.5f);

        // 인벤토리 슬롯 생성
        Transform[] invSlots = new Transform[12];
        for (int i = 0; i < 12; i++)
        {
            GameObject slot = new GameObject($"InvSlot_{i}");
            slot.transform.parent = inventory.transform;
            float x = -8f + (i % 6) * 3f;
            float z = (i < 6) ? -17f : -19f;
            slot.transform.position = new Vector3(x, 0.2f, z);
            invSlots[i] = slot.transform;
        }

        // SummonManager에 인벤토리 연결
        SummonManager sm = smObj.GetComponent<SummonManager>();
        sm.inventoryParent = inventory.transform;
        sm.inventorySlots = invSlots;

        // === 4. 스토리 보스 ===
        GameObject bossObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bossObj.name = "StoryBoss";
        bossObj.transform.parent = map.transform;
        bossObj.transform.position = Vector3.zero; // 맵 중앙
        bossObj.transform.localScale = new Vector3(3f, 3f, 3f);
        bossObj.GetComponent<Renderer>().sharedMaterial.color = new Color(0.8f, 0f, 0f);
        StoryBoss boss = bossObj.AddComponent<StoryBoss>();

        // StoryModeManager에 보스 연결
        StoryModeManager stm = stObj.GetComponent<StoryModeManager>();
        stm.storyBoss = boss;
        stm.bossSpawnPoint = bossObj.transform;

        // === 5. 기본 적 프리팹 (임시) ===
        GameObject enemyPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemyPrefab.name = "EnemyPrefab_Temp";
        enemyPrefab.transform.localScale = Vector3.one * 0.8f;
        enemyPrefab.AddComponent<EnemyController>();
        wm.defaultEnemyPrefab = enemyPrefab;
        enemyPrefab.SetActive(false); // 프리팹으로 사용

        // === 6. 카메라 설정 ===
        Camera.main.transform.position = new Vector3(0f, 30f, -15f);
        Camera.main.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        Camera.main.orthographic = false;
        Camera.main.fieldOfView = 60f;

        Debug.Log("[SceneSetup] 테스트 씬 세팅 완료!");
        Debug.Log("디버그 키: G=골드+100, S=소환, N=웨이브스킵, 1/2/3=속도, R=재시작");
    }

    private static GameObject CreateOrFind(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
        }
        return obj;
    }

    private static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.parent = parent.transform;
        return obj;
    }
#endif
}
