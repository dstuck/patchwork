using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Patchwork.Gameplay;
using Patchwork.Data;

namespace Patchwork.UI
{
    public class CompanySlot : MonoBehaviour
    {
        #region UI References
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_CompanyNameText;
        [SerializeField] private RectTransform m_BonusesContainer;
        [SerializeField] private RectTransform m_DangersContainer;
        [SerializeField] private Image m_SelectionHighlight;
        #endregion

        #region Private Fields
        private GameObject m_CollectiblePreviewPrefab;
        private CompanyData m_CompanyData;
        private readonly List<CollectiblePreview> m_BonusPreviews = new List<CollectiblePreview>();
        private readonly List<CollectiblePreview> m_DangerPreviews = new List<CollectiblePreview>();
        #endregion

        #region Public Methods
        public void Initialize(CompanyData companyData, GameObject collectiblePreviewPrefab)
        {
            m_CompanyData = companyData;
            m_CollectiblePreviewPrefab = collectiblePreviewPrefab;
            
            // Set company name
            if (m_CompanyNameText != null)
            {
                m_CompanyNameText.text = companyData.Name;
            }

            // Ensure selection highlight doesn't block raycasts (for tooltips)
            if (m_SelectionHighlight != null)
            {
                m_SelectionHighlight.raycastTarget = false;
            }

            // Create bonus previews
            CreateCollectiblePreviews(companyData.Bonuses, m_BonusesContainer, m_BonusPreviews);
            
            // Create danger previews
            CreateCollectiblePreviews(companyData.Dangers, m_DangersContainer, m_DangerPreviews);
            
            // Initially not selected
            SetSelected(false);
        }
        #endregion

        #region Private Methods
        private void CreateCollectiblePreviews(List<ICollectible> collectibles, RectTransform container, List<CollectiblePreview> previews)
        {
            if (container == null)
            {
                Debug.LogError("[CompanySlot] Container is null! Please assign m_BonusesContainer or m_DangersContainer in the prefab.", this);
                return;
            }
            
            if (m_CollectiblePreviewPrefab == null)
            {
                Debug.LogError("[CompanySlot] CollectiblePreviewPrefab is null! Please assign it in CompanySelectUI or pass it to Initialize().", this);
                return;
            }

            if (collectibles == null || collectibles.Count == 0)
            {
                // Nothing to display
                foreach (Transform child in container)
                {
                    Destroy(child.gameObject);
                }
                previews.Clear();
                return;
            }

            // Clear existing previews
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            previews.Clear();

            // Create new previews
            foreach (var collectible in collectibles)
            {
                GameObject previewObj = Instantiate(m_CollectiblePreviewPrefab, container);
                if (previewObj.TryGetComponent<CollectiblePreview>(out var preview))
                {
                    preview.Initialize(collectible);
                    previews.Add(preview);
                }
                else
                {
                    Debug.LogError("[CompanySlot] CollectiblePreview component not found on preview prefab instance.");
                }
            }
        }
        #endregion

        #region Public API
        public void SetSelected(bool selected)
        {
            if (m_SelectionHighlight != null)
            {
                m_SelectionHighlight.enabled = selected;
                // Disable raycast target so it doesn't block tooltips on collectible previews
                m_SelectionHighlight.raycastTarget = false;
            }
        }

        public CompanyData GetCompanyData()
        {
            return m_CompanyData;
        }
        #endregion
    }
}
