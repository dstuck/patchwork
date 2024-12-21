using UnityEngine;
using UnityEngine.UI;
using Patchwork.Data;

namespace Patchwork.UI
{
    public class TilePreview : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private Image m_OutlineImage;
        [SerializeField] private RectTransform m_TileContainer;
        [SerializeField] private GameObject m_SquarePrefab;
        [SerializeField] private float m_SquareSize = 20f;
        [SerializeField] private float m_SquareSpacing = 2f;
        [SerializeField] private Color m_SelectedColor = new Color(1f, 1f, 1f, 0.06f);  // ~15/255 alpha
        #endregion

        #region Public Methods
        public void Initialize(TileData _tileData)
        {
            // Clear existing squares
            foreach (Transform child in m_TileContainer)
            {
                if (child != m_OutlineImage.transform)
                    Destroy(child.gameObject);
            }

            // Ensure outline is initially disabled
            if (m_OutlineImage != null)
            {
                m_OutlineImage.enabled = false;
            }

            // Create preview squares
            foreach (Vector2Int pos in _tileData.Squares)
            {
                GameObject square = Instantiate(m_SquarePrefab, m_TileContainer);
                RectTransform rectTransform = square.GetComponent<RectTransform>();
                Image squareImage = square.GetComponent<Image>();
                
                if (squareImage != null)
                {
                    squareImage.sprite = _tileData.TileSprite;
                    squareImage.color = _tileData.TileColor;
                }
                
                rectTransform.anchoredPosition = new Vector2(
                    pos.x * (m_SquareSize + m_SquareSpacing),
                    pos.y * (m_SquareSize + m_SquareSpacing)
                );
                
                rectTransform.sizeDelta = new Vector2(m_SquareSize, m_SquareSize);
            }
        }

        public void SetSelected(bool _selected)
        {
            if (m_OutlineImage != null)
            {
                m_OutlineImage.enabled = _selected;
                if (_selected)
                {
                    m_OutlineImage.color = m_SelectedColor;
                }
            }
        }
        #endregion
    }
} 