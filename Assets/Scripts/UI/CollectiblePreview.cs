using UnityEngine;
using UnityEngine.UI;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    public class CollectiblePreview : MonoBehaviour
    {
        [SerializeField] private Image m_Icon;
        [SerializeField] private Image m_SelectionOutline;
        
        private ICollectible m_Collectible;

        public void Initialize(ICollectible collectible)
        {
            m_Collectible = collectible;
            
            m_Icon.sprite = collectible.GetDisplaySprite();
            m_Icon.enabled = true;
            
            // Add tooltip
            var tooltipTrigger = gameObject.AddComponent<TooltipTrigger>();
            tooltipTrigger.Initialize(collectible);
            
            // Initially not selected
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (m_SelectionOutline != null)
            {
                m_SelectionOutline.enabled = selected;
            }
        }
    }
} 