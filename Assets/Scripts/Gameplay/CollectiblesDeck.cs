using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Gameplay
{
    [RequireComponent(typeof(CollectiblesDeck))]
    public class CollectiblesDeck : MonoBehaviour
    {
        #region Private Fields
        private List<ICollectible> m_DeckCollectibles = new List<ICollectible>();
        private List<ICollectible> m_DrawPile = new List<ICollectible>();
        private static CollectiblesDeck s_Instance;
        private bool m_IsInitialized;
        #endregion

        #region Public Properties
        public static CollectiblesDeck Instance => s_Instance;
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
        #endregion

        #region Private Methods
        private void ShuffleDeck()
        {
            int n = m_DrawPile.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                ICollectible temp = m_DrawPile[k];
                m_DrawPile[k] = m_DrawPile[n];
                m_DrawPile[n] = temp;
            }
        }
        #endregion

        #region Public Methods
        public void AddCollectibleToDeck(ICollectible collectible)
        {
            if (collectible != null)
            {
                m_DeckCollectibles.Add(collectible);
                m_DrawPile.Add(collectible);
                ShuffleDeck();
            }
        }

        public List<ICollectible> GetCollectibles()
        {
            return new List<ICollectible>(m_DrawPile);
        }

        public void ResetForNewStage()
        {
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckCollectibles);
            ShuffleDeck();
        }

        public void Initialize()
        {
            if (m_IsInitialized) return;
            
            ResetDeck();
            m_IsInitialized = true;
        }

        public void ResetDeck()
        {
            m_DrawPile.Clear();
            m_DrawPile.AddRange(m_DeckCollectibles);
            ShuffleDeck();
        }

        public void ClearDeck()
        {
            m_DeckCollectibles.Clear();
            m_DrawPile.Clear();
        }
        #endregion
    }
} 