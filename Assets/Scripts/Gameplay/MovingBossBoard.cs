using UnityEngine;
using System.Collections.Generic;

namespace Patchwork.Gameplay
{
    /// <summary>
    /// Boss board variant where holes and collectibles shift in a random direction periodically.
    /// Creates an extra challenge as the player must adapt to the moving layout.
    /// </summary>
    public class MovingBossBoard : Board
    {
        #region Constants
        private const float c_MinMoveInterval = 1.0f;
        private const float c_MaxMoveInterval = 3.0f;
        #endregion

        #region Private Fields
        private float m_NextMoveTime;
        private static readonly Vector2Int[] s_Directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Call base initialization
            InitializeBoard();
            
            // Schedule first movement
            ScheduleNextMove();
        }
        #endregion

        #region Public Methods
        public override void OnUpdate()
        {
            if (Time.time >= m_NextMoveTime)
            {
                MoveAllElements();
                ScheduleNextMove();
            }
        }
        #endregion

        #region Private Methods
        private void ScheduleNextMove()
        {
            float interval = Random.Range(c_MinMoveInterval, c_MaxMoveInterval);
            m_NextMoveTime = Time.time + interval;
        }

        private void MoveAllElements()
        {
            // Find a valid direction that won't push anything off the edge
            Vector2Int direction = FindValidDirection();
            
            // If no valid direction found, skip this move
            if (direction == Vector2Int.zero)
            {
                return;
            }
            
            // Move all elements in the chosen direction
            MoveHoles(direction);
            MoveCollectibles(direction);
            MovePlacedTiles(direction);
        }

        private Vector2Int FindValidDirection()
        {
            // Shuffle directions for randomness
            List<Vector2Int> shuffledDirections = new List<Vector2Int>(s_Directions);
            for (int i = shuffledDirections.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = shuffledDirections[i];
                shuffledDirections[i] = shuffledDirections[j];
                shuffledDirections[j] = temp;
            }
            
            // Try each direction until we find one that works
            foreach (var direction in shuffledDirections)
            {
                if (CanMoveInDirection(direction))
                {
                    return direction;
                }
            }
            
            // No valid direction found
            return Vector2Int.zero;
        }

        private bool CanMoveInDirection(Vector2Int _direction)
        {
            // Check all holes
            foreach (var kvp in m_Holes)
            {
                Vector2Int newPos = kvp.Key + _direction;
                if (!IsWithinBounds(newPos))
                {
                    return false;
                }
            }
            
            // Check all collectibles
            foreach (var collectible in m_Collectibles)
            {
                Vector2Int newPos = collectible.GridPosition + _direction;
                if (!IsWithinBounds(newPos))
                {
                    return false;
                }
            }
            
            // Check all placed tiles
            foreach (var tile in m_PlacedTiles)
            {
                // Check all squares occupied by the tile
                foreach (var localPos in tile.GetOccupiedPositions())
                {
                    Vector2Int newPos = localPos + _direction;
                    if (!IsWithinBounds(newPos))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }

        private void MoveHoles(Vector2Int _direction)
        {
            Dictionary<Vector2Int, GameObject> newHoles = new Dictionary<Vector2Int, GameObject>();
            
            foreach (var kvp in m_Holes)
            {
                Vector2Int newPos = kvp.Key + _direction;
                
                // Update hole position
                kvp.Value.transform.position = new Vector3(
                    (newPos.x + 0.5f) * m_GridSettings.CellSize,
                    (newPos.y + 0.5f) * m_GridSettings.CellSize,
                    0f
                );
                kvp.Value.name = $"Hole_{newPos.x}_{newPos.y}";
                newHoles[newPos] = kvp.Value;
            }
            
            m_Holes = newHoles;
        }

        private void MoveCollectibles(Vector2Int _direction)
        {
            foreach (var collectible in m_Collectibles)
            {
                Vector2Int newPos = collectible.GridPosition + _direction;
                collectible.UpdatePosition(newPos);
            }
        }

        private void MovePlacedTiles(Vector2Int _direction)
        {
            foreach (var tile in m_PlacedTiles)
            {
                Vector2Int newPos = tile.GridPosition + _direction;
                tile.UpdatePosition(newPos);
            }
        }

        private bool IsWithinBounds(Vector2Int _position)
        {
            return _position.x >= 0 && _position.x < m_GridSettings.GridSize.x &&
                   _position.y >= 0 && _position.y < m_GridSettings.GridSize.y;
        }
        #endregion
    }
}

