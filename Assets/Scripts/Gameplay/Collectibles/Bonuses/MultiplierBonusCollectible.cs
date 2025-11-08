using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class MultiplierBonusCollectible : BaseCollectible
    {
        
        public override string DisplayName => "Multiplier Bonus";
        public override string Description => $"Increases score multiplier by {GetIncrease():0.0}x";
        
        protected override Sprite GetSprite() => GameResources.Instance.MultiplierBonusSprite;
        protected override float GetScale() => GameResources.Instance.MultiplierBonusScale;

        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                // Increase the multiplier in GameManager
                GameManager.Instance.IncreaseMultiplier(GetIncrease());
                return true;
            }
            return false;
        }

        private float GetIncrease()
        {
            return m_Level switch { 1 => 0.5f, 2 => 1.0f, _ => 2.0f };
        }
    }
} 