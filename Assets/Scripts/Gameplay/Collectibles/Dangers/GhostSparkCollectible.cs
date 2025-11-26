using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class GhostSparkCollectible : BaseDangerCollectible
    {
        private int GetRevealThreshold() => m_Level switch { 1 => 1, 2 => 2, _ => 3 };
        protected override int GetDamage() => m_Level switch { 1 => 1, 2 => 2, _ => 2 };
        
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
                    // Update level indicators now that we're visible
                    UpdateVisualLevel();
                }
            }
        }

        public override bool IsVisible => m_IsVisible && !m_IsCollected;
        
        protected override bool ShouldShowLevelIndicators() => IsVisible;
        
        // Override GetDisplaySprite to always show level indicators in UI, regardless of visibility
        public override Sprite GetDisplaySprite()
        {
            Sprite mainSprite = GetSpriteForLevel(m_Level);
            
            // Always show level indicators in UI previews, even if not visible in game world
            if (m_Level <= 1)
            {
                return mainSprite;
            }
            
            // Generate composite sprite with level indicators
            return GenerateCompositeSprite(mainSprite);
        }
    }
} 