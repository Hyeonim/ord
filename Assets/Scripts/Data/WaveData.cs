using UnityEngine;

/// <summary>
/// 웨이브 정보를 정의하는 ScriptableObject.
/// 각 웨이브마다 어떤 적이 몇 마리 나오는지 설정.
/// </summary>
[CreateAssetMenu(fileName = "NewWaveData", menuName = "RandomDefense/WaveData")]
public class WaveData : ScriptableObject
{
    public string waveName;
    public int waveNumber;
    public WaveEntry[] entries;
    public float timeBetweenSpawns = 1f; // 적 생성 간격(초)
}

[System.Serializable]
public class WaveEntry
{
    public EnemyData enemyData;
    public int count = 5;
}
