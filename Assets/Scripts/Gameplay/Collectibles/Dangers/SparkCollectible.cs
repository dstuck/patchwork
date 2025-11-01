using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class SparkCollectible : BaseCollectible
    {
        public override string DisplayName => "Spark";
        public override string Description => "Dangerous if not cleaned up";

        protected override Sprite GetSprite() => GameResources.Instance.SparkSprite;
        protected override float GetScale() => GameResources.Instance.SparkScale;

        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                int damage = m_Level switch { 1 => 1, 2 => 2, _ => 4 };
                GameManager.Instance.DecreaseLives(damage);
            }
        }
    }
} 