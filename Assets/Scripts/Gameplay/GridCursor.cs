using UnityEngine;
using Patchwork.Data;
using UnityEngine.InputSystem;
using Patchwork.Input;

namespace Patchwork.Gameplay
{
    public class GridCursor : MonoBehaviour
    {
        #region Private Fields
        private GridSettings m_GridSettings;
        [SerializeField] private TileHand m_TileHand;
        private float m_MoveCooldown = 0.1f;
        private float m_RotateCooldown = 0.2f;
        [SerializeField] private Board m_Board;
        
        private Vector2Int m_CurrentGridPosition;
        private float m_LastMoveTime;
        private float m_LastRotateTime;
        private int m_CurrentRotation;
        private PlacedTile m_PreviewTile;
        private GameControls m_Controls;
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

            m_Controls = new GameControls();
            
            m_Controls.Movement.Move.performed += OnMove;
            m_Controls.Movement.Rotate.performed += OnRotate;
            m_Controls.Movement.Place.performed += OnPlace;
            m_Controls.Movement.CycleTile.performed += OnCycleTile;
        }

        private void OnEnable()
        {
            m_Controls.Enable();
        }

        private void OnDisable()
        {
            m_Controls.Disable();
        }

        private void Start()
        {
            if (m_TileHand == null)
            {
                Debug.LogError("TileHand reference not set in GridCursor!");
                return;
            }

            if (m_Board == null)
            {
                Debug.LogError("Board reference not set in GridCursor!");
                return;
            }

            // Wait for TileHand to be ready
            if (m_TileHand.CurrentTile == null)
            {
                // Subscribe to the OnTileChanged event
                m_TileHand.OnTileChanged += InitializeFirstTile;
            }
            else
            {
                InitializeComponents();
            }

            m_TileHand.OnLastTilePlaced += HandleLastTilePlaced;
        }

        private void InitializeFirstTile()
        {
            m_TileHand.OnTileChanged -= InitializeFirstTile;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            m_CurrentGridPosition = new Vector2Int(m_GridSettings.GridSize.x / 2, m_GridSettings.GridSize.y / 2);

            // Create preview tile
            GameObject previewObj = new GameObject("PreviewTile");
            previewObj.transform.SetParent(transform);
            previewObj.transform.localPosition = Vector3.zero;
            m_PreviewTile = previewObj.AddComponent<PlacedTile>();
            
            UpdateCurrentTile();
            UpdatePosition();
        }

        private void OnDestroy()
        {
            if (m_TileHand != null)
            {
                m_TileHand.OnLastTilePlaced -= HandleLastTilePlaced;
            }
        }
        #endregion

        #region Private Methods
        private void OnMove(InputAction.CallbackContext context)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (Time.time - m_LastMoveTime < m_MoveCooldown) return;
            
            Vector2 input = context.ReadValue<Vector2>();
            Vector2Int moveDirection = new Vector2Int(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y));
            
            if (moveDirection != Vector2Int.zero)
            {
                TryMove(moveDirection);
            }
        }

        private void OnRotate(InputAction.CallbackContext context)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (Time.time - m_LastRotateTime < m_RotateCooldown) 
            {
                return;
            }
            
            float rotation = context.ReadValue<float>();
            if (rotation != 0)
            {
                RotateTile(rotation > 0);
                m_LastRotateTime = Time.time;
            }
        }

        private void OnPlace(InputAction.CallbackContext context)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            if (m_TileHand.CurrentTile != null)
            {
                PlaceTile();
                m_TileHand.RemoveCurrentTile();
                
                if (m_TileHand.CurrentTile == null)
                {
                    if (m_PreviewTile != null)
                    {
                        m_PreviewTile.InitializePreview(null, Color.clear);
                    }
                }
                else
                {
                    UpdateCurrentTile();
                }
            }
        }

        private void OnCycleTile(InputAction.CallbackContext context)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;
            float direction = context.ReadValue<float>();
            if (direction > 0)
            {
                m_TileHand.CycleToNextTile();
                UpdateCurrentTile();
            }
            else
            {
                m_TileHand.CycleToPreviousTile();
                UpdateCurrentTile();
            }
        }

        private void UpdateCurrentTile()
        {
            if (m_PreviewTile != null && m_TileHand.CurrentTile != null)
            {
                m_CurrentRotation = 0;  // Reset rotation when switching tiles
                m_PreviewTile.InitializePreview(m_TileHand.CurrentTile, new Color(1f, 1f, 1f, 0.5f), m_CurrentRotation);
            }
            else
            {
                Debug.LogWarning($"Cannot update preview: PreviewTile: {m_PreviewTile != null}, CurrentTile: {m_TileHand.CurrentTile != null}");
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
                
                // Play movement sound
                if (SoundFXManager.instance != null && GameResources.Instance.MoveSoundFX != null && GameResources.Instance.MoveSoundFX.Length > 0)
                {
                    SoundFXManager.instance.PlayRandomSoundFXClip(GameResources.Instance.MoveSoundFX, transform);
                }
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

        private void RotateTile(bool _clockwise)
        {
            // Additional safety check
            if (m_TileHand.CurrentTile == null || m_PreviewTile == null) return;

            m_CurrentRotation += _clockwise ? 90 : -90;
            
            // Keep rotation between 0 and 359
            if (m_CurrentRotation >= 360) m_CurrentRotation -= 360;
            if (m_CurrentRotation < 0) m_CurrentRotation += 360;
                        
            if (m_PreviewTile != null)
            {
                m_PreviewTile.UpdateRotation(m_CurrentRotation);
            }
            
            // Play rotation sound
            if (SoundFXManager.instance != null)
            {
                AudioClip rotationClip = _clockwise ? GameResources.Instance.RotateRightSoundFX : GameResources.Instance.RotateLeftSoundFX;
                if (rotationClip != null)
                {
                    SoundFXManager.instance.PlaySoundFXClip(rotationClip, transform);
                }
            }
        }

        private void PlaceTile()
        {
            if (m_TileHand.CurrentTile == null)
            {
                Debug.LogError("Cannot place tile: No current tile selected");
                return;
            }

            GameObject placedTile = new GameObject("PlacedTile");
            placedTile.transform.position = transform.position;
            
            PlacedTile tile = placedTile.AddComponent<PlacedTile>();
            tile.Initialize(m_TileHand.CurrentTile, m_CurrentGridPosition, m_CurrentRotation);

            if (m_Board != null)
            {
                m_Board.AddPlacedTile(tile);  // Add the tile first
                m_Board.OnTilePlaced(tile);   // Then notify collectibles
                m_Board.CalculateTotalScore(); // Temporarily display score
                SoundFXManager.instance.PlayRandomSoundFXClip(GameResources.Instance.PlaceTileSoundFX, transform);
            }
        }

        private void HandleLastTilePlaced()
        {
            // First check collectibles for end-of-level effects
            if (m_Board != null)
            {
                m_Board.OnLevelComplete();
            }

            int finalScore = m_Board.CalculateTotalScore();
            
            // Transition to next stage via GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteStage(finalScore);
            }
        }

        #if UNITY_INCLUDE_TESTS
        public void SetupForTesting(GridSettings settings, TileHand hand, Board board)
        {
            m_GridSettings = settings;
            m_TileHand = hand;
            m_Board = board;
            InitializeComponents();
        }

        public void SetPositionForTesting(Vector2Int position)
        {
            m_CurrentGridPosition = position;
            UpdatePosition();
        }

        public void RotateForTesting(bool clockwise)
        {
            RotateTile(clockwise);
        }

        public void PlaceForTesting()
        {
            PlaceTile();
            m_TileHand.RemoveCurrentTile();
        }
        #endif
        #endregion
    } 
}