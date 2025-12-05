using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Gameplay
{
    /// <summary>
    /// Boss board variant where holes crack and disappear over time.
    /// Creates time pressure as the board slowly shrinks.
    /// </summary>
    public class CrumblingBoard : Board
    {
        #region Constants
        private const float c_CrackDuration = 4.0f; // Time before a cracked hole disappears
        private const float c_TimeBetweenCracks = 1.5f; // Time between selecting new holes to crack
        #endregion

        #region Private Fields
        private float m_NextCrackTime;
        private List<CrackingHole> m_CrackingHoles = new List<CrackingHole>();
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
            
            // Schedule first crack
            m_NextCrackTime = Time.time + c_TimeBetweenCracks;
        }
        #endregion

        #region Public Methods
        public override void OnUpdate()
        {
            // Update all cracking holes
            UpdateCrackingHoles();
            
            // Check if it's time to start cracking a new hole
            if (Time.time >= m_NextCrackTime)
            {
                StartCrackingRandomHole();
                m_NextCrackTime = Time.time + c_TimeBetweenCracks;
            }
        }
        #endregion

        #region Private Methods
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
    }
}

