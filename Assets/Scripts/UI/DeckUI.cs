using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    public class DeckUI : MonoBehaviour
    {
        #region Private Fields
        private Deck m_Deck;
        [SerializeField] private Button m_DeckButton;
        [SerializeField] private Button m_CloseButton;
        [SerializeField] private TextMeshProUGUI m_TileCountText;
        [SerializeField] private GameObject m_DeckPopup;
        [SerializeField] private RectTransform m_PopupTileContainer;
        [SerializeField] private GameObject m_TilePreviewPrefab;
        [SerializeField] private float m_TileSpacing = 10f;
        private GridLayoutGroup m_GridLayout;
        private bool m_IsInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Try to get Deck from GameManager first
            if (GameManager.Instance != null)
            {
                m_Deck = GameManager.Instance.Deck;
            }
            
            // Fallback to finding in scene if needed
            if (m_Deck == null)
            {
                m_Deck = FindFirstObjectByType<Deck>();
            }

            if (m_Deck == null)
            {
                Debug.LogError("DeckUI: Could not find Deck reference!");
                return;
            }

            InitializeUI();
        }

        private void InitializeUI()
        {
            m_DeckButton.onClick.AddListener(TogglePopup);
            m_CloseButton.onClick.AddListener(() => m_DeckPopup.SetActive(false));
            m_DeckPopup.SetActive(false);
            
            m_GridLayout = m_PopupTileContainer.GetComponent<GridLayoutGroup>();
            if (m_GridLayout != null)
            {
                m_GridLayout.spacing = new Vector2(m_TileSpacing, m_TileSpacing);
            }
            
            m_IsInitialized = true;
        }

        private void Start()
        {
            if (m_IsInitialized)
            {
                UpdateTileCount();
            }
        }

        private void OnEnable()
        {
            if (m_IsInitialized && m_Deck != null)
            {
                m_Deck.OnDeckChanged += UpdateTileCount;
                UpdateTileCount();
            }
        }

        private void OnDisable()
        {
            if (m_Deck != null)
            {
                m_Deck.OnDeckChanged -= UpdateTileCount;
            }
        }
        #endregion

        #region Private Methods
        private void UpdateTileCount()
        {
            if (m_TileCountText != null)
            {
                m_TileCountText.text = m_Deck.GetRemainingTileCount().ToString();
            }
        }

        private void TogglePopup()
        {
            m_DeckPopup.SetActive(!m_DeckPopup.activeSelf);
            if (m_DeckPopup.activeSelf)
            {
                RefreshDeckDisplay();
            }
        }

        private void RefreshDeckDisplay()
        {
            // Clear existing previews
            foreach (Transform child in m_PopupTileContainer)
            {
                Destroy(child.gameObject);
            }

            // Get all tiles from deck
            var allTiles = m_Deck.GetTiles();
            foreach (var tile in allTiles)
            {
                GameObject previewObj = Instantiate(m_TilePreviewPrefab, m_PopupTileContainer);
                TilePreview preview = previewObj.GetComponent<TilePreview>();
                if (preview != null)
                {
                    preview.Initialize(tile);
                }
            }
        }
        #endregion
    }
} 