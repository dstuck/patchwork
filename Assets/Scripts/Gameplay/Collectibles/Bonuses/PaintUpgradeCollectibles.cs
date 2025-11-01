using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PristinePaintCollectible : PaintUpgradeCollectible
    {
        protected override ITileUpgrade GetUpgrade()
        {
            int amount = m_Level switch { 1 => 5, 2 => 10, _ => 20 };
            return new PristineBonus(amount);
        }
    }

    public class LenientPaintCollectible : PaintUpgradeCollectible
    {
        protected override ITileUpgrade GetUpgrade() => new LenientBonus();
    }
}
