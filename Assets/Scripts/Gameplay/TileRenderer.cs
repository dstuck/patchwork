using UnityEngine;
using Patchwork.Data;
using System.Collections;

namespace Patchwork.Gameplay
{
    public class TileRenderer : MonoBehaviour
    {
        #region Private Fields
        [SerializeField] private TileData m_TileData;
        [SerializeField] private GridSettings m_GridSettings;
        [SerializeField] private float m_RotationDuration = 0.2f;
        private SpriteRenderer[] m_SquareRenderers;
        private int m_CurrentRotation;
        private bool m_IsRotating;
        private GameObject m_TileRoot;
        #endregion

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
        }

        #region Public Methods
        public void Initialize(TileData _tileData, Color _color)
        {
            m_TileData = _tileData;
            m_CurrentRotation = 0;
            CreateVisuals(_color);
        }

        public void UpdateRotation(int _targetRotation)
        {
            if (m_IsRotating) return;
            
            // Store the rotation values
            int startRotation = m_CurrentRotation;
            m_CurrentRotation = _targetRotation;
            
            // Determine visual rotation direction
            float startAngle = m_TileRoot.transform.eulerAngles.z;
            float endAngle = startAngle;
            
            if (_targetRotation > startRotation && _targetRotation - startRotation <= 180 ||
                _targetRotation < startRotation && startRotation - _targetRotation > 180)
            {
                // Rotate clockwise
                endAngle -= 90;
            }
            else
            {
                // Rotate counter-clockwise
                endAngle += 90;
            }

            StartCoroutine(AnimateRotation(startAngle, endAngle, _targetRotation));
        }
        #endregion

        #region Private Methods
        private IEnumerator AnimateRotation(float _startAngle, float _endAngle, int _targetRotation)
        {
            m_IsRotating = true;
            float startTime = Time.time;

            // Animate the visual rotation
            while (Time.time - startTime < m_RotationDuration)
            {
                float t = (Time.time - startTime) / m_RotationDuration;
                t = Mathf.SmoothStep(0, 1, t);
                
                float currentAngle = Mathf.Lerp(_startAngle, _endAngle, t);
                m_TileRoot.transform.rotation = Quaternion.Euler(0, 0, currentAngle);
                
                yield return null;
            }

            // Snap to final grid-based positions
            m_TileRoot.transform.rotation = Quaternion.identity;
            CreateVisuals(m_SquareRenderers[0].color);
            
            m_IsRotating = false;
        }

        private void CreateVisuals(Color _color)
        {
            // Clean up existing visuals
            if (m_TileRoot != null)
            {
                Destroy(m_TileRoot);
            }

            if (m_TileData == null) return;

            // Create root object
            m_TileRoot = new GameObject("TileRoot");
            m_TileRoot.transform.SetParent(transform);
            m_TileRoot.transform.localPosition = Vector3.zero;
            m_TileRoot.transform.localRotation = Quaternion.identity;

            // Get grid-based rotated positions
            Vector2Int[] squares = m_TileData.GetRotatedSquares(m_CurrentRotation);
            m_SquareRenderers = new SpriteRenderer[squares.Length];

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
                renderer.sprite = GameResources.Instance.TileSquareSprite;
                renderer.color = _color;
                m_SquareRenderers[i] = renderer;
            }
        }
        #endregion
    }
}