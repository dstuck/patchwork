using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class SparkCollectible : BaseCollectible
    {
        protected override void Awake()
        {
            base.Awake();
            m_SpriteRenderer.sprite = GameResources.Instance.SparkSprite;
        }

        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                GameManager.Instance.IncreaseDanger();
            }
        }
    }
} 