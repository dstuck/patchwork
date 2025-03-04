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

        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                // No penalty for missing score bonus
                return;
            }
        }

        public override bool TryCollect()
        {
            if (base.TryCollect())
            {
                // Increase the tile points bonus in GameManager
                GameManager.Instance.IncreaseTilePoints(BONUS_AMOUNT);
                return true;
            }
            return false;
        }
    }
} 