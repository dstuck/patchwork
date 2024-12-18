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
                SetupUI();
                GenerateRewardOptions();
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
            m_StageText.text = $"Stage {GameManager.Instance.CurrentStage} Complete!";
            m_ScoreText.text = $"Total Score: {GameManager.Instance.CumulativeScore}";
            m_ContinueButtonText.text = "Next Stage";
            m_ContinueButton.onClick.AddListener(OnContinueClicked);
            m_RewardPromptText.text = "Select a tile to add to your deck:";
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

            
            // Get 3 random tiles
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = Random.Range(0, allTiles.Length);
                m_RewardOptions.Add(allTiles[randomIndex]);
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
                
                // Set a fixed size for the preview
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = new Vector2(100f, 100f); // Adjust these values as needed
                }

                TilePreview preview = previewObj.GetComponent<TilePreview>();
                if (preview != null)
                {
                    preview.Initialize(tileData);
                    m_RewardPreviews.Add(preview);
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
            if (m_SelectedRewardIndex >= 0 && m_SelectedRewardIndex < m_RewardOptions.Count)
            {
                TileData selectedTile = m_RewardOptions[m_SelectedRewardIndex];
                if (selectedTile != null && GameManager.Instance?.Deck != null)
                {
                    GameManager.Instance.Deck.AddTileToDeck(selectedTile);
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
        #endregion
    }
} 