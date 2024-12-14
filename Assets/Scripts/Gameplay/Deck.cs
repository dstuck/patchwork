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
        #endregion

        #region Public Methods
        public void Initialize()
        {
            InitializeDeck();
        }

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
            Debug.Log($"Resetting deck. Base deck size: {m_DeckTiles.Count}");
            InitializeDeck();
            Debug.Log($"After reset. Draw pile size: {m_DrawPile.Count}");
        }

        public int GetRemainingTileCount()
        {
            return m_DrawPile.Count;
        }

        public List<TileData> GetTiles()
        {
            Debug.Log($"GetTiles called. Draw pile size: {m_DrawPile.Count}");
            return new List<TileData>(m_DrawPile);
        }

        public void AddTileToDeck(TileData _tileData)
        {
            if (_tileData != null && !m_DeckTiles.Contains(_tileData))
            {
                m_DeckTiles.Add(_tileData);
                m_DrawPile.Add(_tileData);
                Debug.Log($"Added tile to deck. Base deck size: {m_DeckTiles.Count}, Draw pile size: {m_DrawPile.Count}");
                ShuffleDeck();
            }
        }
        #endregion

        #region Private Methods
        private void InitializeDeck()
        {
            Debug.Log($"Initializing deck. Base deck size: {m_DeckTiles.Count}");
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckTiles);
            ShuffleDeck();
            Debug.Log($"After initialize. Draw pile size: {m_DrawPile.Count}");
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
    }
} 