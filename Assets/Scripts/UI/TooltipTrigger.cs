using UnityEngine;
using UnityEngine.EventSystems;
using Patchwork.Data;

namespace Patchwork.UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ITooltipContent m_Content;

        public void Initialize(ITooltipContent content)
        {
            m_Content = content;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_Content != null && TooltipSystem.Instance != null)
            {
                Vector2 screenPos = eventData.position;
                TooltipSystem.Instance.Show(m_Content.DisplayName, m_Content.Description, screenPos);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.Hide();
            }
        }
    }
} 