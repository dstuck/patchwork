using UnityEngine;
using UnityEngine.EventSystems;
using Patchwork.Data;

namespace Patchwork.UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ITileUpgrade m_Upgrade;

        public void Initialize(ITileUpgrade upgrade)
        {
            m_Upgrade = upgrade;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_Upgrade != null && TooltipSystem.Instance != null)
            {
                Vector2 screenPos = eventData.position;
                TooltipSystem.Instance.Show(m_Upgrade.DisplayName, m_Upgrade.Description, screenPos);
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