using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class PristineBonus : BaseTileUpgrade
    {
        private readonly int m_BaseBonusAmount = 5;

        public PristineBonus() : base(1) {}
        public PristineBonus(int level) : base(level) {}
        
        private int m_BonusAmount => m_Level switch { 1 => m_BaseBonusAmount, 2 => m_BaseBonusAmount * 2, _ => m_BaseBonusAmount * 4 };

        public override string DisplayName => "Pristine Placement";
        public override string Description => $"+{m_BonusAmount} points if perfectly placed";
        
        protected override Color BaseDisplayColor => new Color(1f, 0.8f, 0.2f);  // Golden yellow

        public override int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            bool allOverHoles = true;
            bool anyOverlap = false;

            foreach (Vector2Int square in _tile.GetSquares())
            {
                Vector2Int worldPos = square + _tile.GridPosition;
                
                if (!_board.IsHoleAt(worldPos))
                {
                    allOverHoles = false;
                    break;
                }

                foreach (PlacedTile other in _otherTiles)
                {
                    if (other != _tile && other.OccupiesPosition(worldPos))
                    {
                        anyOverlap = true;
                        break;
                    }
                }

                if (anyOverlap) break;
            }

            if (allOverHoles && !anyOverlap)
            {
                return _baseScore + m_BonusAmount;
            }
            return _baseScore;
        }
    }
} 