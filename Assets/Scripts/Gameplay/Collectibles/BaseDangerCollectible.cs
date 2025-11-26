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

        #region Protected Methods
        /// <summary>
        /// Returns the amount of damage this danger collectible deals if not collected.
        /// </summary>
        protected abstract int GetDamage();
        #endregion

        #region Public Methods
        public override void OnLevelEnd()
        {
            if (!m_IsCollected)
            {
                GameManager.Instance.DecreaseLives(GetDamage());
            }
        }
        #endregion
    }
}

