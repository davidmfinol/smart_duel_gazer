namespace Code.Core.DataManager.GameObjects.Entities
{
    public static class GameObjectKeys
    {
        public const string SetCardKey = "SetCard";
        public const string DestructionParticlesKey = "DestructionParticles";
        public const string ActivateEffectParticlesKey = "ActivateEffectParticles";

        // Projectile Keys
        // When Adding new keys ensure that they've been added to the Projectiles Enum below and spelled the same!!
        // The Enum is used to set the projectile from the Inspector
        public const string MagicalProjectileKey = "MagicalProjectile";
        public const string BulletProjectileKey = "BulletProjectile";
        public const string FireProjectileKey = "FireProjectile";
    }

    public enum ProjectileKeys
    {
        MagicalProjectile,
        BulletProjectile,
        FireProjectile
    }
}