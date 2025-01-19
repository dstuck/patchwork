using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Patchwork.Gameplay;
using Patchwork.Data;
using System.Collections.Generic;
using Patchwork.Input;

namespace Patchwork.UI
{
    public class TransitionUI : MonoBehaviour
    {
        #region Private Fields
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_StageText;
        [SerializeField] private TextMeshProUGUI m_ScoreText;
        [SerializeField] private Button m_ContinueButton;
        [SerializeField] private TextMeshProUGUI m_ContinueButtonText;
        
        [Header("Reward UI")]
        [SerializeField] private RectTransform m_RewardTileContainer;
        [SerializeField] private GameObject m_TilePreviewPrefab;
        [SerializeField] private TextMeshProUGUI m_RewardPromptText;
        [SerializeField] private float m_TileSpacing = 10f;
        
        [Header("Animation")]
        [SerializeField] private float m_FadeInDuration = 0.5f;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [Header("Input")]
        private float m_InputCooldown = 0.15f;  // Prevent double-inputs
        private float m_LastInputTime;

        private List<TilePreview> m_RewardPreviews = new List<TilePreview>();
        private List<TileData> m_RewardOptions = new List<TileData>();
        private int m_SelectedRewardIndex = 0;
        private const string c_TilesPath = "Data/BaseTiles";
        private GameControls m_Controls;
        private HorizontalLayoutGroup m_LayoutGroup;
        private bool m_IsBossReward;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }

            m_LayoutGroup = m_RewardTileContainer.GetComponent<HorizontalLayoutGroup>();
            if (m_LayoutGroup == null)
            {
                m_LayoutGroup = m_RewardTileContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            SetupLayoutGroup();

            m_Controls = new GameControls();
            m_Controls.UI.Navigate.performed += OnNavigate;
            m_Controls.UI.Submit.performed += OnSubmit;
        }

        private void SetupLayoutGroup()
        {
            m_LayoutGroup.spacing = m_TileSpacing;
            m_LayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            m_LayoutGroup.childForceExpandWidth = true;
            m_LayoutGroup.childForceExpandHeight = true;
            // m_LayoutGroup.padding = new RectOffset(0, 10, 10, 10);
        }

        private void OnEnable()
        {
            m_Controls.Enable();
        }

        private void OnDisable()
        {
            m_Controls.Disable();
        }

        private void OnDestroy()
        {
            m_Controls.UI.Navigate.performed -= OnNavigate;
            m_Controls.UI.Submit.performed -= OnSubmit;
        }
        
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                m_IsBossReward = GameManager.Instance.IsPostBossStage;
                SetupUI();
                if (m_IsBossReward)
                {
                    GenerateBossRewardOptions();
                }
                else
                {
                    GenerateRewardOptions();
                }
                CreateRewardPreviews();
                StartCoroutine(FadeIn());
            }
            else
            {
                Debug.LogError("Failed to initialize GameManager!");
            }
        }
        #endregion

        #region Input Handlers
        private void OnNavigate(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (Time.time - m_LastInputTime < m_InputCooldown) return;
            
            Vector2 navigation = context.ReadValue<Vector2>();
            if (navigation.x > 0)
            {
                m_SelectedRewardIndex = Mathf.Min(m_RewardOptions.Count - 1, m_SelectedRewardIndex + 1);
                UpdateRewardSelection();
                m_LastInputTime = Time.time;
            }
            else if (navigation.x < 0)
            {
                m_SelectedRewardIndex = Mathf.Max(0, m_SelectedRewardIndex - 1);
                UpdateRewardSelection();
                m_LastInputTime = Time.time;
            }
        }

        private void OnSubmit(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (Time.time - m_LastInputTime < m_InputCooldown) return;
            
            m_LastInputTime = Time.time;
            OnContinueClicked();
        }
        #endregion

        #region Private Methods
        private void SetupUI()
        {
            m_StageText.text = $"Stage {GameManager.Instance.CurrentStage - 1} Complete!";
            m_ScoreText.text = $"Total Score: {GameManager.Instance.CumulativeScore}";
            m_ContinueButtonText.text = "Next Stage";
            m_ContinueButton.onClick.AddListener(OnContinueClicked);
            m_RewardPromptText.text = m_IsBossReward ? "Choose your reward:" : "Select a tile to add to your deck:";
        }

        private void GenerateBossRewardOptions()
        {
            m_RewardOptions.Clear();
            
            // Load boss reward tiles from Resources
            TileData[] bossRewards = Resources.LoadAll<TileData>("Data/BossRewards");
            
            if (bossRewards == null || bossRewards.Length == 0)
            {
                Debug.LogError($"No boss reward tiles found in Resources/Data/BossRewards");
                return;
            }

            // Add both reward options
            foreach (var rewardTile in bossRewards)
            {
                // Create a copy of the tile data to modify
                TileData rewardCopy = Instantiate(rewardTile);
                
                // Update the name and add upgrade based on reward type
                if (rewardCopy.name.Contains("Multiplier"))
                {
                    rewardCopy.name = $"x{GameManager.Instance.BaseMultiplier + 0.5f} Multiplier";
                    rewardCopy.AddUpgrade(new MultiplierBonus());
                }
                else if (rewardCopy.name.Contains("Points"))
                {
                    rewardCopy.name = "+2 Points";
                    rewardCopy.AddUpgrade(new PointsBonus());
                }
                
                m_RewardOptions.Add(rewardCopy);
            }
        }

        private void GenerateRewardOptions()
        {
            TileData[] allTiles = Resources.LoadAll<TileData>(c_TilesPath);
            m_RewardOptions.Clear();
            
            if (allTiles == null || allTiles.Length == 0)
            {
                Debug.LogError($"No tiles found in Resources/{c_TilesPath}");
                return;
            }

            // Get two random tiles
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, allTiles.Length);
                // Create a copy of the tile data to modify
                TileData rewardTile = Instantiate(allTiles[randomIndex]);
                
                // Apply appropriate upgrade based on index
                if (i == 0)
                {
                    rewardTile.AddUpgrade(new PristineBonus());
                }
                else if (i == 1)
                {
                    rewardTile.AddUpgrade(new LenientBonus());
                }
                
                m_RewardOptions.Add(rewardTile);
            }
        }

        private void CreateRewardPreviews()
        {
            if (m_RewardOptions.Count == 0)
            {
                Debug.LogError("No reward options available to create previews");
                return;
            }

            // Clean up existing previews
            foreach (Transform child in m_RewardTileContainer)
            {
                Destroy(child.gameObject);
            }
            m_RewardPreviews.Clear();

            // Create new previews
            foreach (var tileData in m_RewardOptions)
            {
                GameObject previewObj = Instantiate(m_TilePreviewPrefab, m_RewardTileContainer);
                RectTransform rectTransform = previewObj.GetComponent<RectTransform>();
                
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(100f, 100f);
                }

                TilePreview preview = previewObj.GetComponent<TilePreview>();
                if (preview != null)
                {
                    preview.Initialize(tileData);
                    m_RewardPreviews.Add(preview);

                    // Add tooltip trigger if tile has upgrades
                    if (tileData.Upgrades.Count > 0)
                    {
                        var tooltipTrigger = previewObj.AddComponent<TooltipTrigger>();
                        tooltipTrigger.Initialize(tileData.Upgrades[0]);
                        
                        // Add a Box Collider 2D for UI raycasting if not already present
                        if (!previewObj.TryGetComponent<BoxCollider2D>(out _))
                        {
                            var collider = previewObj.AddComponent<BoxCollider2D>();
                            collider.isTrigger = true;
                            collider.size = rectTransform.sizeDelta;
                        }
                    }
                }
            }

            UpdateRewardSelection();
        }

        private void UpdateRewardSelection()
        {
            for (int i = 0; i < m_RewardPreviews.Count; i++)
            {
                m_RewardPreviews[i].SetSelected(i == m_SelectedRewardIndex);
            }
        }

        private void OnContinueClicked()
        {
            if (m_IsBossReward)
            {
                // Apply boss reward based on the selected tile's name
                TileData selectedTile = m_RewardOptions[m_SelectedRewardIndex];
                if (selectedTile.name.Contains("Multiplier"))
                {
                    GameManager.Instance.IncreaseMultiplier();
                }
                else if (selectedTile.name.Contains("Points"))
                {
                    GameManager.Instance.IncreaseTilePoints();
                }
            }
            else
            {
                // Regular tile reward
                if (m_SelectedRewardIndex >= 0 && m_SelectedRewardIndex < m_RewardOptions.Count)
                {
                    GameManager.Instance.Deck.AddTileToDeck(m_RewardOptions[m_SelectedRewardIndex]);
                }
            }
            StartCoroutine(FadeOutAndContinue());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float elapsedTime = 0;
            m_CanvasGroup.alpha = 0;

            while (elapsedTime < m_FadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                m_CanvasGroup.alpha = elapsedTime / m_FadeInDuration;
                yield return null;
            }

            m_CanvasGroup.alpha = 1;
        }

        private System.Collections.IEnumerator FadeOutAndContinue()
        {
            float elapsedTime = 0;
            m_CanvasGroup.alpha = 1;

            while (elapsedTime < m_FadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                m_CanvasGroup.alpha = 1 - (elapsedTime / m_FadeInDuration);
                yield return null;
            }

            m_CanvasGroup.alpha = 0;
            GameManager.Instance.StartNextStage();
        }

        private bool IsBossStage(int _stageNumber)
        {
            return _stageNumber % GameManager.Instance.BossStageInterval == 0;
        }
        #endregion
    }

    // Move upgrade classes outside TransitionUI but inside namespace
    public class MultiplierBonus : ITileUpgrade
    {
        public string DisplayName => "Multiplier Boost";
        public string Description => "Increases score multiplier by 0.5x\nFaster scoring but less precise";
        public Color DisplayColor => Color.yellow;
        
        public int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            // This is just a visual upgrade for the reward choice, actual multiplier is handled by GameManager
            return _baseScore;
        }
    }

    public class PointsBonus : ITileUpgrade
    {
        public string DisplayName => "Points Boost";
        public string Description => "Increases base points by 2\nMore points per tile";
        public Color DisplayColor => Color.green;
        
        public int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            // This is just a visual upgrade for the reward choice, actual points bonus is handled by GameManager
            return _baseScore;
        }
    }
} 