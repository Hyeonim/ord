using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 테스트용 ScriptableObject 데이터를 자동 생성하는 에디터 유틸리티.
/// 메뉴에서 실행하면 가상의 유닛/적 데이터가 생성된다.
/// 
/// 사용법: Unity 메뉴 → RandomDefense → Generate Test Data
/// </summary>
public class TestDataGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("RandomDefense/Generate Test Data")]
    public static void GenerateTestData()
    {
        // 폴더 확인/생성
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Units"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Units");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Enemies"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Enemies");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Waves"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Waves");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // === 유닛 데이터 생성 ===
        string[] commonNames = { "루피_견습", "조로_견습", "나미_견습", "우솝_견습", "상디_견습" };
        string[] uncommonNames = { "루피_수련", "조로_수련", "나미_수련" };
        string[] rareNames = { "루피_각성", "조로_각성" };
        string[] epicNames = { "루피_기어2", "조로_삼도류" };
        string[] legendaryNames = { "루피_기어5", "조로_염왕" };

        var allUnits = new System.Collections.Generic.List<UnitData>();

        // Common (★1)
        foreach (string name in commonNames)
        {
            UnitData data = CreateUnitData(name, UnitGrade.Common, 10f, 1f, 3f, allUnits.Count + 1);
            allUnits.Add(data);
        }

        // Uncommon (★2)
        foreach (string name in uncommonNames)
        {
            UnitData data = CreateUnitData(name, UnitGrade.Uncommon, 25f, 1.2f, 3.5f, allUnits.Count + 1);
            allUnits.Add(data);
        }

        // Rare (★3)
        foreach (string name in rareNames)
        {
            UnitData data = CreateUnitData(name, UnitGrade.Rare, 60f, 1.5f, 4f, allUnits.Count + 1);
            allUnits.Add(data);
        }

        // Epic (★4)
        foreach (string name in epicNames)
        {
            UnitData data = CreateUnitData(name, UnitGrade.Epic, 150f, 2f, 4.5f, allUnits.Count + 1);
            allUnits.Add(data);
        }

        // Legendary (★5)
        foreach (string name in legendaryNames)
        {
            UnitData data = CreateUnitData(name, UnitGrade.Legendary, 400f, 2.5f, 5f, allUnits.Count + 1);
            allUnits.Add(data);
        }

        // === UnitDatabase 생성 ===
        UnitDatabase database = ScriptableObject.CreateInstance<UnitDatabase>();
        database.allUnits = allUnits.ToArray();
        database.commonRate = 50f;
        database.uncommonRate = 30f;
        database.rareRate = 15f;
        database.epicRate = 4f;
        database.legendaryRate = 1f;
        AssetDatabase.CreateAsset(database, "Assets/ScriptableObjects/UnitDatabase.asset");

        // === 적 데이터 생성 ===
        CreateEnemyData("해적_졸병", 100f, 2f, 10, false);
        CreateEnemyData("해적_부대장", 250f, 1.8f, 25, false);
        CreateEnemyData("해적_간부", 500f, 1.5f, 50, false);
        CreateEnemyData("해군_병사", 150f, 2.2f, 15, false);
        CreateEnemyData("해군_장교", 400f, 1.7f, 40, false);

        // === 웨이브 데이터 생성 ===
        // (실제로는 EnemyData 참조가 필요하므로 여기서는 구조만 생성)
        for (int i = 1; i <= 5; i++)
        {
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
            wave.waveName = $"웨이브 {i}";
            wave.waveNumber = i;
            wave.timeBetweenSpawns = Mathf.Max(0.5f, 1.5f - i * 0.1f);
            wave.entries = new WaveEntry[0]; // 에디터에서 수동 연결 필요
            AssetDatabase.CreateAsset(wave, $"Assets/ScriptableObjects/Waves/Wave_{i}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[TestDataGenerator] 테스트 데이터 생성 완료!");
        Debug.Log($"유닛 {allUnits.Count}개, 적 5종, 웨이브 5개 생성됨");
    }

    private static UnitData CreateUnitData(string name, UnitGrade grade, float atk, float atkSpeed, float range, int id)
    {
        UnitData data = ScriptableObject.CreateInstance<UnitData>();
        data.unitName = name;
        data.unitID = id;
        data.grade = grade;
        data.attackDamage = atk;
        data.attackSpeed = atkSpeed;
        data.attackRange = range;
        data.combineCount = 3;
        data.projectileSpeed = 10f;

        // 임시 프리팹 생성
        GameObject prefab = UnitFactory.CreateTempUnitPrefab(grade);
        prefab.name = $"Prefab_{name}";
        
        // 프리팹으로 저장
        string prefabPath = $"Assets/Prefabs/Unit_{name}.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        Object.DestroyImmediate(prefab);
        data.prefab = savedPrefab;

        AssetDatabase.CreateAsset(data, $"Assets/ScriptableObjects/Units/Unit_{name}.asset");
        return data;
    }

    private static void CreateEnemyData(string name, float hp, float speed, int gold, bool isBoss)
    {
        EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
        data.enemyName = name;
        data.maxHP = hp;
        data.moveSpeed = speed;
        data.goldReward = gold;
        data.isBoss = isBoss;

        // 임시 프리팹 생성
        GameObject prefab = UnitFactory.CreateTempEnemyPrefab();
        prefab.name = $"Prefab_{name}";
        string prefabPath = $"Assets/Prefabs/Enemy_{name}.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        Object.DestroyImmediate(prefab);
        data.prefab = savedPrefab;

        AssetDatabase.CreateAsset(data, $"Assets/ScriptableObjects/Enemies/Enemy_{name}.asset");
    }
#endif
}
