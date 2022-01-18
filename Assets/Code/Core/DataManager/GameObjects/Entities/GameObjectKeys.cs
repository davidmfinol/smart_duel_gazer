namespace Code.Core.DataManager.GameObjects.Entities
{
    // When adding new keys ensure they are added to EndOfDuelUseCase.cs!!
    public static class GameObjectKeys
    {
        public const string SetCardKey = "SetCard";
        public const string DestructionParticlesKey = "DestructionParticles";
        public const string ActivateEffectParticlesKey = "ActivateEffectParticles";

        // Projectile Keys
        public const string MagicalProjectileKey = "MagicalProjectile";
        public const string BulletProjectileKey = "BulletProjectile";
        public const string FireProjectileKey = "FireProjectile";
    }
}