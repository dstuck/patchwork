using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;

namespace Patchwork.Gameplay
{
    public class JumpingSparkCollectible : BaseDangerCollectible
    {
        private int GetJumpDistance() => m_Level switch { 1 => 1, 2 => 2, _ => 4 };
        protected override int GetDamage() => m_Level switch { 1 => 1, 2 => 2, _ => 3 };

        public override string DisplayName => "Jumping Spark";
        public override string Description => $"Moves {GetJumpDistance()} square{(GetJumpDistance() > 1 ? "s" : "")} after each tile placement; costs {GetDamage()} life{(GetDamage() > 1 ? "s" : "")} if not cleaned up";

        protected override Sprite GetSprite() => GameResources.Instance.JumpingSparkSprite;
        protected override float GetScale() => GameResources.Instance.JumpingSparkScale;

        public override void OnTilePlaced(Board board, PlacedTile tile)
        {
            if (m_IsCollected || board == null) return;

            // Get positions to check for jumping
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            // Randomize direction order
            System.Random rng = new System.Random();
            for (int i = directions.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Vector2Int temp = directions[i];
                directions[i] = directions[j];
                directions[j] = temp;
            }

            // Try each direction until we find a valid spot
            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPos = m_GridPosition + (dir * GetJumpDistance());
                
                // Check if the new position is valid and has a hole
                if (board.IsHoleAt(newPos) && 
                    !board.IsCollectibleAtPosition(newPos) && 
                    !board.IsPlacedTileAtPosition(newPos))
                {
                    // Move to new position
                    m_GridPosition = newPos;
                    UpdatePosition(newPos);
                    break;
                }
            }
        }
    }
} 