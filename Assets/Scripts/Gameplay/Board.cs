using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Patchwork.Data;
using Patchwork.UI;
using Patchwork.Gameplay;

namespace Patchwork.Gameplay
{
    public class Board : MonoBehaviour
    {
        #region Private Fields
        protected GridSettings m_GridSettings;
        [SerializeField] private Color m_HoleColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private bool m_ShowGridLines = true;
        [SerializeField] private Color m_GridLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        protected Dictionary<Vector2Int, GameObject> m_Holes = new Dictionary<Vector2Int, GameObject>();
        protected List<PlacedTile> m_PlacedTiles = new List<PlacedTile>();
        protected List<ICollectible> m_Collectibles = new List<ICollectible>();
        private const int c_BaseHoleCount = 28;     // Base number of holes
        public const int NonHolePenalty = -2;       // Made public for upgrade reference

        private int m_CurrentTooltipIndex = -1;

        // Add these fields to the Private Fields region
        private List<string> m_AvailableTileNames;
        private HashSet<string> m_UsedTileNames = new HashSet<string>();
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

            // Initialize available tile names
            m_AvailableTileNames = TileFactory.AvailableTileNames.ToList();
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
        private int CalculateAdditionalHolesFromCollectibles()
        {
            int additionalHoles = 0;
            var collectibles = GameManager.Instance.GetCollectiblesForStage();
            
            foreach (var collectible in collectibles)
            {
                additionalHoles += collectible.AdditionalHoleCount();
            }
            
            return additionalHoles;
        }

        private Vector2Int[] GetHolePattern(int additionalHoles)
        {
            // Calculate total holes based on additional holes from collectibles
            int totalHoles = c_BaseHoleCount + additionalHoles;
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

            int CountNeighbors(Vector2Int pos) => directions.Count(dir => _holes.Contains(pos + dir));

            // Find first position with exactly 2 neighbors, or fallback to first position
            var match = _holes.Where(pos => CountNeighbors(pos) == 2).Take(1).ToList();
            return match.Count > 0 ? match[0] : _holes.First();
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

        protected virtual void InitializeBoard()
        {
            // Calculate additional holes from collectibles that will be placed
            int additionalHoles = CalculateAdditionalHolesFromCollectibles();
            
            // Create parent object for holes
            GameObject holesParent = new GameObject("Holes");
            holesParent.transform.SetParent(transform);

            // Get hole pattern based on additional holes from collectibles
            Vector2Int[] holePattern = GetHolePattern(additionalHoles);
            
            // Create holes
            foreach (Vector2Int pos in holePattern)
            {
                CreateHoleAt(pos, holesParent.transform);
            }

            // Create parent object for collectibles
            GameObject collectiblesParent = new GameObject("Collectibles");
            collectiblesParent.transform.SetParent(transform);

            var availableHoles = m_Holes.Keys.ToList();

            // Get collectibles from the deck via GameManager
            var collectibles = GameManager.Instance.GetCollectiblesForStage();
            foreach (var collectible in collectibles)
            {
                // Find an empty hole position
                if (availableHoles.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableHoles.Count);
                    Vector2Int pos = availableHoles[randomIndex];
                    
                    GameObject collectibleObj = new GameObject(collectible.DisplayName);
                    collectibleObj.transform.SetParent(collectiblesParent.transform);
                    
                    var newCollectible = collectibleObj.AddComponent(((MonoBehaviour)collectible).GetType()) as ICollectible;
                    newCollectible.SetLevel(collectible.Level);
                    newCollectible.Initialize(pos);
                    m_Collectibles.Add(newCollectible);
                    
                    availableHoles.RemoveAt(randomIndex);
                }
            }
        }

        /// <summary>
        /// Creates a hole GameObject at the specified grid position.
        /// </summary>
        protected void CreateHoleAt(Vector2Int _position, Transform _parent)
        {
            if (m_Holes.ContainsKey(_position)) return;
            
            GameObject hole = new GameObject($"Hole_{_position.x}_{_position.y}");
            hole.transform.SetParent(_parent);
            
            // Set position
            hole.transform.position = new Vector3(
                (_position.x + 0.5f) * m_GridSettings.CellSize,
                (_position.y + 0.5f) * m_GridSettings.CellSize,
                0f
            );
            
            // Add visual component
            SpriteRenderer renderer = hole.AddComponent<SpriteRenderer>();
            renderer.sprite = GameResources.Instance.TileSquareSprite;
            renderer.color = Color.gray;
            renderer.sortingOrder = -1;
            
            m_Holes[_position] = hole;
        }

        private List<Vector2Int> FindAllPositionsWithTwoNeighbors(HashSet<Vector2Int> _holes)
        {
            Vector2Int[] directions = new Vector2Int[]
            {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            int CountNeighbors(Vector2Int pos) => directions.Count(dir => _holes.Contains(pos + dir));

            return _holes.Where(pos => CountNeighbors(pos) == 2).ToList();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Called every frame by GameManager. Override in subclasses for custom update behavior.
        /// </summary>
        public virtual void OnUpdate()
        {
            // Base board has no update behavior
        }

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


        public bool IsHole(Vector2Int _position)
        {
            return m_Holes.ContainsKey(_position);
        }

        public int GetPlacedTileCount()
        {
            return m_PlacedTiles.Count;
        }

        //  Required to allow the collectible to add new collectibles when it spreads
        public virtual void AddFlameCollectible(Vector2Int position, int level)
        {
            GameObject collectibleObj = new GameObject("Flame");
            collectibleObj.transform.SetParent(transform);
            
            FlameCollectible flame = collectibleObj.AddComponent<FlameCollectible>();
            flame.SetLevel(level);
            flame.Initialize(position);
            m_Collectibles.Add(flame);
        }

        public int CheckCollectibles(Vector2Int position, PlacedTile collectingTile)
        {
            int collectedCount = 0;
            var collectiblesAtPosition = m_Collectibles
                .Where(c => c.GridPosition == position)
                .ToList();
                
            foreach (var collectible in collectiblesAtPosition)
            {
                if (collectible.TryCollect(collectingTile))
                {
                    m_Collectibles.Remove(collectible);
                    SoundFXManager.instance.PlaySoundFXClip(GameResources.Instance.PickupSoundFX, transform);
                    collectedCount++;
                }
            }
            
            return collectedCount;
        }

        public void OnLevelComplete()
        {
            foreach (var collectible in m_Collectibles)
            {
                collectible.OnLevelEnd();
            }
            m_Collectibles.Clear();
        }

        public virtual void OnTilePlaced(PlacedTile tile)
        {
            // Create a temporary copy of the collectibles list to avoid modification during iteration
            var collectiblesCopy = m_Collectibles.ToList();
            foreach (var collectible in collectiblesCopy)
            {
                collectible.OnTilePlaced(this, tile);
            }
            
            // Notify upgrades on the placed tile
            if (tile.TileData != null && tile.TileData.Upgrades != null)
            {
                foreach (var upgrade in tile.TileData.Upgrades)
                {
                    upgrade.OnTilePlaced(tile, this);
                }
            }
        }

        public bool IsCollectibleAtPosition(Vector2Int _position)
        {
            return m_Collectibles.Any(c => c.GridPosition == _position);
        }

        public bool IsPlacedTileAtPosition(Vector2Int _position)
        {
            return m_PlacedTiles.Any(tile => tile.OccupiesPosition(_position));
        }


        public List<ICollectible> GetActiveCollectibles()
        {
            var collectibles = m_Collectibles.Where(c => c.IsVisible).ToList();
            return collectibles;
        }

        public void ToggleCollectibleTooltips(bool show)
        {
            if (!show)
            {
                TooltipSystem.Instance.Hide();
                m_CurrentTooltipIndex = -1;
                return;
            }

            var collectibles = GetActiveCollectibles();
            if (collectibles.Count == 0) return;

            m_CurrentTooltipIndex = 0;
            ShowCurrentCollectibleTooltip(collectibles);
        }

        public bool CycleToNextCollectibleTooltip()
        {
            var collectibles = GetActiveCollectibles();
            if (collectibles.Count == 0) return false;

            m_CurrentTooltipIndex++;
            if (m_CurrentTooltipIndex >= collectibles.Count)
            {
                TooltipSystem.Instance.Hide();
                m_CurrentTooltipIndex = -1;
                return false;
            }

            ShowCurrentCollectibleTooltip(collectibles);
            return true;
        }

        private void ShowCurrentCollectibleTooltip(List<ICollectible> collectibles)
        {
            var collectible = collectibles[m_CurrentTooltipIndex];
            Vector3 worldPos = ((MonoBehaviour)collectible).transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            TooltipSystem.Instance.Show(collectible.DisplayName, collectible.Description, screenPos);
        }
        #endregion

        #region Public Properties
        public GridSettings GridSettings => m_GridSettings;
        #endregion
    }
} 