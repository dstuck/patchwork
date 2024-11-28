using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Private Fields
    [SerializeField, Range(1f, 20f)] 
    private float m_MoveSpeed = 5f;
    
    private Rigidbody2D m_Rigidbody;
    private Vector2 m_Movement;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Cache the Rigidbody2D component
        if (!TryGetComponent(out m_Rigidbody))
        {
            Debug.LogError("Rigidbody2D component missing from player!");
        }
    }

    private void Update()
    {
        // Get input values
        m_Movement.x = Input.GetAxisRaw("Horizontal");
        m_Movement.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // Apply movement in FixedUpdate for consistent physics
        Move();
    }
    #endregion

    #region Private Methods
    private void Move()
    {
        // Normalize the movement vector to prevent faster diagonal movement
        m_Movement.Normalize();
        
        // Move the rigidbody
        m_Rigidbody.linearVelocity = m_Movement * m_MoveSpeed;
    }
    #endregion
} 