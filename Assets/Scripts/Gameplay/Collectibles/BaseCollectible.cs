using UnityEngine;
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
        protected virtual void OnLevelChanged() {}
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            m_SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            m_SpriteRenderer.sortingOrder = 1;  // Above holes, below tiles
            
            // Load GridSettings
            m_GridSettings = Resources.Load<GridSettings>("GridSettings");
            if (m_GridSettings == null)
            {
                Debug.LogError("GridSettings not found!");
            }

            // Set sprite and scale
            m_SpriteRenderer.sprite = GetSprite();
            float scale = GetScale();
            transform.localScale = new Vector3(scale, scale, 1f);
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
        }
        #endregion
    }
} 