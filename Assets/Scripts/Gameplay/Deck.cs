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
                m_IsInitialized = true;
            }
            InitializeDeck();
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
                m_DeckTiles.AddRange(shuffledTiles.Take(m_InitialTileCount));
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
            return drawnTile;
        }

        public void ResetForNewStage()
        {            
            // Make sure draw pile has all available tiles
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckTiles);
            ShuffleDeck();
            
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