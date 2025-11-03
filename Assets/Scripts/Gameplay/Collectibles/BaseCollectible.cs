using UnityEngine;
using System.Collections.Generic;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public abstract class BaseCollectible : MonoBehaviour, ICollectible
    {
        #region Protected Fields
        protected Vector2Int m_GridPosition;
        protected bool m_IsCollected;
        protected SpriteRenderer m_SpriteRenderer;
        protected GridSettings m_GridSettings;
        protected int m_Power = 2;  // Default power value
        [SerializeField] protected int m_Level = 1;
        private List<SpriteRenderer> m_LevelIndicators = new List<SpriteRenderer>();  // Child sprites for + indicators
        private static Sprite s_PlusSprite;  // Cached + sprite, initialized once in first Awake
        private static bool s_PlusSpriteInitialized;  // Flag to track initialization
        #endregion

        #region Public Properties
        public Vector2Int GridPosition => m_GridPosition;
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public virtual bool IsVisible => !m_IsCollected;
        public virtual int Power => m_Power;  // New property implementation
        public virtual int Level => m_Level;
        #endregion

        #region Protected Abstract Methods
        protected abstract Sprite GetSprite();
        protected virtual float GetScale() => 1f;  // Default scale of 1, override if needed
        protected virtual void OnLevelChanged()
        {
            UpdateVisualLevel();
        }
        
        // Override this for level-specific sprites (e.g., heart pieces with different fill levels)
        protected virtual Sprite GetSpriteForLevel(int level)
        {
            return GetSprite();  // Default: same sprite for all levels
        }
        
        // Override this to customize indicator position or disable indicators
        protected virtual bool ShouldShowLevelIndicators() => true;
        protected virtual Vector2 GetLevelIndicatorOffset() => Vector2.zero;  // Override to adjust position
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            // Initialize + sprite once (thread-safe lazy initialization in Unity context)
            if (!s_PlusSpriteInitialized)
            {
                InitializePlusSprite();
                s_PlusSpriteInitialized = true;
            }
            
            m_SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            m_SpriteRenderer.sortingOrder = 1;  // Above holes, below tiles
            
            // Load GridSettings
            m_GridSettings = Resources.Load<GridSettings>("GridSettings");
            if (m_GridSettings == null)
            {
                Debug.LogError("GridSettings not found!");
            }

            // Set sprite and scale
            UpdateMainSprite();
            float scale = GetScale();
            transform.localScale = new Vector3(scale, scale, 1f);
            
            // Update level indicators
            UpdateVisualLevel();
        }
        #endregion

        #region Private Methods
        private static void InitializePlusSprite()
        {
            if (s_PlusSprite != null) return;
            
            // Create a 16x16 texture for the + symbol
            int size = 16;
            int lineWidth = 3;  // Width of the cross lines
            int center = size / 2;
            
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            // Fill with transparent
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            // Draw horizontal line
            for (int x = center - lineWidth / 2; x <= center + lineWidth / 2; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int index = y * size + x;
                    if (index >= 0 && index < pixels.Length)
                    {
                        pixels[index] = Color.white;
                    }
                }
            }
            
            // Draw vertical line
            for (int y = center - lineWidth / 2; y <= center + lineWidth / 2; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int index = y * size + x;
                    if (index >= 0 && index < pixels.Length)
                    {
                        pixels[index] = Color.white;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Create sprite with pivot at center
            s_PlusSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s_PlusSprite.name = "PlusIndicator";
        }
        
        private static Sprite GetPlusSprite()
        {
            // Ensure sprite is initialized (fallback - should normally be initialized in Awake)
            if (!s_PlusSpriteInitialized)
            {
                InitializePlusSprite();
                s_PlusSpriteInitialized = true;
            }
            return s_PlusSprite;
        }

        private void UpdateMainSprite()
        {
            m_SpriteRenderer.sprite = GetSpriteForLevel(m_Level);
        }

        private void UpdateVisualLevel()
        {
            // Update main sprite
            UpdateMainSprite();
            
            // Update level indicators
            if (!ShouldShowLevelIndicators())
            {
                ClearLevelIndicators();
                return;
            }
            
            // Number of + indicators: level 2 = 1, level 3 = 2
            int indicatorCount = m_Level > 1 ? m_Level - 1 : 0;
            
            // Remove any null indicators (cleanup)
            m_LevelIndicators.RemoveAll(ind => ind == null || ind.gameObject == null);
            
            // Adjust to match current count
            while (m_LevelIndicators.Count < indicatorCount)
            {
                CreateLevelIndicator();
            }
            while (m_LevelIndicators.Count > indicatorCount)
            {
                var toRemove = m_LevelIndicators[m_LevelIndicators.Count - 1];
                DestroyIndicator(toRemove);
                m_LevelIndicators.RemoveAt(m_LevelIndicators.Count - 1);
            }
            
            // Update positions
            UpdateIndicatorPositions();
        }

        private void CreateLevelIndicator()
        {
            GameObject indicatorObj = new GameObject($"LevelIndicator_{m_LevelIndicators.Count}");
            indicatorObj.transform.SetParent(transform);
            indicatorObj.transform.localScale = Vector3.one;
            
            SpriteRenderer indicatorRenderer = indicatorObj.AddComponent<SpriteRenderer>();
            indicatorRenderer.sprite = GetPlusSprite();
            indicatorRenderer.sortingOrder = m_SpriteRenderer.sortingOrder + 1;  // Above main sprite
            indicatorRenderer.color = Color.white;
            
            m_LevelIndicators.Add(indicatorRenderer);
        }

        private void UpdateIndicatorPositions()
        {
            if (m_SpriteRenderer == null || m_SpriteRenderer.sprite == null) return;
            
            // Get sprite bounds in local space
            Bounds spriteBounds = m_SpriteRenderer.sprite.bounds;
            float spriteWidth = spriteBounds.size.x;
            float spriteHeight = spriteBounds.size.y;
            
            // Calculate indicator scale first (30% of smaller sprite dimension)
            float baseIndicatorSize = GetPlusSprite().bounds.size.x;
            float scale = Mathf.Min(spriteWidth, spriteHeight) * 0.3f / baseIndicatorSize;
            float scaledIndicatorSize = baseIndicatorSize * scale;
            
            // Position in top-right corner with offset
            Vector2 baseOffset = GetLevelIndicatorOffset();
            float spacing = scaledIndicatorSize * 1.2f;  // Space between indicators (20% gap)
            
            for (int i = 0; i < m_LevelIndicators.Count; i++)
            {
                // Position from top-right corner, moving down (vertically) for each indicator
                float xOffset = (spriteWidth * 0.5f) - (scaledIndicatorSize * 0.5f);
                float yOffset = (spriteHeight * 0.5f) - (scaledIndicatorSize * 0.5f) - (i * spacing);
                
                Vector3 position = new Vector3(
                    baseOffset.x + xOffset,
                    baseOffset.y + yOffset,
                    0
                );
                
                m_LevelIndicators[i].transform.localPosition = position;
                m_LevelIndicators[i].transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void ClearLevelIndicators()
        {
            foreach (var indicator in m_LevelIndicators)
            {
                if (indicator != null)
                {
                    DestroyIndicator(indicator);
                }
            }
            m_LevelIndicators.Clear();
        }

        private void DestroyIndicator(SpriteRenderer indicator)
        {
            if (indicator != null && indicator.gameObject != null)
            {
                Destroy(indicator.gameObject);
            }
        }
        #endregion

        #region Public Methods
        public virtual void Initialize(Vector2Int position)
        {
            m_GridPosition = position;
            UpdatePosition(position);
        }

        public virtual bool TryCollect(PlacedTile collectingTile)
        {
            if (!m_IsCollected)
            {
                m_IsCollected = true;
                gameObject.SetActive(false);
                Destroy(gameObject);
                return true;
            }
            return false;
        }

        public virtual void OnLevelEnd()
        {
            // Base implementation does nothing
        }

        public virtual void UpdatePosition(Vector2Int newPosition)
        {
            m_GridPosition = newPosition;
            transform.position = new Vector3(
                (newPosition.x + 0.5f) * m_GridSettings.CellSize,
                (newPosition.y + 0.5f) * m_GridSettings.CellSize,
                0
            );
        }

        public virtual void OnTilePlaced(Board board, PlacedTile tile)
        {
            // Base implementation does nothing
        }

        public Sprite GetDisplaySprite() => GetSprite();

        public virtual void SetLevel(int level)
        {
            int clamped = Mathf.Clamp(level, 1, 3);
            if (m_Level == clamped) return;
            m_Level = clamped;
            OnLevelChanged();
            UpdateVisualLevel();  // Ensure visuals update even if OnLevelChanged is overridden
        }
        #endregion
    }
} 