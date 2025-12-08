using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public interface ITileUpgrade : ITooltipContent
    {
        string DisplayName { get; }
        string Description { get; }
        Color DisplayColor { get; }
        int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles);
        
        /// <summary>
        /// Called when a tile with this upgrade is placed on the board.
        /// Default implementation does nothing.
        /// </summary>
        /// <param name="_tile">The tile that was placed</param>
        /// <param name="_board">The board the tile was placed on</param>
        void OnTilePlaced(PlacedTile _tile, Board _board)
        {
            // Default implementation does nothing
        }
        
        /// <summary>
        /// Returns whether a tile with this upgrade can be rotated.
        /// Default implementation returns true.
        /// </summary>
        /// <returns>True if the tile can be rotated, false otherwise</returns>
        bool CanRotate()
        {
            return true;
        }
    }
} 