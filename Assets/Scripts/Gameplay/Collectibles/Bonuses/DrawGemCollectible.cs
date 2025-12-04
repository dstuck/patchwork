using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class DrawGemCollectible : BaseCollectible
    {
        public int GetDrawCount() => m_Level switch { 1 => 1, 2 => 2, _ => 4 };
        
        public override string DisplayName => "Draw Gem";
        public override string Description => $"Draw {GetDrawCount()} additional tile{(GetDrawCount() > 1 ? "s" : "")}";

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
                    int draws = GetDrawCount();
                    for (int i = 0; i < draws; i++)
                    {
                        gameManager.Deck.DrawTile();
                    }
                }
                return true;
            }
            return false;
        }

        public override int AdditionalHoleCount()
        {
            return 6 * GetDrawCount();
        }
        #endregion
    }
} 