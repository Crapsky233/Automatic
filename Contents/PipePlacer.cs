namespace Automatic.Contents
{
    public class PipePlacer : ModItem
    {
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WireKite);
            Item.shoot = ModContent.ProjectileType<PipePlacerProjectile>();
        }
    }
}
