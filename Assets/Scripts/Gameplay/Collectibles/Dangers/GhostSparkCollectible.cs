using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class GhostSparkCollectible : BaseCollectible
    {
        private int GetRevealThreshold() => m_Level switch { 1 => 1, 2 => 2, _ => 3 };
        private int GetDamage() => m_Level switch { 1 => 1, 2 => 2, _ => 2 };
        
        public override string DisplayName => "Ghost Spark";
        public override string Description => $"Invisible until {GetRevealThreshold()} tile{(GetRevealThreshold() > 1 ? "s are" : " is")} placed; costs {GetDamage()} life{(GetDamage() > 1 ? "s" : "")} if not cleaned up";

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
                GameManager.Instance.DecreaseLives(GetDamage());
            }
        }

        public override void OnTilePlaced(Board board, PlacedTile tile)
        {
            if (m_IsCollected) return;

            if (!m_IsVisible)
            {
                m_TilesPlacedSinceSpawn++;
                if (m_TilesPlacedSinceSpawn >= GetRevealThreshold())
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