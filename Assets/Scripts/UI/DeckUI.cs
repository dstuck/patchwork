using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    public class DeckUI : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private Deck m_Deck;
        [SerializeField] private Button m_DeckButton;
        [SerializeField] private Button m_CloseButton;
        [SerializeField] private TextMeshProUGUI m_TileCountText;
        [SerializeField] private GameObject m_DeckPopup;
        [SerializeField] private RectTransform m_PopupTileContainer;
        [SerializeField] private GameObject m_TilePreviewPrefab;
        [SerializeField] private float m_TileSpacing = 10f;
        private GridLayoutGroup m_GridLayout;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            m_DeckButton.onClick.AddListener(TogglePopup);
            m_CloseButton.onClick.AddListener(() => m_DeckPopup.SetActive(false));
            m_DeckPopup.SetActive(false);
            
            m_GridLayout = m_PopupTileContainer.GetComponent<GridLayoutGroup>();
            if (m_GridLayout != null)
            {
                m_GridLayout.spacing = new Vector2(m_TileSpacing, m_TileSpacing);
            }
        }

        private void Start()
        {
            UpdateTileCount();
        }

        private void OnEnable()
        {
            UpdateTileCount();
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
            var allTiles = m_Deck.GetAllTiles();
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