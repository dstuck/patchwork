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
        /// Generates unique company names with no overlapping words.
        /// </summary>
        /// <param name="count">Number of company names to generate</param>
        /// <returns>A list of company names</returns>
        public static List<string> GenerateCompanyNames(int count)
        {
            List<string> companyNames = new List<string>();

            // Shuffle and pick the requested number of words from each corpus
            var shuffledFirstWords = s_FirstWords.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
            var shuffledMiddleWords = s_MiddleWords.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
            var shuffledEndings = s_Endings.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();

            // Create company names, always including all 3 parts
            for (int i = 0; i < count; i++)
            {
                string companyName = $"{shuffledFirstWords[i]} {shuffledMiddleWords[i]} {shuffledEndings[i]}";
                companyNames.Add(companyName);
            }

            return companyNames;
        }
        #endregion
    }
}
