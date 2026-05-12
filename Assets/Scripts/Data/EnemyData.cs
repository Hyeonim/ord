using UnityEngine;

/// <summary>
/// 적(몬스터)의 기본 데이터를 정의하는 ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "RandomDefense/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;
    public int enemyID;
    public GameObject prefab;

    [Header("스탯")]
    public float maxHP = 100f;
    public float moveSpeed = 2f;
    public int goldReward = 10;

    [Header("보스 여부")]
    public bool isBoss = false;
    public float bossHPMultiplier = 10f;
}
