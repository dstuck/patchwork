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
            
            // Create test tile
            m_TestTile = ScriptableObject.CreateInstance<TileData>();
            var so = new SerializedObject(m_TestTile);
            var squaresProp = so.FindProperty("m_Squares");
            squaresProp.arraySize = 1;
            squaresProp.GetArrayElementAtIndex(0).vector2IntValue = Vector2Int.zero;
            so.ApplyModifiedProperties();

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
            Object.DestroyImmediate(m_TestTile);
        }

        [Test]
        public void SingleSquare_OverHole_Scores2Points()
        {
            // Arrange
            GameObject tileObj = new GameObject("TestPlacedTile");
            PlacedTile placedTile = tileObj.AddComponent<PlacedTile>();
            
            // Manually set the occupied squares since we can't call Initialize
            var occupiedSquaresField = typeof(PlacedTile).GetField("m_OccupiedSquares", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            occupiedSquaresField.SetValue(placedTile, new Vector2Int[] { Vector2Int.zero });
            
            // Set TileData
            var tileDataField = typeof(PlacedTile).GetField("m_TileData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            tileDataField.SetValue(placedTile, m_TestTile);

            // Create TextMeshPro component for score display
            GameObject textObj = new GameObject("ScoreText");
            textObj.transform.SetParent(tileObj.transform);
            var scoreText = textObj.AddComponent<TextMeshPro>();
            var scoreTextField = typeof(PlacedTile).GetField("m_ScoreText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            scoreTextField.SetValue(placedTile, scoreText);

            // Act
            int score = placedTile.CalculateScore(m_Board, new List<PlacedTile>());

            // Assert
            Assert.AreEqual(2, score);
            Object.DestroyImmediate(tileObj);
        }
    }
} 