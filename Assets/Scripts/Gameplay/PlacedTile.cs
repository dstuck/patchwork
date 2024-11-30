using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PlacedTile : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private TileData m_TileData;
        private Vector2Int m_GridPosition;
        private TileRenderer m_TileRenderer;
        #endregion

        #region Public Methods
        public void Initialize(TileData _tileData, Vector2Int _gridPosition)
        {
            m_TileData = _tileData;
            m_GridPosition = _gridPosition;
            m_TileRenderer = gameObject.AddComponent<TileRenderer>();
            m_TileRenderer.Initialize(m_TileData, m_TileData.TileColor);
        }
        #endregion
    } 
}