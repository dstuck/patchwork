using UnityEngine;
using System.Collections.Generic;

namespace Patchwork.UI
{
    public class PlayerResourceUI : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private GameObject m_HeartPrefab;
        [SerializeField] private Transform m_HeartsContainer;
        private List<GameObject> m_HeartObjects = new List<GameObject>();
        #endregion

        #region Public Methods
        public void Initialize(int maxLives)
        {
            // Clear any existing hearts
            foreach (var heart in m_HeartObjects)
            {
                Destroy(heart);
            }
            m_HeartObjects.Clear();

            // Create new hearts
            for (int i = 0; i < maxLives; i++)
            {
                GameObject heart = Instantiate(m_HeartPrefab, m_HeartsContainer);
                m_HeartObjects.Add(heart);
            }
        }

        public void UpdateLives(int currentLives)
        {
            for (int i = 0; i < m_HeartObjects.Count; i++)
            {
                m_HeartObjects[i].SetActive(i < currentLives);
            }
        }
        #endregion
    }
} 