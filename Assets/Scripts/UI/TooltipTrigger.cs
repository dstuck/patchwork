using UnityEngine;
using UnityEngine.EventSystems;
using Patchwork.Data;

namespace Patchwork.UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static UpgradeTooltip s_Tooltip;
        private ITileUpgrade m_Upgrade;

        private void Awake()
        {
            if (s_Tooltip == null)
            {
                var tooltipPrefab = GameResources.Instance.UpgradeTooltipPrefab;
                if (tooltipPrefab != null)
                {
                    var canvas = FindFirstObjectByType<Canvas>();
                    if (canvas != null)
                    {
                        var tooltipObj = Instantiate(tooltipPrefab, canvas.transform);
                        s_Tooltip = tooltipObj.GetComponent<UpgradeTooltip>();
                    }
                }
            }
        }

        public void Initialize(ITileUpgrade upgrade)
        {
            m_Upgrade = upgrade;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_Upgrade != null && s_Tooltip != null)
            {
                s_Tooltip.Show(m_Upgrade.DisplayName, m_Upgrade.Description, eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (s_Tooltip != null)
            {
                s_Tooltip.Hide();
            }
        }
    }
} 