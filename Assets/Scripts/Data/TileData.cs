using UnityEngine;
using System.Collections.Generic;
using System.Linq;  // Add this for ToList()

namespace Patchwork.Data
{
    [System.Serializable]
    public class TileData
    {
        #region Private Fields
        [SerializeField] private string m_TileName;
        [SerializeField] private Vector2Int[] m_Squares;
        [SerializeField] private Color m_TileColor = Color.white;  // Default white, only changed by upgrades
        private Sprite m_TileSprite;
        private List<ITileUpgrade> m_Upgrades = new List<ITileUpgrade>();
        public event System.Action OnDataChanged;
        #endregion

        #region Public Properties
        public string TileName => m_TileName;
        public Vector2Int[] Squares => m_Squares;
        public Color TileColor 
        {
            get => (m_Upgrades != null && m_Upgrades.Count > 0 && m_Upgrades[0] != null) 
                ? m_Upgrades[0].DisplayColor 
                : m_TileColor;
        }
        public Sprite TileSprite 
        { 
            get => GameResources.Instance?.TileSquareSprite;
        }
        public IReadOnlyList<ITileUpgrade> Upgrades => m_Upgrades;
        #endregion

        public TileData(string name, Vector2Int[] squares)
        {
            m_TileName = name;
            m_Squares = squares;
            m_Upgrades = new List<ITileUpgrade>();
        }

        public void AddUpgrade(ITileUpgrade _upgrade)
        {
            if (!m_Upgrades.Contains(_upgrade))
            {
                m_Upgrades.Add(_upgrade);
                OnDataChanged?.Invoke();
            }
        }

        public void RemoveUpgrade(ITileUpgrade _upgrade)
        {
            if (m_Upgrades.Remove(_upgrade))
            {
                OnDataChanged?.Invoke();
            }
        }

        // Add this for any other property changes that should trigger visual updates
        protected void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        #region Public Methods
        public bool OccupiesPosition(Vector2Int _relativePosition)
        {
            return System.Array.Exists(m_Squares, square => square == _relativePosition);
        }

        public Vector2Int[] GetWorldPositions(Vector2Int _origin)
        {
            Vector2Int[] worldPositions = new Vector2Int[m_Squares.Length];
            for (int i = 0; i < m_Squares.Length; i++)
            {
                worldPositions[i] = _origin + m_Squares[i];
            }
            return worldPositions;
        }
        #endregion

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure tile has at least one square
            if (m_Squares.Length == 0)
            {
                m_Squares = new Vector2Int[] { Vector2Int.zero };
            }
        }
        #endif

        #if UNITY_INCLUDE_TESTS
        public static TileData CreateTestTile(Vector2Int[] squares)
        {
            return new TileData("TestTile", squares);
        }
        #endif
    }

    // Factory class for creating basic tile shapes
    public static class TileFactory
    {
        private static readonly Dictionary<string, Vector2Int[]> s_TileConfigurations = new Dictionary<string, Vector2Int[]>
        {
            { "L", new Vector2Int[] 
                {
                    new Vector2Int(2, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2)
                }
            },
            { "T", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2)
                }
            },
            { "4", new Vector2Int[] 
                {
                    new Vector2Int(0, 2),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(-1, -1)
                }
            },
            { "F", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(-1, 1),
                    new Vector2Int(0, 2)
                }
            },
            { "I", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2),
                    new Vector2Int(0, 3),
                    new Vector2Int(0, 4)
                }
            },
            { "J", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2),
                    new Vector2Int(-1, 2),
                    new Vector2Int(-1, 1)
                }
            },
            { "M", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(0, 1),
                    new Vector2Int(-1, 1)
                }
            },
            { "P", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(0, 2)
                }
            },
            { "R", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2)
                }
            },
            { "S", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(-1, 1),
                    new Vector2Int(-1, 2)
                }
            },
            { "U", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2)
                }
            },
            { "X", new Vector2Int[] 
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1)
                }
            }
        };

        public static IReadOnlyList<string> AvailableTileNames => s_TileConfigurations.Keys.ToList();

        public static TileData CreateTile(string _tileName)
        {
            if (!s_TileConfigurations.TryGetValue(_tileName, out Vector2Int[] squares))
            {
                Debug.LogError($"Tile configuration not found for tile name: {_tileName}");
                return null;
            }
            return new TileData(_tileName, squares);
        }

        public static TileData CreateRandomTile()
        {
            var tileNames = s_TileConfigurations.Keys.ToList();
            string randomName = tileNames[Random.Range(0, tileNames.Count)];
            return CreateTile(randomName);
        }
    }
}