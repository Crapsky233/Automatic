using Terraria.UI;

namespace Automatic
{
    public class ComponentsDrawing : ModSystem
    {
        public override void PostDrawTiles() {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            int extraRangeX = 12;
            int extraRangeY = 10;
            Point screenOverdrawOffset = Main.GetScreenOverdrawOffset();
            Rectangle drawRange = new(((int)Main.screenPosition.X >> 4) - extraRangeX + screenOverdrawOffset.X,
                ((int)Main.screenPosition.Y >> 4) - extraRangeY + screenOverdrawOffset.Y,
                (Main.screenWidth >> 4) + (extraRangeX - screenOverdrawOffset.X << 1),
                (Main.screenHeight >> 4) + (extraRangeY - screenOverdrawOffset.Y << 1));

            foreach (var component in from c in ComponentSystem.component
                                      where c.Type is not ComponentID.None && c.StoredItem is not null &&
                                      !c.StoredItem.IsAir && drawRange.Contains(c.Position)
                                      select c) {
                Vector2 worldPosition = Utils.ToWorldCoordinates(component.Position, 0, 0);
                Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
                Vector2 screenPosition = worldPosition - unscaledPosition;

                // 物品丝滑移动的Vector2
                Item item = component.StoredItem;

                if (Automatic.Config.PipeAnimation) {
                    Vector2 positionOffset = Vector2.Zero;
                    foreach (var animation in component.Animations) {
                        // 物品丝滑移动的Vector2，用Animation来独立绘制动画
                        // 入点决定方向
                        positionOffset = (component.Position - animation.InPoint).ToVector2();
                        positionOffset *= MathHelper.Lerp(0f, -16f, (float)animation.Timer / (float)StoredItemGlobal.PassCooldown);
                        DrawItem(animation.Item, screenPosition, positionOffset, 8, 16);
                    }
                }

                if (!Automatic.Config.PipeAnimation || item.stack > 1 || component.Animations.Count == 0) {
                    DrawItem(item, screenPosition, Vector2.Zero, 8, 16);
                }
            }

            foreach (var component in from c in ComponentSystem.component where drawRange.Contains(c.Position) select c) {
                Vector2 worldPosition = Utils.ToWorldCoordinates(component.Position, 0, 0);
                Vector2 unscaledPosition = Main.Camera.UnscaledPosition;
                Vector2 screenPosition = worldPosition - unscaledPosition;

                switch (component.Type) {
                    case ComponentID.Pipe:
                        DrawPipe(component, screenPosition);
                        break;
                    case ComponentID.Extractor:
                        Main.spriteBatch.Draw(TextureAssets.WireNew.Value, screenPosition, new Rectangle(0, 16, 16, 16), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        break;
                    case ComponentID.Depositor:
                        Main.spriteBatch.Draw(TextureAssets.WireNew.Value, screenPosition, new Rectangle(54, 0, 16, 16), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        break;
                    case ComponentID.ItemStorage:
                        Main.spriteBatch.Draw(TextureAssets.WireNew.Value, screenPosition, new Rectangle(54, 16, 16, 16), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                        break;
                }


                //if (component.ConnectedUp) {
                //    Utils.DrawBorderString(Main.spriteBatch, "⬆️", screenPosition, Color.White);
                //}
                //if (component.ConnectedDown) {
                //    Utils.DrawBorderString(Main.spriteBatch, "⬇️", screenPosition, Color.White);
                //}
                //if (component.ConnectedLeft) {
                //    Utils.DrawBorderString(Main.spriteBatch, "⬅️", screenPosition, Color.White);
                //}
                //if (component.ConnectedRight) {
                //    Utils.DrawBorderString(Main.spriteBatch, "➡️", screenPosition, Color.White);
                //}
            }

            Main.spriteBatch.End();
        }

        public static void DrawItem(Item item, Vector2 screenPosition, Vector2 positionOffset, float availableWidth, float slotSize) {
            if (!TextureAssets.Item.IndexInRange(item.type)) {
                return;
            }

            int type = item.type;
            Main.instance.LoadItem(item.type);
            Texture2D itemTexture = TextureAssets.Item[type].Value;
            Rectangle frame;

            if (Main.itemAnimations[type] != null) {
                frame = Main.itemAnimations[type].GetFrame(itemTexture);
            }
            else {
                frame = itemTexture.Frame(1, 1, 0, 0);
            }

            Color newColor = Color.White;
            float pulseScale = 1f;
            ItemSlot.GetItemLight(ref newColor, ref pulseScale, item, false);
            int height = frame.Height;
            int width = frame.Width;
            float drawScale = 1f;

            if (width > availableWidth || height > availableWidth) {
                if (width > height) {
                    drawScale = availableWidth / width;
                }
                else {
                    drawScale = availableWidth / height;
                }
            }

            Vector2 size = new(slotSize);
            Vector2 origin = frame.Size() * (pulseScale / 2f - 0.5f);

            Vector2 drawPosition = screenPosition + size / 2f - frame.Size() * drawScale / 2f + positionOffset;
            drawPosition = drawPosition.Floor();
            if (ItemLoader.PreDrawInInventory(item, Main.spriteBatch, drawPosition, frame, item.GetAlpha(newColor),
                item.GetColor(Color.White), origin, drawScale * pulseScale)) {
                Main.spriteBatch.Draw(itemTexture, drawPosition, frame, item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);

                if (item.color != Color.Transparent) {
                    Main.spriteBatch.Draw(itemTexture, drawPosition, frame, item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
                }
            }

            ItemLoader.PostDrawInInventory(item, Main.spriteBatch, drawPosition, frame, item.GetAlpha(newColor), item.GetColor(Color.White), origin, drawScale * pulseScale);

            if (ItemID.Sets.TrapSigned[type]) {
                Vector2 wirePosition = screenPosition + new Vector2(12, 12) + positionOffset;
                Main.spriteBatch.Draw(TextureAssets.Wire.Value, wirePosition, new Rectangle?(new Rectangle(4, 58, 8, 8)), Color.White, 0f, new Vector2(4f), 0.5f, SpriteEffects.None, 0f);
            }
        }

        public static void DrawPipe(Component pipe, Vector2 screenPosition) {

            int nodeCount = 0;
            bool nodeUp = false;
            bool nodeDown = false;
            bool nodeLeft = false;
            bool nodeRight = false;

            // 绘制出点
            if (pipe.ConnectedUp) {
                QuickDrawPipe(screenPosition, GetPipeFrame(0, 6));
                nodeCount++;
                nodeUp = true;
            }
            if (pipe.ConnectedDown) {
                QuickDrawPipe(screenPosition, GetPipeFrame(1, 6));
                nodeCount++;
                nodeDown = true;
            }
            if (pipe.ConnectedLeft) {
                QuickDrawPipe(screenPosition, GetPipeFrame(2, 6));
                nodeCount++;
                nodeLeft = true;
            }
            if (pipe.ConnectedRight) {
                QuickDrawPipe(screenPosition, GetPipeFrame(3, 6));
                nodeCount++;
                nodeRight = true;
            }

            // 把所有与这个相连的component选出来
            var componentUp = ComponentSystem.component.Find(c => c.X == pipe.X && c.Y == pipe.Y - 1);
            var componentDown = ComponentSystem.component.Find(c => c.X == pipe.X && c.Y == pipe.Y + 1);
            var componentLeft = ComponentSystem.component.Find(c => c.X == pipe.X - 1 && c.Y == pipe.Y);
            var componentRight = ComponentSystem.component.Find(c => c.X == pipe.X + 1 && c.Y == pipe.Y);
            // 绘制出点
            if (componentUp is not null && componentUp.Type != ComponentID.None && componentUp.ConnectedDown) {
                QuickDrawPipe(screenPosition, GetPipeFrame(0, 5));
                nodeCount++;
                nodeUp = true;
            }
            if (componentDown is not null && componentDown.Type != ComponentID.None && componentDown.ConnectedUp) {
                QuickDrawPipe(screenPosition, GetPipeFrame(1, 5));
                nodeCount++;
                nodeDown = true;
            }
            if (componentLeft is not null && componentLeft.Type != ComponentID.None && componentLeft.ConnectedRight) {
                QuickDrawPipe(screenPosition, GetPipeFrame(2, 5));
                nodeCount++;
                nodeLeft = true;
            }
            if (componentRight is not null && componentRight.Type != ComponentID.None && componentRight.ConnectedLeft) {
                QuickDrawPipe(screenPosition, GetPipeFrame(3, 5));
                nodeCount++;
                nodeRight = true;
            }

            // 决定转接口 - 穷举
            var connectorRectangle = GetPipeFrame(1, 0);
            switch (nodeCount) {
                case 0:
                    connectorRectangle = GetPipeFrame(0, 0);
                    break;
                case 1:
                    if (nodeUp) connectorRectangle = GetPipeFrame(0, 1);
                    if (nodeDown) connectorRectangle = GetPipeFrame(1, 1);
                    if (nodeLeft) connectorRectangle = GetPipeFrame(2, 1);
                    if (nodeRight) connectorRectangle = GetPipeFrame(3, 1);
                    break;
                case 2:
                    // 直的
                    if (nodeUp && nodeDown) {
                        connectorRectangle = GetPipeFrame(2, 0); // 上下无方向性（两边都连向自己）
                        if (pipe.ConnectedUp) connectorRectangle = GetPipeFrame(0, 2); // 从下到上
                        if (pipe.ConnectedDown) connectorRectangle = GetPipeFrame(1, 2); // 从上到下
                    }
                    if (nodeLeft && nodeRight) {
                        connectorRectangle = GetPipeFrame(3, 0); // 左右无方向性（两边都连向自己）
                        if (pipe.ConnectedLeft) connectorRectangle = GetPipeFrame(2, 2); // 从右到左
                        if (pipe.ConnectedRight) connectorRectangle = GetPipeFrame(3, 2); // 从左到右
                    }
                    // 弯的
                    if (nodeUp && nodeLeft) connectorRectangle = GetPipeFrame(0, 3); // 左 - 上
                    if (nodeUp && nodeRight) connectorRectangle = GetPipeFrame(1, 3); // 右 - 上
                    if (nodeDown && nodeLeft) connectorRectangle = GetPipeFrame(2, 3); // 左 - 下
                    if (nodeDown && nodeRight) connectorRectangle = GetPipeFrame(3, 3); // 右 - 下
                    break;
                case 3: {
                        if (!nodeUp) connectorRectangle = GetPipeFrame(0, 4); // 左 - 下 - 右
                        if (!nodeDown) connectorRectangle = GetPipeFrame(1, 4); // 左 - 上 - 右
                        if (!nodeLeft) connectorRectangle = GetPipeFrame(2, 4); // 上 - 右 - 下
                        if (!nodeRight) connectorRectangle = GetPipeFrame(3, 4); // 上 - 左 - 下
                        break;
                    }
            }
            QuickDrawPipe(screenPosition, connectorRectangle);
        }

        private static void QuickDrawPipe(Vector2 position, Rectangle sourceRectangle) {
            var tex = Automatic.Config.PipeTransparent == PipeTransparent.Translucent ? ResourceManager.PipeTransparent.Value :
                Automatic.Config.PipeTransparent == PipeTransparent.Transparent ? ResourceManager.PipeTotallyTransparent.Value :
                ResourceManager.Pipe.Value;
            Main.spriteBatch.Draw(tex, position, sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        private static Rectangle GetPipeFrame(int frameX, int frameY) => ResourceManager.Pipe.Frame(4, 7, frameX, frameY);
    }
}
