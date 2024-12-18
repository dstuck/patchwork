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
        [SerializeField] private int m_HandSize = 3;
        private Deck m_Deck;  // Reference to Deck
        #endregion

        #region Public Properties
        public TileData CurrentTile => m_CurrentTile;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (m_Deck == null)
            {
                m_Deck = GameManager.Instance?.Deck;
            }
            InitializeTileHand();
        }

        private void OnEnable()
        {
            InitializeTileHand();
        }
        #endregion

        #region Private Methods
        private void InitializeTileHand()
        {
            if (m_Deck != null)
            {
                m_AvailableTiles.Clear();
                
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
        }
        #endregion

        #region Public Methods
        public void SetDeck(Deck _deck)
        {
            m_Deck = _deck;
            InitializeTileHand();
        }

        public void CycleToNextTile()
        {
            if (m_AvailableTiles.Count == 0) return;
            
            m_CurrentTileIndex = (m_CurrentTileIndex + 1) % m_AvailableTiles.Count;
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
            OnTileChanged?.Invoke();
        }

        public void CycleToPreviousTile()
        {
            if (m_AvailableTiles.Count == 0) return;
            
            m_CurrentTileIndex--;
            if (m_CurrentTileIndex < 0) 
                m_CurrentTileIndex = m_AvailableTiles.Count - 1;
            
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
            OnTileChanged?.Invoke();
        }

        public bool SelectTile(int _index)
        {
            if (_index < 0 || _index >= m_AvailableTiles.Count) return false;
            
            m_CurrentTileIndex = _index;
            m_CurrentTile = m_AvailableTiles[m_CurrentTileIndex];
            OnTileChanged?.Invoke();
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
                OnLastTilePlaced?.Invoke();
            }
        }
        #endregion
    }
} 