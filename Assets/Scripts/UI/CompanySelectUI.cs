using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Patchwork.Gameplay;
using Patchwork.Data;
using Patchwork.Input;

namespace Patchwork.UI
{
    public class CompanySelectUI : MonoBehaviour
    {
        #region UI References
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private RectTransform m_CompanySlotsContainer;
        [SerializeField] private GameObject m_CompanySlotPrefab;
        [SerializeField] private GameObject m_CollectiblePreviewPrefab;
        [SerializeField] private GameObject m_TilePreviewPrefab;
        #endregion

        #region Input
        [Header("Input")]
        [SerializeField, Range(0f, 1f)] private float m_InputCooldown = 0.15f;
        private float m_LastInputTime;
        #endregion

        #region Private Fields
        private readonly List<CompanySlot> m_CompanySlots = new List<CompanySlot>();
        private List<CompanyData> m_CompanyOptions = new List<CompanyData>();
        private int m_SelectedIndex = 0;
        private GameControls m_Controls;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            m_Controls = new GameControls();
            m_Controls.UI.Navigate.performed += OnNavigate;
            m_Controls.UI.Submit.performed += OnSubmit;
        }

        private void OnEnable()
        {
            if (m_Controls != null)
            {
                m_Controls.Enable();
            }
        }

        private void OnDisable()
        {
            if (m_Controls != null)
            {
                m_Controls.Disable();
            }
        }

        private void OnDestroy()
        {
            if (m_Controls != null)
            {
                m_Controls.UI.Navigate.performed -= OnNavigate;
                m_Controls.UI.Submit.performed -= OnSubmit;
            }
        }

        private void Start()
        {
            // Set title
            if (m_TitleText != null)
            {
                m_TitleText.text = "Select Your Company";
            }

            // Generate company options
            GenerateCompanyOptions();
            
            // Create company slot UI elements
            CreateCompanySlots();
            
            // Select first company by default
            UpdateSelection();
        }
        #endregion

        #region Private Methods
        private void GenerateCompanyOptions()
        {
            if (GameManager.Instance != null)
            {
                m_CompanyOptions = GameManager.Instance.GenerateCompanyOptions();
            }
            else
            {
                Debug.LogError("GameManager not found!");
            }
        }

        private void CreateCompanySlots()
        {
            if (m_CompanySlotsContainer == null)
            {
                Debug.LogError("[CompanySelectUI] CompanySlotsContainer is not assigned!");
                return;
            }

            if (m_CompanySlotPrefab == null)
            {
                Debug.LogError("[CompanySelectUI] CompanySlotPrefab is not assigned!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in m_CompanySlotsContainer)
            {
                Destroy(child.gameObject);
            }
            m_CompanySlots.Clear();

            // Create slots for each company
            foreach (var company in m_CompanyOptions)
            {
                GameObject slotObj = Instantiate(m_CompanySlotPrefab, m_CompanySlotsContainer);
                if (slotObj.TryGetComponent<CompanySlot>(out var slot))
                {
                    slot.Initialize(company, m_CollectiblePreviewPrefab, m_TilePreviewPrefab);
                    m_CompanySlots.Add(slot);
                }
                else
                {
                    Debug.LogError("[CompanySelectUI] CompanySlot component not found on prefab!");
                }
            }
        }

        private void OnNavigate(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (Time.time - m_LastInputTime < m_InputCooldown) return;

            Vector2 navigation = context.ReadValue<Vector2>();
            
            if (navigation.y > 0)
            {
                // Up - move to previous company (cycling)
                m_SelectedIndex--;
                if (m_SelectedIndex < 0)
                {
                    m_SelectedIndex = m_CompanySlots.Count - 1;
                }
                UpdateSelection();
                m_LastInputTime = Time.time;
            }
            else if (navigation.y < 0)
            {
                // Down - move to next company (cycling)
                m_SelectedIndex++;
                if (m_SelectedIndex >= m_CompanySlots.Count)
                {
                    m_SelectedIndex = 0;
                }
                UpdateSelection();
                m_LastInputTime = Time.time;
            }
        }

        private void OnSubmit(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (Time.time - m_LastInputTime < m_InputCooldown) return;
            
            m_LastInputTime = Time.time;
            SelectCompany();
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < m_CompanySlots.Count; i++)
            {
                m_CompanySlots[i].SetSelected(i == m_SelectedIndex);
            }
        }

        private void SelectCompany()
        {
            if (m_SelectedIndex >= 0 && m_SelectedIndex < m_CompanyOptions.Count)
            {
                CompanyData selectedCompany = m_CompanyOptions[m_SelectedIndex];
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGameWithCompany(selectedCompany);
                }
                else
                {
                    Debug.LogError("GameManager not found!");
                }
            }
        }
        #endregion
    }
}
