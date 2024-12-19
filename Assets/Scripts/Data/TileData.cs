using UnityEngine;
using System.Collections.Generic;

namespace Patchwork.Data
{
    [CreateAssetMenu(fileName = "NewTile", menuName = "Game/Tile Data")]
    public class TileData : ScriptableObject
    {
        #region Private Fields
        [SerializeField] private string m_TileName;
        [SerializeField] private Vector2Int[] m_Squares;
        [SerializeField] private Color m_TileColor = Color.white;
        private List<ITileUpgrade> m_Upgrades = new List<ITileUpgrade>();
        #endregion

        #region Public Properties
        public string TileName => m_TileName;
        public Vector2Int[] Squares => m_Squares;
        public Color TileColor => m_TileColor;
        public IReadOnlyList<ITileUpgrade> Upgrades => m_Upgrades;
        #endregion

        public void AddUpgrade(ITileUpgrade _upgrade)
        {
            if (!m_Upgrades.Contains(_upgrade))
            {
                m_Upgrades.Add(_upgrade);
            }
        }

        public void RemoveUpgrade(ITileUpgrade _upgrade)
        {
            m_Upgrades.Remove(_upgrade);
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

        public Vector2Int[] GetRotatedSquares(int _rotation)
        {
            // Normalize rotation to 0, 90, 180, or 270
            int normalizedRotation = ((_rotation % 360) + 360) % 360;
            int quarterTurns = normalizedRotation / 90;
            
            Vector2Int[] rotatedSquares = new Vector2Int[m_Squares.Length];
            
            for (int i = 0; i < m_Squares.Length; i++)
            {
                Vector2Int point = m_Squares[i];
                
                // Apply rotation based on number of 90-degree turns
                switch (quarterTurns)
                {
                    case 1: // 90 degrees clockwise
                        rotatedSquares[i] = new Vector2Int(point.y, -point.x);
                        break;
                    case 2: // 180 degrees
                        rotatedSquares[i] = new Vector2Int(-point.x, -point.y);
                        break;
                    case 3: // 270 degrees clockwise
                        rotatedSquares[i] = new Vector2Int(-point.y, point.x);
                        break;
                    default: // 0 degrees
                        rotatedSquares[i] = point;
                        break;
                }
            }
            
            return rotatedSquares;
        }
        #endregion

        #region Private Methods
        private Vector2Int RotatePoint(Vector2Int _point, int _degrees)
        {
            // Convert degrees to radians
            float radians = _degrees * Mathf.Deg2Rad;
            
            // Rotate point using 2D rotation matrix
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            
            return new Vector2Int(
                Mathf.RoundToInt(_point.x * cos - _point.y * sin),
                Mathf.RoundToInt(_point.x * sin + _point.y * cos)
            );
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
            var testTile = CreateInstance<TileData>();
            testTile.m_Squares = squares;
            testTile.m_TileName = "TestTile";
            return testTile;
        }
        #endif
    } 
}