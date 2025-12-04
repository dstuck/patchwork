using UnityEngine;
using TMPro;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    /// <summary>
    /// Displays a warning showing how many more points are needed to meet the stage requirement.
    /// Hides automatically when the requirement is already met.
    /// </summary>
    public class PointsWarningUI : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private TextMeshProUGUI m_MinimumPointsText;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            UpdateWarning();
        }

        private void Update()
        {
            UpdateWarning();
        }
        #endregion

        #region Private Methods
        private void UpdateWarning()
        {
            if (GameManager.Instance == null)
            {
                gameObject.SetActive(false);
                return;
            }

            int currentScore = GameManager.Instance.CumulativeScore;
            int requiredScore = GameManager.Instance.RequiredScore;
            int pointsNeeded = requiredScore - currentScore;

            if (pointsNeeded > 0)
            {
                gameObject.SetActive(true);
                if (m_MinimumPointsText != null)
                {
                    m_MinimumPointsText.text = $"Target points: {pointsNeeded}";
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        #endregion
    }
}

