using UnityEngine;
using Patchwork.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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
        private bool m_IsInitialized = false;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Remove InitializeTileHand call from Start
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Only initialize if this is the gameplay scene
            if (!m_IsInitialized && scene.name == "GameplayScene")
            {
                // Wait one frame to ensure GameManager is ready
                StartCoroutine(InitializeNextFrame());
            }
        }

        private IEnumerator InitializeNextFrame()
        {
            yield return null; // Wait one frame

            if (GameManager.Instance != null && GameManager.Instance.Deck != null)
            {
                m_Deck = GameManager.Instance.Deck;
                
                Debug.Log($"TileHand initialized with deck. Available tiles in deck: {m_Deck.GetRemainingTileCount()}");

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
                
                m_IsInitialized = true;
            }
            else
            {
                Debug.LogError("TileHand: GameManager or Deck still not available after waiting!");
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