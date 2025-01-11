using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Patchwork.Data;
using Patchwork.Gameplay;

namespace Patchwork.Gameplay
{
    public class Board : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private GridSettings m_GridSettings;
        [SerializeField] private Color m_HoleColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private bool m_ShowGridLines = true;
        [SerializeField] private Color m_GridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        private Dictionary<Vector2Int, GameObject> m_Holes = new Dictionary<Vector2Int, GameObject>();
        private List<PlacedTile> m_PlacedTiles = new List<PlacedTile>();
        private List<DrawGem> m_DrawGems = new List<DrawGem>();
        private const int c_BaseHoleCount = 28;     // Base number of holes
        private const int c_HolesPerGem = 6;        // Additional holes per draw gem
        private const int c_DefaultGemCount = 2;     // Default starting gems
        private int m_GemCount;                      // Actual gem count, can be modified
        public const int NonHolePenalty = -2;       // Made public for upgrade reference
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            m_GemCount = c_DefaultGemCount;  // Initialize with default value
        }

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
        private Vector2Int[] GetHolePattern()
        {
            // Calculate total holes based on current gem count
            int totalHoles = c_BaseHoleCount + (c_HolesPerGem * m_GemCount);
            return GenerateRandomWalkPattern(totalHoles);
        }

        private Vector2Int[] GenerateRandomWalkPattern(int _count)
        {
            HashSet<Vector2Int> selectedSquares = new HashSet<Vector2Int>();
            Vector2Int currentPos = new Vector2Int(
                m_GridSettings.GridSize.x / 2,
                m_GridSettings.GridSize.y / 2
            );
            
            // Add starting position
            selectedSquares.Add(currentPos);
            
            Vector2Int lastPos = currentPos;
            while (selectedSquares.Count < _count)
            {
                Vector2Int nextPos = GetNextPosition(currentPos);
                
                // Ensure we stay within grid bounds
                if (nextPos.x >= 0 && nextPos.x < m_GridSettings.GridSize.x &&
                    nextPos.y >= 0 && nextPos.y < m_GridSettings.GridSize.y)
                {
                    selectedSquares.Add(nextPos);
                    currentPos = nextPos;
                }
            }

            return selectedSquares.ToArray();
        }

        private Vector2Int FindPositionWithTwoNeighbors(HashSet<Vector2Int> _holes)
        {
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (Vector2Int pos in _holes)
            {
                int neighborCount = 0;
                foreach (Vector2Int dir in directions)
                {
                    if (_holes.Contains(pos + dir))
                    {
                        neighborCount++;
                    }
                }

                if (neighborCount == 2)
                {
                    return pos;
                }
            }

            // Fallback to first position if no suitable spot found
            return _holes.First();
        }

        private void CreateDrawGem(Vector2Int _position)
        {
            GameObject gemObj = new GameObject("DrawGem");
            gemObj.transform.SetParent(transform);
            
            // Center gem on grid point and move it slightly forward in Z
            Vector3 worldPos = new Vector3(
                (_position.x + 0.5f) * m_GridSettings.CellSize,
                (_position.y + 0.5f) * m_GridSettings.CellSize,
                0
            );
            gemObj.transform.position = worldPos;
            
            DrawGem gem = gemObj.AddComponent<DrawGem>();
            gem.Initialize(_position);
            m_DrawGems.Add(gem);
        }

        private Vector2Int GetNextPosition(Vector2Int currentPos)
        {
            // Define possible directions (up, right, down, left)
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };
            
            // Calculate center of grid
            Vector2Int center = new Vector2Int(
                m_GridSettings.GridSize.x / 2,
                m_GridSettings.GridSize.y / 2
            );
            
            // 10% chance to bias towards center
            if (Random.value < 0.15f)
            {
                // Move towards center
                Vector2Int towardsCenter = new Vector2Int(
                    (center.x - currentPos.x) > 0 ? 1 : -1,
                    (center.y - currentPos.y) > 0 ? 1 : -1
                );
                
                return currentPos + new Vector2Int(
                    Mathf.Abs(center.x - currentPos.x) > 0 ? towardsCenter.x : 0,
                    Mathf.Abs(center.y - currentPos.y) > 0 ? towardsCenter.y : 0
                );
            }
            
            // Random direction
            return currentPos + directions[Random.Range(0, directions.Length)];
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

            Vector2Int[] holePositions = GetHolePattern();

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

            // Create holes with offset positions
            foreach (Vector2Int pos in holePositions)
            {
                Vector2Int gridPos = pos - min + offset;
                
                // Center holes on grid points
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

            // Now find positions for multiple gems
            HashSet<Vector2Int> usedGemPositions = new HashSet<Vector2Int>();
            
            for (int i = 0; i < m_GemCount; i++)
            {
                // Find all positions with two neighbors that aren't already used
                List<Vector2Int> validPositions = FindAllPositionsWithTwoNeighbors(m_Holes.Keys.ToHashSet())
                    .Where(pos => !usedGemPositions.Contains(pos))
                    .ToList();

                if (validPositions.Count == 0)
                {
                    Debug.LogWarning($"[Board] Not enough valid positions for gem {i + 1}. Using fallback position.");
                    // Fallback to any unused hole position
                    validPositions = m_Holes.Keys
                        .Where(pos => !usedGemPositions.Contains(pos))
                        .ToList();
                }

                if (validPositions.Count > 0)
                {
                    Vector2Int gemPosition = validPositions[Random.Range(0, validPositions.Count)];
                    usedGemPositions.Add(gemPosition);
                    CreateDrawGem(gemPosition);
                }
                else
                {
                    Debug.LogError($"[Board] No valid positions left for gem {i + 1}!");
                }
            }
        }

        private List<Vector2Int> FindAllPositionsWithTwoNeighbors(HashSet<Vector2Int> _holes)
        {
            List<Vector2Int> validPositions = new List<Vector2Int>();
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (Vector2Int pos in _holes)
            {
                int neighborCount = 0;
                foreach (Vector2Int dir in directions)
                {
                    if (_holes.Contains(pos + dir))
                    {
                        neighborCount++;
                    }
                }

                if (neighborCount == 2)
                {
                    validPositions.Add(pos);
                }
            }

            return validPositions;
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

        public bool TryCollectDrawGem(Vector2Int _position)
        {
            DrawGem gem = m_DrawGems.Find(g => g.GetGridPosition() == _position);
            if (gem != null && gem.TryCollect())
            {
                // Find the GameManager to access the deck
                var gameManager = FindFirstObjectByType<GameManager>();
                if (gameManager != null && gameManager.Deck != null)
                {
                    gameManager.Deck.DrawTile();
                }
                return true;
            }
            return false;
        }

        #if UNITY_INCLUDE_TESTS
        public PlacedTile GetTileAt(Vector2Int position)
        {
            return m_PlacedTiles.Find(t => t.GridPosition == position);
        }

        public void Initialize(GridSettings settings)
        {
            m_GridSettings = settings;
            InitializeBoard();
        }
        #endif

        public void SetGemCount(int _count)
        {
            m_GemCount = Mathf.Max(0, _count);  // Ensure non-negative
        }
        #endregion
    }
} 