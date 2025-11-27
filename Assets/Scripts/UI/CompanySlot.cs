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
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI m_CompanyNameText;
        [SerializeField] private RectTransform m_BonusesContainer;
        [SerializeField] private RectTransform m_DangersContainer;
        [SerializeField] private GameObject m_CollectiblePreviewPrefab;
        [SerializeField] private Image m_SelectionHighlight;
        [SerializeField] private TextMeshProUGUI m_BonusesLabel;
        [SerializeField] private TextMeshProUGUI m_DangersLabel;

        private CompanyData m_CompanyData;
        private List<CollectiblePreview> m_BonusPreviews = new List<CollectiblePreview>();
        private List<CollectiblePreview> m_DangerPreviews = new List<CollectiblePreview>();

        public void Initialize(CompanyData companyData, GameObject collectiblePreviewPrefab)
        {
            m_CompanyData = companyData;
            m_CollectiblePreviewPrefab = collectiblePreviewPrefab;
            
            // Set company name
            if (m_CompanyNameText != null)
            {
                m_CompanyNameText.text = companyData.Name;
            }

            // Create bonus previews
            CreateCollectiblePreviews(companyData.Bonuses, m_BonusesContainer, m_BonusPreviews);
            
            // Create danger previews
            CreateCollectiblePreviews(companyData.Dangers, m_DangersContainer, m_DangerPreviews);
            
            // Initially not selected
            SetSelected(false);
        }

        private void CreateCollectiblePreviews(List<ICollectible> collectibles, RectTransform container, List<CollectiblePreview> previews)
        {
            if (container == null || m_CollectiblePreviewPrefab == null)
            {
                Debug.LogError("Container or preview prefab is null!");
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
                CollectiblePreview preview = previewObj.GetComponent<CollectiblePreview>();
                
                if (preview != null)
                {
                    preview.Initialize(collectible);
                    previews.Add(preview);
                }
            }
        }

        public void SetSelected(bool selected)
        {
            if (m_SelectionHighlight != null)
            {
                m_SelectionHighlight.enabled = selected;
            }
        }

        public CompanyData GetCompanyData()
        {
            return m_CompanyData;
        }
    }
}
