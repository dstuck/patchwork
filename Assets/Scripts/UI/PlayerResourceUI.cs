using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Patchwork.UI
{
    public class PlayerResourceUI : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private GameObject m_HeartPrefab;
        [SerializeField] private Transform m_HeartsContainer;
        private List<GameObject> m_Hearts = new List<GameObject>();
        private float m_HeartHeight;
        #endregion

        #region Public Methods
        public void Initialize(float maxLives)
        {
            // Clear any existing hearts
            foreach (var heart in m_Hearts)
            {
                Destroy(heart);
            }
            m_Hearts.Clear();

            // Create new hearts
            int totalHearts = Mathf.CeilToInt(maxLives);
            for (int i = 0; i < totalHearts; i++)
            {
                GameObject heart = Instantiate(m_HeartPrefab, m_HeartsContainer);
                m_Hearts.Add(heart);
            }

            // Store the heart height for masking calculations
            if (m_Hearts.Count > 0)
            {
                var rectTransform = m_Hearts[0].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    m_HeartHeight = rectTransform.rect.height;
                }
            }
        }

        public void UpdateLives(float currentLives)
        {
            int fullHearts = Mathf.FloorToInt(currentLives);
            float remainder = currentLives - fullHearts;

            // Add visual bias to make partial hearts look cleaner
            if (remainder > 0)
            {
                if (remainder < 0.3f) remainder = 0.3f;
                else if (remainder > 0.7f) remainder = 0.7f;
            }

            for (int i = 0; i < m_Hearts.Count; i++)
            {
                if (i < fullHearts)
                {
                    // Full hearts
                    m_Hearts[i].SetActive(true);
                }
                else if (i == fullHearts && remainder > 0)
                {
                    // Partial heart - show only the bottom portion
                    var mask = m_Hearts[i].GetComponent<RectMask2D>();
                    if (mask != null)
                    {
                        float padding = m_HeartHeight * (1 - remainder);  // Inverted the remainder
                        mask.padding = new Vector4(0, 0, 0, padding);  // Bottom padding
                    }
                    m_Hearts[i].SetActive(true);
                }
                else
                {
                    m_Hearts[i].SetActive(false);
                }
            }
        }
        #endregion
    }
} 