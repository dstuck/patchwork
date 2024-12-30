using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Patchwork.UI
{
    public class TooltipSystem : MonoBehaviour
    {
        private static TooltipSystem s_Instance;
        
        [SerializeField] private RectTransform m_TooltipPanel;
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private TextMeshProUGUI m_DescriptionText;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private float m_ShowDelay = 0.5f;
        
        private float m_ShowTimer;
        private bool m_ShouldShow;
        private Vector2 m_TargetPosition;
        
        public static TooltipSystem Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    // Try to find existing instance
                    s_Instance = FindFirstObjectByType<TooltipSystem>();
                    
                    // If none exists, create one from prefab
                    if (s_Instance == null)
                    {
                        var prefab = Resources.Load<GameObject>("UI/TooltipSystem");
                        if (prefab != null)
                        {
                            var tooltipObj = Instantiate(prefab);
                            tooltipObj.name = "TooltipSystem";
                            s_Instance = tooltipObj.GetComponent<TooltipSystem>();
                        }
                        else
                        {
                            Debug.LogError("TooltipSystem prefab not found in Resources/UI/TooltipSystem");
                        }
                    }
                }
                return s_Instance;
            }
        }

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Make sure we have a Canvas and it's set up correctly
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Make sure it's on top
                
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                
                gameObject.AddComponent<GraphicRaycaster>();
            }
            
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();

            // Set up the tooltip panel size
            if (m_TooltipPanel != null)
            {
                m_TooltipPanel.sizeDelta = new Vector2(200f, 100f); // Default size
                
                // Set anchors to prevent stretching
                m_TooltipPanel.anchorMin = new Vector2(0.5f, 0.5f);
                m_TooltipPanel.anchorMax = new Vector2(0.5f, 0.5f);
                m_TooltipPanel.pivot = new Vector2(0.5f, 0);  // Pivot at bottom center
            }
                
            Debug.Log($"TooltipSystem initialized. Canvas: {canvas}, CanvasGroup: {m_CanvasGroup}");
            Hide();
        }

        public void Show(string title, string description, Vector2 position)
        {
            Debug.Log($"Showing tooltip: {title} - {description} at {position}");
            m_TitleText.text = title;
            m_DescriptionText.text = description;
            
            // Adjust position to account for tooltip size and screen bounds
            position.y += m_TooltipPanel.sizeDelta.y * 0.5f; // Move up by half height
            position.x = Mathf.Clamp(position.x, m_TooltipPanel.sizeDelta.x * 0.5f, 
                Screen.width - m_TooltipPanel.sizeDelta.x * 0.5f);
            position.y = Mathf.Clamp(position.y, m_TooltipPanel.sizeDelta.y, 
                Screen.height - m_TooltipPanel.sizeDelta.y * 0.5f);
            
            m_TargetPosition = position;
            m_ShouldShow = true;
            m_ShowTimer = Time.time;
            
            // Force immediate show for testing
            m_CanvasGroup.alpha = 1;
            m_TooltipPanel.position = position;
        }

        public void Hide()
        {
            Debug.Log("Hiding tooltip"); // Debug log
            m_ShouldShow = false;
            m_CanvasGroup.alpha = 0;
            m_CanvasGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            if (m_ShouldShow && Time.time - m_ShowTimer >= m_ShowDelay)
            {
                m_CanvasGroup.alpha = 1;
                m_CanvasGroup.blocksRaycasts = true;
                m_TooltipPanel.position = m_TargetPosition;
            }
        }
    }
} 