using UnityEngine;

/// <summary>
/// 게임 씬 자동 초기화.
/// 필요한 모든 매니저를 자동으로 생성하고 연결한다.
/// 빈 씬에 이 스크립트 하나만 있으면 게임이 동작한다.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("카메라 설정 (모바일 세로)")]
    [SerializeField] private bool autoSetupCamera = true;
    [SerializeField] private float cameraHeight = 25f;
    [SerializeField] private float cameraAngle = 60f;

    private void Awake()
    {
        Debug.Log("[GameBootstrap] 게임 초기화 시작...");

        // 1. 필수 매니저 생성
        EnsureManager<GameManager>("GameManager");
        EnsureManager<SummonManager>("SummonManager");
        EnsureManager<WaveManager>("WaveManager");
        EnsureManager<CombineManager>("CombineManager");
        EnsureManager<DragDropManager>("DragDropManager");
        EnsureManager<UIManager>("UIManager");
        EnsureManager<MobileInputManager>("MobileInputManager");

        // 2. 맵 생성기
        MapGenerator map = FindObjectOfType<MapGenerator>();
        if (map == null)
        {
            GameObject mapObj = new GameObject("MapGenerator");
            map = mapObj.AddComponent<MapGenerator>();
        }

        // 3. 카메라 설정 (모바일 세로 화면 최적화)
        if (autoSetupCamera)
        {
            SetupCamera();
        }

        // 4. 조명 설정
        SetupLighting();

        Debug.Log("[GameBootstrap] 게임 초기화 완료!");
        Debug.Log("[GameBootstrap] 디버그 키: S=소환, G=골드+100, W=목재+3, N=웨이브스킵, 1/2/3=속도, R=재시작");
    }

    private T EnsureManager<T>(string name) where T : MonoBehaviour
    {
        T existing = FindObjectOfType<T>();
        if (existing != null) return existing;

        GameObject obj = new GameObject(name);
        return obj.AddComponent<T>();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        // 모바일 세로 화면에 맞춘 카메라 위치
        cam.transform.position = new Vector3(0, cameraHeight, -10f);
        cam.transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
        cam.orthographic = false;
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
    }

    private void SetupLighting()
    {
        Light existingLight = FindObjectOfType<Light>();
        if (existingLight != null) return;

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
    }
}
