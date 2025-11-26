using System.Collections.Generic;
using NUnit.Framework;
using Patchwork.Data;

namespace Tests
{
    public class CompanyNameGeneratorTests
    {
        [Test]
        public void GenerateCompanyNames_Returns3Names()
        {
            // Act
            var companyNames = CompanyNameGenerator.GenerateCompanyNames();

            // Assert
            Assert.IsNotNull(companyNames, "Company names list should not be null");
            Assert.AreEqual(3, companyNames.Count, "Should generate exactly 3 company names");
        }

        [Test]
        public void GenerateCompanyNames_NamesAreNotEmpty()
        {
            // Act
            var companyNames = CompanyNameGenerator.GenerateCompanyNames();

            // Assert
            foreach (var name in companyNames)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(name), "Company names should not be null or empty");
            }
        }

        [Test]
        public void GenerateCompanyNames_NoOverlappingWords()
        {
            // Act
            var companyNames = CompanyNameGenerator.GenerateCompanyNames();

            // Assert
            // Extract all words from all company names
            var allWords = new List<string>();
            foreach (var name in companyNames)
            {
                var words = name.Split(' ');
                allWords.AddRange(words);
            }

            // Check for duplicates (excluding common endings like &)
            var wordCounts = new Dictionary<string, int>();
            foreach (var word in allWords)
            {
                // Skip single character words like & and punctuation-only words
                if (word.Length <= 1 || word == "&")
                {
                    continue;
                }

                if (wordCounts.ContainsKey(word))
                {
                    wordCounts[word]++;
                }
                else
                {
                    wordCounts[word] = 1;
                }
            }

            // Verify no word appears more than once (except for & which is part of endings)
            foreach (var kvp in wordCounts)
            {
                Assert.AreEqual(1, kvp.Value, 
                    $"Word '{kvp.Key}' appears {kvp.Value} times. Words should not overlap between company names.");
            }
        }

        [Test]
        public void GenerateCompanyNames_ContainsValidParts()
        {
            // Act
            var companyNames = CompanyNameGenerator.GenerateCompanyNames();

            // Assert
            foreach (var name in companyNames)
            {
                // Each name should have at least 2 parts (first + middle)
                var parts = name.Split(' ');
                Assert.IsTrue(parts.Length >= 2, 
                    $"Company name '{name}' should have at least 2 parts (first + middle)");
            }
        }

        [Test]
        public void GenerateCompanyNames_ProducesVariety()
        {
            // Act - Generate multiple sets of names
            var allGeneratedNames = new HashSet<string>();
            for (int i = 0; i < 10; i++)
            {
                var names = CompanyNameGenerator.GenerateCompanyNames();
                foreach (var name in names)
                {
                    allGeneratedNames.Add(name);
                }
            }

            // Assert - Should generate more than 3 unique names across 10 runs
            // (showing randomness is working)
            Assert.IsTrue(allGeneratedNames.Count > 3, 
                "Generator should produce variety across multiple runs");
        }
    }
}
