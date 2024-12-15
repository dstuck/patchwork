using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;

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
        #endregion

        #region Public Properties
        public static Deck Instance => s_Instance;
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
            Debug.Log($"Deck Start - IsInitialized: {m_IsInitialized}, Current Deck Size: {m_DeckTiles.Count}");
            if (!m_IsInitialized)
            {
                LoadTilesFromResources();
                m_IsInitialized = true;
            }
            InitializeDeck();
            Debug.Log($"Deck After Init - Deck Size: {m_DeckTiles.Count}, Draw Pile Size: {m_DrawPile.Count}");
        }
        #endregion

        #region Private Methods
        private void LoadTilesFromResources()
        {
            TileData[] tiles = Resources.LoadAll<TileData>(c_TilesPath);
            if (tiles != null && tiles.Length > 0)
            {
                m_DeckTiles.Clear();
                m_DeckTiles.AddRange(tiles);
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
            Debug.Log($"Resetting deck for new stage. Current deck size: {m_DeckTiles.Count}");
            
            // Make sure draw pile has all available tiles
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckTiles);
            ShuffleDeck();
            
            Debug.Log($"After reset. Draw pile size: {m_DrawPile.Count}, DeckTiles size: {m_DeckTiles.Count}");
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
                Debug.Log($"Added tile to deck. New deck size: {m_DeckTiles.Count}");
            }
        }
        #endregion
    }
} 