using UnityEngine;
using UnityEngine.UI;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    public class CollectiblePreview : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private Image m_Icon;
        [SerializeField] private Image m_SelectionOutline;
        [SerializeField] private Button m_Button;
        
        private ICollectible m_Collectible;
        private System.Action<ICollectible> m_OnClicked;
        private bool m_IsEnabled = true;
        private bool m_IsDarkened = false;
        #endregion

        #region Public Properties
        public ICollectible Collectible => m_Collectible;
        #endregion

        #region Public Methods
        public void Initialize(ICollectible collectible)
        {
            Initialize(collectible, null);
        }

        public void Initialize(ICollectible collectible, System.Action<ICollectible> onClicked)
        {
            m_Collectible = collectible;
            m_OnClicked = onClicked;
            
            if (m_Icon != null)
            {
                if (collectible != null)
                {
                    m_Icon.sprite = collectible.GetDisplaySprite();
                    m_Icon.enabled = true;
                }
                else
                {
                    m_Icon.enabled = false;
                }
            }
            
            if (m_Button != null)
            {
                m_Button.onClick.RemoveAllListeners();
                if (onClicked != null)
                {
                    m_OnClicked = onClicked;
                    // Store collectible in closure to ensure it's captured correctly
                    ICollectible capturedCollectible = collectible;
                    System.Action<ICollectible> capturedOnClicked = onClicked;
                    m_Button.onClick.AddListener(() => capturedOnClicked?.Invoke(capturedCollectible));
                }
                else
                {
                    m_OnClicked = null;
                }
                // Keep button interactable when collectible exists to allow tooltip hover events.
                // Button interactability is based on collectible presence, not the enabled state.
                // Visual feedback for disabled items is provided via SetDarkened().
                // Click validation is handled by the click handler (e.g., CraftingUI.OnCollectibleClicked).
                m_Button.interactable = (collectible != null);
            }
            
            // Add tooltip only if collectible exists
            if (collectible != null)
            {
                var tooltipTrigger = GetComponent<TooltipTrigger>();
                if (tooltipTrigger == null)
                {
                    tooltipTrigger = gameObject.AddComponent<TooltipTrigger>();
                }
                tooltipTrigger.Initialize(collectible);
            }
            else
            {
                // Remove tooltip if no collectible
                var tooltipTrigger = GetComponent<TooltipTrigger>();
                if (tooltipTrigger != null)
                {
                    Destroy(tooltipTrigger);
                }
            }
            
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

        public void SetEnabled(bool enabled)
        {
            m_IsEnabled = enabled;
            // Don't modify button interactability here - it's controlled in Initialize() based on collectible presence.
            // This design ensures tooltip hover events work even for disabled items.
            // Button interactability: controlled by Initialize() (line 64)
            // Visual feedback: SetDarkened() grays out disabled items
            // Click validation: handled by click handlers (e.g., CraftingUI.OnCollectibleClicked)
            UpdateVisuals();
        }

        public void SetDarkened(bool darkened)
        {
            m_IsDarkened = darkened;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (m_Icon != null)
            {
                if (m_IsDarkened)
                {
                    m_Icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                else
                {
                    m_Icon.color = Color.white;
                }
            }
        }
        #endregion
    }
} 