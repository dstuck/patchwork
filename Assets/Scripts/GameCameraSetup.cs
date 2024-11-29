using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameCameraSetup : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private GridSettings m_GridSettings;
    [SerializeField] private float m_CameraPadding = 1f;
    
    private Camera m_Camera;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        m_Camera = GetComponent<Camera>();
        SetupCamera();
    }
    #endregion

    #region Private Methods
    private void SetupCamera()
    {
        // Calculate grid dimensions in world space
        float gridWidth = m_GridSettings.GridSize.x * m_GridSettings.CellSize;
        float gridHeight = m_GridSettings.GridSize.y * m_GridSettings.CellSize;

        // Position camera at center of grid
        transform.position = new Vector3(
            gridWidth * 0.5f,
            gridHeight * 0.5f,
            transform.position.z
        );

        // Calculate required orthographic size to fit grid with padding
        float verticalSize = (gridHeight + m_CameraPadding * 2) * 0.5f;
        float horizontalSize = (gridWidth + m_CameraPadding * 2) * 0.5f / m_Camera.aspect;

        // Use the larger size to ensure entire grid is visible
        m_Camera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
    #endregion
} 