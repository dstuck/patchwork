using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class DrawGemCollectible : BaseCollectible
    {
        public override string DisplayName => "Draw Gem";
        public override string Description => "Draw an additional tile";

        protected override Sprite GetSprite() => GameResources.Instance.DrawGemSprite;
        protected override float GetScale() => GameResources.Instance.DrawGemScale;


        #region Public Methods
        public override bool TryCollect(PlacedTile collectingTile)
        {
            if (base.TryCollect(collectingTile))
            {
                var gameManager = Object.FindFirstObjectByType<GameManager>();
                if (gameManager != null && gameManager.Deck != null)
                {
                    int draws = m_Level switch { 1 => 1, 2 => 2, _ => 4 };
                    for (int i = 0; i < draws; i++)
                    {
                        gameManager.Deck.DrawTile();
                    }
                }
                return true;
            }
            return false;
        }
        #endregion
    }
} 