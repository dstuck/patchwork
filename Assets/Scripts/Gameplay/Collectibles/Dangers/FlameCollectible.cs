using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class FlameCollectible : BaseDangerCollectible
    {
        private int GetSpreadCountPerPlacement() => GetLevelMultiplier();
        protected override int GetDamage() => 1;
        
        public override string DisplayName => "Flame";
        public override string Description => $"Spreads to {GetSpreadCountPerPlacement()} square{(GetSpreadCountPerPlacement() > 1 ? "s" : "")} on each tile placement; costs 1 life if not cleaned up";
        
        private const int c_SpreadDistance = 1;

        protected override Sprite GetSprite() => GameResources.Instance.FlameSprite;
        protected override float GetScale() => GameResources.Instance.FlameScale;

        public override void OnTilePlaced(Board board, PlacedTile tile)
        {
            if (m_IsCollected || board == null) return;

            // Get positions to check for spreading
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };
            
            int spreadCount = 0;
            int maxSpreads = GetSpreadCountPerPlacement();
            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPos = m_GridPosition + (dir * c_SpreadDistance);
                
                // Check if the new position is valid and has a hole
                if (board.IsHoleAt(newPos) && !board.IsCollectibleAtPosition(newPos) && !board.IsPlacedTileAtPosition(newPos))
                {
                    board.AddFlameCollectible(newPos, m_Level);
                    spreadCount++;
                    if (spreadCount >= maxSpreads)
                    {
                        break;
                    }
                }
            }

        }
    }
} 