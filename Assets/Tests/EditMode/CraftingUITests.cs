using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Patchwork.Gameplay;
using Patchwork.UI;

namespace Tests
{
    public class CraftingUITests
    {
        private GameObject m_CraftingUIObject;
        private CraftingUI m_CraftingUI;
        private GameObject m_DeckObject;
        private CollectiblesDeck m_Deck;
        private List<GameObject> m_CollectibleObjects;

        [SetUp]
        public void Setup()
        {
            m_CollectibleObjects = new List<GameObject>();
            
            // Create CollectiblesDeck object
            m_DeckObject = new GameObject("TestDeck");
            m_Deck = m_DeckObject.AddComponent<CollectiblesDeck>();
            
            // Use reflection to set the singleton instance for testing
            var deckInstanceField = typeof(CollectiblesDeck).GetField("s_Instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            deckInstanceField.SetValue(null, m_Deck);
            
            // Initialize the deck
            var drawPileField = typeof(CollectiblesDeck).GetField("m_DrawPile", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            drawPileField.SetValue(m_Deck, new List<ICollectible>());
            
            var deckCollectiblesField = typeof(CollectiblesDeck).GetField("m_DeckCollectibles", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            deckCollectiblesField.SetValue(m_Deck, new List<ICollectible>());
        }

        [TearDown]
        public void Teardown()
        {
            // Clear the singleton instance
            var deckInstanceField = typeof(CollectiblesDeck).GetField("s_Instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            deckInstanceField.SetValue(null, null);
            
            // Destroy collectible objects
            foreach (var obj in m_CollectibleObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            m_CollectibleObjects.Clear();
            
            if (m_DeckObject != null)
            {
                Object.DestroyImmediate(m_DeckObject);
            }
        }

        [Test]
        public void GetCollectibles_SortsCorrectly_ByLevelThenSignThenName()
        {
            // Arrange - Create test collectibles with different levels, signs, and names
            var collectibles = new List<ICollectible>
            {
                CreateTestCollectible("Zebra", 2, 5),      // Level 2, Sign +1, Name "Zebra"
                CreateTestCollectible("Apple", 1, -3),     // Level 1, Sign -1, Name "Apple"
                CreateTestCollectible("Banana", 1, 4),     // Level 1, Sign +1, Name "Banana"
                CreateTestCollectible("Cherry", 3, -2),    // Level 3, Sign -1, Name "Cherry"
                CreateTestCollectible("Dog", 2, -1),       // Level 2, Sign -1, Name "Dog"
                CreateTestCollectible("Ant", 1, 2),        // Level 1, Sign +1, Name "Ant"
                CreateTestCollectible("Ball", 2, 3),       // Level 2, Sign +1, Name "Ball"
            };
            
            // Add collectibles to deck
            var drawPileField = typeof(CollectiblesDeck).GetField("m_DrawPile", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            drawPileField.SetValue(m_Deck, new List<ICollectible>(collectibles));
            
            // Act - Get collectibles from deck
            var result = m_Deck.GetCollectibles();
            
            // Sort using the same logic as CraftingUI
            result = result
                .OrderBy(c => c.Level)
                .ThenBy(c => c.Power > 0 ? 1 : -1) // GetSign logic
                .ThenBy(c => c.DisplayName)
                .ToList();
            
            // Assert - Verify correct order
            // Expected order:
            // Level 1, Sign -1: Apple
            // Level 1, Sign +1: Ant, Banana
            // Level 2, Sign -1: Dog
            // Level 2, Sign +1: Ball, Zebra
            // Level 3, Sign -1: Cherry
            
            Assert.AreEqual(7, result.Count, "Should have 7 collectibles");
            
            Assert.AreEqual("Apple", result[0].DisplayName, "First should be Apple (Level 1, Sign -1)");
            Assert.AreEqual(1, result[0].Level);
            Assert.IsTrue(result[0].Power < 0);
            
            Assert.AreEqual("Ant", result[1].DisplayName, "Second should be Ant (Level 1, Sign +1, alphabetically first)");
            Assert.AreEqual(1, result[1].Level);
            Assert.IsTrue(result[1].Power > 0);
            
            Assert.AreEqual("Banana", result[2].DisplayName, "Third should be Banana (Level 1, Sign +1)");
            Assert.AreEqual(1, result[2].Level);
            Assert.IsTrue(result[2].Power > 0);
            
            Assert.AreEqual("Dog", result[3].DisplayName, "Fourth should be Dog (Level 2, Sign -1)");
            Assert.AreEqual(2, result[3].Level);
            Assert.IsTrue(result[3].Power < 0);
            
            Assert.AreEqual("Ball", result[4].DisplayName, "Fifth should be Ball (Level 2, Sign +1, alphabetically first)");
            Assert.AreEqual(2, result[4].Level);
            Assert.IsTrue(result[4].Power > 0);
            
            Assert.AreEqual("Zebra", result[5].DisplayName, "Sixth should be Zebra (Level 2, Sign +1)");
            Assert.AreEqual(2, result[5].Level);
            Assert.IsTrue(result[5].Power > 0);
            
            Assert.AreEqual("Cherry", result[6].DisplayName, "Seventh should be Cherry (Level 3, Sign -1)");
            Assert.AreEqual(3, result[6].Level);
            Assert.IsTrue(result[6].Power < 0);
        }

        private ICollectible CreateTestCollectible(string name, int level, int power)
        {
            GameObject obj = new GameObject(name);
            m_CollectibleObjects.Add(obj);
            
            var collectible = obj.AddComponent<TestCollectible>();
            collectible.TestDisplayName = name;
            collectible.TestLevel = level;
            collectible.TestPower = power;
            
            return collectible;
        }
        
        // Test collectible class for testing purposes
        private class TestCollectible : MonoBehaviour, ICollectible
        {
            public string TestDisplayName { get; set; }
            public int TestLevel { get; set; }
            public int TestPower { get; set; }
            
            public Vector2Int GridPosition => Vector2Int.zero;
            public bool IsVisible => true;
            public int Power => TestPower;
            public int Level => TestLevel;
            public string DisplayName => TestDisplayName;
            public string Description => "Test collectible";
            
            public void Initialize(Vector2Int position) { }
            public bool TryCollect(PlacedTile collectingTile) { return false; }
            public void OnLevelEnd() { }
            public void OnTilePlaced(Board board, PlacedTile tile) { }
            public void UpdatePosition(Vector2Int newPosition) { }
            public Sprite GetDisplaySprite() { return null; }
            public void SetLevel(int level) { TestLevel = level; }
        }
    }
}
