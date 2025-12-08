using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public abstract class BaseTileUpgrade : ITileUpgrade
    {
        protected readonly int m_Level;

        protected BaseTileUpgrade() : this(1) {}
        protected BaseTileUpgrade(int level)
        {
            m_Level = Mathf.Clamp(level, 1, 3);
        }

        public int Level => m_Level;

        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        
        protected abstract Color BaseDisplayColor { get; }

        public Color DisplayColor
        {
            get
            {
                Color baseColor = BaseDisplayColor;
                // Always use full alpha
                baseColor.a = 1f;
                return baseColor;
            }
        }

        public abstract int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles);

        public virtual void OnTilePlaced(PlacedTile _tile, Board _board)
        {
            // Default implementation does nothing
        }

        public virtual bool CanRotate()
        {
            return true;
        }
    }
}

