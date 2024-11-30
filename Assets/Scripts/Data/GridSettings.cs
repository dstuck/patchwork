using UnityEngine;

namespace Patchwork.Data
{
    public class GridSettings : ScriptableObject
    {
        #region Private Fields
        [SerializeField] private Vector2Int m_GridSize = new Vector2Int(8, 8);
        [SerializeField] private float m_CellSize = 1f;
        #endregion

        #region Properties
        public Vector2Int GridSize => m_GridSize;
        public float CellSize => m_CellSize;
        #endregion
    }
}