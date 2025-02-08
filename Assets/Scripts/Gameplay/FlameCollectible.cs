using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class FlameCollectible : BaseCollectible
    {
        protected override Sprite GetSprite() => GameResources.Instance.FlameSprite;
        protected override float GetScale() => GameResources.Instance.FlameScale;

        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                GameManager.Instance.DecreaseLives();
            }
        }
    }
} 