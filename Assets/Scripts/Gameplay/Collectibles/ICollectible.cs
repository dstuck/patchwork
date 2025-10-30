using UnityEngine;

namespace Patchwork.Gameplay
{
    public interface ICollectible : ITooltipContent
    {
        Vector2Int GridPosition { get; }
        bool IsVisible { get; }
        int Power { get; }
        void Initialize(Vector2Int position);
        bool TryCollect(PlacedTile collectingTile);
        void OnLevelEnd();
        void OnTilePlaced(Board board, PlacedTile tile);
        void UpdatePosition(Vector2Int newPosition);  // For moving boards
        Sprite GetDisplaySprite();
    }
} 