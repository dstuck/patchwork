using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class LenientBonus : BaseTileUpgrade
    {
        public LenientBonus() : base(1) {}
        public LenientBonus(int level) : base(level) {}

        public override string DisplayName => "Lenient";
        public override string Description => "No penalty for covering non-hole spaces";
        
        protected override Color BaseDisplayColor => new Color(0.2f, 0.8f, 1f);  // Light blue

        public override int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            int penaltyRemoval = 0;
            
            foreach (Vector2Int square in _tile.GetSquares())
            {
                Vector2Int worldPos = square + _tile.GridPosition;
                if (!_board.IsHoleAt(worldPos))
                {
                    penaltyRemoval += -Board.NonHolePenalty; // Remove the penalty for non-hole placement
                }
            }

            return _baseScore + penaltyRemoval;
        }
    }
} 