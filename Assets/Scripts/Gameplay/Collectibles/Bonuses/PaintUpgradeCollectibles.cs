using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class PristinePaintCollectible : PaintUpgradeCollectible
    {
        protected override ITileUpgrade GetUpgrade() => new PristineBonus();
    }

    public class LenientPaintCollectible : PaintUpgradeCollectible
    {
        protected override ITileUpgrade GetUpgrade() => new LenientBonus();
    }
}
