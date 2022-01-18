using System;

namespace Code.Core.DataManager.GameObjects.Entities
{
    public enum GameObjectKey
    {
        SetCard,
        DestructionParticles,
        ActivateEffectParticles,

        #region Projectiles

        MagicalProjectile,
        BulletProjectile,
        FireProjectile,

        #endregion
    }
    
    public static class GameObjectKeyExtensions
    {
        public static string GetStringValue(this GameObjectKey value)
        {
            return Enum.GetName(typeof(GameObjectKey), value);
        }
    }
}