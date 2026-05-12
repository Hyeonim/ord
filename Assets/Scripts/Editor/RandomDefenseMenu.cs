using UnityEngine;
using UnityEditor;

/// <summary>
/// 에디터 상단 메뉴 "RandomDefense" 복원.
/// 씬 세팅, 데이터 생성, 테스트 등의 에디터 유틸리티를 제공한다.
/// </summary>
public static class RandomDefenseMenu
{
    [MenuItem("RandomDefense/씬 세팅 (GameBootstrap 추가)")]
    private static void SetupScene()
    {
        if (Object.FindObjectOfType<GameBootstrap>() != null)
        {
            Debug.Log("[RandomDefense] GameBootstrap이 이미 씬에 있습니다.");
            return;
        }

        GameObject bootstrapObj = new GameObject("GameBootstrap");
        bootstrapObj.AddComponent<GameBootstrap>();
        Debug.Log("[RandomDefense] GameBootstrap을 씬에 추가했습니다. Play를 누르면 자동으로 게임이 시작됩니다.");
        EditorUtility.SetDirty(bootstrapObj);
    }

    [MenuItem("RandomDefense/테스트 플레이")]
    private static void TestPlay()
    {
        SetupScene();
        EditorApplication.isPlaying = true;
    }

    [MenuItem("RandomDefense/유닛 데이터 생성/흔함 유닛 14종 생성")]
    private static void CreateCommonUnits()
    {
        string path = "Assets/ScriptableObjects/Units/";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Units");
        }

        string[] names = { "루피", "조로", "나미", "우솝", "상디", "쵸파", "로빈",
                          "프랑키", "브룩", "에이스", "마르코", "킨에몬", "로우", "키드" };

        for (int i = 0; i < names.Length; i++)
        {
            string assetPath = $"{path}Unit_{names[i]}_견습.asset";
            if (AssetDatabase.LoadAssetAtPath<UnitData>(assetPath) != null) continue;

            UnitData data = ScriptableObject.CreateInstance<UnitData>();
            data.unitID = i + 1;
            data.unitName = $"{names[i]}_견습";
            data.displayName = $"{names[i]} (견습)";
            data.grade = UnitGrade.Common;
            data.attackDamage = 8f + i * 0.5f;
            data.attackSpeed = 1f;
            data.attackRange = 3f;
            data.combineCount = 3;

            AssetDatabase.CreateAsset(data, assetPath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[RandomDefense] 흔함 유닛 14종 생성 완료!");
    }

    [MenuItem("RandomDefense/유닛 데이터 생성/UnitDatabase 재구성")]
    private static void RebuildUnitDatabase()
    {
        string dbPath = "Assets/ScriptableObjects/UnitDatabase.asset";
        UnitDatabase db = AssetDatabase.LoadAssetAtPath<UnitDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<UnitDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }

        string[] guids = AssetDatabase.FindAssets("t:UnitData", new[] { "Assets/ScriptableObjects/Units" });
        UnitData[] allUnits = new UnitData[guids.Length];
        System.Collections.Generic.List<UnitData> commonPool = new System.Collections.Generic.List<UnitData>();

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            allUnits[i] = AssetDatabase.LoadAssetAtPath<UnitData>(assetPath);
            if (allUnits[i] != null && allUnits[i].grade == UnitGrade.Common)
                commonPool.Add(allUnits[i]);
        }

        db.allUnits = allUnits;
        db.commonPool = commonPool.Count > 0 ? commonPool.ToArray() : allUnits;

        // 조합법도 로드
        string[] recipeGuids = AssetDatabase.FindAssets("t:CombineRecipe", new[] { "Assets/ScriptableObjects" });
        CombineRecipe[] recipes = new CombineRecipe[recipeGuids.Length];
        for (int i = 0; i < recipeGuids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(recipeGuids[i]);
            recipes[i] = AssetDatabase.LoadAssetAtPath<CombineRecipe>(assetPath);
        }
        db.allRecipes = recipes;

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        // Resources에도 복사
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        AssetDatabase.CopyAsset(dbPath, "Assets/Resources/UnitDatabase.asset");

        Debug.Log($"[RandomDefense] UnitDatabase 재구성 완료! 유닛 {allUnits.Length}개, 조합법 {recipes.Length}개");
    }

    [MenuItem("RandomDefense/적 데이터 생성/기본 적 5종 생성")]
    private static void CreateEnemies()
    {
        string path = "Assets/ScriptableObjects/Enemies/";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Enemies");
        }

        string[] names = { "해적_졸병", "해적_부대장", "해적_간부", "해군_병사", "해군_장교" };
        float[] hps = { 100f, 200f, 400f, 150f, 300f };
        float[] speeds = { 2f, 1.8f, 1.5f, 2.2f, 1.7f };
        int[] golds = { 5, 10, 20, 8, 15 };

        for (int i = 0; i < names.Length; i++)
        {
            string assetPath = $"{path}Enemy_{names[i]}.asset";
            if (AssetDatabase.LoadAssetAtPath<EnemyData>(assetPath) != null) continue;

            EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
            data.enemyName = names[i];
            data.maxHP = hps[i];
            data.moveSpeed = speeds[i];
            data.goldReward = golds[i];

            AssetDatabase.CreateAsset(data, assetPath);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[RandomDefense] 기본 적 5종 생성 완료!");
    }

    [MenuItem("RandomDefense/빌드 설정/모바일 (Android)")]
    private static void SetAndroidBuild()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.ord.onepiece.randomdefense");
        PlayerSettings.productName = "원피스 랜덤 디펜스";
        Debug.Log("[RandomDefense] Android 빌드 설정 완료!");
    }

    [MenuItem("RandomDefense/빌드 설정/모바일 (iOS)")]
    private static void SetIOSBuild()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.ord.onepiece.randomdefense");
        PlayerSettings.productName = "원피스 랜덤 디펜스";
        Debug.Log("[RandomDefense] iOS 빌드 설정 완료!");
    }

    [MenuItem("RandomDefense/정보")]
    private static void ShowInfo()
    {
        EditorUtility.DisplayDialog("원피스 랜덤 디펜스",
            "원피스 랜덤 디펜스 모바일 버전\n\n" +
            "- 75라운드 생존\n" +
            "- 위스프 소환 + 3합 조합\n" +
            "- 히든/전설/초월 특수 조합\n" +
            "- 모바일 터치 최적화\n\n" +
            "디버그 키:\n" +
            "S=소환, G=골드+100, W=목재+3\n" +
            "N=웨이브스킵, 1/2/3=속도, R=재시작",
            "확인");
    }
}
