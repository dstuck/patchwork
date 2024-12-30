using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class LenientBonus : ITileUpgrade
    {
        public string DisplayName => "Lenient";
        public string Description => "No penalty for covering non-hole spaces";
        public Color DisplayColor => new Color(0.2f, 0.8f, 1f);  // Light blue

        public int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
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