using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class GhostSparkCollectible : BaseCollectible
    {
        public override string DisplayName => "Ghost Spark";
        public override string Description => "Invisible until a tile is placed; dangerous";

        private bool m_IsVisible = false;

        protected override Sprite GetSprite() => GameResources.Instance.GhostSparkSprite;
        protected override float GetScale() => GameResources.Instance.GhostSparkScale;

        protected override void Awake()
        {
            base.Awake();
            // Start invisible
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.enabled = false;
            }
        }

        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                GameManager.Instance.DecreaseLives();
            }
        }

        public override void OnTilePlaced(Board board, PlacedTile tile)
        {
            if (!m_IsVisible && !m_IsCollected)
            {
                m_IsVisible = true;
                if (m_SpriteRenderer != null)
                {
                    m_SpriteRenderer.enabled = true;
                }
            }
        }

        public override bool IsVisible => m_IsVisible && !m_IsCollected;
    }
} 