using UnityEngine;
using Patchwork.Data;

namespace Patchwork.Gameplay
{
    public class SparkCollectible : BaseDangerCollectible
    {
        protected override int GetDamage() => m_Level switch { 1 => 1, 2 => 2, _ => 4 };
        
        public override string DisplayName => "Spark";
        public override string Description => $"Costs {GetDamage()} life{(GetDamage() > 1 ? "s" : "")} if not cleaned up";

        protected override Sprite GetSprite() => GameResources.Instance.SparkSprite;
        protected override float GetScale() => GameResources.Instance.SparkScale;
    }
} 