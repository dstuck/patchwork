using UnityEngine;

public class GridCursor : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private GridSettings m_GridSettings;
    [SerializeField] private float m_MoveCooldown = 0.15f;
    
    private Vector2Int m_CurrentGridPosition;
    private float m_LastMoveTime;
    #endregion

    #region Properties
    public Vector2Int CurrentGridPosition => m_CurrentGridPosition;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Center cursor in grid
        m_CurrentGridPosition = new Vector2Int(m_GridSettings.GridSize.x / 2, m_GridSettings.GridSize.y / 2);
        UpdatePosition();
    }

    private void Update()
    {
        HandleInput();
    }
    #endregion

    #region Private Methods
    private void HandleInput()
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
        // Convert grid position to world position, centered in cell
        Vector3 worldPosition = new Vector3(
            (m_CurrentGridPosition.x + 0.5f) * m_GridSettings.CellSize, // Added 0.5f to center in cell
            (m_CurrentGridPosition.y + 0.5f) * m_GridSettings.CellSize, // Added 0.5f to center in cell
            0
        );
        
        transform.position = worldPosition;
    }
    #endregion
} 