using UnityEngine;

namespace Patchwork.Data
{
    [CreateAssetMenu(fileName = "GridSettings", menuName = "Patchwork/Grid Settings")]
    public class GridSettings : ScriptableObject
    {
        #region Public Fields
        [SerializeField] private Vector2Int m_GridSize = new Vector2Int(9, 9);
        [SerializeField] private float m_CellSize = 1f;
        #endregion

        #region Public Properties
        public Vector2Int GridSize => m_GridSize;
        public float CellSize => m_CellSize;
        #endregion

        #region Public Methods
        public Vector3 GridToWorld(Vector2Int _gridPosition)
        {
            return new Vector3(
                _gridPosition.x * m_CellSize,
                _gridPosition.y * m_CellSize,
                0f
            );
        }

        public Vector2Int WorldToGrid(Vector3 _worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(_worldPosition.x / m_CellSize),
                Mathf.RoundToInt(_worldPosition.y / m_CellSize)
            );
        }
        #endregion
    }
}