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
        #endregion

        #region Public Properties
        public Vector2Int GridPosition => m_GridPosition;
        #endregion

        #region Protected Abstract Methods
        protected abstract Sprite GetSprite();
        protected virtual float GetScale() => 1f;  // Default scale of 1, override if needed
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

        public virtual bool TryCollect()
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
        #endregion
    }
} 