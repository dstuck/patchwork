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
        private GridSettings m_GridSettings;
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

        // Moving boss specific fields
        private bool m_IsMovingBossBoard;
        private int m_TotalColumns;
        private int m_VisibleStartColumn;
        private Dictionary<Vector2Int, GameObject> m_AllHoles = new Dictionary<Vector2Int, GameObject>(); // Stores all holes including off-screen
        private List<DrawGem> m_AllDrawGems = new List<DrawGem>(); // Store all gems including off-screen
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Load GridSettings from Resources
            m_GridSettings = Resources.Load<GridSettings>("GridSettings");
            if (m_GridSettings == null)
            {
                Debug.LogError("Failed to load GridSettings from Resources!");
                return;
            }

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

        private DrawGem CreateDrawGem(Vector2Int _position, GameObject _parent)
        {
            GameObject gemObj = new GameObject("DrawGem");
            gemObj.transform.SetParent(_parent.transform);
            
            // Set the world position
            gemObj.transform.position = new Vector3(
                (_position.x + 0.5f) * m_GridSettings.CellSize,
                (_position.y + 0.5f) * m_GridSettings.CellSize,
                0
            );
            
            DrawGem gem = gemObj.AddComponent<DrawGem>();
            gem.Initialize(_position);
            
            return gem;
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
            if (m_IsMovingBossBoard)
            {
                // Moving boss board initialization stays the same
                return;
            }

            // Create parent object for holes
            GameObject holesParent = new GameObject("Holes");
            holesParent.transform.SetParent(transform);

            // Get hole pattern
            Vector2Int[] holePattern = GetHolePattern();
            
            // Create holes
            foreach (Vector2Int pos in holePattern)
            {
                GameObject hole = new GameObject($"Hole_{pos.x}_{pos.y}");
                hole.transform.SetParent(holesParent.transform);
                CreateHole(pos, hole);  // Pass the hole GameObject to CreateHole
                m_Holes[pos] = hole;
            }

            // Create parent object for gems
            GameObject gemsParent = new GameObject("Gems");
            gemsParent.transform.SetParent(transform);

            // Place draw gems at random hole positions
            for (int i = 0; i < m_GemCount; i++)
            {
                var availableHoles = m_Holes.Keys.ToList();
                if (availableHoles.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableHoles.Count);
                    Vector2Int gemPos = availableHoles[randomIndex];
                    DrawGem gem = CreateDrawGem(gemPos, gemsParent);
                    if (gem != null)
                    {
                        m_DrawGems.Add(gem);
                    }
                    availableHoles.RemoveAt(randomIndex);
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

        private void GenerateColumnHoles(int _column, int _holesPerColumn, GameObject _parent)
        {
            List<int> availableRows = Enumerable.Range(0, m_GridSettings.GridSize.y).ToList();
            
            // Ensure connectivity by guaranteeing some holes are adjacent to previous column
            if (_column > 0)
            {
                var previousColumnHoles = m_AllHoles.Keys.Where(pos => pos.x == _column - 1).ToList();
                foreach (var prevHole in previousColumnHoles)
                {
                    // 70% chance to create a connecting hole
                    if (Random.value < 0.7f)
                    {
                        int row = prevHole.y;
                        if (availableRows.Contains(row))
                        {
                            CreateHole(new Vector2Int(_column, row), _parent);
                            availableRows.Remove(row);
                            _holesPerColumn--;
                        }
                    }
                }
            }
            
            // Fill remaining holes randomly
            while (_holesPerColumn > 0 && availableRows.Count > 0)
            {
                int randomIndex = Random.Range(0, availableRows.Count);
                int row = availableRows[randomIndex];
                availableRows.RemoveAt(randomIndex);
                
                CreateHole(new Vector2Int(_column, row), _parent);
                _holesPerColumn--;
            }
        }

        private void CreateHole(Vector2Int _position, GameObject _parent)
        {
            if (m_AllHoles.ContainsKey(_position)) return;
            
            GameObject hole = new GameObject($"Hole_{_position.x}_{_position.y}");
            hole.transform.SetParent(_parent.transform);
            
            // Set position
            hole.transform.position = new Vector3(
                (_position.x + 0.5f) * m_GridSettings.CellSize,
                (_position.y + 0.5f) * m_GridSettings.CellSize,
                0f
            );
            
            // Add visual component using existing implementation
            SpriteRenderer renderer = hole.AddComponent<SpriteRenderer>();
            renderer.sprite = GameResources.Instance.TileSquareSprite;
            renderer.color = Color.gray;
            renderer.sortingOrder = -1;
            
            m_AllHoles[_position] = hole;
        }

        private void UpdateVisibleHoles()
        {
            // Clear current visible holes
            m_Holes.Clear();
            
            // Get visible range
            int visibleEndColumn = m_VisibleStartColumn + m_GridSettings.GridSize.x;
            
            // Update visible holes
            foreach (var kvp in m_AllHoles)
            {
                Vector2Int worldPos = kvp.Key;
                Vector2Int visiblePos = new Vector2Int(worldPos.x - m_VisibleStartColumn, worldPos.y);
                
                if (visiblePos.x >= 0 && visiblePos.x < m_GridSettings.GridSize.x)
                {
                    m_Holes[visiblePos] = kvp.Value;
                    kvp.Value.SetActive(true);
                    
                    // Update hole position in world space
                    kvp.Value.transform.position = new Vector3(
                        (visiblePos.x + 0.5f) * m_GridSettings.CellSize,
                        (worldPos.y + 0.5f) * m_GridSettings.CellSize,
                        0
                    );
                }
                else
                {
                    kvp.Value.SetActive(false);
                }
            }
        }

        private void GenerateConstrainedRandomWalk(Vector2Int _start, Vector2Int _end, int _pathLength, GameObject _parent)
        {
            List<Vector2Int> path = new List<Vector2Int>();
            path.Add(_start);
            
            int requiredRightSteps = _end.x - _start.x;
            int currentRightSteps = 0;
            int remainingSteps = _pathLength - 1; // -1 because we already added start
            
            while (remainingSteps > 0)
            {
                Vector2Int currentPos = path[path.Count - 1];
                
                // Calculate minimum required right steps
                int minimumRequiredRights = requiredRightSteps - currentRightSteps;
                int stepsAfterRequired = remainingSteps - minimumRequiredRights;
                
                // Calculate biases
                float centerBias = Mathf.Abs(currentPos.y - (m_GridSettings.GridSize.y / 2f)) / (m_GridSettings.GridSize.y / 2f);
                
                // Only force right movement when we absolutely must
                float rightBias = (stepsAfterRequired <= 0) ? 1f : 0.2f;
                
                // Determine step direction
                Vector2Int step;
                float rand = Random.value;
                
                if (rand < rightBias)
                {
                    // Move right
                    step = Vector2Int.right;
                    currentRightSteps++;
                }
                else
                {
                    // Vertical movement with center bias
                    rand = Random.value;
                    if (currentPos.y > m_GridSettings.GridSize.y / 2)
                    {
                        // Above center, bias downward
                        step = (rand < centerBias) ? Vector2Int.down : Vector2Int.up;
                    }
                    else
                    {
                        // Below center, bias upward
                        step = (rand < centerBias) ? Vector2Int.up : Vector2Int.down;
                    }
                }
                
                Vector2Int nextPos = currentPos + step;
                
                // Validate position is within bounds
                if (nextPos.y >= 0 && nextPos.y < m_GridSettings.GridSize.y)
                {
                    path.Add(nextPos);
                    remainingSteps--;
                }
            }
            
            // Create holes along the path
            foreach (Vector2Int pos in path)
            {
                CreateHole(pos, _parent);
            }
        }

        private void TriggerGemCollection()
        {
            // Find the GameManager to access the deck
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null && gameManager.Deck != null)
            {
                gameManager.Deck.DrawTile();
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

        public bool IsHoleAt(Vector2Int _position)
        {
            if (m_IsMovingBossBoard)
            {
                return m_AllHoles.ContainsKey(_position);
            }
            return m_Holes.ContainsKey(_position);
        }

        public void AddPlacedTile(PlacedTile tile)
        {
            m_PlacedTiles.Add(tile);
        }

        public int CalculateTotalScore()
        {
            int totalScore = 0;
            
            // Calculate score for each placed tile using the existing scoring system
            foreach (var tile in m_PlacedTiles)
            {
                totalScore += tile.CalculateScore(this, m_PlacedTiles);
            }

            return totalScore;
        }

        public bool TryCollectDrawGem(Vector2Int _position)
        {
            if (m_IsMovingBossBoard)
            {
                // For moving boss board, position is already in world coordinates
                Vector2Int worldPos = _position;
                DrawGem gem = m_AllDrawGems.Find(g => g.GetGridPosition() == worldPos);
                if (gem != null && gem.TryCollect())
                {
                    m_AllDrawGems.Remove(gem);
                    TriggerGemCollection();
                    return true;
                }
            }
            else
            {
                // For regular board, check the regular draw gems list
                DrawGem gem = m_DrawGems.Find(g => g.GetGridPosition() == _position);
                if (gem != null && gem.TryCollect())
                {
                    m_DrawGems.Remove(gem);
                    TriggerGemCollection();
                    return true;
                }
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

        public void SetupMovingBossBoard(int _totalColumns)
        {
            m_IsMovingBossBoard = true;
            m_TotalColumns = _totalColumns;
            m_VisibleStartColumn = 0;
            
            // Clear existing holes
            foreach (var hole in m_Holes.Values) Destroy(hole);
            m_Holes.Clear();
            foreach (var hole in m_AllHoles.Values) Destroy(hole);
            m_AllHoles.Clear();
            
            // Create parent object for all holes
            GameObject holesParent = new GameObject("Holes");
            holesParent.transform.SetParent(transform);
            holesParent.transform.position = Vector3.zero;
            
            // Calculate start and end positions for the entire path
            Vector2Int startPos = new Vector2Int(
                0, // Start at the left edge
                m_GridSettings.GridSize.y / 2
            );
            
            Vector2Int endPos = new Vector2Int(
                _totalColumns - 1, // End at the right edge
                m_GridSettings.GridSize.y / 2
            );
            
            // Calculate number of holes based on total columns, not just visible width
            int holesCount = _totalColumns * 6;
            
            GenerateConstrainedRandomWalk(startPos, endPos, holesCount, holesParent);
            UpdateVisibleHoles();
        }

        public void ScrollOneColumn(int _newStartColumn)
        {
            if (!m_IsMovingBossBoard) return;
            
            m_VisibleStartColumn = _newStartColumn;
            
            // Create a new dictionary to store updated hole positions
            Dictionary<Vector2Int, GameObject> updatedHoles = new Dictionary<Vector2Int, GameObject>();
            
            // Update hole positions and visibility
            foreach (var kvp in m_AllHoles)
            {
                Vector2Int newPos = kvp.Key;
                newPos.x -= 1; // Shift left one column
                
                // Update hole position
                Vector3 worldPos = new Vector3(
                    (newPos.x + 0.5f) * m_GridSettings.CellSize,
                    (newPos.y + 0.5f) * m_GridSettings.CellSize,
                    0
                );
                kvp.Value.transform.position = worldPos;
                
                // Set visibility based on position
                bool isVisible = newPos.x >= 0 && newPos.x < m_GridSettings.GridSize.x;
                kvp.Value.SetActive(isVisible);
                
                // Store in new dictionary with updated position
                updatedHoles[newPos] = kvp.Value;
            }
            
            // Replace old dictionary with updated one
            m_AllHoles = updatedHoles;
            
            // Update placed tile positions and visibility
            foreach (var tile in m_PlacedTiles)
            {
                Vector2Int newPos = tile.GridPosition;
                newPos.x -= 1; // Shift left one column
                tile.UpdatePosition(newPos);
                
                // Set visibility based on position
                bool isVisible = newPos.x >= 0 && newPos.x < m_GridSettings.GridSize.x;
                tile.gameObject.SetActive(isVisible);
            }
            
            // Update draw gem positions
            List<DrawGem> gemsToRemove = new List<DrawGem>();
            foreach (var gem in m_AllDrawGems)
            {
                Vector2Int newPos = gem.GetGridPosition();
                newPos.x -= 1; // Shift left one column
                
                if (newPos.x < 0)
                {
                    gemsToRemove.Add(gem);
                }
                else
                {
                    gem.UpdatePosition(newPos);
                    gem.gameObject.SetActive(newPos.x < m_GridSettings.GridSize.x);
                }
            }
            
            // Remove off-screen gems
            foreach (var gem in gemsToRemove)
            {
                m_AllDrawGems.Remove(gem);
                Destroy(gem.gameObject);
            }
        }

        public void PlaceDrawGemInColumn(int _column)
        {
            if (!m_IsMovingBossBoard) return;
            
            // Find a valid hole position in the specified column
            var possiblePositions = m_AllHoles.Keys
                .Where(pos => pos.x == _column)
                .ToList();
            
            if (possiblePositions.Count > 0)
            {
                // Create parent for gems if it doesn't exist
                GameObject gemsParent = transform.Find("Gems")?.gameObject;
                if (gemsParent == null)
                {
                    gemsParent = new GameObject("Gems");
                    gemsParent.transform.SetParent(transform);
                }

                // Pick a random position in this column
                Vector2Int gemPos = possiblePositions[Random.Range(0, possiblePositions.Count)];
                DrawGem gem = CreateDrawGem(gemPos, gemsParent);
                if (gem != null)
                {
                    m_AllDrawGems.Add(gem);
                    // Only show if in visible range
                    gem.gameObject.SetActive(gemPos.x < m_GridSettings.GridSize.x);
                }
            }
        }

        public bool IsHole(Vector2Int _position)
        {
            // For moving boss board, check against all holes
            if (m_IsMovingBossBoard)
            {
                return m_AllHoles.ContainsKey(_position);
            }
            
            // For regular board, use standard holes dictionary
            return m_Holes.ContainsKey(_position);
        }
        #endregion

        #region Public Properties
        public GridSettings GridSettings => m_GridSettings;
        #endregion
    }
} 