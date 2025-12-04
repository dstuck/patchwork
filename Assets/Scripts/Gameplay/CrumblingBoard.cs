using UnityEngine;
using System.Collections.Generic;

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
        private CrackingHole m_CurrentCrackingHole;
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
            // Check if current cracking hole should be destroyed
            if (m_CurrentCrackingHole != null)
            {
                UpdateCrackingHole();
            }
            
            // Check if it's time to start cracking a new hole
            if (Time.time >= m_NextCrackTime && m_CurrentCrackingHole == null)
            {
                StartCrackingRandomHole();
                m_NextCrackTime = Time.time + c_TimeBetweenCracks;
            }
        }
        #endregion

        #region Private Methods
        private void StartCrackingRandomHole()
        {
            // Get list of available holes (not covered by tiles)
            List<Vector2Int> availableHoles = new List<Vector2Int>();
            
            foreach (var kvp in m_Holes)
            {
                // Check if hole is not covered by a placed tile
                if (!IsHoleCoveredByTile(kvp.Key) && kvp.Value.activeSelf)
                {
                    availableHoles.Add(kvp.Key);
                }
            }
            
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
            
            // Create cracking hole data
            m_CurrentCrackingHole = new CrackingHole
            {
                Position = selectedPosition,
                HoleObject = holeObject,
                Renderer = renderer,
                CrackEndTime = Time.time + c_CrackDuration,
                OriginalColor = renderer.color
            };
            
            // Apply red tint to indicate cracking
            renderer.color = new Color(1f, 0.3f, 0.3f, 1f);
        }

        private void UpdateCrackingHole()
        {
            if (m_CurrentCrackingHole == null) return;
            
            // Check if hole is now covered by a tile
            if (IsHoleCoveredByTile(m_CurrentCrackingHole.Position))
            {
                // Tile was placed! Restore hole color and cancel cracking
                m_CurrentCrackingHole.Renderer.color = m_CurrentCrackingHole.OriginalColor;
                m_CurrentCrackingHole = null;
                return;
            }
            
            // Check if crack time has expired
            if (Time.time >= m_CurrentCrackingHole.CrackEndTime)
            {
                // Destroy the hole
                DestroyHole(m_CurrentCrackingHole.Position);
                m_CurrentCrackingHole = null;
            }
            else
            {
                // Pulse the red color to show urgency
                float timeRemaining = m_CurrentCrackingHole.CrackEndTime - Time.time;
                float pulse = 0.5f + 0.5f * Mathf.Sin(timeRemaining * 8f); // Faster pulse as time runs out
                m_CurrentCrackingHole.Renderer.color = new Color(1f, 0.3f * pulse, 0.3f * pulse, 1f);
            }
        }

        private bool IsHoleCoveredByTile(Vector2Int _position)
        {
            foreach (var tile in m_PlacedTiles)
            {
                if (tile.OccupiesPosition(_position))
                {
                    return true;
                }
            }
            return false;
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
            List<ICollectible> toRemove = new List<ICollectible>();
            
            foreach (var collectible in m_Collectibles)
            {
                if (collectible.GridPosition == _position)
                {
                    toRemove.Add(collectible);
                    // Trigger end effect (dangers will still apply their penalty)
                    collectible.OnLevelEnd();
                    Object.Destroy(((MonoBehaviour)collectible).gameObject);
                }
            }
            
            foreach (var collectible in toRemove)
            {
                m_Collectibles.Remove(collectible);
            }
        }
        #endregion
    }
}

