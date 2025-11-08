using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using Patchwork.Gameplay;

namespace Patchwork.UI
{
    public class CraftingUI : MonoBehaviour
    {
        #region Private Fields
        [Header("UI References")]
        [SerializeField] private RectTransform m_CollectiblesContainer;
        [SerializeField] private RectTransform m_CraftingSlotsContainer;
        [SerializeField] private Button m_CraftButton;
        [SerializeField] private Button m_CloseButton;
        [SerializeField] private GameObject m_CollectiblePreviewPrefab;
        
        [Header("Visual Settings")]
        [SerializeField] private Color m_DisabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color m_NormalColor = Color.white;
        
        private List<CollectiblePreview> m_CraftingSlots = new List<CollectiblePreview>();
        private List<CollectiblePreview> m_CollectiblePreviews = new List<CollectiblePreview>();
        private List<ICollectible> m_AvailableCollectibles = new List<ICollectible>();
        
        // Crafting state: 0 = source1, 1 = source2, 2 = target, 3 = output
        // Track by index in m_AvailableCollectibles instead of reference
        private int[] m_CraftingSlotsIndices = new int[4] { -1, -1, -1, -1 };
        private int m_NextSlotIndex = 0;
        
        private CanvasGroup m_CanvasGroup;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (m_CraftButton != null)
            {
                m_CraftButton.onClick.AddListener(OnCraftClicked);
            }
            
            if (m_CloseButton != null)
            {
                m_CloseButton.onClick.AddListener(OnCloseClicked);
            }
            
            InitializeCraftingSlots();
            
            // Deactivate by default (will be activated by ShowUI)
            // Note: If ShowUI is called before Awake completes, it will reactivate us
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            RefreshCollectiblesList();
            RefreshUI();
        }

        private void OnDisable()
        {
            // Cleanup if needed
        }
        #endregion

        #region Private Methods
        private void InitializeCraftingSlots()
        {
            // Clear existing slots
            foreach (Transform child in m_CraftingSlotsContainer)
            {
                Destroy(child.gameObject);
            }
            m_CraftingSlots.Clear();
            
            // Create 4 slots using the same prefab as collectibles
            for (int i = 0; i < 4; i++)
            {
                GameObject slotObj = Instantiate(m_CollectiblePreviewPrefab, m_CraftingSlotsContainer);
                CollectiblePreview slot = slotObj.GetComponent<CollectiblePreview>();
                if (slot == null)
                {
                    slot = slotObj.AddComponent<CollectiblePreview>();
                }
                
                // Store slot index for click handler
                int slotIndex = i;
                
                // Initialize with click handler for removing (only for slots 0-2, not output)
                if (i < 3)
                {
                    slot.Initialize(null, (collectible) => OnSlotClicked(slotIndex));
                }
                else
                {
                    slot.Initialize(null, null);
                }
                
                m_CraftingSlots.Add(slot);
                m_CraftingSlotsIndices[i] = -1;
            }
        }

        private void RefreshCollectiblesList()
        {
            // Clear existing previews
            foreach (Transform child in m_CollectiblesContainer)
            {
                Destroy(child.gameObject);
            }
            m_CollectiblePreviews.Clear();
            
            // Get collectibles from deck
            if (CollectiblesDeck.Instance != null)
            {
                m_AvailableCollectibles = CollectiblesDeck.Instance.GetCollectibles();
            }
            else
            {
                m_AvailableCollectibles = new List<ICollectible>();
            }
            
            // Create preview for each collectible
            for (int i = 0; i < m_AvailableCollectibles.Count; i++)
            {
                var collectible = m_AvailableCollectibles[i];
                int collectibleIndex = i; // Capture index for closure
                
                GameObject previewObj = Instantiate(m_CollectiblePreviewPrefab, m_CollectiblesContainer);
                CollectiblePreview preview = previewObj.GetComponent<CollectiblePreview>();
                if (preview == null)
                {
                    preview = previewObj.AddComponent<CollectiblePreview>();
                }
                
                preview.Initialize(collectible, (c) => OnCollectibleClicked(collectibleIndex));
                m_CollectiblePreviews.Add(preview);
            }
        }

        private void RefreshUI()
        {
            // Update slot visuals
            for (int i = 0; i < m_CraftingSlots.Count; i++)
            {
                var slot = m_CraftingSlots[i];
                int collectibleIndex = m_CraftingSlotsIndices[i];
                
                if (collectibleIndex >= 0 && collectibleIndex < m_AvailableCollectibles.Count)
                {
                    var collectible = m_AvailableCollectibles[collectibleIndex];
                    // Re-initialize with the collectible (will update icon)
                    if (i < 3)
                    {
                        int slotIndex = i; // Capture for closure
                        slot.Initialize(collectible, (c) => OnSlotClicked(slotIndex));
                    }
                    else
                    {
                        slot.Initialize(collectible, null);
                    }
                }
                else
                {
                    // Clear the slot - still allow clicking to add
                    if (i < 3)
                    {
                        int slotIndex = i; // Capture for closure
                        slot.Initialize(null, (c) => OnSlotClicked(slotIndex));
                    }
                    else
                    {
                        slot.Initialize(null, null);
                    }
                }
            }
            
            // Update collectible previews (enable/disable based on rules)
            UpdateCollectibleStates();
            
            // Update craft button state
            UpdateCraftButtonState();
        }

        private void UpdateCollectibleStates()
        {
            // Get required sign and level from first filled slot
            int? requiredSign = null;
            int? requiredLevel = null;
            
            for (int i = 0; i < 3; i++)
            {
                int slotIndex = m_CraftingSlotsIndices[i];
                if (slotIndex >= 0 && slotIndex < m_AvailableCollectibles.Count)
                {
                    var collectible = m_AvailableCollectibles[slotIndex];
                    requiredSign = GetSign(collectible);
                    requiredLevel = collectible.Level;
                    break; // Use first filled slot's sign and level
                }
            }
            
            // Update each preview
            for (int i = 0; i < m_CollectiblePreviews.Count; i++)
            {
                var preview = m_CollectiblePreviews[i];
                var collectible = preview.Collectible;
                if (collectible == null)
                {
                    continue;
                }
                
                // Check if already used
                bool isUsed = false;
                for (int slotIdx = 0; slotIdx < 3; slotIdx++)
                {
                    if (m_CraftingSlotsIndices[slotIdx] == i)
                    {
                        isUsed = true;
                        break;
                    }
                }
                
                // Validate sign and level match
                bool isValid = !isUsed;
                if (isValid && requiredSign.HasValue)
                {
                    isValid = GetSign(collectible) == requiredSign.Value;
                }
                if (isValid && requiredLevel.HasValue)
                {
                    isValid = collectible.Level == requiredLevel.Value;
                }
                
                preview.SetEnabled(isValid);
                preview.SetDarkened(!isValid);
            }
        }

        private void UpdateCraftButtonState()
        {
            bool canCraft = CanCraft();
            if (m_CraftButton != null)
            {
                m_CraftButton.interactable = canCraft;
            }
        }

        private bool CanCraft()
        {
            // Need all 3 slots filled (sources and target)
            int source1Index = m_CraftingSlotsIndices[0];
            int source2Index = m_CraftingSlotsIndices[1];
            int targetIndex = m_CraftingSlotsIndices[2];
            
            if (source1Index < 0 || source1Index >= m_AvailableCollectibles.Count ||
                source2Index < 0 || source2Index >= m_AvailableCollectibles.Count ||
                targetIndex < 0 || targetIndex >= m_AvailableCollectibles.Count)
            {
                return false;
            }
            
            // Validate crafting rules
            return ValidateCraftingRules();
        }

        private bool ValidateCraftingRules()
        {
            int source1Index = m_CraftingSlotsIndices[0];
            int source2Index = m_CraftingSlotsIndices[1];
            int targetIndex = m_CraftingSlotsIndices[2];
            
            if (source1Index < 0 || source1Index >= m_AvailableCollectibles.Count ||
                source2Index < 0 || source2Index >= m_AvailableCollectibles.Count ||
                targetIndex < 0 || targetIndex >= m_AvailableCollectibles.Count)
            {
                return false;
            }
            
            var source1 = m_AvailableCollectibles[source1Index];
            var source2 = m_AvailableCollectibles[source2Index];
            var target = m_AvailableCollectibles[targetIndex];
            
            // All three must have same sign and level
            int sign1 = GetSign(source1);
            int sign2 = GetSign(source2);
            int targetSign = GetSign(target);
            
            return sign1 == sign2 && sign1 == targetSign && 
                   source1.Level == source2.Level && source1.Level == target.Level;
        }

        private void OnCollectibleClicked(int collectibleIndex)
        {
            if (collectibleIndex < 0 || collectibleIndex >= m_AvailableCollectibles.Count)
            {
                return;
            }
            
            var collectible = m_AvailableCollectibles[collectibleIndex];
            if (m_NextSlotIndex >= 3)
            {
                return; // All slots filled
            }
            
            // Check if this collectible index is already in a slot
            for (int i = 0; i < 3; i++)
            {
                if (m_CraftingSlotsIndices[i] == collectibleIndex)
                {
                    return; // Already in a slot
                }
            }
            
            // Validate sign and level match existing slots
            int collectibleSign = GetSign(collectible);
            for (int i = 0; i < 3; i++)
            {
                int slotIndex = m_CraftingSlotsIndices[i];
                if (slotIndex >= 0 && slotIndex < m_AvailableCollectibles.Count)
                {
                    var existing = m_AvailableCollectibles[slotIndex];
                    if (GetSign(existing) != collectibleSign || existing.Level != collectible.Level)
                    {
                        return; // Sign or level doesn't match
                    }
                }
            }
            // Add to slot
            m_CraftingSlotsIndices[m_NextSlotIndex] = collectibleIndex;
            m_NextSlotIndex++;
            
            RefreshUI();
        }

        private void OnSlotClicked(int slotIndex)
        {
            // Linear removal: can only remove from right to left
            if (m_CraftingSlotsIndices[slotIndex] < 0)
            {
                return; // Slot is already empty
            }
            
            // Can only remove if all slots to the right are empty
            for (int i = slotIndex + 1; i < 3; i++)
            {
                if (m_CraftingSlotsIndices[i] >= 0)
                {
                    return; // Can't remove if slots to the right are filled
                }
            }
            
            // Remove from this slot
            m_CraftingSlotsIndices[slotIndex] = -1;
            m_NextSlotIndex = slotIndex;
            
            RefreshUI();
        }

        private void OnCraftClicked()
        {
            if (!CanCraft())
            {
                return;
            }
            
            // Get collectibles from indices
            int source1Index = m_CraftingSlotsIndices[0];
            int source2Index = m_CraftingSlotsIndices[1];
            int targetIndex = m_CraftingSlotsIndices[2];
            
            var source1 = m_AvailableCollectibles[source1Index];
            var source2 = m_AvailableCollectibles[source2Index];
            var target = m_AvailableCollectibles[targetIndex];
            
            // Calculate output level (one higher than sources)
            int outputLevel = source1.Level + 1;
            
            // Determine output type (clones for new instance)
            ICollectible output = DetermineOutputType(source1, source2, target);
            output.SetLevel(outputLevel);
            
            // Remove sources from deck (by reference)
            RemoveCollectibleFromDeck(source1);
            RemoveCollectibleFromDeck(source2);
            
            // Add output to deck
            if (CollectiblesDeck.Instance != null)
            {
                CollectiblesDeck.Instance.AddCollectibleToDeck(output);
            }
            
            // Display output in slot 3 (we'll show it but don't track its index since it's new)
            m_CraftingSlots[3].Initialize(output, null);
            
            // Clear crafting slots (sources and target)
            m_CraftingSlotsIndices[0] = -1;
            m_CraftingSlotsIndices[1] = -1;
            m_CraftingSlotsIndices[2] = -1;
            m_CraftingSlotsIndices[3] = -1; // Output slot also cleared
            m_NextSlotIndex = 0;
            
            RefreshCollectiblesList();
            RefreshUI();
        }

        private ICollectible DetermineOutputType(ICollectible source1, ICollectible source2, ICollectible target)
        {
            // Rule 4: If target is same type as one of the sources, output is that type
            if (AreSameType(target, source1))
            {
                return CloneCollectible(source1);
            }
            if (AreSameType(target, source2))
            {
                return CloneCollectible(source2);
            }
            if (AreSameType(source1, source2))
            {
                return CloneCollectible(source1);
            }
            
            // Rule 5: Otherwise, randomly choose one of the 3 inputs
            int randomIndex = UnityEngine.Random.Range(0, 3);
            ICollectible chosen = randomIndex switch
            {
                0 => source1,
                1 => source2,
                _ => target
            };
            
            return CloneCollectible(chosen);
        }

        private ICollectible CloneCollectible(ICollectible original)
        {
            if (original == null)
            {
                return null;
            }
            
            // Create a new GameObject with the same component type
            GameObject obj = new GameObject(original.DisplayName);
            
            System.Type type = ((MonoBehaviour)original).GetType();
            ICollectible clone = obj.AddComponent(type) as ICollectible;
            
            if (clone != null)
            {
                // Set level before any visual updates
                // The Awake() will have been called when AddComponent runs, so components are initialized
                clone.SetLevel(original.Level);
            }
            
            // Set inactive after component initialization
            obj.SetActive(false);
            
            return clone;
        }

        private bool AreSameType(ICollectible a, ICollectible b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            
            return ((MonoBehaviour)a).GetType() == ((MonoBehaviour)b).GetType();
        }


        private int GetSign(ICollectible collectible)
        {
            if (collectible == null)
            {
                return 0;
            }
            
            // Positive power = positive sign, negative power = negative sign
            return collectible.Power > 0 ? 1 : -1;
        }

        private void RemoveCollectibleFromDeck(ICollectible collectible)
        {
            // Remove the original instance from the deck
            if (CollectiblesDeck.Instance != null && collectible != null)
            {
                CollectiblesDeck.Instance.RemoveCollectible(collectible);
            }
        }

        private void OnCloseClicked()
        {
            HideUI();
        }

        public void ShowUI()
        {
            // Ensure CanvasGroup exists before activating
            if (m_CanvasGroup == null)
            {
                m_CanvasGroup = GetComponent<CanvasGroup>();
                if (m_CanvasGroup == null)
                {
                    m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            // Activate GameObject (this will trigger Awake if not already called)
            gameObject.SetActive(true);
            
            // Ensure it stays active and is visible (Awake might have deactivated it)
            gameObject.SetActive(true);
            m_CanvasGroup.alpha = 1f;
            m_CanvasGroup.interactable = true;
            m_CanvasGroup.blocksRaycasts = true;
            
            // Pause the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }

        public void HideUI()
        {
            // Reset crafting slots when closing
            m_CraftingSlotsIndices[0] = -1;
            m_CraftingSlotsIndices[1] = -1;
            m_CraftingSlotsIndices[2] = -1;
            m_CraftingSlotsIndices[3] = -1;
            m_NextSlotIndex = 0;
            
            if (m_CanvasGroup != null)
            {
                m_CanvasGroup.alpha = 0f;
                m_CanvasGroup.interactable = false;
                m_CanvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
            
            // Resume the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }
        #endregion
    }
}

