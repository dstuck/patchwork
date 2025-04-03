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

        public override string DisplayName => m_Upgrade?.DisplayName ?? "Upgrade";
        public override string Description => m_Upgrade?.Description ?? "Upgrades collecting tile";

        protected override Sprite GetSprite() => GameResources.Instance.PaintSprite;
        protected override float GetScale() => GameResources.Instance.PaintScale;

        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                if (collectingTile != null && m_Upgrade != null)
                {
                    Debug.Log($"[PaintUpgrade] Before upgrade - Color: {collectingTile.TileData.TileColor}");
                    collectingTile.TileData.AddUpgrade(m_Upgrade);
                    Debug.Log($"[PaintUpgrade] After upgrade - Color: {collectingTile.TileData.TileColor}");                    
                }
                return true;
            }
            return false;
        }
    }
} 