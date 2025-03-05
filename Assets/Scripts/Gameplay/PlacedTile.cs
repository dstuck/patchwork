using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Patchwork.UI;
using UnityEngine.EventSystems;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PlacedTile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Private Fields
        [SerializeField] private GridSettings m_GridSettings;
        private TileData m_TileData;
        private Vector2Int m_GridPosition;
        private int m_Rotation;
        private Vector2Int[] m_OccupiedSquares;
        private TileRenderer m_TileRenderer;
        private TileHand m_TileHand;
        private Board m_Board;
        private TextMeshPro m_ScoreText;
        private int m_CurrentScore;
        private TooltipTrigger m_TooltipTrigger;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Load GridSettings if not assigned
            if (m_GridSettings == null)
            {
                m_GridSettings = Resources.Load<GridSettings>("GridSettings");
                if (m_GridSettings == null)
                {
                    Debug.LogError("GridSettings not found in Resources folder!");
                    return;
                }
            }

            // Get TileHand reference
            m_TileHand = FindFirstObjectByType<TileHand>();
            if (m_TileHand == null)
            {
                Debug.LogError("TileHand not found in scene!");
            }

            // Get Board reference
            m_Board = FindFirstObjectByType<Board>();
            if (m_Board == null)
            {
                Debug.LogError("Board not found in scene!");
            }
        }
        #endregion

        #region Public Properties
        public Patchwork.Data.TileData TileData => m_TileData;
        public Vector2Int GridPosition => m_GridPosition;
        public int Rotation => m_Rotation;
        #endregion

        #region Public Methods
        public void Initialize(Patchwork.Data.TileData _tileData, Vector2Int _gridPosition, int _rotation)
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
            
            // Add Rigidbody2D (required for composite collider)
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            
            // Add composite collider first
            var compositeCollider = gameObject.AddComponent<CompositeCollider2D>();
            compositeCollider.isTrigger = true;
            compositeCollider.generationType = CompositeCollider2D.GenerationType.Manual;
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            
            // Create individual box colliders for each square
            foreach (Vector2Int square in localSquares)
            {
                var boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = Vector2.one * m_GridSettings.CellSize;
                boxCollider.offset = new Vector2(
                    square.x * m_GridSettings.CellSize,
                    square.y * m_GridSettings.CellSize
                );
                boxCollider.compositeOperation = CompositeCollider2D.CompositeOperation.Merge;
            }
            
            // Generate the composite collider after all child colliders are added
            compositeCollider.GenerateGeometry();
            
            m_TileRenderer = gameObject.AddComponent<TileRenderer>();
            m_TileRenderer.Initialize(m_TileData, m_TileData.TileColor, _rotation);

            // Check for collectibles under each square
            foreach (Vector2Int square in m_OccupiedSquares)
            {
                if (m_Board != null)
                {
                    m_Board.CheckCollectibles(square);
                }
            }

            // Add tooltip trigger if tile has upgrades
            if (m_TileData.Upgrades.Count > 0)
            {
                m_TooltipTrigger = gameObject.AddComponent<TooltipTrigger>();
                m_TooltipTrigger.Initialize(m_TileData.Upgrades[0]);
            }

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

                if (overHole && !overTile) m_CurrentScore += 2;
                else if (overHole && overTile) m_CurrentScore += 1;
                else if (!overHole) m_CurrentScore -= 2;
            }

            // Apply upgrades
            if (m_TileData != null && m_TileData.Upgrades != null)
            {
                foreach (var upgrade in m_TileData.Upgrades)
                {
                    m_CurrentScore = upgrade.ModifyScore(m_CurrentScore, this, board, otherTiles);
                }
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

        public Vector2Int[] GetSquares()
        {
            return m_TileData.GetRotatedSquares(m_Rotation);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (m_TooltipTrigger != null)
            {
                m_TooltipTrigger.OnPointerEnter(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (m_TooltipTrigger != null)
            {
                m_TooltipTrigger.OnPointerExit(eventData);
            }
        }

        public void UpdatePosition(Vector2Int _newPosition)
        {
            m_GridPosition = _newPosition;
            
            // Update world position
            transform.position = new Vector3(
                (_newPosition.x + 0.5f) * m_GridSettings.CellSize,
                (_newPosition.y + 0.5f) * m_GridSettings.CellSize,
                0f
            );
            
            // Update occupied squares
            Vector2Int[] localSquares = m_TileData.GetRotatedSquares(m_Rotation);
            m_OccupiedSquares = new Vector2Int[localSquares.Length];
            for (int i = 0; i < localSquares.Length; i++)
            {
                m_OccupiedSquares[i] = m_GridPosition + localSquares[i];
            }
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

        private void OnDrawGizmos()
        {
            if (m_OccupiedSquares != null)
            {
                Gizmos.color = Color.yellow;
                foreach (Vector2Int square in m_OccupiedSquares)
                {
                    Gizmos.DrawWireCube(new Vector3(square.x + 0.5f, square.y + 0.5f, 0), Vector3.one);
                }
            }
        }
    }
}