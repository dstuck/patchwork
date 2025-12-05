using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class HeartPieceCollectible : BaseCollectible
    {
        
        public override string DisplayName => "Heart Piece";
        public override string Description => $"Increases maximum health by {GetHeartAmount():0.00}";

        protected override Sprite GetSprite() => GameResources.Instance.HeartPieceSprite;
        protected override float GetScale() => GameResources.Instance.HeartPieceScale;

        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                GameManager.Instance.IncreaseMaxLivesByAmount(GetHeartAmount());
                return true;
            }
            return false;
        }

        private float GetHeartAmount()
        {
            return GetLevelMultiplier() * 0.25f;
        }
    }
} 