using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Patchwork.Gameplay;
using Patchwork.Data;

namespace Patchwork.UI
{
    public class TransitionUI : MonoBehaviour
    {
        #region Private Fields
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_StageText;
        [SerializeField] private TextMeshProUGUI m_ScoreText;
        [SerializeField] private Button m_ContinueButton;
        [SerializeField] private TextMeshProUGUI m_ContinueButtonText;
        
        [Header("Animation")]
        [SerializeField] private float m_FadeInDuration = 0.5f;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }
        }
        
        private void Start()
        {
            if (GameManager.Instance != null)
            {
                SetupUI();
                StartCoroutine(FadeIn());
            }
            else
            {
                Debug.LogError("Failed to initialize GameManager!");
            }
        }
        #endregion

        #region Private Methods
        private void SetupUI()
        {
            m_StageText.text = $"Stage {GameManager.Instance.CurrentStage} Complete!";
            m_ScoreText.text = $"Total Score: {GameManager.Instance.CumulativeScore}";
            m_ContinueButtonText.text = "Next Stage";
            m_ContinueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnContinueClicked()
        {
            // TileData tile = 
            // GameManager.Instance.Deck.AddTileToDeck(tile);
            StartCoroutine(FadeOutAndContinue());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float elapsedTime = 0;
            m_CanvasGroup.alpha = 0;

            while (elapsedTime < m_FadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                m_CanvasGroup.alpha = elapsedTime / m_FadeInDuration;
                yield return null;
            }

            m_CanvasGroup.alpha = 1;
        }

        private System.Collections.IEnumerator FadeOutAndContinue()
        {
            float elapsedTime = 0;
            m_CanvasGroup.alpha = 1;

            while (elapsedTime < m_FadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                m_CanvasGroup.alpha = 1 - (elapsedTime / m_FadeInDuration);
                yield return null;
            }

            m_CanvasGroup.alpha = 0;
            GameManager.Instance.StartNextStage();
        }
        #endregion
    }
} 