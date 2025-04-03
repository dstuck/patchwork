using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Patchwork.UI;
using UnityEngine.EventSystems;
using Patchwork.Data;
using System.Collections;

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
        private TileHand m_TileHand;
        private Board m_Board;
        private TextMeshPro m_ScoreText;
        private int m_CurrentScore;
        private TooltipTrigger m_TooltipTrigger;

        // Fields from TileRenderer
        private SpriteRenderer[] m_SquareRenderers;
        private bool m_IsRotating;
        private GameObject m_TileRoot;
        [SerializeField] private float m_RotationDuration = 0.2f;
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
        public void Initialize(TileData _tileData, Vector2Int _gridPosition, int _rotation)
        {
            m_TileData = _tileData;
            m_TileData.OnDataChanged += OnTileDataChanged;  // Subscribe to changes
            m_GridPosition = _gridPosition;
            m_Rotation = _rotation;
            
            // Calculate world-space occupied squares
            Vector2Int[] localSquares = GetRotatedSquares();
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
            
            // Replace TileRenderer initialization with direct creation
            CreateVisuals(m_TileData.TileColor);

            // Check for collectibles under each square
            foreach (Vector2Int square in m_OccupiedSquares)
            {
                if (m_Board != null)
                {
                    m_Board.CheckCollectibles(square, this);
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
            return GetRotatedSquares();
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
            Vector2Int[] localSquares = GetRotatedSquares();
            m_OccupiedSquares = new Vector2Int[localSquares.Length];
            for (int i = 0; i < localSquares.Length; i++)
            {
                m_OccupiedSquares[i] = m_GridPosition + localSquares[i];
            }
        }

        // Methods from TileRenderer
        private void CreateVisuals(Color _alpha)
        {
            if (m_TileData == null)
            {
                Debug.LogError("[PlacedTile] Cannot create visuals - TileData is null");
                return;
            }
            
            // Clean up existing visuals
            if (m_TileRoot != null)
            {
                Destroy(m_TileRoot);
            }

            // Create root object
            m_TileRoot = new GameObject("TileRoot");
            m_TileRoot.transform.SetParent(transform);
            m_TileRoot.transform.localPosition = Vector3.zero;
            m_TileRoot.transform.localRotation = Quaternion.identity;

            // Get grid-based rotated positions
            Vector2Int[] squares = GetRotatedSquares();
            m_SquareRenderers = new SpriteRenderer[squares.Length];

            Color tileColor = m_TileData.TileColor;
            tileColor.a = _alpha.a;

            for (int i = 0; i < squares.Length; i++)
            {
                GameObject square = new GameObject($"Square_{i}");
                square.transform.SetParent(m_TileRoot.transform);
                square.transform.localPosition = new Vector3(
                    squares[i].x * m_GridSettings.CellSize,
                    squares[i].y * m_GridSettings.CellSize,
                    0
                );

                SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
                renderer.sprite = m_TileData.TileSprite;
                renderer.color = tileColor;
                m_SquareRenderers[i] = renderer;
            }
        }

        public void UpdateRotation(int _targetRotation)
        {            
            if (m_IsRotating)
            {
                StopAllCoroutines();
                m_IsRotating = false;
            }
            
            int startRotation = m_Rotation;
            m_Rotation = _targetRotation;
            
            float startAngle = m_TileRoot.transform.eulerAngles.z;
            float endAngle = startAngle;
            
            if (_targetRotation > startRotation && _targetRotation - startRotation <= 180 ||
                _targetRotation < startRotation && startRotation - _targetRotation > 180)
            {
                endAngle -= 90;
            }
            else
            {
                endAngle += 90;
            }

            StartCoroutine(AnimateRotation(startAngle, endAngle, _targetRotation));
        }

        private IEnumerator AnimateRotation(float _startAngle, float _endAngle, int _targetRotation)
        {
            m_IsRotating = true;
            float startTime = Time.time;

            while (Time.time - startTime < m_RotationDuration)
            {
                float t = (Time.time - startTime) / m_RotationDuration;
                t = Mathf.SmoothStep(0, 1, t);
                
                float currentAngle = Mathf.Lerp(_startAngle, _endAngle, t);
                m_TileRoot.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
                
                yield return null;
            }

            m_TileRoot.transform.rotation = Quaternion.Euler(0, 0, _targetRotation);
            CreateVisuals(m_SquareRenderers[0].color);
            
            m_IsRotating = false;
        }

        // New method for preview initialization
        public void InitializePreview(TileData _tileData, Color _previewColor, float _initialRotation = 0f)
        {
            m_TileData = _tileData;
            m_Rotation = (int)_initialRotation;
            
            if (_tileData != null)
            {
                CreateVisuals(_previewColor);
            }
            else
            {
                // Clean up any existing visuals
                if (m_TileRoot != null)
                {
                    Destroy(m_TileRoot);
                    m_TileRoot = null;
                }
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

        private Vector2Int[] GetRotatedSquares()
        {
            // Normalize rotation to 0, 90, 180, or 270
            int normalizedRotation = ((m_Rotation % 360) + 360) % 360;
            int quarterTurns = normalizedRotation / 90;
            
            Vector2Int[] squares = m_TileData.Squares;
            Vector2Int[] rotatedSquares = new Vector2Int[squares.Length];
            
            for (int i = 0; i < squares.Length; i++)
            {
                Vector2Int point = squares[i];
                
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

        private void OnTileDataChanged()
        {
            Debug.Log($"[PlacedTile] TileData changed - Updating visuals");
            CreateVisuals(m_TileData.TileColor);
        }

        private void OnDestroy()
        {
            if (m_TileData != null)
            {
                m_TileData.OnDataChanged -= OnTileDataChanged;  // Unsubscribe when destroyed
            }
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