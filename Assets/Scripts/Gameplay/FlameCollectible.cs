using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class FlameCollectible : BaseCollectible
    {
        protected override Sprite GetSprite() => GameResources.Instance.FlameSprite;

        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                // Flames are more dangerous than sparks
                GameManager.Instance.IncreaseDanger();
                GameManager.Instance.IncreaseDanger();
            }
        }
    }
} 