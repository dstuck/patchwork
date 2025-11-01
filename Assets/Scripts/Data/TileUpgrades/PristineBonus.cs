using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class PristineBonus : ITileUpgrade
    {
        private readonly int m_BonusAmount;

        public PristineBonus() : this(5) {}
        public PristineBonus(int bonusAmount)
        {
            m_BonusAmount = Mathf.Max(0, bonusAmount);
        }

        public string DisplayName => "Pristine Placement";
        public string Description => $"+{m_BonusAmount} points if perfectly placed";
        public Color DisplayColor => new Color(1f, 0.8f, 0.2f);  // Golden yellow

        public int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
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