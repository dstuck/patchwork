using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Patchwork.Gameplay;
using Patchwork.Data;

namespace Patchwork.Tests.PlayMode
{
    public class TilePlacementTests
    {
        private GameObject m_TestGameObject;
        private GridCursor m_GridCursor;
        private Board m_Board;
        private TileHand m_TileHand;
        private Deck m_Deck;
        private GameManager m_GameManager;

        [UnitySetUp]
        public IEnumerator Setup()
        {            
            // Create test scene objects
            m_TestGameObject = new GameObject("TestScene");
            
            // Setup GameManager
            var gameManagerObject = new GameObject("GameManager");
            m_GameManager = gameManagerObject.AddComponent<GameManager>();
            gameManagerObject.transform.SetParent(m_TestGameObject.transform);
            
            // Create and configure grid settings
            var gridSettings = ScriptableObject.CreateInstance<GridSettings>();
            gridSettings.SetupForTesting(new Vector2Int(10, 10), 1f);
            
            // Setup board with grid settings
            var boardObject = new GameObject("Board");
            boardObject.transform.SetParent(m_TestGameObject.transform);
            m_Board = boardObject.AddComponent<Board>();
            m_Board.Initialize(gridSettings);
            
            // Setup deck with test tile
            var deckObject = new GameObject("Deck");
            deckObject.transform.SetParent(m_TestGameObject.transform);
            m_Deck = deckObject.AddComponent<Deck>();
            
            // Create an L-shaped tile using TileFactory
            var testTile = TileFactory.CreateTile("L");
            m_Deck.AddTileToDeck(testTile);
            
            // Setup tile hand
            var handObject = new GameObject("TileHand");
            handObject.transform.SetParent(m_TestGameObject.transform);
            m_TileHand = handObject.AddComponent<TileHand>();
            
            // Setup grid cursor
            var cursorObject = new GameObject("GridCursor");
            cursorObject.transform.SetParent(m_TestGameObject.transform);
            m_GridCursor = cursorObject.AddComponent<GridCursor>();
            
            m_GridCursor.SetupForTesting(gridSettings, m_TileHand, m_Board);
            m_TileHand.SetDeck(m_Deck);
            
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            Object.Destroy(m_TestGameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TileRotation_90Degrees_RotatesCorrectly()
        {
            // Arrange
            Vector2Int initialPosition = new Vector2Int(5, 5);
            m_GridCursor.SetPositionForTesting(initialPosition);
            
            // Act
            m_GridCursor.RotateForTesting(true); // Rotate 90 degrees clockwise
            yield return null; // Wait a frame
            
            m_GridCursor.PlaceForTesting();
            yield return null;
            
            // Assert
            PlacedTile placedTile = m_Board.GetTileAt(initialPosition);
            Assert.IsNotNull(placedTile, "Tile should be placed");
            Assert.AreEqual(90, placedTile.Rotation, "Tile should be rotated 90 degrees");
            
            // Verify rotated squares match expected positions
            Vector2Int[] expectedSquares = new Vector2Int[] 
            {
                Vector2Int.zero,       // Origin
                new Vector2Int(0, -1), // Down (rotated from right)
                new Vector2Int(1, 0)   // Right (rotated from up)
            };
            
            Vector2Int[] actualSquares = placedTile.GetSquares();
            Assert.IsTrue(CompareSquareArrays(expectedSquares, actualSquares),
                "Rotated squares don't match expected positions");
        }

        private bool CompareSquareArrays(Vector2Int[] expected, Vector2Int[] actual)
        {
            if (expected.Length != actual.Length)
                return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[i])
                    return false;
            }
            return true;
        }
    }
} 