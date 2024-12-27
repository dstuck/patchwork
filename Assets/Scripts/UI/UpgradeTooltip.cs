using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Patchwork.UI
{
    public class UpgradeTooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private TextMeshProUGUI m_DescriptionText;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private float m_VerticalOffset = 50f;

        private void Awake()
        {
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();
            
            Hide();
        }

        public void Show(string title, string description, Vector2 position)
        {
            m_TitleText.text = title;
            m_DescriptionText.text = description;
            transform.position = position + new Vector2(0, m_VerticalOffset);
            m_CanvasGroup.alpha = 1;
        }

        public void Hide()
        {
            m_CanvasGroup.alpha = 0;
        }
    }
} 