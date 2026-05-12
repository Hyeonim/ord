/// <summary>
/// 원피스 랜덤 디펜스 전체 열거형 정의
/// </summary>

/// <summary>
/// 유닛 등급 (원랜디 기준)
/// </summary>
public enum UnitGrade
{
    Common,         // 흔함
    Uncommon,       // 안흔함
    Special,        // 특별함
    Rare,           // 희귀함
    Legendary,      // 전설적인
    Hidden,         // 히든
    Transcendent,   // 초월함
    Immortal,       // 불멸한
    Eternal,        // 영원함
    Limited         // 제한됨
}

/// <summary>
/// 유닛 배치 상태
/// </summary>
public enum UnitPlacement
{
    Inventory,  // 대기석(인벤토리)
    Field       // 전투 필드
}

/// <summary>
/// 유닛 공격 타입
/// </summary>
public enum AttackType
{
    Physical,   // 물리 (방깎 영향 받음)
    Magical,    // 마법 (방깎 무시)
    Percent     // % 데미지
}

/// <summary>
/// 유닛 능력 타입
/// </summary>
public enum AbilityType
{
    None,           // 없음
    Stun,           // 스턴 (몬스터 홀딩)
    Slow,           // 이감 (이동속도 저하)
    ArmorBreak,     // 방깎 (방어력 감소)
    AttackBuff,     // 공증 (공격력 증가)
    AttackSpeedBuff,// 공속증 (공격속도 증가)
    SingleTarget,   // 단일 (현재 체력 % 데미지)
    FinishDamage,   // 끝딜 (전체 체력 % 데미지)
    Penetrate       // 관통
}

/// <summary>
/// 게임 난이도
/// </summary>
public enum Difficulty
{
    Easy,
    Normal,
    Hard,
    God     // 신
}

/// <summary>
/// 게임 상태
/// </summary>
public enum GameState
{
    Preparing,  // 준비 중
    Playing,    // 진행 중
    Paused,     // 일시정지
    Victory,    // 승리
    Defeat      // 패배
}
