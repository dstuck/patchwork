using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Patchwork.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Patchwork.Input;
using Patchwork.Data;
using System.Linq;

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
        [SerializeField] private float m_TimePerGem = 8f;  // Time bonus per gem draw value
        
        private static GameManager s_Instance;
        private bool m_IsInitialized;
        
        private Timer m_Timer;

        [Header("Life Settings")]
        [SerializeField] private float m_MaxLives = 3f;  // Changed to float
        private float m_CurrentLives;  // Changed to float
        private PlayerResourceUI m_ResourceUI;

        [Header("Collectible Settings")]
        [SerializeField] private int m_BaseSparkCount = 2;    // Start with 2 sparks
        [SerializeField] private int m_BaseFlameCount = 0;    // Start with 1 flame
        [SerializeField] private int m_StagesPerSpark = 2;    // Add 1 spark every 2 stages
        [SerializeField] private int m_StagesPerFlame = 3;    // Add 1 flame every 3 stages

        private int m_StageScoreBonus = 0;

        private bool m_ShowingTooltips = false;

        private GameControls m_Controls;

        private bool m_IsBeingDestroyed;

        // Active collectibles for this run
        private List<ICollectible> m_ActiveBonuses = new List<ICollectible>();
        private List<ICollectible> m_ActiveDangers = new List<ICollectible>();
        private ICollectible m_CurrentBonus;
        private ICollectible m_CurrentDanger;
        private int m_BonusCounter;
        private int m_DangerCounter;
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
        public int MaxLives => c_MaxLives;
        public int SparkCount => m_BaseSparkCount + ((m_CurrentStage - 1) / m_StagesPerSpark);
        public int FlameCount => m_BaseFlameCount + ((m_CurrentStage - 1) / m_StagesPerFlame);
        public CollectiblesDeck CollectiblesDeck => m_CollectiblesDeck;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                m_IsBeingDestroyed = true; // Flag that we're being destroyed
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

            m_Controls = new GameControls();
            m_Controls.Movement.ShowTooltips.started += ctx => OnShowTooltip(true);
            m_Controls.Movement.ShowTooltips.canceled += ctx => OnShowTooltip(false);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            m_Controls.Enable();
        }

        private void OnDisable()
        {
            // Skip cleanup if we're the duplicate being destroyed
            if (m_IsBeingDestroyed) return;

            if (m_Controls != null)
            {
                m_Controls.Disable();
            }
            
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
                    // Calculate total draw value for timer (gems contribute time based on their draw value)
                    int totalDrawValue = CalculateTotalDrawValueForTimer();
                    float totalTime = m_BaseTimerDuration + (totalDrawValue * m_TimePerGem);
                    
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
            // Create prototype collectibles
            var newSquare = CreateCollectible<NewSquareCollectible>("NewSquarePrototype");
            var drawGem = CreateCollectible<DrawGemCollectible>("DrawGemPrototype");
            var heartPiece = CreateCollectible<HeartPieceCollectible>("HeartPiecePrototype");
            var pristinePaint = CreateCollectible<PristinePaintCollectible>("PristineUpgradePrototype");
            var lenientPaint = CreateCollectible<LenientPaintCollectible>("LenientUpgradePrototype");
            var spark = CreateCollectible<SparkCollectible>("SparkPrototype");
            var ghostSpark = CreateCollectible<GhostSparkCollectible>("GhostSparkPrototype");
            var jumpingSpark = CreateCollectible<JumpingSparkCollectible>("JumpingSparkPrototype");
            var flame = CreateCollectible<FlameCollectible>("FlamePrototype");

            // Select 3 random bonuses and 2 random dangers for this run
            var allBonuses = new List<ICollectible> { newSquare, drawGem, heartPiece, pristinePaint };
            var allDangers = new List<ICollectible> { spark, ghostSpark, jumpingSpark, flame };

            m_ActiveBonuses = allBonuses.OrderBy(x => Random.value).Take(3).ToList();
            m_ActiveDangers = allDangers.OrderBy(x => Random.value).Take(2).ToList();

            // Select initial collectibles
            SelectNextBonus();
            SelectNextDanger();
        }

        private ICollectible CreateCollectible<T>(string name) where T : BaseCollectible
        {
            var obj = new GameObject(name);
            obj.SetActive(false);
            return obj.AddComponent<T>();
        }

        private void SelectNextBonus()
        {
            if (m_ActiveBonuses.Count > 0)
            {
                m_CurrentBonus = m_ActiveBonuses[Random.Range(0, m_ActiveBonuses.Count)];
                m_BonusCounter = 0;
            }
        }

        private void SelectNextDanger()
        {
            if (m_ActiveDangers.Count > 0)
            {
                m_CurrentDanger = m_ActiveDangers[Random.Range(0, m_ActiveDangers.Count)];
                m_DangerCounter = 0;
            }
        }

        private void UpdateCollectibles()
        {
            // Update bonus counter
            if (m_CurrentBonus != null)
            {
                m_BonusCounter++;
                if (m_BonusCounter >= m_CurrentBonus.Power)
                {
                    // Add to deck and select new bonus
                    m_CollectiblesDeck.AddCollectibleToDeck(m_CurrentBonus);
                    SelectNextBonus();
                }
            }

            // Update danger counter
            if (m_CurrentDanger != null)
            {
                m_DangerCounter++;
                if (m_DangerCounter >= m_CurrentDanger.Power)
                {
                    // Add to deck and select new danger
                    m_CollectiblesDeck.AddCollectibleToDeck(m_CurrentDanger);
                    SelectNextDanger();
                }
            }
        }

        private int CalculateTotalDrawValueForTimer()
        {
            int totalDrawValue = 0;
            var collectibles = GetCollectiblesForStage();
            
            foreach (var collectible in collectibles)
            {
                if (collectible is DrawGemCollectible drawGem)
                {
                    totalDrawValue += drawGem.GetDrawCount();
                }
            }
            
            return totalDrawValue;
        }
        #endregion

        #region Public Methods
        public void StartNewGame()
        {
            // Reset game state
            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            m_CurrentLives = m_MaxLives;
            m_StageScoreBonus = 0;
            
            // Force deck to reinitialize by resetting initialized flag
            if (m_Deck != null)
            {
                m_Deck.IsInitialized = false;  // Need to make this property settable
                m_Deck.Initialize();
            }
            else
            {
                Debug.LogError("[GameManager] Cannot start game - Deck is null");
                return;
            }
            
            // Reset collectibles deck by clearing and reinitializing
            if (m_CollectiblesDeck != null)
            {
                m_CollectiblesDeck.ClearDeck();
                InitializeCollectibles();
            }
            else
            {
                Debug.LogError("[GameManager] Cannot start game - CollectiblesDeck is null");
                return;
            }

            // Reset UI if it exists
            if (m_ResourceUI != null)
            {
                m_ResourceUI.Initialize(m_MaxLives);
                m_ResourceUI.UpdateLives(m_CurrentLives);
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

            // Only update collectibles on non-boss stages
            if (!IsBossStage(m_CurrentStage))
            {
                UpdateCollectibles();
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
            
            if (m_CurrentLives < 1)  // Changed from <= 0
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

        public void IncreaseMaxLivesByAmount(float amount)
        {
            m_MaxLives += amount;
            m_CurrentLives += amount;
            if (m_ResourceUI != null)
            {
                m_ResourceUI.Initialize(m_MaxLives);
                m_ResourceUI.UpdateLives(m_CurrentLives);
            }
        }

        public List<ICollectible> GetCollectiblesForStage()
        {
            // Reset the deck for the new stage
            m_CollectiblesDeck.ResetForNewStage();
            return m_CollectiblesDeck.GetCollectibles();
        }

        private void OnShowTooltip(bool show)
        {
            if (!show) return; // Ignore key release
                        
            var board = FindFirstObjectByType<Board>();
            if (board == null) return;

            if (!m_ShowingTooltips)
            {
                // First press - show first collectible
                m_ShowingTooltips = true;
                board.ToggleCollectibleTooltips(true);
            }
            else
            {
                // Already showing - try to cycle to next
                bool hasMore = board.CycleToNextCollectibleTooltip();
                if (!hasMore)
                {
                    m_ShowingTooltips = false;
                }
            }
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