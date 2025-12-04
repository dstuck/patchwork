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
        
        [Header("Gem Settings")]
        [SerializeField] private float m_TimePerGem = 8f;  // Time bonus per gem draw value
        
        [Header("Score Requirements")]
        [SerializeField] private int m_BaseRequiredScore = 35;  // Base for required score calculation
        [SerializeField] private int m_RequiredScoreIncrement = 5;  // Increment per level (level 1: 40, level 2: 45, etc.)
        
        private static GameManager s_Instance;
        private bool m_IsInitialized;
        private bool m_IsStartingNewGame;
        
        private Timer m_Timer;
        private Board m_Board;

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
        private CraftingUI m_CraftingUI;

        private bool m_IsBeingDestroyed;
        private bool m_IsPaused;
        private bool m_IsStageComplete;

        // Active collectibles for this run
        private List<ICollectible> m_ActiveBonuses = new List<ICollectible>();
        private List<ICollectible> m_ActiveDangers = new List<ICollectible>();
        private ICollectible m_CurrentBonus;
        private ICollectible m_CurrentDanger;
        private int m_BonusCounter;
        private int m_DangerCounter;
        
        // Company info
        private string m_CompanyName;
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
        public int RequiredScore => CalculateRequiredScore(m_CurrentStage);
        public Deck Deck => m_Deck;
        public float BaseMultiplier => m_BaseMultiplier;
        public int BossStageInterval => m_BossStageInterval;
        public bool IsPostBossStage => IsBossStage(m_CurrentStage - 1);
        public int MaxLives => c_MaxLives;
        public int SparkCount => m_BaseSparkCount + ((m_CurrentStage - 1) / m_StagesPerSpark);
        public int FlameCount => m_BaseFlameCount + ((m_CurrentStage - 1) / m_StagesPerFlame);
        public CollectiblesDeck CollectiblesDeck => m_CollectiblesDeck;
        public string CompanyName => m_CompanyName;
        public bool IsPaused => m_IsPaused;
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

            // Initialize basic state - references will be found when gameplay scene loads
            Initialize();

            m_Controls = new GameControls();
            m_Controls.Movement.ShowTooltips.started += ctx => OnShowTooltip(true);
            m_Controls.Movement.ShowTooltips.canceled += ctx => OnShowTooltip(false);
            m_Controls.Movement.Pause.performed += OnPausePressed;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (m_Controls != null)
            {
                m_Controls.Enable();
            }
        }

        private void OnDisable()
        {
            // Skip cleanup if we're the duplicate being destroyed
            if (m_IsBeingDestroyed) return;

            if (m_Controls != null)
            {
                m_Controls.Movement.Pause.performed -= OnPausePressed;
                m_Controls.Disable();
            }
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            m_CurrentLives = m_MaxLives;
        }
        #endregion

        #region Private Methods
        private void Initialize()
        {
            if (m_IsInitialized) return;
            
            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            m_CurrentLives = m_MaxLives;

            m_IsInitialized = true;
        }

        /// <summary>
        /// Finds and initializes all required references when the gameplay scene loads.
        /// This is called from OnSceneLoaded when entering the gameplay scene.
        /// </summary>
        private void InitializeGameplaySceneReferences()
        {
            // Find Deck if not assigned
            if (m_Deck == null)
            {
                m_Deck = FindFirstObjectByType<Deck>();
                if (m_Deck == null)
                {
                    Debug.LogError("[GameManager] Deck not found in gameplay scene!");
                    return;
                }
            }

            // Find CollectiblesDeck if not assigned
            if (m_CollectiblesDeck == null)
            {
                m_CollectiblesDeck = FindFirstObjectByType<CollectiblesDeck>();
                if (m_CollectiblesDeck == null)
                {
                    Debug.LogError("[GameManager] CollectiblesDeck not found in gameplay scene!");
                    return;
                }
            }

            // Initialize Deck
            if (!m_Deck.IsInitialized)
            {
                m_Deck.Initialize();
            }

            if (!m_CollectiblesDeck.IsInitialized)
            {
                m_CollectiblesDeck.Initialize();
            }

            // Clear decks if starting a new game
            if (m_IsStartingNewGame)
            {
                m_CollectiblesDeck.ClearDeck();
                // Force deck to reinitialize by resetting initialized flag
                m_Deck.IsInitialized = false;
                m_Deck.Initialize();
                m_IsStartingNewGame = false;
            }

            // Initialize collectibles if we have active bonuses/dangers from company selection
            // Otherwise, generate random ones
            if (m_ActiveBonuses.Count == 0 && m_ActiveDangers.Count == 0)
            {
                InitializeCollectibles();
            }

            // Find PlayerResourceUI
            m_ResourceUI = FindFirstObjectByType<PlayerResourceUI>();
            if (m_ResourceUI != null)
            {
                m_ResourceUI.Initialize(m_MaxLives);
                m_ResourceUI.UpdateLives(m_CurrentLives);
            }
            else
            {
                Debug.LogWarning("[GameManager] PlayerResourceUI not found in gameplay scene!");
            }

            // Find Timer (will be used when stage starts)
            m_Timer = FindFirstObjectByType<Timer>();
            if (m_Timer == null)
            {
                Debug.LogWarning("[GameManager] Timer not found in gameplay scene!");
            }
        }

        private bool IsBossStage(int stageNumber)
        {
            // // Temporary: Make first stage a boss stage for testing
            // return stageNumber == 1;
            
            return stageNumber % m_BossStageInterval == 0;
        }

        private void Update()
        {
            if (m_IsPaused || m_IsStageComplete) return;
            
            // Let the board handle its own update logic (e.g., for boss boards)
            Board board = GetBoard();
            if (board != null)
            {
                board.OnUpdate();
            }
        }

        private void OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
        {
            if (_scene.name == m_GameplaySceneName)
            {
                // Clear cached references for new scene
                m_Board = null;
                
                // Setup boss board if this is a boss stage
                if (IsBossStage(m_CurrentStage))
                {
                    SetupBossBoard();
                }

                // Initialize all required references for gameplay scene
                InitializeGameplaySceneReferences();

                // Reset stage-specific state
                m_StageScoreBonus = 0;
                m_IsStageComplete = false;
                
                if (m_CollectiblesDeck != null)
                {
                    m_CollectiblesDeck.ResetForNewStage();
                }

                // Calculate total draw value for timer (gems contribute time based on their draw value)
                int totalDrawValue = CalculateTotalDrawValueForTimer();
                float totalTime = m_BaseTimerDuration + (totalDrawValue * m_TimePerGem);
                
                if (m_Timer != null)
                {
                    // Always start at 1 and decay from the current max multiplier
                    m_Timer.StartTimer(totalTime, m_TimerStartDelay, 1f, m_BaseMultiplier);
                }
                
                // Reset deck for both normal and boss stages
                if (m_Deck != null)
                {
                    m_Deck.ResetForNewStage();
                }
            }
        }

        /// <summary>
        /// Replaces the standard Board with the appropriate boss board variant.
        /// </summary>
        private void SetupBossBoard()
        {
            Board existingBoard = FindFirstObjectByType<Board>();
            if (existingBoard == null)
            {
                Debug.LogError("[GameManager] No Board found in scene to replace for boss stage!");
                return;
            }

            // Get the board GameObject
            GameObject boardObject = existingBoard.gameObject;
            
            // Destroy the existing Board component (before Start runs)
            DestroyImmediate(existingBoard);
            
            // Select boss type based on which boss number this is (cycles through available types)
            int bossNumber = m_CurrentStage / m_BossStageInterval;
            int bossTypeCount = 3; // Number of boss board types available
            int bossTypeIndex = (bossNumber - 1) % bossTypeCount;
            
            switch (bossTypeIndex)
            {
                case 0:
                    boardObject.AddComponent<CrumblingBoard>();
                    break;
                case 1:
                    boardObject.AddComponent<MovingBossBoard>();
                    break;
                case 2:
                    boardObject.AddComponent<MysterySpriteBoard>();
                    break;
                default:
                    boardObject.AddComponent<MovingBossBoard>();
                    break;
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
            // Generate random collectible selection for a single company
            var (bonuses, dangers) = GenerateRandomCollectibles("Prototype");
            m_ActiveBonuses = bonuses;
            m_ActiveDangers = dangers;

            // Select initial collectibles
            SelectNextBonus();
            SelectNextDanger();
        }

        /// <summary>
        /// Generates a random selection of bonuses and dangers for a company.
        /// Creates 3 random bonuses from a pool of 4, and 2 random dangers from a pool of 4.
        /// </summary>
        /// <param name="namePrefix">Prefix for the created GameObject names</param>
        /// <returns>Tuple of (bonuses, dangers) lists</returns>
        private (List<ICollectible> bonuses, List<ICollectible> dangers) GenerateRandomCollectibles(string namePrefix)
        {
            // Create prototype collectibles
            var newSquare = CreateCollectible<NewSquareCollectible>($"{namePrefix}_NewSquare");
            var drawGem = CreateCollectible<DrawGemCollectible>($"{namePrefix}_DrawGem");
            var heartPiece = CreateCollectible<HeartPieceCollectible>($"{namePrefix}_HeartPiece");
            var pristinePaint = CreateCollectible<PristinePaintCollectible>($"{namePrefix}_PristinePaint");
            var spark = CreateCollectible<SparkCollectible>($"{namePrefix}_Spark");
            var ghostSpark = CreateCollectible<GhostSparkCollectible>($"{namePrefix}_GhostSpark");
            var jumpingSpark = CreateCollectible<JumpingSparkCollectible>($"{namePrefix}_JumpingSpark");
            var flame = CreateCollectible<FlameCollectible>($"{namePrefix}_Flame");

            // Select 3 random bonuses and 2 random dangers
            var allBonuses = new List<ICollectible> { newSquare, drawGem, heartPiece, pristinePaint };
            var allDangers = new List<ICollectible> { spark, ghostSpark, jumpingSpark, flame };

            var selectedBonuses = allBonuses.OrderBy(x => Random.value).Take(3).ToList();
            var selectedDangers = allDangers.OrderBy(x => Random.value).Take(2).ToList();

            // Destroy unselected collectibles to prevent memory leaks
            foreach (var bonus in allBonuses.Except(selectedBonuses))
            {
                Destroy((bonus as MonoBehaviour)?.gameObject);
            }
            foreach (var danger in allDangers.Except(selectedDangers))
            {
                Destroy((danger as MonoBehaviour)?.gameObject);
            }
            return (selectedBonuses, selectedDangers);
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
            if (m_CollectiblesDeck == null)
            {
                Debug.LogError("[GameManager] Cannot update collectibles - CollectiblesDeck is null!");
                return;
            }

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

        /// <summary>
        /// Calculates the cumulative required score for a given stage.
        /// Level 1: 40, Level 2: 40+45=85, Level 3: 40+45+50=135, etc.
        /// Each level requires (base + increment * level) = (35 + 5 * level)
        /// </summary>
        private int CalculateRequiredScore(int _stage)
        {
            // Sum of (base + increment*i) for i from 1 to stage
            // = base*stage + increment*(1+2+...+stage)
            // = base*stage + increment*stage*(stage+1)/2
            int requiredScore = m_BaseRequiredScore * _stage + m_RequiredScoreIncrement * _stage * (_stage + 1) / 2;
            return requiredScore;
        }

        /// <summary>
        /// Checks if the player has met the required score for the completed stage.
        /// Triggers game over if the score requirement is not met.
        /// </summary>
        /// <param name="_completedStage">The stage that was just completed</param>
        private void CheckScoreRequirement(int _completedStage)
        {
            int requiredScore = CalculateRequiredScore(_completedStage);
            if (m_CumulativeScore < requiredScore)
            {
                TriggerGameOver();
            }
            else
            {
                // Score requirement met, proceed to transition
                SceneManager.LoadScene(m_TransitionSceneName);
            }
        }

        /// <summary>
        /// Gets the cached Board reference, finding it if not yet cached.
        /// </summary>
        private Board GetBoard()
        {
            if (m_Board == null)
            {
                m_Board = FindFirstObjectByType<Board>();
            }
            return m_Board;
        }

        /// <summary>
        /// Triggers the game over state - plays lose sound and loads the GameOver scene.
        /// </summary>
        private void TriggerGameOver()
        {
            SoundFXManager.instance.PlaySoundFXClip(GameResources.Instance.LoseSoundFX, transform);
            SceneManager.LoadScene("GameOver");
        }
        #endregion

        #region Public Methods
        public void StartNewGame()
        {
            // Reset game state
            m_CurrentStage = 1;
            m_CumulativeScore = 0;
            m_MaxLives = c_MaxLives;  // Reset max lives to constant value
            m_CurrentLives = m_MaxLives;
            m_StageScoreBonus = 0;
            
            // Set flag to clear decks when gameplay scene loads
            m_IsStartingNewGame = true;
            
            // Clear decks now if they exist (they might not exist yet if instantiated in CompanySelect scene)
            if (m_CollectiblesDeck != null)
            {
                m_CollectiblesDeck.ClearDeck();
            }
            
            if (m_Deck != null)
            {
                m_Deck.IsInitialized = false;
                m_Deck.Initialize();
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

        public void CompleteStage()
        {
            m_IsStageComplete = true;
            
            float multiplier = 1f;
            if (m_Timer != null)
            {
                if (m_Timer.GetTimeRemaining() > 0)
                {
                    multiplier = m_Timer.GetCurrentMultiplier();
                }
                m_Timer.StopTimer();
            }
            
            // Calculate base score from board (clamped to 0 minimum)
            Board board = GetBoard();
            int baseScore = board != null ? Mathf.Max(0, board.CalculateTotalScore()) : 0;
            
            // Add bonus to base score before multiplier
            int scoreWithBonus = baseScore + m_StageScoreBonus;
            int stageScore = Mathf.RoundToInt(scoreWithBonus * multiplier);
            m_CumulativeScore += stageScore;
            
            var scoringPopup = FindFirstObjectByType<ScoringPopupUI>();
            if (scoringPopup != null)
            {
                // Capture current stage for score check (before increment)
                int completedStage = m_CurrentStage;
                scoringPopup.OnPopupComplete.AddListener(() => {
                    m_CurrentStage++;
                    CheckScoreRequirement(completedStage);
                });
                
                int requiredScore = CalculateRequiredScore(completedStage);
                scoringPopup.ShowScoring(scoreWithBonus, multiplier, stageScore, m_CumulativeScore, requiredScore);
            }
            
            // Reset the bonus for next stage
            m_StageScoreBonus = 0;

            // Update collectibles for next stage
            UpdateCollectibles();
        }

        private int GetTotalTilesPlaced()
        {
            Board board = GetBoard();
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
            if (amount > 0)
            {
                SoundFXManager.instance.PlaySoundFXClip(GameResources.Instance.DamageSoundFX, transform);
            }
            if (m_ResourceUI != null)
            {
                m_ResourceUI.UpdateLives(m_CurrentLives);
            }
            
            if (m_CurrentLives < 1)
            {
                TriggerGameOver();
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
            if (m_CollectiblesDeck == null)
            {
                Debug.LogError("[GameManager] Cannot get collectibles for stage - CollectiblesDeck is null!");
                return new List<ICollectible>();
            }

            // Reset the deck for the new stage
            m_CollectiblesDeck.ResetForNewStage();
            return m_CollectiblesDeck.GetCollectibles();
        }

        public void PauseGame()
        {
            if (m_IsPaused) return;
            
            m_IsPaused = true;
            
            // Pause timer
            if (m_Timer != null)
            {
                m_Timer.PauseTimer();
            }
        }

        public void ResumeGame()
        {
            if (!m_IsPaused) return;
            
            m_IsPaused = false;
            
            // Resume timer
            if (m_Timer != null)
            {
                m_Timer.ResumeTimer();
            }
        }

        private void OnShowTooltip(bool show)
        {
            if (!show) return; // Ignore key release
                        
            var board = GetBoard();
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

        private void OnPausePressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            // Find CraftingUI if not cached (include inactive objects)
            if (m_CraftingUI == null)
            {
                m_CraftingUI = FindFirstObjectByType<CraftingUI>(FindObjectsInactive.Include);
            }

            if (m_CraftingUI != null)
            {
                // Toggle visibility (ShowUI/HideUI will handle pause/resume)
                bool isVisible = m_CraftingUI.gameObject.activeSelf;
                if (isVisible)
                {
                    m_CraftingUI.HideUI();
                }
                else
                {
                    m_CraftingUI.ShowUI();
                }
            }
            else
            {
                Debug.LogWarning("CraftingUI not found in scene!");
            }
        }

        public List<Data.CompanyData> GenerateCompanyOptions()
        {
            var companies = new List<Data.CompanyData>();
            
            // Generate 3 unique company names using v0.11 API
            var companyNames = Data.CompanyNameGenerator.GenerateCompanyNames(3);
            
            for (int i = 0; i < 3; i++)
            {
                // Generate random collectibles for this company using the shared logic
                var (bonuses, dangers) = GenerateRandomCollectibles($"Company{i}");
                
                companies.Add(new Data.CompanyData(companyNames[i], bonuses, dangers));
            }
            
            return companies;
        }

        public void SetSelectedCompany(Data.CompanyData company)
        {
            m_CompanyName = company.Name;
            m_ActiveBonuses = company.Bonuses;
            m_ActiveDangers = company.Dangers;
            
            // Select initial collectibles
            SelectNextBonus();
            SelectNextDanger();
        }

        public void StartGameWithCompany(Data.CompanyData company)
        {
            SetSelectedCompany(company);
            StartNewGame();
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