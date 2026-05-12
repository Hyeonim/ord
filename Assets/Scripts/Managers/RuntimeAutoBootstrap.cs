using UnityEngine;

/// <summary>
/// 런타임 자동 부트스트랩.
/// [RuntimeInitializeOnLoadMethod]를 사용하여 씬에 GameBootstrap이 없으면 자동 생성.
/// 빌드 환경에서도 동작한다.
/// </summary>
public static class RuntimeAutoBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        // GameBootstrap이 씬에 없으면 자동 생성
        if (Object.FindObjectOfType<GameBootstrap>() == null)
        {
            GameObject bootstrapObj = new GameObject("GameBootstrap (Auto)");
            bootstrapObj.AddComponent<GameBootstrap>();
            Object.DontDestroyOnLoad(bootstrapObj);
            Debug.Log("[RuntimeAutoBootstrap] GameBootstrap 자동 생성 완료");
        }
    }
}
