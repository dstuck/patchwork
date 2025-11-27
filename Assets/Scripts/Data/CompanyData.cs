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
            Bonuses = bonuses;
            Dangers = dangers;
        }
    }
}
