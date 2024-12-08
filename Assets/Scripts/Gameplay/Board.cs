using UnityEngine;
using System.Collections.Generic;
using Patchwork.Data;
using Patchwork.Gameplay;

namespace Patchwork.Gameplay
{
    public class Board : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private GridSettings m_GridSettings;
        [SerializeField] private Color m_HoleColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private int m_PatternIndex = 0;  // Choose pattern in inspector
        [SerializeField] private bool m_ShowGridLines = true;
        [SerializeField] private Color m_GridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        private Dictionary<Vector2Int, GameObject> m_Holes = new Dictionary<Vector2Int, GameObject>();
        private List<PlacedTile> m_PlacedTiles = new List<PlacedTile>();
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeBoard();
        }

        private void OnDrawGizmos()
        {
            if (!m_ShowGridLines || m_GridSettings == null) return;

            Gizmos.color = m_GridLineColor;

            // Draw vertical lines
            for (int x = 0; x <= m_GridSettings.GridSize.x; x++)
            {
                Vector3 start = new Vector3(x * m_GridSettings.CellSize, 0, 0);
                Vector3 end = new Vector3(x * m_GridSettings.CellSize, m_GridSettings.GridSize.y * m_GridSettings.CellSize, 0);
                Gizmos.DrawLine(start, end);
            }

            // Draw horizontal lines
            for (int y = 0; y <= m_GridSettings.GridSize.y; y++)
            {
                Vector3 start = new Vector3(0, y * m_GridSettings.CellSize, 0);
                Vector3 end = new Vector3(m_GridSettings.GridSize.x * m_GridSettings.CellSize, y * m_GridSettings.CellSize, 0);
                Gizmos.DrawLine(start, end);
            }
        }
        #endregion

        #region Private Methods
        private Vector2Int[] GetHolePattern(int _patternIndex)
        {
            switch (_patternIndex)
            {
                case 0: // 5x5 Square (25 squares)
                    return new Vector2Int[]
                    {
                        // Row 0
                        new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(3,0), new Vector2Int(4,0),
                        // Row 1
                        new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(3,1), new Vector2Int(4,1),
                        // Row 2
                        new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2), new Vector2Int(3,2), new Vector2Int(4,2),
                        // Row 3
                        new Vector2Int(0,3), new Vector2Int(1,3), new Vector2Int(2,3), new Vector2Int(3,3), new Vector2Int(4,3),
                        // Row 4
                        new Vector2Int(0,4), new Vector2Int(1,4), new Vector2Int(2,4), new Vector2Int(3,4), new Vector2Int(4,4)
                    };

                case 1: // Long rectangle
                    return new Vector2Int[]
                    {
                        // 5x5 rows
                        new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(3,0), new Vector2Int(4,0),
                        new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1), new Vector2Int(3,1), new Vector2Int(4,1),
                        new Vector2Int(0,2), new Vector2Int(1,2), new Vector2Int(2,2), new Vector2Int(3,2), new Vector2Int(4,2),
                        new Vector2Int(0,3), new Vector2Int(1,3), new Vector2Int(2,3), new Vector2Int(3,3), new Vector2Int(4,3),
                        new Vector2Int(0,4), new Vector2Int(1,4), new Vector2Int(2,4), new Vector2Int(3,4), new Vector2Int(4,4)
                    };

                case 2: // Zigzag
                    return new Vector2Int[]
                    {
                        // First column
                        new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(0,3), new Vector2Int(0,4),
                        // Second column
                        new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2), new Vector2Int(1,3), new Vector2Int(1,4),
                        // Third column
                        new Vector2Int(2,0), new Vector2Int(2,1), new Vector2Int(2,2), new Vector2Int(2,3), new Vector2Int(2,4),
                        // Fourth column
                        new Vector2Int(3,0), new Vector2Int(3,1), new Vector2Int(3,2), new Vector2Int(3,3), new Vector2Int(3,4),
                        // Fifth column
                        new Vector2Int(4,0), new Vector2Int(4,1), new Vector2Int(4,2), new Vector2Int(4,3), new Vector2Int(4,4)
                    };
            }

            return new Vector2Int[0];
        }

        private void InitializeBoard()
        {
            if (m_GridSettings == null)
            {
                Debug.LogError("Board: GridSettings is not assigned!");
                return;
            }
            
            GameObject holesParent = new GameObject("Holes");
            holesParent.transform.SetParent(transform);
            holesParent.transform.position = Vector3.zero;

            Vector2Int[] holePositions = GetHolePattern(m_PatternIndex);

            // Calculate pattern bounds
            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
            foreach (Vector2Int pos in holePositions)
            {
                min.x = Mathf.Min(min.x, pos.x);
                min.y = Mathf.Min(min.y, pos.y);
                max.x = Mathf.Max(max.x, pos.x);
                max.y = Mathf.Max(max.y, pos.y);
            }

            // Calculate pattern size and offset to center
            Vector2Int patternSize = max - min + Vector2Int.one;
            Vector2Int offset = (m_GridSettings.GridSize - patternSize) / 2;

            foreach (Vector2Int pos in holePositions)
            {
                Vector2Int gridPos = pos - min + offset;
                
                // Center holes on grid points, just like the cursor
                Vector3 worldPos = new Vector3(
                    (gridPos.x + 0.5f) * m_GridSettings.CellSize,
                    (gridPos.y + 0.5f) * m_GridSettings.CellSize,
                    0
                );
                
                GameObject hole = new GameObject($"Hole_{gridPos.x}_{gridPos.y}");
                hole.transform.SetParent(holesParent.transform);
                hole.transform.position = worldPos;
                
                SpriteRenderer renderer = hole.AddComponent<SpriteRenderer>();
                renderer.sprite = GameResources.Instance.TileSquareSprite;
                renderer.color = m_HoleColor;
                renderer.sortingOrder = -1;
                
                hole.transform.localScale = Vector3.one * m_GridSettings.CellSize * 0.9f;
                
                m_Holes[gridPos] = hole;
            }
        }
        #endregion

        #region Public Methods
        public void FillHole(Vector2Int _position)
        {
            if (m_Holes.TryGetValue(_position, out GameObject hole))
            {
                hole.SetActive(false);
            }
        }

        public void ClearHole(Vector2Int _position)
        {
            if (m_Holes.TryGetValue(_position, out GameObject hole))
            {
                hole.SetActive(true);
            }
        }

        public bool IsHoleAt(Vector2Int position)
        {
            return m_Holes.TryGetValue(position, out GameObject hole) && hole.activeSelf;
        }

        public void AddPlacedTile(PlacedTile tile)
        {
            m_PlacedTiles.Add(tile);
        }

        public int CalculateTotalScore()
        {
            int totalScore = 0;
            foreach (PlacedTile tile in m_PlacedTiles)
            {
                totalScore += tile.CalculateScore(this, m_PlacedTiles);
            }
            return totalScore;
        }
        #endregion
    }
} 