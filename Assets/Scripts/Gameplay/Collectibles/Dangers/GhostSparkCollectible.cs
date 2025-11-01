using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class GhostSparkCollectible : BaseCollectible
    {
        public override string DisplayName => "Ghost Spark";
        public override string Description => "Invisible until a tile is placed; dangerous";

        private bool m_IsVisible = false;
        private int m_TilesPlacedSinceSpawn = 0;

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
                // Level mapping (Sneaky Spark): 1=>1 danger, 2=>2 danger, 3=>2 danger
                int damage = m_Level switch { 1 => 1, 2 => 2, _ => 2 };
                GameManager.Instance.DecreaseLives(damage);
            }
        }

        public override void OnTilePlaced(Board board, PlacedTile tile)
        {
            if (m_IsCollected) return;

            // Reveal after N tiles placed based on level: 1, 2, 3
            int threshold = m_Level switch { 1 => 1, 2 => 2, _ => 3 };
            if (!m_IsVisible)
            {
                m_TilesPlacedSinceSpawn++;
                if (m_TilesPlacedSinceSpawn >= threshold)
                {
                    m_IsVisible = true;
                    if (m_SpriteRenderer != null)
                    {
                        m_SpriteRenderer.enabled = true;
                    }
                }
            }
        }

        public override bool IsVisible => m_IsVisible && !m_IsCollected;
    }
} 