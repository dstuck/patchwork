using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;

namespace Patchwork.Gameplay
{
    public class TileHand : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private List<TileData> m_AvailableTiles = new List<TileData>();
        [SerializeField] private TileData m_CurrentTile;
        private int m_CurrentTileIndex;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Initialize with first tile
            if (m_AvailableTiles.Count > 0)
            {
                m_CurrentTileIndex = 0;
                m_CurrentTile = m_AvailableTiles[0];
            }
        }
        #endregion

        #region Public Properties
        public TileData CurrentTile => m_CurrentTile;
        #endregion

        #region Public Methods
        public void CycleToNextTile()
        {
            if (m_AvailableTiles.Count == 0) return;
            
            m_CurrentTileIndex = (m_CurrentTileIndex + 1) % m_AvailableTiles.Count;
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
        }

        public void CycleToPreviousTile()
        {
            if (m_AvailableTiles.Count == 0) return;
            
            m_CurrentTileIndex--;
            if (m_CurrentTileIndex < 0) 
                m_CurrentTileIndex = m_AvailableTiles.Count - 1;
            
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
        }

        public bool SelectTile(int _index)
        {
            if (_index < 0 || _index >= m_AvailableTiles.Count) return false;
            
            m_CurrentTileIndex = _index;
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
            return true;
        }

        public int GetTileCount()
        {
            return m_AvailableTiles.Count;
        }
        #endregion
    }
} 