using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Gameplay
{
    public class Deck : MonoBehaviour
    {
        #region Private Fields
        private List<TileData> m_DeckTiles = new List<TileData>();
        private List<TileData> m_DrawPile = new List<TileData>();
        private const string c_TilesPath = "Data/BaseTiles";  // Path relative to Resources folder
        private static Deck s_Instance;
        private bool m_IsInitialized;
        [SerializeField] private int m_InitialTileCount = 6;  // Add this field
        [SerializeField] private TileHand m_TileHand;  // Reference to TileHand
        public System.Action OnDeckChanged;  // Event for when deck contents change
        #endregion

        #region Public Properties
        public static Deck Instance => s_Instance;
        public bool IsInitialized => m_IsInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!m_IsInitialized)
            {
                LoadTilesFromResources();
                InitializeDeck();
                m_IsInitialized = true;
            }
            
            // Find TileHand if not set
            if (m_TileHand == null)
            {
                m_TileHand = FindFirstObjectByType<TileHand>();
            }
        }
        #endregion

        #region Private Methods
        private void LoadTilesFromResources()
        {
            TileData[] tiles = Resources.LoadAll<TileData>(c_TilesPath);
            if (tiles != null && tiles.Length > 0)
            {
                m_DeckTiles.Clear();
                // Take random subset of tiles
                List<TileData> shuffledTiles = new List<TileData>(tiles);
                for (int i = shuffledTiles.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    TileData temp = shuffledTiles[i];
                    shuffledTiles[i] = shuffledTiles[j];
                    shuffledTiles[j] = temp;
                }

                // Take first N tiles
                var selectedTiles = shuffledTiles.Take(m_InitialTileCount).ToList();
                
                // Create copies of tiles
                for (int i = 0; i < selectedTiles.Count; i++)
                {
                    TileData tileCopy = Instantiate(selectedTiles[i]);
                                        
                    m_DeckTiles.Add(tileCopy);
                }
            }
            else
            {
                Debug.LogError($"No tiles found in Resources/{c_TilesPath}");
            }
        }

        private void InitializeDeck()
        {
            // Reset draw pile with current deck tiles
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckTiles);
            ShuffleDeck();
        }

        private void ShuffleDeck()
        {
            int n = m_DrawPile.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                TileData temp = m_DrawPile[k];
                m_DrawPile[k] = m_DrawPile[n];
                m_DrawPile[n] = temp;
            }
        }
        #endregion

        #region Public Methods
        public TileData DrawTile()
        {
            if (m_DrawPile.Count == 0)
            {
                return null;
            }

            TileData drawnTile = m_DrawPile[m_DrawPile.Count - 1];
            m_DrawPile.RemoveAt(m_DrawPile.Count - 1);
            
            // Add tile to hand automatically
            if (m_TileHand != null)
            {
                m_TileHand.AddTile(drawnTile);
            }
            
            OnDeckChanged?.Invoke();  // Trigger the event
            return drawnTile;
        }

        public void ResetForNewStage()
        {            
            // Make sure draw pile has all available tiles
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckTiles);
            ShuffleDeck();
            
            // Find the new TileHand instance in the new scene
            m_TileHand = FindFirstObjectByType<TileHand>();
            
            // Draw initial hand if we have a TileHand reference
            if (m_TileHand != null)
            {
                for (int i = 0; i < m_TileHand.HandSize; i++)
                {
                    DrawTile();
                }
            }
            else
            {
                Debug.LogError("[Deck] Could not find TileHand in scene!");
            }
        }

        public int GetRemainingTileCount()
        {
            return m_DrawPile.Count;
        }

        public List<TileData> GetTiles()
        {
            return new List<TileData>(m_DrawPile);
        }

        public void AddTileToDeck(TileData _tileData)
        {
            if (_tileData != null)
            {
                m_DeckTiles.Add(_tileData);
                m_DrawPile.Add(_tileData);
                ShuffleDeck();
            }
        }

        public void Initialize()
        {
            if (m_IsInitialized) return;
            
            LoadTilesFromResources();
            ResetDeck();
            m_IsInitialized = true;
        }

        public void ResetDeck()
        {
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckTiles);
            ShuffleDeck();
        }
        #endregion
    }
} 