using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class MultiplierBonusCollectible : BaseCollectible
    {
        private const float MULTIPLIER_INCREASE = 0.5f;
        
        public override string DisplayName => "Multiplier Bonus";
        public override string Description => $"Increases score multiplier by {MULTIPLIER_INCREASE:0.0}x";
        
        protected override Sprite GetSprite() => GameResources.Instance.MultiplierBonusSprite;
        protected override float GetScale() => GameResources.Instance.MultiplierBonusScale;

        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                // Increase the multiplier in GameManager
                GameManager.Instance.IncreaseMultiplier(MULTIPLIER_INCREASE);
                return true;
            }
            return false;
        }
    }
} 