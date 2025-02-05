using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class FlameCollectible : BaseCollectible
    {
        protected override void Awake()
        {
            base.Awake();
            m_SpriteRenderer.sprite = GameResources.Instance.FlameSprite;
        }

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