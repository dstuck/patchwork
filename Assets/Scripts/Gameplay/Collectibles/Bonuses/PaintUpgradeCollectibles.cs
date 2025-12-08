using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PristinePaintCollectible : PaintUpgradeCollectible
    {
        protected override ITileUpgrade GetUpgrade()
        {
            return new PristineBonus(m_Level);
        }
    }

    public class CollectorsPaintCollectible : PaintUpgradeCollectible
    {
        protected override ITileUpgrade GetUpgrade()
        {
            return new CollectorsBonus(m_Level);
        }
    }
}
