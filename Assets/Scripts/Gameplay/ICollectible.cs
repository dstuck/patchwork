using UnityEngine;

namespace Patchwork.Gameplay
{
    public interface ICollectible
    {
        Vector2Int GridPosition { get; }
        void Initialize(Vector2Int position);
        bool TryCollect();
        void OnLevelEnd();
        void OnTilePlaced();
        void UpdatePosition(Vector2Int newPosition);  // For moving boards
    }
} 