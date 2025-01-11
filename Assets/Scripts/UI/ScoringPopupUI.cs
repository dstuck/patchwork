using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Patchwork.Input;

namespace Patchwork.UI
{
    public class ScoringPopupUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_BaseScoreText;
        [SerializeField] private TextMeshProUGUI m_MultiplierText;
        [SerializeField] private TextMeshProUGUI m_StageScoreText;
        [SerializeField] private TextMeshProUGUI m_TotalScoreText;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private Button m_BackgroundButton;
        
        [Header("Animation")]
        [SerializeField] private float m_FadeInDuration = 0.5f;

        public UnityEvent OnPopupComplete = new UnityEvent();
        private bool m_IsInteractable;
        private GameControls m_Controls;
        #endregion

        private void Awake()
        {
            m_Controls = new GameControls();
            m_Controls.UI.Submit.performed += OnSubmit;
            m_CanvasGroup.alpha = 0f;
            m_IsInteractable = false;

            if (m_BackgroundButton == null)
            {
                m_BackgroundButton = GetComponent<Button>();
            }
            if (m_BackgroundButton != null)
            {
                m_BackgroundButton.onClick.AddListener(OnClick);
            }
        }

        private void OnEnable()
        {
            m_Controls.UI.Enable();
        }

        private void OnDisable()
        {
            m_Controls.UI.Disable();
        }

        private void OnDestroy()
        {
            m_Controls.UI.Submit.performed -= OnSubmit;
            m_Controls.Dispose();
        }

        private void OnSubmit(InputAction.CallbackContext context)
        {
            if (m_IsInteractable)
            {
                OnPopupComplete.Invoke();
            }
        }

        private void OnClick()
        {
            if (m_IsInteractable)
            {
                OnPopupComplete.Invoke();
            }
        }

        public void ShowScoring(int baseScore, float multiplier, int stageScore, int totalScore)
        {
            Debug.Log("[ScoringPopupUI] ShowScoring called");
            m_BaseScoreText.text = $"Base Score:{baseScore,6}";
            m_MultiplierText.text = $"Time Multiplier:  x{multiplier:0.#}";
            m_StageScoreText.text = $"Stage Score:{stageScore,6}";
            m_TotalScoreText.text = $"Total Score:{totalScore,6}";
            
            Debug.Log("[ScoringPopupUI] Starting fade in");
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            m_CanvasGroup.alpha = 0f;
            
            while (elapsed < m_FadeInDuration)
            {
                elapsed += Time.deltaTime;
                m_CanvasGroup.alpha = elapsed / m_FadeInDuration;
                yield return null;
            }
            
            m_CanvasGroup.alpha = 1f;
            m_IsInteractable = true;
        }
    }
} 