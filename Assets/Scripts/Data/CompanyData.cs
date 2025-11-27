using System.Collections.Generic;
using Patchwork.Gameplay;

namespace Patchwork.Data
{
    public class CompanyData
    {
        public string Name { get; private set; }
        public List<ICollectible> Bonuses { get; private set; }
        public List<ICollectible> Dangers { get; private set; }

        public CompanyData(string name, List<ICollectible> bonuses, List<ICollectible> dangers)
        {
            Name = name;
            // Create defensive copies to prevent external modification
            Bonuses = new List<ICollectible>(bonuses);
            Dangers = new List<ICollectible>(dangers);
        }
    }
}
