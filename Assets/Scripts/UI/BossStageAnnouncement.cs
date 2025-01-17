using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Patchwork.Input;
using UnityEngine.SceneManagement;

namespace Patchwork.UI
{
    public class BossStageAnnouncement : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private TextMeshProUGUI m_AnnouncementText;
        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private float m_FadeDuration = 0.5f;
        [SerializeField] private float m_DisplayDuration = 2f;
        
        private GameControls m_Controls;
        private bool m_IsFading;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            m_Controls = new GameControls();
            m_Controls.Movement.Place.performed += _ => StartFadeOut();
            
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            // Start visible and begin the announcement sequence
            gameObject.SetActive(true);
            m_CanvasGroup.alpha = 1f;
            StartCoroutine(AutoFadeAfterDelay());
        }

        private void OnEnable()
        {
            m_Controls.Enable();
        }

        private void OnDisable()
        {
            m_Controls.Disable();
        }
        #endregion

        #region Private Methods
        private IEnumerator AutoFadeAfterDelay()
        {
            yield return new WaitForSeconds(m_DisplayDuration);
            if (!m_IsFading)
            {
                StartFadeOut();
            }
        }

        private void StartFadeOut()
        {
            if (!m_IsFading)
            {
                m_IsFading = true;
                StartCoroutine(FadeOut());
            }
        }

        private IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            float startAlpha = m_CanvasGroup.alpha;

            while (elapsedTime < m_FadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / m_FadeDuration;
                m_CanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, normalizedTime);
                yield return null;
            }

            // Load gameplay scene after fade out
            SceneManager.LoadScene("GameplayScene");
        }
        #endregion

        #region Public Methods
        // Removed ShowAnnouncement since we auto-start in Start()
        #endregion
    }
} 