using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Gameplay
{
    /// <summary>
    /// Final boss board that combines fire spawning, crumbling holes, and moving board mechanics.
    /// The ultimate challenge combining all boss mechanics.
    /// </summary>
    public class FinalBossBoard : Board
    {
        #region Constants
        // Fire spawning
        private const int c_FireLevel = 1;
        
        // Crumbling
        private const float c_CrackDuration = 4.0f;
        private const float c_TimeBetweenCracks = 1.5f;
        
        // Moving
        private const float c_MinMoveInterval = 1.0f;
        private const float c_MaxMoveInterval = 3.0f;
        
        // Visual
        private static readonly Color c_DarkRedHoleColor = new Color(0.4f, 0.1f, 0.1f, 1f);
        
        // Directions for movement
        private static readonly Vector2Int[] s_Directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        #endregion

        #region Private Fields
        // Crumbling
        private float m_NextCrackTime;
        private List<CrackingHole> m_CrackingHoles = new List<CrackingHole>();
        
        // Moving
        private float m_NextMoveTime;
        #endregion

        #region Nested Types
        private class CrackingHole
        {
            public Vector2Int Position;
            public GameObject HoleObject;
            public SpriteRenderer Renderer;
            public float CrackEndTime;
            public Color OriginalColor;
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Call base initialization
            InitializeBoard();
            
            // Change all holes to dark red color
            UpdateHoleColors();
            
            // Schedule first crack and movement
            m_NextCrackTime = Time.time + c_TimeBetweenCracks;
            ScheduleNextMove();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Override OnTilePlaced to spawn a fire after each non-final tile placement.
        /// </summary>
        public override void OnTilePlaced(PlacedTile tile)
        {
            // Call base implementation first (handles collectible notifications)
            base.OnTilePlaced(tile);
            
            // Check if there are more tiles remaining (not the final placement)
            TileHand tileHand = FindFirstObjectByType<TileHand>();
            if (tileHand != null && tileHand.GetTileCount() > 2)
            {
                // Spawn a fire in a random empty spot
                SpawnFireInRandomEmptySpot();
            }
        }

        /// <summary>
        /// Override OnUpdate to handle both crumbling and moving mechanics.
        /// </summary>
        public override void OnUpdate()
        {
            // Update crumbling holes
            UpdateCrackingHoles();
            
            // Check if it's time to start cracking a new hole
            if (Time.time >= m_NextCrackTime)
            {
                StartCrackingRandomHole();
                m_NextCrackTime = Time.time + c_TimeBetweenCracks;
            }
            
            // Handle board movement
            if (Time.time >= m_NextMoveTime)
            {
                MoveAllElements();
                ScheduleNextMove();
            }
        }
        #endregion

        #region Private Methods - Fire Spawning
        /// <summary>
        /// Updates all hole colors to dark red to match the fire theme.
        /// </summary>
        private void UpdateHoleColors()
        {
            foreach (var holePair in m_Holes)
            {
                if (holePair.Value != null)
                {
                    SpriteRenderer renderer = holePair.Value.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        renderer.color = c_DarkRedHoleColor;
                    }
                }
            }
        }

        /// <summary>
        /// Finds an empty hole position (no collectible, no placed tile) and spawns a fire there.
        /// </summary>
        private void SpawnFireInRandomEmptySpot()
        {
            // Build a HashSet of all positions occupied by placed tiles for efficient lookup
            HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
            foreach (var tile in m_PlacedTiles)
            {
                Vector2Int[] tilePositions = tile.GetOccupiedPositions();
                foreach (Vector2Int pos in tilePositions)
                {
                    occupiedPositions.Add(pos);
                }
            }
            
            // Find all empty hole positions (hole exists, no collectible, not occupied by any tile)
            List<Vector2Int> emptyHoles = m_Holes.Keys
                .Where(pos => IsHoleAt(pos) && 
                             !IsCollectibleAtPosition(pos) && 
                             !occupiedPositions.Contains(pos))
                .ToList();
            
            if (emptyHoles.Count == 0)
            {
                // No empty holes available, can't spawn fire
                return;
            }
            
            // Select a random empty hole
            Vector2Int randomPosition = emptyHoles[Random.Range(0, emptyHoles.Count)];
            
            // Spawn fire at the selected position
            AddFlameCollectible(randomPosition, c_FireLevel);
        }
        #endregion

        #region Private Methods - Crumbling
        private void StartCrackingRandomHole()
        {
            // Get list of available holes (not covered by tiles and not already cracking)
            List<Vector2Int> availableHoles = m_Holes
                .Where(kvp => !IsHoleCoveredByTile(kvp.Key) && kvp.Value.activeSelf && !IsAlreadyCracking(kvp.Key))
                .Select(kvp => kvp.Key)
                .ToList();
            
            if (availableHoles.Count == 0)
            {
                return; // No available holes to crack
            }
            
            // Select random hole
            Vector2Int selectedPosition = availableHoles[Random.Range(0, availableHoles.Count)];
            GameObject holeObject = m_Holes[selectedPosition];
            SpriteRenderer renderer = holeObject.GetComponent<SpriteRenderer>();
            
            if (renderer == null)
            {
                return;
            }
            
            // Create cracking hole data and add to list
            CrackingHole newCrack = new CrackingHole
            {
                Position = selectedPosition,
                HoleObject = holeObject,
                Renderer = renderer,
                CrackEndTime = Time.time + c_CrackDuration,
                OriginalColor = renderer.color
            };
            
            m_CrackingHoles.Add(newCrack);
            
            // Apply red tint to indicate cracking
            renderer.color = new Color(1f, 0.3f, 0.3f, 1f);
        }

        private bool IsAlreadyCracking(Vector2Int _position)
        {
            return m_CrackingHoles.Any(crack => crack.Position == _position);
        }

        private void UpdateCrackingHoles()
        {
            List<CrackingHole> toRemove = new List<CrackingHole>();
            
            foreach (var crack in m_CrackingHoles)
            {
                // Check if hole is now covered by a tile
                if (IsHoleCoveredByTile(crack.Position))
                {
                    // Tile was placed! Restore hole color and cancel cracking
                    crack.Renderer.color = crack.OriginalColor;
                    toRemove.Add(crack);
                    continue;
                }
                
                // Check if crack time has expired
                if (Time.time >= crack.CrackEndTime)
                {
                    // Destroy the hole
                    DestroyHole(crack.Position);
                    toRemove.Add(crack);
                }
                else
                {
                    // Pulse the red color to show urgency
                    float timeRemaining = crack.CrackEndTime - Time.time;
                    float pulse = 0.5f + 0.5f * Mathf.Sin(timeRemaining * 8f); // Faster pulse as time runs out
                    crack.Renderer.color = new Color(1f, 0.3f * pulse, 0.3f * pulse, 1f);
                }
            }
            
            // Remove processed cracks
            foreach (var crack in toRemove)
            {
                m_CrackingHoles.Remove(crack);
            }
        }

        private bool IsHoleCoveredByTile(Vector2Int _position)
        {
            return m_PlacedTiles.Any(tile => tile.OccupiesPosition(_position));
        }

        private void DestroyHole(Vector2Int _position)
        {
            if (m_Holes.TryGetValue(_position, out GameObject holeObject))
            {
                // Check for collectibles at this position and destroy them too
                DestroyCollectiblesAtPosition(_position);
                
                // Remove and destroy the hole
                m_Holes.Remove(_position);
                Destroy(holeObject);
            }
        }

        private void DestroyCollectiblesAtPosition(Vector2Int _position)
        {
            List<ICollectible> toRemove = m_Collectibles
                .Where(c => c.GridPosition == _position)
                .ToList();
            
            foreach (var collectible in toRemove)
            {
                // Trigger end effect (dangers will still apply their penalty)
                collectible.OnLevelEnd();
                Object.Destroy(((MonoBehaviour)collectible).gameObject);
                m_Collectibles.Remove(collectible);
            }
        }
        #endregion

        #region Private Methods - Moving
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
            
            // Update cracking holes positions after movement
            UpdateCrackingHolesAfterMovement(direction);
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
            
            // Find first valid direction, or return zero if none found
            return shuffledDirections.FirstOrDefault(dir => CanMoveInDirection(dir));
        }

        private bool CanMoveInDirection(Vector2Int _direction)
        {
            // Check all holes can move within bounds
            bool holesValid = m_Holes.All(kvp => IsWithinBounds(kvp.Key + _direction));
            if (!holesValid) return false;
            
            // Check all collectibles can move within bounds
            bool collectiblesValid = m_Collectibles.All(c => IsWithinBounds(c.GridPosition + _direction));
            if (!collectiblesValid) return false;
            
            // Check all placed tile positions can move within bounds
            bool tilesValid = m_PlacedTiles.All(tile => 
                tile.GetOccupiedPositions().All(pos => IsWithinBounds(pos + _direction)));
            
            return tilesValid;
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

        private void UpdateCrackingHolesAfterMovement(Vector2Int _direction)
        {
            // Update all cracking hole positions after board movement
            foreach (var crack in m_CrackingHoles)
            {
                crack.Position += _direction;
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

