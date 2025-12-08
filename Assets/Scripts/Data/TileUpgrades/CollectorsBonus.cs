using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class CollectorsBonus : BaseTileUpgrade
    {
        private readonly int m_BasePointsPerCollectible = 2;

        public CollectorsBonus() : base(1) {}
        public CollectorsBonus(int level) : base(level) {}
        
        private int m_PointsPerCollectible => m_Level switch { 1 => m_BasePointsPerCollectible, 2 => m_BasePointsPerCollectible * 2, _ => m_BasePointsPerCollectible * 4 };

        public override string DisplayName => "Collector's Bonus";
        public override string Description => $"+{m_PointsPerCollectible} points per collectible";
        
        protected override Color BaseDisplayColor => Color.green;

        public override int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            int bonus = _tile.CollectedCollectibleCount * m_PointsPerCollectible;
            return _baseScore + bonus;
        }
    }
}

