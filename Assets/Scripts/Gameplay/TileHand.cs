using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Gameplay
{
    public class TileHand : MonoBehaviour
    {
        #region Events
        public System.Action OnTileChanged;  // Event for tile selection changes
        public System.Action OnLastTilePlaced;  // Event for when hand is empty
        #endregion

        #region Private Fields
        [SerializeField] private List<TileData> m_AvailableTiles = new List<TileData>();
        [SerializeField] private TileData m_CurrentTile;
        private int m_CurrentTileIndex;
        [SerializeField] private Deck m_Deck;
        [SerializeField] private int m_HandSize = 3;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (m_Deck == null)
            {
                Debug.LogError("TileHand: Deck reference is missing!");
                return;
            }

            // Draw initial hand
            for (int i = 0; i < m_HandSize; i++)
            {
                TileData drawnTile = m_Deck.DrawTile();
                if (drawnTile != null)
                {
                    m_AvailableTiles.Add(drawnTile);
                }
            }

            if (m_AvailableTiles.Count > 0)
            {
                m_CurrentTileIndex = 0;
                m_CurrentTile = m_AvailableTiles[0];
                OnTileChanged?.Invoke();
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
            OnTileChanged?.Invoke();  // Notify listeners
        }

        public void CycleToPreviousTile()
        {
            if (m_AvailableTiles.Count == 0) return;
            
            m_CurrentTileIndex--;
            if (m_CurrentTileIndex < 0) 
                m_CurrentTileIndex = m_AvailableTiles.Count - 1;
            
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
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
                OnTileChanged?.Invoke();
            }
            else
            {
                m_CurrentTile = null;
                OnTileChanged?.Invoke();
                OnLastTilePlaced?.Invoke();  // Notify that the hand is empty
            }
        }

        #if UNITY_INCLUDE_TESTS
        public void SetDeck(Deck deck)
        {
            m_Deck = deck;
        }
        #endif
        #endregion
    }
} 