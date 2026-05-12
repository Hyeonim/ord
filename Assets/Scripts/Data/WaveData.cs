using UnityEngine;
using System;

/// <summary>
/// 웨이브(라운드) 데이터를 정의하는 ScriptableObject.
/// 각 라운드에 등장하는 적의 종류와 수를 관리한다.
/// </summary>
[CreateAssetMenu(fileName = "NewWaveData", menuName = "ORD/Wave Data")]
public class WaveData : ScriptableObject
{
    public string waveName;
    public int waveNumber;
    public WaveEntry[] entries;
    public float timeBetweenSpawns = 1f;    // 적 스폰 간격
    public int wispReward = 2;              // 라운드 시작 시 지급 위스프 수
    public bool hasBoss = false;            // 보스 라운드 여부
    public float bossTimeLimit = 30f;       // 보스 제한시간
}

/// <summary>
/// 웨이브 내 적 구성 항목
/// </summary>
[Serializable]
public class WaveEntry
{
    public EnemyData enemyData;
    public int count = 5;
}
