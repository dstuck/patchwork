using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class NewSquareCollectible : BaseCollectible
    {
        #region Public Properties
        public override string DisplayName => "New Square";
        public override string Description => $"Adds {GetSquareCount()} new square{(GetSquareCount() > 1 ? "s" : "")} to the tile";
        #endregion

        #region Protected Methods
        protected override Sprite GetSprite() => GameResources.Instance.NewSquareSprite;
        protected override float GetScale() => GameResources.Instance.NewSquareScale;
        #endregion

        #region Private Methods
        private int GetSquareCount()
        {
            return m_Level switch { 1 => 1, 2 => 2, _ => 3 };
        }
        #endregion

        #region Public Methods
        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                if (collectingTile != null)
                {
                    int squareCount = GetSquareCount();
                    for (int i = 0; i < squareCount; i++)
                    {
                        collectingTile.AddSquare();
                    }
                }
                return true;
            }
            return false;
        }

        public override int AdditionalHoleCount()
        {
            // Use the same level multiplier pattern as DrawGem: 1, 2, 4
            int multiplier = m_Level switch { 1 => 1, 2 => 2, _ => 4 };
            return 3 * multiplier;
        }
        #endregion
    }
}