using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public interface ITileUpgrade
    {
        string DisplayName { get; }
        string Description { get; }
        Color DisplayColor { get; }
        int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles);
    }
} 