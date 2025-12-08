using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class CollectorsBonus : ITileUpgrade
    {
        private readonly int m_PointsPerCollectible;

        public CollectorsBonus() : this(2) {}
        public CollectorsBonus(int pointsPerCollectible)
        {
            m_PointsPerCollectible = Mathf.Max(0, pointsPerCollectible);
        }

        public string DisplayName => "Collector's Bonus";
        public string Description => $"+{m_PointsPerCollectible} points per collectible";
        public Color DisplayColor => Color.green;

        public int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            int bonus = _tile.CollectedCollectibleCount * m_PointsPerCollectible;
            return _baseScore + bonus;
        }
    }
}

