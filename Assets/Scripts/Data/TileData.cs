using UnityEngine;

namespace Patchwork.Data
{
    [CreateAssetMenu(fileName = "NewTile", menuName = "Game/Tile Data")]
    public class TileData : ScriptableObject
    {
        #region Private Fields
        [SerializeField] private Vector2Int[] m_Squares = new Vector2Int[0];  // Positions relative to tile origin
        [SerializeField] private Color m_TileColor = Color.white;
        [SerializeField] private string m_TileName = "New Tile";
        
        // Future properties we might want to add:
        // [SerializeField] private TileAbility[] m_Abilities;
        // [SerializeField] private int m_ScoreValue;
        // [SerializeField] private TileRarity m_Rarity;
        // [SerializeField] private Sprite m_TileSprite;
        #endregion

        #region Properties
        public Vector2Int[] Squares => m_Squares;
        public Color TileColor => m_TileColor;
        public string TileName => m_TileName;
        #endregion

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
    } 
}