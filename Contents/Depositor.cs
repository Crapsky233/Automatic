namespace Automatic.Contents
{
    internal class Depositor : ModItem
    {
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.WoodenSword);
            Item.shoot = ProjectileID.PurificationPowder;
            Item.useAnimation = 2;
            Item.useTime = 30;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            Point tilePosition = Main.MouseWorld.ToTileCoordinates();
            var c = ComponentSystem.component.Find(c => c.Position == tilePosition);
            if (c is not null) {
                Main.NewText(c.ToString());
                ComponentSystem.component.Remove(c);
            }
            else {
                Main.NewText("Spawn Depositor");
                c = new Component() {
                    Position = tilePosition,
                    Type = ComponentID.Depositor
                };
                ComponentSystem.component.Add(c);
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
}
