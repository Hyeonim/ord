using UnityEngine;

/// <summary>
/// 적(몬스터) 데이터를 정의하는 ScriptableObject.
/// 라운드별 적의 능력치를 관리한다.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ORD/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    public string enemyName;
    public float maxHP = 100f;
    public float moveSpeed = 2f;
    public float armor = 0f;        // 방어력 (물리 데미지 감소)
    public int goldReward = 10;

    [Header("보스 설정")]
    public bool isBoss = false;
    public float bossScale = 1.5f;  // 보스 크기 배율
    public float timeLimit = 30f;   // 보스 제한시간 (초)

    [Header("프리팹")]
    public GameObject prefab;

    [Header("스토리 보스")]
    public bool isStoryBoss = false;
    public float storyBossHP = 10000f;
}
