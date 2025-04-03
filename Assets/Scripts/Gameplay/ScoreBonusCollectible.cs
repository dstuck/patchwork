using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class ScoreBonusCollectible : BaseCollectible
    {
        private const int BONUS_AMOUNT = 10;
        
        public override string DisplayName => "Score Bonus";
        public override string Description => $"Increases tile points by {BONUS_AMOUNT}";
        
        protected override Sprite GetSprite() => GameResources.Instance.ScoreBonusSprite;
        protected override float GetScale() => GameResources.Instance.ScoreBonusScale;

        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                GameManager.Instance.IncreaseScoreBonus(BONUS_AMOUNT);
                return true;
            }
            return false;
        }
    }
} 