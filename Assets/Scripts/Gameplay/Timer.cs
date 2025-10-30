using UnityEngine;
using UnityEngine.Events;

namespace Patchwork.Gameplay
{
    public class Timer : MonoBehaviour
    {
        #region Events
        public UnityEvent<float> OnTimerTick = new UnityEvent<float>();  // Sends normalized time (0-1)
        public System.Action<float> OnMultiplierChanged;
        #endregion

        #region Private Fields
        private float m_Duration;
        private float m_StartDelay;
        private float m_CurrentTime;
        private bool m_IsRunning;
        private bool m_IsDelaying;
        private float m_MinMultiplier = 1f;
        private float m_MaxMultiplier = 2f;
        #endregion

        #region Public Methods
        public void StartTimer(float duration, float startDelay, float minMultiplier = 1f, float maxMultiplier = 2f)
        {
            m_Duration = duration;
            m_StartDelay = startDelay;
            m_CurrentTime = m_Duration;
            m_MinMultiplier = minMultiplier;
            m_MaxMultiplier = maxMultiplier;
            m_IsRunning = true;
            m_IsDelaying = true;
        }

        public void StopTimer()
        {
            m_IsRunning = false;
        }

        public float GetTimeRemaining()
        {
            return m_CurrentTime;
        }

        public float GetNormalizedTimeRemaining()
        {
            return Mathf.Clamp01(m_CurrentTime / m_Duration);
        }

        public float GetCurrentMultiplier()
        {
            float rawMultiplier = Mathf.Lerp(m_MinMultiplier, m_MaxMultiplier, GetNormalizedTimeRemaining());
            return Mathf.Round(rawMultiplier * 2f) / 2f;  // Round to nearest 0.5
        }

        public void IncreaseCurrentMultiplier(float amount)
        {
            m_MaxMultiplier += amount;
            OnMultiplierChanged?.Invoke(GetCurrentMultiplier());
        }
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (!m_IsRunning) return;

            if (m_IsDelaying)
            {
                m_StartDelay -= Time.deltaTime;
                if (m_StartDelay <= 0)
                {
                    m_IsDelaying = false;
                }
                return;
            }

            m_CurrentTime -= Time.deltaTime;
            OnTimerTick.Invoke(GetNormalizedTimeRemaining());
            OnMultiplierChanged?.Invoke(GetCurrentMultiplier());

            if (m_CurrentTime <= 0)
            {
                m_CurrentTime = 0;
                StopTimer();
            }
        }
        #endregion
    }
} 