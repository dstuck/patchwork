using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class RigidBonus : ITileUpgrade
    {
        private readonly int m_BonusAmount;

        public RigidBonus() : this(6) {}
        public RigidBonus(int bonusAmount)
        {
            m_BonusAmount = Mathf.Max(0, bonusAmount);
        }

        public string DisplayName => "Rigid";
        public string Description => $"+{m_BonusAmount} points, cannot be rotated";
        public Color DisplayColor => Color.red;

        public int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            return _baseScore + m_BonusAmount;
        }

        public bool CanRotate()
        {
            return false;
        }
    }
}

