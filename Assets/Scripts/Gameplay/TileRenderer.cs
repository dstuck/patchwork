using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class TileRenderer : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private TileData m_TileData;
        [SerializeField] private GridSettings m_GridSettings;
        private SpriteRenderer[] m_SquareRenderers;
        #endregion

        private void Awake()
        {
            // Load GridSettings if not assigned
            if (m_GridSettings == null)
            {
                m_GridSettings = Resources.Load<GridSettings>("GridSettings");
                if (m_GridSettings == null)
                {
                    Debug.LogError("GridSettings not found in Resources folder!");
                    return;
                }
            }
        }

        #region Public Methods
        public void Initialize(TileData _tileData, Color _color)
        {
            m_TileData = _tileData;
            CreateVisuals(_color);
        }

        public void UpdateColor(Color _color)
        {
            if (m_SquareRenderers == null) return;
            
            foreach (var renderer in m_SquareRenderers)
            {
                renderer.color = _color;
            }
        }
        #endregion

        #region Private Methods
        private void CreateVisuals(Color _color)
        {
            // Clean up any existing visuals
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            if (m_TileData == null) return;

            // Get the sprite reference
            Sprite squareSprite = GameResources.Instance.TileSquareSprite;
            if (squareSprite == null)
            {
                Debug.LogError("Tile square sprite not set in GameResources!");
                return;
            }

            // Create new visuals for each square
            Vector2Int[] squares = m_TileData.Squares;
            m_SquareRenderers = new SpriteRenderer[squares.Length];

            for (int i = 0; i < squares.Length; i++)
            {
                GameObject square = new GameObject($"Square_{i}");
                square.transform.SetParent(transform);
                square.transform.localPosition = new Vector3(
                    squares[i].x * m_GridSettings.CellSize,
                    squares[i].y * m_GridSettings.CellSize,
                    0
                );
                square.transform.localScale = Vector3.one;

                SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
                renderer.sprite = squareSprite;
                renderer.color = _color;
                m_SquareRenderers[i] = renderer;
            }
        }
        #endregion
    }
}