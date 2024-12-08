using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;

namespace Patchwork.Gameplay
{
    public class Deck : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private List<TileData> m_DeckTiles = new List<TileData>();
        private List<TileData> m_DrawPile = new List<TileData>();
        private List<TileData> m_DiscardPile = new List<TileData>();
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeDeck();
        }
        #endregion

        #region Private Methods
        private void InitializeDeck()
        {
            m_DrawPile.Clear();
            m_DiscardPile.Clear();
            
            // Copy all tiles to draw pile
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
                if (m_DiscardPile.Count > 0)
                {
                    // Shuffle discard pile into draw pile
                    m_DrawPile.AddRange(m_DiscardPile);
                    m_DiscardPile.Clear();
                    ShuffleDeck();
                }
                else
                {
                    return null; // No cards left
                }
            }

            TileData drawnTile = m_DrawPile[m_DrawPile.Count - 1];
            m_DrawPile.RemoveAt(m_DrawPile.Count - 1);
            return drawnTile;
        }

        public void DiscardTile(TileData tile)
        {
            if (tile != null)
            {
                m_DiscardPile.Add(tile);
            }
        }

        public int GetRemainingTileCount()
        {
            return m_DrawPile.Count + m_DiscardPile.Count;
        }
        #endregion

        #region Public Methods
        public IEnumerable<TileData> GetAllTiles()
        {
            List<TileData> allTiles = new List<TileData>();
            allTiles.AddRange(m_DrawPile);
            allTiles.AddRange(m_DiscardPile);
            return allTiles;
        }
        #endregion
    }
} 