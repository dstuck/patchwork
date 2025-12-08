using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Patchwork.Gameplay;
using Patchwork.Data;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private GameObject m_CollectiblePreviewPrefab;
        [SerializeField] private TextMeshProUGUI m_RewardPromptText;
        [SerializeField] private float m_TileSpacing = 10f;
        
        [Header("Animation")]
        [SerializeField] private float m_FadeInDuration = 0.5f;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [Header("Input")]
        private float m_InputCooldown = 0.15f;  // Prevent double-inputs
        private float m_LastInputTime;

        private List<MonoBehaviour> m_RewardPreviews = new List<MonoBehaviour>();
        private List<TileData> m_TileRewardOptions = new List<TileData>();
        private List<ICollectible> m_CollectibleRewardOptions = new List<ICollectible>();
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
            int maxIndex = m_IsBossReward ? m_CollectibleRewardOptions.Count - 1 : m_TileRewardOptions.Count - 1;
            
            if (navigation.x > 0)
            {
                m_SelectedRewardIndex = Mathf.Min(maxIndex, m_SelectedRewardIndex + 1);
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
            m_CollectibleRewardOptions.Clear();
            m_CollectibleRewardOptions.Add(new MultiplierBonusCollectible());
            m_CollectibleRewardOptions.Add(new ScoreBonusCollectible());
        }

        private void GenerateRewardOptions()
        {
            m_TileRewardOptions.Clear();
            
            // Get active upgrades from GameManager
            var activeUpgrades = GameManager.Instance?.ActiveUpgrades;
            if (activeUpgrades == null || activeUpgrades.Count == 0)
            {
                Debug.LogWarning("No active upgrades available from GameManager");
            }
            
            // Get three random tiles using TileFactory
            for (int i = 0; i < 3; i++)
            {
                TileData rewardTile = TileFactory.CreateRandomTile();
                
                // Randomly assign an upgrade from the active upgrades list, or no upgrade
                if (activeUpgrades != null && activeUpgrades.Count > 0)
                {
                    // 70% chance of upgrade, 30% chance of no upgrade
                    if (Random.value < 0.7f)
                    {
                        int randomIndex = Random.Range(0, activeUpgrades.Count);
                        rewardTile.AddUpgrade(activeUpgrades[randomIndex]);
                    }
                }
                
                m_TileRewardOptions.Add(rewardTile);
            }
        }

        private void CreateRewardPreviews()
        {
            // Clean up existing previews
            foreach (Transform child in m_RewardTileContainer)
            {
                Destroy(child.gameObject);
            }
            m_RewardPreviews.Clear();

            if (m_IsBossReward)
            {
                if (m_CollectibleRewardOptions.Count == 0)
                {
                    Debug.LogError("No collectible reward options available");
                    return;
                }

                // Create collectible previews
                foreach (var collectible in m_CollectibleRewardOptions)
                {
                    GameObject previewObj = Instantiate(m_CollectiblePreviewPrefab, m_RewardTileContainer);
                    CollectiblePreview preview = previewObj.GetComponent<CollectiblePreview>();
                    if (preview != null)
                    {
                        preview.Initialize(collectible);
                        m_RewardPreviews.Add(preview);
                    }
                }
            }
            else
            {
                if (m_TileRewardOptions.Count == 0)
                {
                    Debug.LogError("No tile reward options available");
                    return;
                }

                // Create tile previews
                foreach (var tileData in m_TileRewardOptions)
                {
                    GameObject previewObj = Instantiate(m_TilePreviewPrefab, m_RewardTileContainer);
                    TilePreview preview = previewObj.GetComponent<TilePreview>();
                    if (preview != null)
                    {
                        preview.Initialize(tileData);
                        m_RewardPreviews.Add(preview);

                        if (tileData.Upgrades.Count > 0)
                        {
                            var tooltipTrigger = previewObj.AddComponent<TooltipTrigger>();
                            tooltipTrigger.Initialize(tileData.Upgrades[0]);
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
                if (m_IsBossReward)
                {
                    ((CollectiblePreview)m_RewardPreviews[i]).SetSelected(i == m_SelectedRewardIndex);
                }
                else
                {
                    ((TilePreview)m_RewardPreviews[i]).SetSelected(i == m_SelectedRewardIndex);
                }
            }
        }

        private void OnContinueClicked()
        {
            if (m_IsBossReward)
            {
                if (m_SelectedRewardIndex >= 0 && m_SelectedRewardIndex < m_CollectibleRewardOptions.Count)
                {
                    var deck = GameManager.Instance?.CollectiblesDeck;
                    if (deck != null)
                    {
                        deck.AddCollectibleToDeck(m_CollectibleRewardOptions[m_SelectedRewardIndex]);
                    }
                    else
                    {
                        Debug.LogError("CollectiblesDeck not found!");
                    }
                }
            }
            else
            {
                if (m_SelectedRewardIndex >= 0 && m_SelectedRewardIndex < m_TileRewardOptions.Count)
                {
                    GameManager.Instance.Deck.AddTileToDeck(m_TileRewardOptions[m_SelectedRewardIndex]);
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
} 