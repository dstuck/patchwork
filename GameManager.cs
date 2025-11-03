// Assuming the line of code in GameManager.cs where lives are reset is something like `m_CurrentLives = ...`
// Here's the modification to prevent fractional values.

// Set m_CurrentLives to maximum lives without fractional values
m_CurrentLives = Mathf.Floor(m_MaxLives); // this example assumes you're using Unity's Mathf for flooring
