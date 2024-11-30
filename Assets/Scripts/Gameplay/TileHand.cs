using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;

namespace Patchwork.Gameplay
{
    public class TileHand : MonoBehaviour
    {
        #region Events
        public System.Action OnTileChanged;  // Event for tile selection changes
        #endregion

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
                OnTileChanged?.Invoke();  // Notify about initial selection
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
            Debug.Log($"Cycled to next tile: {m_CurrentTile.name} at index {m_CurrentTileIndex}");
            OnTileChanged?.Invoke();  // Notify listeners
        }

        public void CycleToPreviousTile()
        {
            if (m_AvailableTiles.Count == 0) return;
            
            m_CurrentTileIndex--;
            if (m_CurrentTileIndex < 0) 
                m_CurrentTileIndex = m_AvailableTiles.Count - 1;
            
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
            Debug.Log($"Cycled to previous tile: {m_CurrentTile.name} at index {m_CurrentTileIndex}");
            OnTileChanged?.Invoke();  // Notify listeners
        }

        public bool SelectTile(int _index)
        {
            if (_index < 0 || _index >= m_AvailableTiles.Count) return false;
            
            m_CurrentTileIndex = _index;
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
            OnTileChanged?.Invoke();  // Notify listeners
            return true;
        }

        public int GetTileCount()
        {
            return m_AvailableTiles.Count;
        }

        public TileData GetTileAt(int _index)
        {
            if (_index < 0 || _index >= m_AvailableTiles.Count) return null;
            
            return m_AvailableTiles[_index];
        }

        public void RemoveCurrentTile()
        {
            if (m_AvailableTiles.Count == 0) return;

            m_AvailableTiles.RemoveAt(m_CurrentTileIndex);

            if (m_AvailableTiles.Count > 0)
            {
                m_CurrentTileIndex %= m_AvailableTiles.Count;
                m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
            }
            else
            {
                m_CurrentTile = null;
            }

            OnTileChanged?.Invoke();
        }
        #endregion
    }
} 