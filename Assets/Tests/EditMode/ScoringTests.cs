using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Patchwork.Gameplay;
using Patchwork.Data;
using UnityEditor;
using TMPro;

namespace Tests
{
    public class ScoringTests
    {
        private Board m_Board;
        private TileData m_TestTile;
        private GameObject m_BoardObject;

        [SetUp]
        public void Setup()
        {
            // Create board
            m_BoardObject = new GameObject("TestBoard");
            m_Board = m_BoardObject.AddComponent<Board>();
            
            // Create test tile using factory
            m_TestTile = TileFactory.CreateTile("L");

            // Create hole at (0,0)
            var holes = new Dictionary<Vector2Int, GameObject>();
            var holeObj = new GameObject("Hole_0_0");
            holeObj.SetActive(true);
            holes[Vector2Int.zero] = holeObj;
            
            var holesField = typeof(Board).GetField("m_Holes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            holesField.SetValue(m_Board, holes);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_BoardObject);
            // Remove m_TestTile destruction since it's no longer a ScriptableObject
        }

        [Test]
        public void CalculateScore_SingleTileOverHole_ReturnsTwo()
        {
            // Arrange
            var tileData = TileFactory.CreateTile("L"); // Use factory instead of CreateInstance
            var board = CreateTestBoard();
            var tile = CreateTestTile(tileData, new Vector2Int(1, 1));
            
            // Act
            int score = tile.CalculateScore(board, new List<PlacedTile>());
            
            // Assert
            Assert.AreEqual(2, score);
        }

        private PlacedTile CreateTestTile(TileData tileData, Vector2Int position)
        {
            GameObject tileObj = new GameObject("TestTile");
            var placedTile = tileObj.AddComponent<PlacedTile>();
            placedTile.Initialize(tileData, position, 0);
            return placedTile;
        }

        private Board CreateTestBoard()
        {
            // Implementation of CreateTestBoard method
            // This is a placeholder and should be replaced with the actual implementation
            return m_Board;
        }
    }
} 