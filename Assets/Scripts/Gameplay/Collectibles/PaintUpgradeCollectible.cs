using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PaintUpgradeCollectible : BaseCollectible
    {
        private ITileUpgrade m_Upgrade;
        private Color m_Color;

        public void Initialize(ITileUpgrade upgrade)
        {
            m_Upgrade = upgrade;
            m_Color = upgrade.DisplayColor;
            
            // Update the sprite renderer color if it exists
            if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.color = m_Color;
            }
        }

        public ITileUpgrade CurrentUpgrade => m_Upgrade;

        public override string DisplayName => m_Upgrade?.DisplayName ?? "Upgrade";
        public override string Description => m_Upgrade?.Description ?? "Upgrades collecting tile";

        protected override Sprite GetSprite() => GameResources.Instance.PaintSprite;
        protected override float GetScale() => GameResources.Instance.PaintScale;

        public override bool TryCollect()
        {
            if (base.TryCollect())
            {
                // Get the tile that collected this
                var board = FindFirstObjectByType<Board>();
                if (board != null)
                {
                    var tile = board.GetTileAt(m_GridPosition);
                    if (tile != null && m_Upgrade != null)
                    {
                        tile.TileData.AddUpgrade(m_Upgrade);
                    }
                }
                return true;
            }
            return false;
        }
    }
} 