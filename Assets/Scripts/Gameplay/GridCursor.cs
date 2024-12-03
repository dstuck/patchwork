using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class GridCursor : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private GridSettings m_GridSettings;
        [SerializeField] private TileHand m_TileHand;
        [SerializeField] private float m_MoveCooldown = 0.15f;
        [SerializeField] private float m_RotateCooldown = 0.15f;
        
        private Vector2Int m_CurrentGridPosition;
        private float m_LastMoveTime;
        private float m_LastRotateTime;
        private int m_CurrentRotation;
        private TileRenderer m_TileRenderer;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (m_TileHand == null)
            {
                Debug.LogError("TileHand reference not set in GridCursor!");
                return;
            }

            m_CurrentGridPosition = new Vector2Int(m_GridSettings.GridSize.x / 2, m_GridSettings.GridSize.y / 2);
            m_TileRenderer = gameObject.AddComponent<TileRenderer>();
            
            // Initialize with the first tile
            UpdateCurrentTile();
            UpdatePosition();
        }

        private void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleTileSelection();
            HandleTilePlacement();
        }
        #endregion

        #region Private Methods
        private void HandleTileSelection()
        {
            // Number keys 1-9 for direct selection
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    if (m_TileHand.SelectTile(i))
                    {
                        UpdateCurrentTile();
                    }
                }
            }

            // Tab to cycle through tiles
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                m_TileHand.CycleToNextTile();
                UpdateCurrentTile();
            }

            // Cycle tiles with J/K
            if (Input.GetKeyDown(KeyCode.J))
            {
                m_TileHand.CycleToPreviousTile();
                UpdateCurrentTile();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                m_TileHand.CycleToNextTile();
                UpdateCurrentTile();
            }
        }

        private void UpdateCurrentTile()
        {
            if (m_TileRenderer != null && m_TileHand.CurrentTile != null)
            {
                m_CurrentRotation = 0;
                m_TileRenderer.Initialize(m_TileHand.CurrentTile, new Color(1f, 1f, 1f, 0.5f), m_CurrentRotation);
            }
        }

        private void HandleMovement()
        {
            // Check if enough time has passed since last move
            if (Time.time - m_LastMoveTime < m_MoveCooldown) return;

            Vector2Int moveDirection = Vector2Int.zero;

            // Get input
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                moveDirection.x = -1;
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                moveDirection.x = 1;
            
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                moveDirection.y = 1;
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                moveDirection.y = -1;

            if (moveDirection != Vector2Int.zero)
            {
                TryMove(moveDirection);
            }
        }

        private void TryMove(Vector2Int _direction)
        {
            Vector2Int newPosition = m_CurrentGridPosition + _direction;

            // Check if new position is within grid bounds
            if (newPosition.x >= 0 && newPosition.x < m_GridSettings.GridSize.x &&
                newPosition.y >= 0 && newPosition.y < m_GridSettings.GridSize.y)
            {
                m_CurrentGridPosition = newPosition;
                m_LastMoveTime = Time.time;
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            Vector3 worldPosition = new Vector3(
                (m_CurrentGridPosition.x + 0.5f) * m_GridSettings.CellSize,
                (m_CurrentGridPosition.y + 0.5f) * m_GridSettings.CellSize,
                0
            );
            
            transform.position = worldPosition;
        }

        private void HandleRotation()
        {
            // Don't handle rotation if we have no current tile
            if (m_TileHand.CurrentTile == null) return;
            
            if (Time.time - m_LastRotateTime < m_RotateCooldown) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                RotateTile(true);
                m_LastRotateTime = Time.time;
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateTile(false);
                m_LastRotateTime = Time.time;
            }
        }

        private void RotateTile(bool _clockwise)
        {
            // Additional safety check
            if (m_TileHand.CurrentTile == null || m_TileRenderer == null) return;

            m_CurrentRotation += _clockwise ? 90 : -90;
            
            // Keep rotation between 0 and 359
            if (m_CurrentRotation >= 360) m_CurrentRotation -= 360;
            if (m_CurrentRotation < 0) m_CurrentRotation += 360;
            
            if (m_TileRenderer != null)
            {
                m_TileRenderer.UpdateRotation(m_CurrentRotation);
            }
        }

        private void HandleTilePlacement()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (m_TileHand.CurrentTile != null)
                {
                    PlaceTile();
                    m_TileHand.RemoveCurrentTile();
                    
                    // Reset rotation when switching to next tile
                    m_CurrentRotation = 0;
                    
                    // Clear the cursor's tile renderer if no more tiles
                    if (m_TileHand.CurrentTile == null)
                    {
                        if (m_TileRenderer != null)
                        {
                            m_TileRenderer.Initialize(null, Color.clear);
                        }
                    }
                    else
                    {
                        UpdateCurrentTile();
                    }
                }
            }
        }

        private void PlaceTile()
        {
            GameObject placedTile = new GameObject("PlacedTile");
            placedTile.transform.position = transform.position;
            
            PlacedTile tile = placedTile.AddComponent<PlacedTile>();
            tile.Initialize(m_TileHand.CurrentTile, m_CurrentGridPosition, m_CurrentRotation);
        }
        #endregion
    } 
}