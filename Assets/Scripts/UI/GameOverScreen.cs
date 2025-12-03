using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Patchwork.Input;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_ScoreText;
        private GameControls m_Controls;

        private void Awake()
        {
            m_Controls = new GameControls();
            m_Controls.Movement.Place.performed += OnAnyInput;
        }

        private void OnEnable()
        {
            m_Controls.Enable();
        }

        private void OnDisable()
        {
            m_Controls.Disable();
        }

        private void Start()
        {
            if (m_ScoreText != null)
            {
                m_ScoreText.text = $"Game Over!\nFinal Score: {GameManager.Instance.CumulativeScore}";
            }
        }

        private void OnAnyInput(InputAction.CallbackContext context)
        {
            SceneManager.LoadScene("CompanySelect");
        }
    }
} 