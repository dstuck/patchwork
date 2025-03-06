using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Patchwork.UI;
using System.Collections.Generic;

namespace Patchwork.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        #region Constants
        private const int c_MaxLives = 3;
        #endregion

        #region Private Fields
        [Header("Scene Names")]
        [SerializeField] private string m_MainMenuSceneName = "MainMenu";
        [SerializeField] private string m_GameplaySceneName = "GameplayScene";
        [SerializeField] private string m_TransitionSceneName = "TransitionScene";
        
        [Header("References")]
        [SerializeField] private Deck m_Deck;
        [SerializeField] private CollectiblesDeck m_CollectiblesDeck;
        
        [Header("Timer Settings")]
        [SerializeField] private float m_BaseTimerDuration = 30f;
        [SerializeField] private float m_TimerStartDelay = 6f;
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
        [SerializeField] private float m_TimePerGem = 8f;  // Time bonus per gem
        private const int c_MaxGemCount = 3;
        private const int c_StagesPerGem = 2;
        
        private static GameManager s_Instance;
        private bool m_IsInitialized;
        
        private Timer m_Timer;

        [Header("Life Settings")]
        [SerializeField] private int m_MaxLives = 3;  // Starting max lives
        private int m_CurrentLives;
        private PlayerResourceUI m_ResourceUI;

        [Header("Collectible Settings")]
        [SerializeField] private int m_BaseSparkCount = 2;    // Start with 2 sparks
        [SerializeField] private int m_BaseFlameCount = 0;    // Start with 1 flame
        [SerializeField] private int m_StagesPerSpark = 2;    // Add 1 spark every 2 stages
        [SerializeField] private int m_StagesPerFlame = 3;    // Add 1 flame every 3 stages

        private int m_StageScoreBonus = 0;
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
        public int MaxLives => m_MaxLives;
        public int SparkCount => m_BaseSparkCount + ((m_CurrentStage - 1) / m_StagesPerSpark);
        public int FlameCount => m_BaseFlameCount + ((m_CurrentStage - 1) / m_StagesPerFlame);
        public CollectiblesDeck CollectiblesDeck => m_CollectiblesDeck;
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

            // Try to find CollectiblesDeck if not assigned
            if (m_CollectiblesDeck == null)
            {
                m_CollectiblesDeck = FindFirstObjectByType<CollectiblesDeck>();
                if (m_CollectiblesDeck == null)
                {
                    Debug.LogError("[GameManager] CollectiblesDeck not found in scene!");
                    return;
                }
            }

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

        private void Start()
        {
            m_CurrentLives = m_MaxLives;
            m_ResourceUI = FindFirstObjectByType<PlayerResourceUI>();
            if (m_ResourceUI != null)
            {
                m_ResourceUI.Initialize(m_MaxLives);
                m_ResourceUI.UpdateLives(m_CurrentLives);
            }
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            if (m_IsInitialized) return;
            
            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            m_CurrentLives = m_MaxLives;
            
            if (m_Deck != null)
            {
                m_Deck.Initialize();
            }

            if (m_CollectiblesDeck == null)
            {
                Debug.LogError("[GameManager] CollectiblesDeck reference is missing!");
                return;
            }

            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            InitializeCollectibles();

            if (!m_CollectiblesDeck.IsInitialized)
            {
                m_CollectiblesDeck.Initialize();
            }

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
                // Reset stage-specific bonuses
                m_StageScoreBonus = 0;
                
                // Add stage progress collectibles before resetting deck
                AddStageProgressCollectibles();
                
                if (m_CollectiblesDeck != null)
                {
                    m_CollectiblesDeck.ResetForNewStage();
                }
                
                // Initialize UI
                m_ResourceUI = FindFirstObjectByType<PlayerResourceUI>();
                if (m_ResourceUI != null)
                {
                    m_ResourceUI.Initialize(m_MaxLives);
                    m_ResourceUI.UpdateLives(m_CurrentLives);
                }

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

        private void InitializeCollectibles()
        {
            // Add base number of sparks
            for (int i = 0; i < m_BaseSparkCount; i++)
            {
                var sparkObj = new GameObject("SparkPrototype");
                sparkObj.SetActive(false);  // Hide the prototype
                var spark = sparkObj.AddComponent<SparkCollectible>();
                m_CollectiblesDeck.AddCollectibleToDeck(spark);
                Destroy(sparkObj);  // Destroy after adding to deck
            }

            // Add base number of flames
            for (int i = 0; i < m_BaseFlameCount; i++)
            {
                var flameObj = new GameObject("FlamePrototype");
                flameObj.SetActive(false);  // Hide the prototype
                var flame = flameObj.AddComponent<FlameCollectible>();
                m_CollectiblesDeck.AddCollectibleToDeck(flame);
                Destroy(flameObj);  // Destroy after adding to deck
            }
        }

        private void AddStageProgressCollectibles()
        {
            // Add new spark if it's time
            if (m_CurrentStage > 0 && m_CurrentStage % m_StagesPerSpark == 0)
            {
                var sparkObj = new GameObject("SparkPrototype");
                sparkObj.SetActive(false);  // Hide the prototype
                var spark = sparkObj.AddComponent<SparkCollectible>();
                m_CollectiblesDeck.AddCollectibleToDeck(spark);
                Destroy(sparkObj);  // Destroy after adding to deck
            }

            // Add new flame if it's time
            if (m_CurrentStage > 0 && m_CurrentStage % m_StagesPerFlame == 0)
            {
                var flameObj = new GameObject("FlamePrototype");
                flameObj.SetActive(false);  // Hide the prototype
                var flame = flameObj.AddComponent<FlameCollectible>();
                m_CollectiblesDeck.AddCollectibleToDeck(flame);
                Destroy(flameObj);  // Destroy after adding to deck
            }

            // Add new draw gem if it's time
            if (m_CurrentStage > 0 && m_CurrentStage % c_StagesPerGem == 0 && (m_CurrentStage / c_StagesPerGem) <= c_MaxGemCount)
            {
                var gemObj = new GameObject("DrawGemPrototype");
                gemObj.SetActive(false);  // Hide the prototype
                var gem = gemObj.AddComponent<DrawGemCollectible>();
                m_CollectiblesDeck.AddCollectibleToDeck(gem);
                Destroy(gemObj);  // Destroy after adding to deck
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
            
            // Add bonus to base score before multiplier
            int scoreWithBonus = _baseScore + m_StageScoreBonus;
            int stageScore = Mathf.RoundToInt(scoreWithBonus * multiplier);
            m_CumulativeScore += stageScore;
            
            var scoringPopup = FindFirstObjectByType<ScoringPopupUI>();
            if (scoringPopup != null)
            {
                scoringPopup.OnPopupComplete.AddListener(() => {
                    m_CurrentStage++;
                    SceneManager.LoadScene(m_TransitionSceneName);
                });
                
                scoringPopup.ShowScoring(scoreWithBonus, multiplier, stageScore, m_CumulativeScore);
            }
            
            // Reset the bonus for next stage
            m_StageScoreBonus = 0;
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

        public void IncreaseMultiplier(float amount)
        {
            if (m_Timer != null)
            {
                m_Timer.IncreaseCurrentMultiplier(amount);
            }
        }

        public void IncreaseScoreBonus(int amount)
        {
            m_StageScoreBonus += amount;
        }

        public void DecreaseLives(int amount = 1)
        {
            m_CurrentLives -= amount;
            if (m_ResourceUI != null)
            {
                m_ResourceUI.UpdateLives(m_CurrentLives);
            }
            
            if (m_CurrentLives <= 0)
            {
                SceneManager.LoadScene("GameOver");
            }
        }

        public void ResetLives()
        {
            m_CurrentLives = m_MaxLives;
            if (m_ResourceUI != null)
            {
                m_ResourceUI.UpdateLives(m_CurrentLives);
            }
        }

        public void IncreaseMaxLives()
        {
            m_MaxLives++;
            m_CurrentLives++;
            if (m_ResourceUI != null)
            {
                m_ResourceUI.Initialize(m_MaxLives);  // Reinitialize UI with new max
                m_ResourceUI.UpdateLives(m_CurrentLives);
            }
        }

        public List<ICollectible> GetCollectiblesForStage()
        {
            // Reset the deck for the new stage
            m_CollectiblesDeck.ResetForNewStage();
            return m_CollectiblesDeck.GetCollectibles();
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