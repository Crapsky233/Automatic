using Terraria.ID;

namespace Automatic.Contents
{
    public class PipePlacerProjectile : ModProjectile
    {
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.WireKite);
            Projectile.hide = true;
        }

        public override void Kill(int timeLeft) {
            if (Projectile.localAI[0] == 1f && Projectile.owner == Main.myPlayer) {
                Player master = Main.player[Projectile.owner];
                Point ps = new Vector2(Projectile.ai[0], Projectile.ai[1]).ToPoint(); // 起始坐标
                Point pe = Projectile.Center.ToTileCoordinates(); // 终止坐标
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    //NetMessage.SendData(109, -1, -1, null, ps.X, ps.Y, pe.X, pe.Y, (int)WiresUI.Settings.ToolMode);
                    return;
                }
                int lengthX = Math.Abs(ps.X - pe.X);
                int lengthY = Math.Abs(ps.Y - pe.Y);
                int directionX = Math.Sign(pe.X - ps.X);
                int directionY = Math.Sign(pe.Y - ps.Y);

                if (lengthX == 0 && lengthY == 0) {
                    return;
                }

                //for (int i = 0; i <= lengthX; i++) {
                //    int x = ps.X + i * directionX;
                //    Point placePosition = new Point(x, ps.Y);
                //    var component = ComponentSystem.component.Find(c => c.Position == placePosition);
                //    if (component is not null && component.Type != ComponentID.Pipe) {
                //        return;
                //    }
                //}

                //for (int i = 0; i <= lengthY; i++) {
                //    int y = ps.Y + i * directionY;
                //    Point placePosition = new Point(pe.X, y);
                //    var component = ComponentSystem.component.Find(c => c.Position == placePosition);
                //    if (component is not null && component.Type != ComponentID.Pipe) {
                //        return;
                //    }
                //}

                for (int i = 0; i <= lengthX; i++) {
                    int x = ps.X + i * directionX;
                    Point placePosition = new Point(x, ps.Y);

                    var c = ComponentSystem.component.Find(c => c.Position == placePosition);
                    if (c is null) {
                        c = new Component() {
                            Position = placePosition,
                            Type = ComponentID.Pipe,
                        };
                        ComponentSystem.component.Add(c);
                    }

                    if (i != lengthX) {
                        switch (directionX) {
                            case 1:
                                c.TryConnect("right");
                                break;
                            case -1:
                                c.TryConnect("left");
                                break;
                        }
                    }
                    else {
                        switch (directionY) {
                            case 1:
                                c.TryConnect("down");
                                break;
                            case -1:
                                c.TryConnect("up");
                                break;
                        }
                    }
                }

                for (int i = 0; i <= lengthY; i++) {
                    int y = ps.Y + i * directionY;
                    Point placePosition = new Point(pe.X, y);

                    var c = ComponentSystem.component.Find(c => c.Position == placePosition);
                    if (c is null) {
                        c = new Component() {
                            Position = placePosition,
                            Type = ComponentID.Pipe,
                        };
                        ComponentSystem.component.Add(c);
                    }

                    if (i != lengthY) {
                        switch (directionY) {
                            case 1:
                                c.TryConnect("down");
                                break;
                            case -1:
                                c.TryConnect("up");
                                break;
                        }
                    }
                }
            }
        }
    }
}
