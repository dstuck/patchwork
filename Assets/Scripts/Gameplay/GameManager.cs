using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Patchwork.UI;

namespace Patchwork.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        #region Private Fields
        [Header("Scene Names")]
        [SerializeField] private string m_MainMenuSceneName = "MainMenu";
        [SerializeField] private string m_GameplaySceneName = "GameplayScene";
        [SerializeField] private string m_TransitionSceneName = "TransitionScene";
        
        [Header("References")]
        [SerializeField] private Deck m_Deck;
        
        [Header("Timer Settings")]
        [SerializeField] private float m_BaseTimerDuration = 24f;
        [SerializeField] private float m_TimerStartDelay = 4f;
        [SerializeField] private float m_BaseMultiplier = 2f;
        
        [Header("Boss Battle Settings")]
        [SerializeField] private int m_BossStageInterval = 4; // Every X stages is a boss

        [Header("Moving Boss Settings")]
        [SerializeField] private int m_MovingBossBoardWidth = 5; // Number of standard board widths
        [SerializeField] private float m_MovingBossColumnDelay = 0.5f; // Seconds between column moves

        private bool m_IsMovingBossStage;
        private int m_MovingBossCurrentColumn;
        private float m_MovingBossNextMoveTime;
        private int m_MovingBossTotalColumns;
        private bool m_MovingBossComplete;
        
        [Header("Gem Settings")]
        [SerializeField] private float m_TimePerGem = 6f;  // Time bonus per gem
        private const int c_MaxGemCount = 3;
        private const int c_StagesPerGem = 2;
        
        private static GameManager s_Instance;
        private bool m_IsInitialized;
        
        private Timer m_Timer;
        
        private bool m_IsBossStage;
        private int m_CurrentColumn;
        private float m_NextColumnMoveTime;
        private int m_TotalColumns; // Total number of columns in the full board
        private bool m_BossStageComplete;
        private int m_TilePointsBonus = 0;

        private int m_DangerLevel = 0;
        private const int c_MaxDangerLevel = 3;
        #endregion

        #region Game State
        private int m_CurrentStage;
        private int m_CumulativeScore;
        #endregion

        #region Public Properties
        public static GameManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    InitializeInstance();
                }
                return s_Instance;
            }
        }

        public int CurrentStage => m_CurrentStage;
        public int CumulativeScore => m_CumulativeScore;
        public Deck Deck => m_Deck;
        public float BaseMultiplier => m_BaseMultiplier;
        public int BossStageInterval => m_BossStageInterval;
        public bool IsPostBossStage => IsBossStage(m_CurrentStage - 1);
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            if (m_IsInitialized) return;
            
            if (m_Deck == null)
            {
                Debug.LogError("[GameManager] Deck reference is missing!");
                return;
            }

            if (!m_Deck.IsInitialized)
            {
                m_Deck.Initialize();
            }

            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            m_IsInitialized = true;
        }

        private bool IsBossStage(int stageNumber)
        {
            // // Temporary: Make first stage a boss stage for testing
            // return stageNumber == 1;
            
            return stageNumber % m_BossStageInterval == 0;
        }

        private void SetupMovingBossStage()
        {
            m_IsMovingBossStage = true;
            m_MovingBossCurrentColumn = 0;
            m_MovingBossComplete = false;
            m_MovingBossNextMoveTime = Time.time + m_MovingBossColumnDelay;
            
            Board board = FindFirstObjectByType<Board>();
            if (board != null)
            {
                m_MovingBossTotalColumns = board.GridSettings.GridSize.x * m_MovingBossBoardWidth;
                board.SetupMovingBossBoard(m_MovingBossTotalColumns);
                
                // Place gems evenly across the full width
                int gemsToPlace = m_MovingBossBoardWidth;
                float gemSpacing = m_MovingBossTotalColumns / (float)(gemsToPlace + 1);
                for (int i = 0; i < gemsToPlace; i++)
                {
                    int gemColumn = Mathf.RoundToInt(gemSpacing * (i + 1));
                    board.PlaceDrawGemInColumn(gemColumn);
                }
            }
            
            // Setup timer based on exactly how long it will take to scroll the entire board
            m_Timer = FindFirstObjectByType<Timer>();
            if (m_Timer != null)
            {
                float totalTime = m_MovingBossTotalColumns * m_MovingBossColumnDelay;
                m_Timer.StartTimer(totalTime, m_TimerStartDelay, 1f, 1f);
            }
        }

        private void UpdateMovingBoss()
        {
            if (m_MovingBossComplete) return;

            if (Time.time >= m_MovingBossNextMoveTime)
            {
                m_MovingBossCurrentColumn++;
                m_MovingBossNextMoveTime = Time.time + m_MovingBossColumnDelay;

                Board board = FindFirstObjectByType<Board>();
                if (board != null)
                {
                    board.ScrollOneColumn(m_MovingBossCurrentColumn);
                }
                
                // Check if time ran out
                if (m_Timer != null && m_Timer.GetTimeRemaining() <= 0)
                {
                    m_MovingBossComplete = true;
                    int finalScore = board.CalculateTotalScore();
                    CompleteStage(finalScore);
                }
            }
        }

        private void Update()
        {
            if (m_IsMovingBossStage)
            {
                UpdateMovingBoss();
            }
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
            if (_scene.name == m_GameplaySceneName)
            {
                if (IsBossStage(m_CurrentStage))
                {
                    SetupMovingBossStage();
                }
                else
                {
                    // Existing normal stage setup code
                    int gemCount = Mathf.Min((m_CurrentStage - 1) / c_StagesPerGem, c_MaxGemCount);
                    float totalTime = m_BaseTimerDuration + (gemCount * m_TimePerGem);
                    
                    Board board = FindFirstObjectByType<Board>();
                    if (board != null)
                    {
                        board.SetGemCount(gemCount);
                    }
                    
                    m_Timer = FindFirstObjectByType<Timer>();
                    if (m_Timer != null)
                    {
                        // Always start at 1 and decay from the current max multiplier
                        m_Timer.StartTimer(totalTime, m_TimerStartDelay, 1f, m_BaseMultiplier);
                    }
                }
                
                // Reset deck for both normal and boss stages
                if (m_Deck != null)
                {
                    m_Deck.ResetForNewStage();
                }
            }
        }

        private static void InitializeInstance()
        {
            // First try to find existing instance
            s_Instance = FindFirstObjectByType<GameManager>();

            if (s_Instance == null)
            {
                // Load and instantiate from Resources if not found
                GameObject prefab = Resources.Load<GameObject>("Prefabs/GameManager");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab);
                    s_Instance = go.GetComponent<GameManager>();
                    if (s_Instance != null)
                    {
                        s_Instance.name = "GameManager";
                        DontDestroyOnLoad(go);
                    }
                    else
                    {
                        Debug.LogError("GameManager component missing from prefab!");
                    }
                }
                else
                {
                    Debug.LogError("GameManager prefab not found in Resources/Prefabs/GameManager!");
                }
            }
        }
        #endregion

        #region Public Methods
        public void StartNewGame()
        {
            m_CumulativeScore = 0;
            
            if (m_Deck != null)
            {
                m_Deck.ResetDeck();
            }
            else
            {
                Debug.LogError("[GameManager] Cannot start game - Deck is null");
                return;
            }

            SceneManager.LoadScene(m_GameplaySceneName);
        }

        public void StartNextStage()
        {
            bool isNextStageBoss = IsBossStage(m_CurrentStage);
            if (isNextStageBoss)
            {
                SceneManager.LoadScene("BossAnnouncement");
            }
            else
            {
                SceneManager.LoadScene(m_GameplaySceneName);
            }
        }

        public void CompleteStage(int _baseScore)
        {
            float multiplier = 1f;
            if (m_Timer != null)
            {
                if (m_Timer.GetTimeRemaining() > 0)
                {
                    multiplier = m_Timer.GetCurrentMultiplier();
                }
                m_Timer.StopTimer();
            }
            
            int stageScore = Mathf.RoundToInt(_baseScore * multiplier);
            m_CumulativeScore += stageScore;
            
            var scoringPopup = FindFirstObjectByType<ScoringPopupUI>();
            if (scoringPopup != null)
            {
                scoringPopup.OnPopupComplete.AddListener(() => {
                    m_CurrentStage++;
                    SceneManager.LoadScene(m_TransitionSceneName);
                });
                
                scoringPopup.ShowScoring(_baseScore, multiplier, stageScore, m_CumulativeScore);
            }
        }

        private int GetTotalTilesPlaced()
        {
            Board board = FindFirstObjectByType<Board>();
            return board != null ? board.GetPlacedTileCount() : 0;
        }

        private IEnumerator CompleteStageRoutine(int _finalScore)
        {
            // Remove the delay or reduce it significantly
            yield return new WaitForSeconds(0.04f);  // Just a tiny delay to ensure clean transition
            
            // Store the score
            m_CumulativeScore = _finalScore;
            
            // Load the transition scene
            SceneManager.LoadScene(m_TransitionSceneName);
        }

        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene(m_MainMenuSceneName);
        }

        public void IncreaseMultiplier()
        {
            m_BaseMultiplier += 0.5f;
        }

        public void IncreaseTilePoints()
        {
            m_TilePointsBonus += 2;
        }

        public int GetTilePointsBonus()
        {
            return m_TilePointsBonus;
        }

        public void IncreaseDanger()
        {
            m_DangerLevel++;
            if (m_DangerLevel >= c_MaxDangerLevel)
            {
                // Trigger game over
                SceneManager.LoadScene("GameOver");
            }
        }

        public void ResetDanger()
        {
            m_DangerLevel = 0;
        }
        #endregion

        #if UNITY_EDITOR
        [ContextMenu("Force Initialize")]
        private void ForceInitialize()
        {
            Initialize();
        }
        #endif
    }
} 