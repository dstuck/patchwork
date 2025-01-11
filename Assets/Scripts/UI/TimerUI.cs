using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    public class TimerUI : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private Image m_TimerFillImage;
        [SerializeField] private TextMeshProUGUI m_MultiplierText;
        
        private Timer m_Timer;
        private Color m_NormalColor;
        #endregion

        private void Start()
        {
            m_Timer = FindFirstObjectByType<Timer>();
            if (m_Timer != null)
            {
                m_Timer.OnTimerTick.AddListener(UpdateTimer);
                UpdateTimer(m_Timer.GetNormalizedTimeRemaining());
            }
        }

        private void OnDestroy()
        {
            if (m_Timer != null)
            {
                m_Timer.OnTimerTick.RemoveListener(UpdateTimer);
            }
        }

        public void UpdateTimer(float normalizedTime)
        {
            m_TimerFillImage.fillAmount = normalizedTime;

            if (m_MultiplierText != null)
            {
                float multiplier = m_Timer.GetCurrentMultiplier();
                string multiplierText = multiplier.ToString("0.##");
                m_MultiplierText.text = $"×{multiplierText}";
            }
        }
    }
} 