using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class NewSquareCollectible : BaseCollectible
    {
        #region Public Properties
        public override string DisplayName => "New Square";
        public override string Description => "Adds a new square to the tile";
        #endregion

        #region Protected Methods
        protected override Sprite GetSprite() => GameResources.Instance.NewSquareSprite;
        protected override float GetScale() => GameResources.Instance.NewSquareScale;
        #endregion

        #region Public Methods
        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                if (collectingTile != null)
                {
                    collectingTile.AddSquare();
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}