using UnityEngine;

namespace Patchwork.Gameplay
{
    /// <summary>
    /// Base class for danger collectibles. Defaults to negative power (-2).
    /// </summary>
    public abstract class BaseDangerCollectible : BaseCollectible
    {
        #region Public Properties
        public override int Power => -2;
        #endregion
    }
}

