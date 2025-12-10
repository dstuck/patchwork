using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Gameplay
{
    /// <summary>
    /// Boss board variant where a fire spawns in a random empty spot after each non-final tile placement.
    /// Creates pressure as fires accumulate and must be covered before the level ends.
    /// </summary>
    public class FireBossBoard : Board
    {
        #region Constants
        private const int c_FireLevel = 1; // Level 1 fire (can be adjusted if needed)

        private static readonly Color c_DarkRedHoleColor = new Color(0.4f, 0.1f, 0.1f, 1f); // Dark red color for holes
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Call base initialization
            InitializeBoard();
            
            // Change all holes to dark red color
            UpdateHoleColors();
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
        #endregion

        #region Private Methods
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
    }
}

