using UnityEngine;
using UnityEditor;

/// <summary>
/// 에디터에서 씬 실행 시 GameBootstrap이 없으면 자동 추가.
/// </summary>
[InitializeOnLoad]
public static class AutoSceneSetup
{
    static AutoSceneSetup()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            if (Object.FindObjectOfType<GameBootstrap>() == null)
            {
                GameObject bootstrapObj = new GameObject("GameBootstrap");
                bootstrapObj.AddComponent<GameBootstrap>();
                Debug.Log("[AutoSceneSetup] GameBootstrap이 씬에 없어서 자동 생성했습니다.");
            }
        }
    }
}
