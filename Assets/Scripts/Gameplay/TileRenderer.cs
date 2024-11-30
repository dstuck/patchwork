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
        private int m_CurrentRotation;
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
            m_CurrentRotation = 0;
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

        public void UpdateRotation(int _rotation)
        {
            m_CurrentRotation = _rotation;
            
            // Store current color before recreating visuals
            Color currentColor = Color.white;
            if (m_SquareRenderers != null && m_SquareRenderers.Length > 0)
            {
                currentColor = m_SquareRenderers[0].color;
            }
            
            CreateVisuals(currentColor);
        }
        #endregion

        #region Private Methods
        private void CreateVisuals(Color _color)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            if (m_TileData == null) return;

            Vector2Int[] squares = m_TileData.GetRotatedSquares(m_CurrentRotation);
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

                SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
                renderer.sprite = GameResources.Instance.TileSquareSprite;
                renderer.color = _color;
                m_SquareRenderers[i] = renderer;
            }
        }
        #endregion
    }
}