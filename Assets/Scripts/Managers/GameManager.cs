using UnityEngine;
using System;

/// <summary>
/// 게임 전체를 관리하는 핵심 매니저.
/// 골드, 라이프, 게임 상태 등을 총괄한다.
/// 모든 하위 매니저들의 중심 허브 역할을 한다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("플레이어 자원")]
    [SerializeField] private int gold = 200;
    [SerializeField] private int lives = 20;

    [Header("게임 상태")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private float gameTime = 0f;

    // 프로퍼티
    public int Gold => gold;
    public int Lives => lives;
    public bool IsGameOver => isGameOver;
    public bool IsPaused => isPaused;
    public float GameTime => gameTime;

    // 이벤트
    public event Action<int> OnGoldChanged;
    public event Action<int> OnLivesChanged;
    public event Action OnGameOverEvent;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (!isGameOver && !isPaused)
        {
            gameTime += Time.deltaTime;
        }

        // 디버그 키
        HandleDebugInput();
    }

    #region 골드 관리

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(gold);
        Debug.Log($"[GameManager] 골드 +{amount} (현재: {gold})");
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount) return false;
        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public bool CanAfford(int amount) => gold >= amount;

    #endregion

    #region 라이프 관리

    public void LoseLife(int amount)
    {
        if (isGameOver) return;

        lives -= amount;
        lives = Mathf.Max(0, lives);
        OnLivesChanged?.Invoke(lives);

        Debug.Log($"[GameManager] 라이프 -{amount} (남은: {lives})");

        if (lives <= 0)
        {
            GameOver();
        }
    }

    #endregion

    #region 게임 상태 관리

    private void GameOver()
    {
        isGameOver = true;
        OnGameOverEvent?.Invoke();
        Debug.Log("[GameManager] === 게임 오버! ===");
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void SetGameSpeed(float speed)
    {
        if (!isPaused && !isGameOver)
        {
            Time.timeScale = speed;
        }
    }

    #endregion

    #region 디버그

    private void HandleDebugInput()
    {
        // G: 골드 추가
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(100);
            Debug.Log("[Debug] 골드 +100");
        }

        // S: 유닛 소환
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (SummonManager.Instance != null)
            {
                SummonManager.Instance.SummonRandomUnit();
            }
        }

        // N: 웨이브 스킵
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.SkipWave();
                Debug.Log("[Debug] 웨이브 스킵");
            }
        }

        // 1, 2, 3: 게임 속도
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetGameSpeed(1f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetGameSpeed(2f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetGameSpeed(3f);

        // R: 재시작
        if (Input.GetKeyDown(KeyCode.R) && isGameOver)
        {
            RestartGame();
        }
    }

    #endregion
}
