using UnityEngine;
using System;

/// <summary>
/// 게임의 전체 흐름을 관리하는 핵심 매니저.
/// 골드, 라이프, 위스프, 게임 상태, 라운드 진행을 총괄한다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 설정")]
    [SerializeField] private int maxRounds = 75;
    [SerializeField] private int maxUnitCount = 30;
    [SerializeField] private Difficulty difficulty = Difficulty.God;

    [Header("자원")]
    [SerializeField] private int gold = 0;
    [SerializeField] private int wisps = 0;
    [SerializeField] private int wood = 0;
    [SerializeField] private int lives = 20;

    [Header("상태")]
    [SerializeField] private int currentRound = 0;
    [SerializeField] private int unitCount = 0;
    [SerializeField] private GameState gameState = GameState.Preparing;
    [SerializeField] private float gameSpeed = 1f;

    public int Gold => gold;
    public int Wisps => wisps;
    public int Wood => wood;
    public int Lives => lives;
    public int CurrentRound => currentRound;
    public int MaxRounds => maxRounds;
    public int UnitCount => unitCount;
    public int MaxUnitCount => maxUnitCount;
    public GameState State => gameState;
    public float GameSpeed => gameSpeed;
    public bool IsGameOver => gameState == GameState.Victory || gameState == GameState.Defeat;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnWispsChanged;
    public event Action<int> OnWoodChanged;
    public event Action<int> OnLivesChanged;
    public event Action<int> OnRoundChanged;
    public event Action<int> OnUnitCountChanged;
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        gold = 100;
        wisps = 2;
        wood = 0;
        lives = 20;
        currentRound = 0;
        unitCount = 0;
        gameState = GameState.Preparing;
        Time.timeScale = 1f;
        gameSpeed = 1f;

        OnGoldChanged?.Invoke(gold);
        OnWispsChanged?.Invoke(wisps);
        OnWoodChanged?.Invoke(wood);
        OnLivesChanged?.Invoke(lives);
        OnRoundChanged?.Invoke(currentRound);
        OnUnitCountChanged?.Invoke(unitCount);
        OnGameStateChanged?.Invoke(gameState);
    }

    public void StartNewRound()
    {
        currentRound++;
        gameState = GameState.Playing;
        AddWisps(2);
        AddGold(10 + currentRound * 2);
        OnRoundChanged?.Invoke(currentRound);
        OnGameStateChanged?.Invoke(gameState);
        Debug.Log($"[GameManager] === 라운드 {currentRound}/{maxRounds} 시작! ===");
    }

    public void OnRoundComplete()
    {
        if (currentRound >= maxRounds) { Victory(); return; }
        if (unitCount > maxUnitCount) { Defeat("유닛 카운트 초과!"); return; }
        gameState = GameState.Preparing;
        OnGameStateChanged?.Invoke(gameState);
    }

    // === 자원 관리 ===
    public void AddGold(int amount) { gold += amount; OnGoldChanged?.Invoke(gold); }
    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount; OnGoldChanged?.Invoke(gold); return true;
    }

    public void AddWisps(int amount) { wisps += amount; OnWispsChanged?.Invoke(wisps); }
    public bool UseWisp()
    {
        if (wisps <= 0) return false;
        wisps--; OnWispsChanged?.Invoke(wisps); return true;
    }

    public void AddWood(int amount) { wood += amount; OnWoodChanged?.Invoke(wood); }
    public bool SpendWood(int amount)
    {
        if (wood < amount) return false;
        wood -= amount; OnWoodChanged?.Invoke(wood); return true;
    }

    public void LoseLife(int amount = 1)
    {
        lives -= amount; OnLivesChanged?.Invoke(lives);
        if (lives <= 0) Defeat("라이프 소진!");
    }

    public void AddUnitCount(int amount = 1)
    {
        unitCount += amount; OnUnitCountChanged?.Invoke(unitCount);
        if (unitCount > maxUnitCount) Defeat("유닛 카운트 초과!");
    }

    public void ReduceUnitCount(int amount = 1)
    {
        unitCount = Mathf.Max(0, unitCount - amount);
        OnUnitCountChanged?.Invoke(unitCount);
    }

    // === 게임 속도 ===
    public void SetGameSpeed(float speed) { gameSpeed = speed; Time.timeScale = speed; }
    public void PauseGame() { gameState = GameState.Paused; Time.timeScale = 0f; OnGameStateChanged?.Invoke(gameState); }
    public void ResumeGame() { gameState = GameState.Playing; Time.timeScale = gameSpeed; OnGameStateChanged?.Invoke(gameState); }

    private void Victory()
    {
        gameState = GameState.Victory; Time.timeScale = 0f;
        OnGameStateChanged?.Invoke(gameState);
        Debug.Log($"[GameManager] 승리! {currentRound}라운드 클리어, 유카: {unitCount}");
    }

    private void Defeat(string reason)
    {
        gameState = GameState.Defeat; Time.timeScale = 0f;
        OnGameStateChanged?.Invoke(gameState);
        Debug.Log($"[GameManager] 패배! 사유: {reason}");
    }

    public void RestartGame()
    {
        InitializeGame();
        if (WaveManager.Instance != null) WaveManager.Instance.ResetWaves();
        if (SummonManager.Instance != null) SummonManager.Instance.ResetInventory();
    }
}
