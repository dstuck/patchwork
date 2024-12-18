using UnityEngine;
using Patchwork.Data;
using System.Collections.Generic;
using TMPro;

namespace Patchwork.Gameplay
{
    public class PlacedTile : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private TileData m_TileData;
        private Vector2Int m_GridPosition;
        private int m_Rotation;
        private TileRenderer m_TileRenderer;
        private Vector2Int[] m_OccupiedSquares;
        private TextMeshPro m_ScoreText;
        private int m_CurrentScore;
        #endregion

        #region Public Properties
        public TileData TileData => m_TileData;
        public Vector2Int GridPosition => m_GridPosition;
        public int Rotation => m_Rotation;
        #endregion

        #region Public Methods
        public void Initialize(TileData _tileData, Vector2Int _gridPosition, int _rotation)
        {
            m_TileData = _tileData;
            m_GridPosition = _gridPosition;
            m_Rotation = _rotation;
            
            // Calculate world-space occupied squares
            Vector2Int[] localSquares = m_TileData.GetRotatedSquares(_rotation);
            m_OccupiedSquares = new Vector2Int[localSquares.Length];
            for (int i = 0; i < localSquares.Length; i++)
            {
                m_OccupiedSquares[i] = m_GridPosition + localSquares[i];
            }
            
            m_TileRenderer = gameObject.AddComponent<TileRenderer>();
            m_TileRenderer.Initialize(m_TileData, m_TileData.TileColor, _rotation);

            // Create score text
            GameObject textObj = new GameObject("ScoreText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.zero;
            
            m_ScoreText = textObj.AddComponent<TextMeshPro>();
            m_ScoreText.alignment = TextAlignmentOptions.Center;
            m_ScoreText.fontSize = 5;
            m_ScoreText.color = Color.black;
            m_ScoreText.text = "";
            
            // Additional TextMeshPro settings
            m_ScoreText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            m_ScoreText.verticalAlignment = VerticalAlignmentOptions.Middle;
            m_ScoreText.enableAutoSizing = true;
            m_ScoreText.fontSizeMin = 3;
            m_ScoreText.fontSizeMax = 5;
            m_ScoreText.rectTransform.sizeDelta = new Vector2(5, 5);
            m_ScoreText.sortingOrder = 2;
            m_ScoreText.enabled = false;  // Start disabled
        }

        public int CalculateScore(Board board, List<PlacedTile> otherTiles)
        {
            m_CurrentScore = 0;
            foreach (Vector2Int square in m_OccupiedSquares)
            {
                bool overHole = board.IsHoleAt(square);
                bool overTile = IsOverlappingOtherTile(square, otherTiles);

                if (overHole && !overTile) m_CurrentScore += 10;
                else if (overHole && overTile) m_CurrentScore += 2;
                else if (!overHole) m_CurrentScore -= 5;
            }

            // Only show text when we have a score
            m_ScoreText.enabled = true;
            m_ScoreText.text = m_CurrentScore.ToString();
            
            return m_CurrentScore;
        }

        public bool OccupiesPosition(Vector2Int position)
        {
            return System.Array.Exists(m_OccupiedSquares, square => square == position);
        }
        #endregion

        #region Private Methods
        private bool IsOverlappingOtherTile(Vector2Int position, List<PlacedTile> otherTiles)
        {
            foreach (PlacedTile other in otherTiles)
            {
                if (other != this && other.OccupiesPosition(position))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}