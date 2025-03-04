using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class DrawGem : BaseCollectible
    {
        public override string DisplayName => "Draw Gem";
        public override string Description => "Draw an additional tile";

        protected override Sprite GetSprite() => GameResources.Instance.DrawGemSprite;
        protected override float GetScale() => GameResources.Instance.DrawGemScale;


        #region Public Methods
        public override bool TryCollect()
        {
            if (base.TryCollect())
            {
                var gameManager = Object.FindFirstObjectByType<GameManager>();
                if (gameManager != null && gameManager.Deck != null)
                {
                    gameManager.Deck.DrawTile();
                }
                return true;
            }
            return false;
        }
        #endregion
    }
} 