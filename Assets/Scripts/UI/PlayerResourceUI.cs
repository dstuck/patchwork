using UnityEngine;
using System.Collections.Generic;

namespace Patchwork.UI
{
    public class PlayerResourceUI : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private GameObject m_HeartPrefab;
        [SerializeField] private Transform m_HeartsContainer;
        private List<GameObject> m_Hearts = new List<GameObject>();
        private List<SpriteRenderer> m_HeartRenderers = new List<SpriteRenderer>();
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

            // Store the heart height for scaling calculations
            if (m_Hearts.Count > 0)
            {
                var renderer = m_Hearts[0].GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    m_HeartHeight = renderer.bounds.size.y;
                }
            }
        }

        public void UpdateLives(float currentLives)
        {
            int fullHearts = Mathf.FloorToInt(currentLives);
            float remainder = currentLives - fullHearts;

            for (int i = 0; i < m_Hearts.Count; i++)
            {
                if (i < fullHearts)
                {
                    m_Hearts[i].transform.localScale = Vector3.one;
                    m_Hearts[i].transform.localPosition = Vector3.zero;
                    m_Hearts[i].SetActive(true);
                }
                else if (i == fullHearts && remainder > 0)
                {
                    // Show partial heart from top down
                    m_Hearts[i].transform.localScale = new Vector3(1, remainder, 1);
                    // Move the heart up to align with the top
                    float offset = m_HeartHeight * (1 - remainder) * 0.5f;
                    m_Hearts[i].transform.localPosition = new Vector3(0, offset, 0);
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