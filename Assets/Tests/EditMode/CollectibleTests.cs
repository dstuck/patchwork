using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Patchwork.Gameplay;
using Patchwork.Data;

namespace Tests
{
    public class CollectibleTests
    {
        private GameObject m_TestObject;

        [TearDown]
        public void Teardown()
        {
            if (m_TestObject != null)
            {
                Object.DestroyImmediate(m_TestObject);
            }
        }

        [Test]
        public void DrawGemCollectible_Level1_Returns6AdditionalHoles()
        {
            // Arrange
            m_TestObject = new GameObject("TestDrawGem");
            var drawGem = m_TestObject.AddComponent<DrawGemCollectible>();
            drawGem.SetLevel(1);

            // Act
            int additionalHoles = drawGem.AdditionalHoleCount();

            // Assert
            Assert.AreEqual(6, additionalHoles, "Level 1 Draw Gem should add 6 holes (6 * 1)");
        }

        [Test]
        public void DrawGemCollectible_Level2_Returns12AdditionalHoles()
        {
            // Arrange
            m_TestObject = new GameObject("TestDrawGem");
            var drawGem = m_TestObject.AddComponent<DrawGemCollectible>();
            drawGem.SetLevel(2);

            // Act
            int additionalHoles = drawGem.AdditionalHoleCount();

            // Assert
            Assert.AreEqual(12, additionalHoles, "Level 2 Draw Gem should add 12 holes (6 * 2)");
        }

        [Test]
        public void DrawGemCollectible_Level3_Returns24AdditionalHoles()
        {
            // Arrange
            m_TestObject = new GameObject("TestDrawGem");
            var drawGem = m_TestObject.AddComponent<DrawGemCollectible>();
            drawGem.SetLevel(3);

            // Act
            int additionalHoles = drawGem.AdditionalHoleCount();

            // Assert
            Assert.AreEqual(24, additionalHoles, "Level 3 Draw Gem should add 24 holes (6 * 4)");
        }

        [Test]
        public void NewSquareCollectible_Level1_Returns3AdditionalHoles()
        {
            // Arrange
            m_TestObject = new GameObject("TestNewSquare");
            var newSquare = m_TestObject.AddComponent<NewSquareCollectible>();
            newSquare.SetLevel(1);

            // Act
            int additionalHoles = newSquare.AdditionalHoleCount();

            // Assert
            Assert.AreEqual(3, additionalHoles, "Level 1 New Square should add 3 holes (3 * 1)");
        }

        [Test]
        public void NewSquareCollectible_Level2_Returns6AdditionalHoles()
        {
            // Arrange
            m_TestObject = new GameObject("TestNewSquare");
            var newSquare = m_TestObject.AddComponent<NewSquareCollectible>();
            newSquare.SetLevel(2);

            // Act
            int additionalHoles = newSquare.AdditionalHoleCount();

            // Assert
            Assert.AreEqual(6, additionalHoles, "Level 2 New Square should add 6 holes (3 * 2)");
        }

        [Test]
        public void NewSquareCollectible_Level3_Returns12AdditionalHoles()
        {
            // Arrange
            m_TestObject = new GameObject("TestNewSquare");
            var newSquare = m_TestObject.AddComponent<NewSquareCollectible>();
            newSquare.SetLevel(3);

            // Act
            int additionalHoles = newSquare.AdditionalHoleCount();

            // Assert
            Assert.AreEqual(12, additionalHoles, "Level 3 New Square should add 12 holes (3 * 4)");
        }

        [Test]
        public void OtherCollectibles_ReturnZeroAdditionalHoles()
        {
            // Arrange
            m_TestObject = new GameObject("TestHeartPiece");
            var heartPiece = m_TestObject.AddComponent<HeartPieceCollectible>();

            // Act
            int additionalHoles = heartPiece.AdditionalHoleCount();

            // Assert
            Assert.AreEqual(0, additionalHoles, "Heart Piece collectible should not add any holes");
        }
    }
}
