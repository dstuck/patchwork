using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class HeartPieceCollectible : BaseCollectible
    {
        private const float HEART_AMOUNT = 0.25f;
        
        public override string DisplayName => "Heart Piece";
        public override string Description => $"Increases maximum health by {HEART_AMOUNT:0.00}";

        protected override Sprite GetSprite() => GameResources.Instance.HeartPieceSprite;
        protected override float GetScale() => GameResources.Instance.HeartPieceScale;

        public override bool TryCollect()
        {
            if (base.TryCollect())
            {
                GameManager.Instance.IncreaseMaxLivesByAmount(HEART_AMOUNT);
                return true;
            }
            return false;
        }
    }
} 