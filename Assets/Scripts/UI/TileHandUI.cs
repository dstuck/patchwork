using UnityEngine;
using UnityEngine.UI;
using Patchwork.Gameplay;
using Patchwork.Data;
using TMPro;

namespace Patchwork.UI
{
    public class TileHandUI : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private TileHand m_TileHand;
        [SerializeField] private GameObject m_TilePreviewPrefab;
        [SerializeField] private RectTransform m_TileContainer;
        [SerializeField] private float m_TileSpacing = 10f;
        [SerializeField] private Color m_SelectedTileOutlineColor = Color.white;
        
        private TilePreview[] m_TilePreviews;
        private HorizontalLayoutGroup m_LayoutGroup;
        private TileData m_LastSelectedTile;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            m_LayoutGroup = m_TileContainer.GetComponent<HorizontalLayoutGroup>();
            if (m_LayoutGroup != null)
            {
                m_LayoutGroup.spacing = m_TileSpacing;
            }
        }

        private void OnEnable()
        {
            if (m_TileHand != null)
            {
                Debug.Log("TileHandUI: Subscribing to OnTileChanged");
                m_TileHand.OnTileChanged += UpdateSelection;
            }
            else
            {
                Debug.LogError("TileHandUI: m_TileHand is null!");
            }
        }

        private void OnDisable()
        {
            if (m_TileHand != null)
            {
                m_TileHand.OnTileChanged -= UpdateSelection;
            }
        }

        private void Start()
        {
            CreateTilePreviews();
            UpdateSelection();
        }
        #endregion

        #region Private Methods
        private void CreateTilePreviews()
        {
            // Clean up existing previews
            foreach (Transform child in m_TileContainer)
            {
                Destroy(child.gameObject);
            }

            int tileCount = m_TileHand.GetTileCount();
            m_TilePreviews = new TilePreview[tileCount];

            for (int i = 0; i < tileCount; i++)
            {
                GameObject previewObj = Instantiate(m_TilePreviewPrefab, m_TileContainer);
                TilePreview preview = previewObj.GetComponent<TilePreview>();
                
                if (preview != null)
                {
                    preview.Initialize(m_TileHand.GetTileAt(i));
                    m_TilePreviews[i] = preview;
                }
            }
        }

        private void UpdateSelection()
        {
            Debug.Log("TileHandUI: Updating selection");
            if (m_TilePreviews == null)
            {
                Debug.LogError("TileHandUI: m_TilePreviews is null!");
                return;
            }

            for (int i = 0; i < m_TilePreviews.Length; i++)
            {
                if (m_TilePreviews[i] != null)
                {
                    bool isSelected = m_TileHand.GetTileAt(i) == m_TileHand.CurrentTile;
                    Debug.Log($"Tile {i} selected: {isSelected}");
                    m_TilePreviews[i].SetSelected(isSelected);
                }
            }
        }
        #endregion
    }
} 