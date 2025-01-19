using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class DrawGem : MonoBehaviour
    {
        #region Private Fields
        private Vector2Int m_GridPosition;
        private bool m_IsCollected = false;
        private SpriteRenderer m_SpriteRenderer;
        private GridSettings m_GridSettings;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            m_SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            // If we have a gem sprite in GameResources, use it
            if (GameResources.Instance.DrawGemSprite != null)
            {
                m_SpriteRenderer.sprite = GameResources.Instance.DrawGemSprite;
            }
            else
            {
                // Create a diamond shape if no sprite is available
                m_SpriteRenderer.sprite = CreateDiamondSprite();
            }
            
            m_SpriteRenderer.color = new Color(0.2f, 0.8f, 1f, 0.8f);  // Light blue, slightly transparent
            m_SpriteRenderer.sortingOrder = 1;  // Above holes but below tiles
            
            transform.localScale = Vector3.one * 1.5f;
        }
        #endregion

        #region Private Methods
        private Sprite CreateDiamondSprite()
        {
            // Create a small texture for the diamond
            int texSize = 120;
            Texture2D tex = new Texture2D(texSize, texSize);
            
            // Clear to transparent
            Color[] colors = new Color[texSize * texSize];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.clear;
            }
            
            // Draw diamond shape
            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    // Create diamond shape using Manhattan distance
                    float distanceFromCenter = Mathf.Abs(x - texSize/2) + Mathf.Abs(y - texSize/2);
                    if (distanceFromCenter < texSize/4)
                    {
                        colors[y * texSize + x] = Color.white;
                    }
                }
            }
            
            tex.SetPixels(colors);
            tex.Apply();
            
            // Create sprite from texture
            return Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), 100f);
        }
        #endregion

        #region Public Methods
        public void Initialize(Vector2Int _gridPosition)
        {
            // Load GridSettings if not assigned
            if (m_GridSettings == null)
            {
                m_GridSettings = Resources.Load<GridSettings>("GridSettings");
                if (m_GridSettings == null)
                {
                    Debug.LogError("GridSettings not found in Resources folder!");
                    return;
                }
            }
            
            m_GridPosition = _gridPosition;
        }

        public Vector2Int GetGridPosition()
        {
            return m_GridPosition;
        }

        public bool TryCollect()
        {
            if (!m_IsCollected)
            {
                m_IsCollected = true;
                gameObject.SetActive(false);
                return true;
            }
            return false;
        }

        public void UpdatePosition(Vector2Int _newPosition)
        {
            m_GridPosition = _newPosition;
            transform.position = new Vector3(
                (_newPosition.x + 0.5f) * m_GridSettings.CellSize,
                (_newPosition.y + 0.5f) * m_GridSettings.CellSize,
                0
            );
        }
        #endregion
    }
} 