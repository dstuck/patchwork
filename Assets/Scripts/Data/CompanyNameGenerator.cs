using System.Collections.Generic;
using System.Linq;

namespace Patchwork.Data
{
    /// <summary>
    /// Generates random company names by combining word parts.
    /// Not a Unity component - just pure logic for name generation.
    /// </summary>
    public class CompanyNameGenerator
    {
        #region Word Corpus
        private static readonly string[] s_FirstWords = new string[]
        {
            "Frank's",
            "United",
            "Allied",
            "General",
            "Boston",
            "Enterprise",
            "Texas",
            "National",
            "William",
            "Advanced",
            "Universal",
            "Revised",
            "Premium",
            "Jeffrey",
            "James",
            "Royal"
        };

        private static readonly string[] s_MiddleWords = new string[]
        {
            "Chemical",
            "Energy",
            "Industries",
            "Incorporated",
            "Wexler",
            "Health",
            "Technologies",
            "Construction",
            "Robotics",
            "Pharmaceuticals",
            "Pacific",
            "Atlantic",
            "Builders",
            "Jameson",
            "Williams",
            "Jefferson",
            "Systems",
            "Imports",
            "Exports",
            "Exeter",
            "Chambers",
            "Ironworks",
            "Traders",
            "Minerals",
            "Blackwell",
            "Holdings",
            "Caddington",
            "Bingham",
            "Crown"
        };

        private static readonly string[] s_Endings = new string[]
        {
            "Ltd",
            "Inc",
            "& Co",
            "& Sons",
            "Co",
            "Corp",
            "of America",
            "& Partners",
            "Consolidated"
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates 3 unique company names with no overlapping words.
        /// </summary>
        /// <returns>A list of 3 company names</returns>
        public static List<string> GenerateCompanyNames()
        {
            List<string> companyNames = new List<string>();

            // Shuffle and pick 3 first words
            var shuffledFirstWords = s_FirstWords.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();
            
            // Shuffle and pick 3 middle words
            var shuffledMiddleWords = s_MiddleWords.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();
            
            // Shuffle and pick 3 endings (optional)
            var shuffledEndings = s_Endings.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();

            // Create 3 company names
            for (int i = 0; i < 3; i++)
            {
                string companyName = BuildCompanyName(shuffledFirstWords[i], shuffledMiddleWords[i], shuffledEndings[i]);
                companyNames.Add(companyName);
            }

            return companyNames;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Builds a company name from the selected parts.
        /// Randomly decides whether to include the ending modifier.
        /// </summary>
        private static string BuildCompanyName(string first, string middle, string ending)
        {
            // Randomly decide if we include the ending (50% chance)
            bool includeEnding = UnityEngine.Random.value > 0.5f;

            if (includeEnding)
            {
                return $"{first} {middle} {ending}";
            }
            else
            {
                return $"{first} {middle}";
            }
        }
        #endregion
    }
}
