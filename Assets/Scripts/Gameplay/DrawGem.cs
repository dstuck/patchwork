using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class DrawGem : BaseCollectible
    {
        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();  // This will create the SpriteRenderer
            
            // Set sprite from GameResources
            m_SpriteRenderer.sprite = GameResources.Instance.DrawGemSprite;

            // Set color and scale
            m_SpriteRenderer.color = new Color(0.2f, 0.8f, 1f, 0.8f);  // Light blue, slightly transparent
            float scale = GameResources.Instance.DrawGemScale;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
        #endregion

        #region Public Methods
        public override bool TryCollect()
        {
            if (base.TryCollect())
            {
                var gameManager = Object.FindFirstObjectByType<GameManager>();
                if (gameManager != null && gameManager.Deck != null)
                {
                    gameManager.Deck.DrawTile();
                }
                return true;
            }
            return false;
        }

        public override void OnLevelEnd()
        {
            // DrawGems don't have any special end-of-level behavior
            base.OnLevelEnd();
        }

        public override void UpdatePosition(Vector2Int newPosition)
        {
            base.UpdatePosition(newPosition);
        }
        #endregion
    }
} 