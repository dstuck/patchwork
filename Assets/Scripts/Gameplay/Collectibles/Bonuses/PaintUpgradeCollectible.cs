using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PaintUpgradeCollectible : BaseCollectible
    {
        private ITileUpgrade m_Upgrade;

        protected virtual ITileUpgrade GetUpgrade() => null;

        protected override void Awake()
        {
            base.Awake();
            m_Upgrade = GetUpgrade();
            if (m_Upgrade != null && m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = m_Upgrade.DisplayColor;
            }
        }

        public ITileUpgrade CurrentUpgrade => m_Upgrade;

        public override string DisplayName
        {
            get
            {
                // Get the upgrade (call GetUpgrade() directly in case Awake() hasn't run yet)
                ITileUpgrade upgrade = m_Upgrade ?? GetUpgrade();
                if (upgrade != null)
                {
                    return $"{upgrade.DisplayName} Paint";
                }
                return "Paint Upgrade";
            }
        }

        public override string Description
        {
            get
            {
                // Get the upgrade (call GetUpgrade() directly in case Awake() hasn't run yet)
                ITileUpgrade upgrade = m_Upgrade ?? GetUpgrade();
                if (upgrade != null)
                {
                    return $"Upgrades collecting tile: {upgrade.Description}";
                }
                return "Upgrades collecting tile";
            }
        }

        protected override Sprite GetSprite() => GameResources.Instance.PaintSprite;
        protected override float GetScale() => GameResources.Instance.PaintScale;

        public override Sprite GetDisplaySprite()
        {
            Sprite mainSprite = GetSpriteForLevel(m_Level);
            
            // Get the upgrade (call GetUpgrade() directly in case Awake() hasn't run yet)
            ITileUpgrade upgrade = m_Upgrade ?? GetUpgrade();
            
            // Get the tint color from the upgrade, defaulting to white
            Color tintColor = upgrade?.DisplayColor ?? Color.white;
            
            // Apply tint to the sprite first
            Sprite tintedSprite = ApplyTintToSprite(mainSprite, tintColor);
            
            // If no level indicators needed, return the tinted sprite
            if (m_Level <= 1 || !ShouldShowLevelIndicators())
            {
                return tintedSprite;
            }
            
            // Generate composite sprite with level indicators (tint already applied)
            return GenerateCompositeSprite(tintedSprite, Color.white);
        }

        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                if (collectingTile != null && m_Upgrade != null)
                {
                    collectingTile.TileData.AddUpgrade(m_Upgrade);
                }
                return true;
            }
            return false;
        }
    }
} 