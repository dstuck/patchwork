using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class RigidBonus : BaseTileUpgrade
    {
        private readonly int m_BaseBonusAmount = 6;

        public RigidBonus() : base(1) {}
        public RigidBonus(int level) : base(level) {}
        
        private int m_BonusAmount => m_Level switch { 1 => m_BaseBonusAmount, 2 => m_BaseBonusAmount * 2, _ => m_BaseBonusAmount * 4 };

        public override string DisplayName => "Rigid";
        public override string Description => $"+{m_BonusAmount} points, cannot be rotated";
        
        protected override Color BaseDisplayColor => Color.red;

        public override int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            return _baseScore + m_BonusAmount;
        }

        public override bool CanRotate()
        {
            return false;
        }
    }
}

