using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class SparkCollectible : BaseCollectible
    {
        protected override Sprite GetSprite() => GameResources.Instance.SparkSprite;

        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                GameManager.Instance.IncreaseDanger();
            }
        }
    }
} 