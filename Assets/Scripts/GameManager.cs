using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

[Serializable]
public struct WaveInfo
{
    public int InitialEnemyCount;
    // public float MaxEnemySpeed;
    // public float MinEnemySpeed;
    public float SpawnInterval;
    public float WaveTime;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("關卡設定")]
    public WaveInfo[] Waves; // 關卡資訊陣列

    public float CurrentWaveTimeCountDown = 0f; // 當前關卡剩餘時間
    public int CurrentWave = 1;

    [Header("敵人生成設定")]
    public BoxCollider2D[] EnemySpawnAreas; // 用於生成敵人的區域
    public GameObject EnemyPrefab; // 敵人預製體
    public List<GameObject> EnemyPool;
    public float LastEnemySpawnTime = 0f; // 上次生成敵人的時間

    [Header("UI設定")]
    public TMPro.TextMeshProUGUI GameOverText;
    public TMPro.TextMeshProUGUI WaveText;
    public TMPro.TextMeshProUGUI NextWaveCountDownText;

    [Header ("玩家參考")]
    public Player Player; // 玩家參考

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 確保只有一個GameManager存在
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // 切換場景時不銷毀GameManager
        }

        Debug.Log("[GameManager] 已設置為單例實例。");
    }

    void Start()
    {
        if (EnemySpawnAreas == null || EnemySpawnAreas.Length == 0)
        {
            Debug.LogError("[GameManager] 沒有設置敵人生成區域，請在Inspector中添加BoxCollider2D作為生成區域！");
            gameObject.SetActive(false); // 禁用GameManager以防止錯誤
        }

        if (EnemyPrefab == null)
        {
            Debug.LogError("[GameManager] 沒有設置敵人預製體，請在Inspector中指定EnemyPrefab！");
            gameObject.SetActive(false); // 禁用GameManager以防止錯誤
        }

        if (Waves == null || Waves.Length == 0)
        {
            Debug.LogError("[GameManager] 沒有設置關卡資訊，請在Inspector中添加WaveInfo陣列！");
            gameObject.SetActive(false); // 禁用GameManager以防止錯誤
        }

        SetupWave(CurrentWave); // 設置第一波敵人
    }

    void Update()
    {
        // 輸入
        var keyboard = Keyboard.current;
        if (keyboard.rKey.wasPressedThisFrame)
        {
            if (CurrentWave > Waves.Length)
            {
                CurrentWave = 1; // 如果已經完成所有關卡，重置回第一波
            }
            SetupWave(CurrentWave);
        }
        if (keyboard.f1Key.wasPressedThisFrame) SetupWave(1); // 按下 F1 鍵設置第一波敵人

        // 關卡時間和敵人生成
        if (CurrentWaveTimeCountDown > 0)
        {
            CurrentWaveTimeCountDown -= Time.deltaTime;
            NextWaveCountDownText.text = $"Next Wave: {Mathf.CeilToInt(CurrentWaveTimeCountDown)}s";
            if (CurrentWaveTimeCountDown <= 0)
            {
                CurrentWaveTimeCountDown = 0;
                Debug.Log($"第 {CurrentWave} 波結束！");
                NextWaveCountDownText.text = $"Wave {CurrentWave} Completed!";
                CurrentWave++;
                if (CurrentWave > Waves.Length)
                {
                    Debug.Log("所有關卡已完成，遊戲勝利！按下F1鍵重新開始。");
                    ClearEnemyPool(); // 清除所有敵人
                    ShowGameOverUI($"Yous Win! You completed all {Waves.Length} Waves. Press F1 to Restart.");   
                    // 在這裡可以添加遊戲勝利的邏輯，例如顯示勝利UI等
                    return;
                }
                else
                {
                    SetupWave(CurrentWave); // 設置下一波敵人
                }
            }
            // 根據當前波數和剩餘時間計算生成敵人的頻率
            float spawnInterval = Waves[CurrentWave - 1].SpawnInterval;
            if (Time.time - LastEnemySpawnTime >= spawnInterval) //TODO: fuzzy spawn interval
            {
                SpawnEnemy();
            }
        }
    }

    void SetupWave(int waveNumber)
    {
        ClearEnemyPool(); // 清除現有的敵人
        CurrentWaveTimeCountDown = Waves[waveNumber - 1].WaveTime; // 設置當前關卡剩餘時間
        for (int i = 0; i < Waves[waveNumber - 1].InitialEnemyCount; i++) SpawnEnemy();

        //UI
        GameOverText.gameObject.SetActive(false); // 隱藏遊戲結束UI
        NextWaveCountDownText.gameObject.SetActive(true); // 顯示下一波倒數UI
        
        Player.CurrentHealth = Player.MaxHealth; // 重置玩家血量

        WaveText.text = $"Wave {waveNumber}"; // 更新波數UI
        Debug.Log($"[GameManager] 第 {waveNumber} 波開始！");
    }

    void ClearEnemyPool()
    {
        if (EnemyPool != null)
        {
            foreach (var enemy in EnemyPool)
            {
                if (enemy != null)
                {
                    Destroy(enemy); // 銷毀現有的敵人
                }
            }
        }
    }

    void SpawnEnemy()
    {
        var SpawnArea = EnemySpawnAreas[UnityEngine.Random.Range(0, EnemySpawnAreas.Length)];
        Vector2 spawnPosition = new Vector2(
            UnityEngine.Random.Range(SpawnArea.bounds.min.x, SpawnArea.bounds.max.x),
            UnityEngine.Random.Range(SpawnArea.bounds.min.y, SpawnArea.bounds.max.y)
        );
        var enemyObj = Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);

        Debug.Log($"[GameManager] 生成了一個敵人，位置：{spawnPosition}");
        EnemyPool.Add(enemyObj);
        LastEnemySpawnTime = Time.time;
    }

    public void CharacterDied(Character character)
    {
        if (character is Player)
        {
            Debug.Log("玩家已死亡，遊戲結束！");
            ShowGameOverUI($"You Died! You reached Wave {CurrentWave}. Press F1 to Restart.");
            // 在這裡可以添加更多的遊戲結束邏輯，例如停止遊戲、顯示分數等
        }        
    }

    public void ShowGameOverUI(string message)
    {
        GameOverText.gameObject.SetActive(true);
        GameOverText.text = message;
    }
}

