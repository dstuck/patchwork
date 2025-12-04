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
            // Pick a random direction
            Vector2Int direction = s_Directions[Random.Range(0, s_Directions.Length)];
            
            // Move all holes
            MoveHoles(direction);
            
            // Move all collectibles
            MoveCollectibles(direction);
        }

        private void MoveHoles(Vector2Int _direction)
        {
            Dictionary<Vector2Int, GameObject> newHoles = new Dictionary<Vector2Int, GameObject>();
            List<GameObject> holesToDestroy = new List<GameObject>();
            
            foreach (var kvp in m_Holes)
            {
                Vector2Int newPos = kvp.Key + _direction;
                
                // Check if new position is within bounds
                if (IsWithinBounds(newPos))
                {
                    // Update hole position
                    kvp.Value.transform.position = new Vector3(
                        (newPos.x + 0.5f) * m_GridSettings.CellSize,
                        (newPos.y + 0.5f) * m_GridSettings.CellSize,
                        0f
                    );
                    kvp.Value.name = $"Hole_{newPos.x}_{newPos.y}";
                    newHoles[newPos] = kvp.Value;
                }
                else
                {
                    // Hole moved out of bounds, destroy it
                    holesToDestroy.Add(kvp.Value);
                }
            }
            
            // Destroy out-of-bounds holes
            foreach (var hole in holesToDestroy)
            {
                Destroy(hole);
            }
            
            m_Holes = newHoles;
        }

        private void MoveCollectibles(Vector2Int _direction)
        {
            List<ICollectible> collectiblesToRemove = new List<ICollectible>();
            
            foreach (var collectible in m_Collectibles)
            {
                Vector2Int newPos = collectible.GridPosition + _direction;
                
                // Check if new position is within bounds
                if (IsWithinBounds(newPos))
                {
                    collectible.UpdatePosition(newPos);
                }
                else
                {
                    // Collectible moved out of bounds, mark for removal and trigger end effect
                    collectiblesToRemove.Add(collectible);
                    collectible.OnLevelEnd();
                    Object.Destroy(((MonoBehaviour)collectible).gameObject);
                }
            }
            
            // Remove destroyed collectibles from list
            foreach (var collectible in collectiblesToRemove)
            {
                m_Collectibles.Remove(collectible);
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

