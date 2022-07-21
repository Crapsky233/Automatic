namespace Automatic
{
    public class Component
    {
        /// <summary>
        /// 用于让物品在管道内丝滑显示的类
        /// </summary>
        public class ItemAnimation
        {
            public Item Item;
            public int Timer;
            public Point InPoint;

            public ItemAnimation(Item item, int timer, Point inPoint) {
                Item = item;
                Timer = timer;
                InPoint = inPoint;
            }
        }

        public int X;
        public int Y;
        public Point Position { get => new(X, Y); set { X = value.X; Y = value.Y; } }
        public bool ConnectedUp;
        public bool ConnectedDown;
        public bool ConnectedLeft;
        public bool ConnectedRight;
        public byte Type;
        public Item StoredItem = new();
        public List<ItemAnimation> Animations = new();

        public int ItemStorageWorkCooldown;

        public Component() {
            StoredItem.SetDefaults(ItemID.None);
        }

        /// <summary>
        /// 视效更新
        /// </summary>
        public void VisualUpdate() {
            for (int i = 0; i < Animations.Count; i++) {
                Animations[i].Timer--;
                if (Animations[i].Timer <= 0) {
                    Animations.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Update() {
            if (StoredItem is not null && StoredItem.TryGetGlobalItem<StoredItemGlobal>(out var globalItem)) {
                if (globalItem.ItemPassCooldown > 0) {
                    globalItem.ItemPassCooldown--;
                }
            }

            // 自动断连 - 对方没了，或者对方已经连着自己了
            var target = ComponentSystem.component.Find(c => c.X == X && c.Y == Y - 1);
            if (target is null || target.Type == ComponentID.None || target.ConnectedDown) {
                ConnectedUp = false;
            }

            target = ComponentSystem.component.Find(c => c.X == X && c.Y == Y + 1);
            if (target is null || target.Type == ComponentID.None || target.ConnectedUp) {
                ConnectedDown = false;
            }

            target = ComponentSystem.component.Find(c => c.X == X - 1 && c.Y == Y);
            if (target is null || target.Type == ComponentID.None || target.ConnectedRight) {
                ConnectedLeft = false;
            }

            target = ComponentSystem.component.Find(c => c.X == X + 1 && c.Y == Y);
            if (target is null || target.Type == ComponentID.None || target.ConnectedLeft) {
                ConnectedRight = false;
            }
        }

        /// <summary>
        /// 提取器 - 用于从容器中提取物品
        /// </summary>
        public void UpdateExtractors() {
            if (!ModUtils.TryFindChest(Position, out var chest, out int index)) {
                return;
            }

            if (Main.netMode != NetmodeID.SinglePlayer && ModUtils.IsPlayerInChest(index, out _)) {
                return;
            }

            for (int i = 0; i < Chest.maxItems; i++) {
                var item = chest.item[i];
                if (item is null || item.stack <= 0 || item.netID is ItemID.None) {
                    continue;
                }
                // 因为要给ItemPassCooldown-1所以分开判断，避免globalItem为null
                if (!item.TryGetGlobalItem<StoredItemGlobal>(out var globalItem)) {
                    continue;
                }
                if (globalItem.ItemPassCooldown > 0) {
                    globalItem.ItemPassCooldown--;
                    continue;
                }

                if (StoredItem is null || StoredItem.IsAir) {
                    StoredItem = item.Clone();
                    StoredItem.stack = 1;
                    StoredItem.GetGlobalItem<StoredItemGlobal>().ItemPassCooldown = StoredItemGlobal.PassCooldown;
                }
                // 已有同类物品且没到最大堆叠，注意这里不应该重置pass冷却
                else if (StoredItem.netID == item.netID && StoredItem.stack < StoredItem.maxStack && ItemLoader.CanStack(item, StoredItem)) {
                    StoredItem.stack++;
                }
                // 还不是，继续下一个
                else continue;

                if (--item.stack <= 0) {
                    item.TurnToAir();
                    if (Main.netMode == NetmodeID.Server) {
                        NetMessage.SendData(MessageID.SyncChestItem, number: index, number2: i);
                    }
                }
                else {
                    globalItem.ItemPassCooldown = StoredItemGlobal.PassCooldown;
                }

                if (Main.netMode == NetmodeID.Server) {
                    NetComponent.Send(-1, -1, Position);
                }

                return;
            }
        }

        /// <summary>
        /// 存入器 - 用于给容器存入物品
        /// </summary>
        public void UpdateDepositors() {
            if (!StoredItem.IsAir && ModUtils.TryFindChest(Position, out var chest, out int index)) {
                for (int i = 0; i < Chest.maxItems; i++) {
                    if (chest.item[i] is null || chest.item[i].IsAir) {
                        chest.item[i] = StoredItem.Clone();
                        chest.item[i].stack = 1;
                    }
                    // 已有同类物品且没到最大堆叠，注意这里不应该重置pass冷却
                    else if (StoredItem.netID == chest.item[i].netID && chest.item[i].stack < chest.item[i].maxStack) {
                        chest.item[i].stack++;
                    }
                    // 还不是，继续下一个
                    else continue;

                    if (--StoredItem.stack <= 0) {
                        StoredItem.TurnToAir();
                        if (Main.netMode == NetmodeID.Server) {
                            NetMessage.SendData(MessageID.SyncChestItem, number: index, number2: i);
                        }
                    }
                    else {
                        StoredItem.GetGlobalItem<StoredItemGlobal>().ItemPassCooldown = StoredItemGlobal.PassCooldown;
                    }

                    if (Main.netMode == NetmodeID.Server) {
                        NetComponent.Send(-1, -1, Position);
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// 物品吸取器 - 自动吸取物品
        /// </summary>
        public void UpdateItemStorage() {
            ItemStorageWorkCooldown--;
            if (ItemStorageWorkCooldown > 0) {
                return;
            }

            for (int i = 0; i < Main.maxItems; i++) {
                var item = Main.item[i];
                if (item is null || item.IsAir || Vector2.Distance(item.Bottom, Position.ToWorldCoordinates()) > 100) {
                    continue;
                }
                var worldPosition = Position.ToWorldCoordinates().ToPoint();
                var detectRange = new Rectangle(worldPosition.X - 24, worldPosition.Y - 24, 48, 48);
                if (item.Hitbox.Intersects(detectRange)) {
                    if (StoredItem.IsAir) {
                        StoredItem = item.Clone();
                        StoredItem.stack = 1;
                        StoredItem.GetGlobalItem<StoredItemGlobal>().ItemPassCooldown = StoredItemGlobal.PassCooldown;
                    }
                    // 已有同类物品且没到最大堆叠，注意这里不应该重置pass冷却
                    else if (StoredItem.netID == item.netID && StoredItem.stack < StoredItem.maxStack) {
                        StoredItem.stack++;
                    }
                    // 还不是，继续下一个
                    else continue;

                    if (--item.stack <= 0) {
                        item.TurnToAir();
                    }

                    ItemStorageWorkCooldown = 4;

                    if (Main.netMode == NetmodeID.Server) {
                        NetMessage.SendData(MessageID.SyncItem, number: i);
                    }
                }
            }
        }

        public void UpdateItemPass() {
            WeightedRandom<Point> random = new WeightedRandom<Point>();
            if (ConnectedUp && CheckCanPassItem(X, Y - 1)) {
                random.Add(new Point(X, Y - 1));
            }
            if (ConnectedDown && CheckCanPassItem(X, Y + 1)) {
                random.Add(new Point(X, Y + 1));
            }
            if (ConnectedLeft && CheckCanPassItem(X - 1, Y)) {
                random.Add(new Point(X - 1, Y));
            }
            if (ConnectedRight && CheckCanPassItem(X + 1, Y)) {
                random.Add(new Point(X + 1, Y));
            }

            if (random.elements.Count == 0) {
                return;
            }

            var targetPosition = random.Get();
            var target = ComponentSystem.component.Find(c => c.Position == targetPosition);

            if (Main.netMode != NetmodeID.Server && Automatic.Config.PipeAnimation) {
                target.Animations.Add(new ItemAnimation(StoredItem.Clone(), StoredItemGlobal.PassCooldown, Position));
            }

            PassItem(ref StoredItem, ref target.StoredItem);
            if (Main.netMode == NetmodeID.Server) {
                ComponentPassItem.Send(-1, -1, Position, targetPosition, target.StoredItem.Clone(), StoredItem.stack, target.StoredItem.stack);
            }
        }

        public static void PassItem(ref Item itemFrom, ref Item itemTo) {
            if (itemTo.IsAir) {
                itemTo = itemFrom.Clone();
                itemTo.stack = 1;
                itemTo.GetGlobalItem<StoredItemGlobal>().ItemPassCooldown = StoredItemGlobal.PassCooldown;
            }
            // 已有同类物品且没到最大堆叠，注意这里不应该重置pass冷却
            else {
                itemTo.stack++;
            }

            if (--itemFrom.stack <= 0) {
                itemFrom.TurnToAir();
            }
            else {
                itemFrom.GetGlobalItem<StoredItemGlobal>().ItemPassCooldown = StoredItemGlobal.PassCooldown;
            }
        }

        private bool CheckCanPassItem(int X, int Y) {
            var target = ComponentSystem.component.Find(c => c.X == X && c.Y == Y);
            if (target is not null && (target.StoredItem.IsAir || (target.StoredItem.netID == StoredItem.netID && target.StoredItem.stack < target.StoredItem.maxStack && ItemLoader.CanStack(StoredItem, target.StoredItem)))) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 不能与已经连接到自己的元件连接
        /// </summary>
        /// <param name="mode">up/down/left/right</param>
        /// <returns>是否成功连接</returns>
        public bool TryConnect(string mode) {
            switch (mode) {
                case "up":
                    var target = ComponentSystem.component.Find(c => c.X == X && c.Y == Y - 1);
                    if (target is not null && target.Type != ComponentID.None && target.ConnectedDown) {
                        return false;
                    }
                    ConnectedUp = true;
                    return true;
                case "down":
                    target = ComponentSystem.component.Find(c => c.X == X && c.Y == Y + 1);
                    if (target is not null && target.Type != ComponentID.None && target.ConnectedUp) {
                        return false;
                    }
                    ConnectedDown = true;
                    return true;
                case "left":
                    target = ComponentSystem.component.Find(c => c.X == X - 1 && c.Y == Y);
                    if (target is not null && target.Type != ComponentID.None && target.ConnectedRight) {
                        return false;
                    }
                    ConnectedLeft = true;
                    return true;
                case "right":
                    target = ComponentSystem.component.Find(c => c.X == X + 1 && c.Y == Y);
                    if (target is not null && target.Type != ComponentID.None && target.ConnectedLeft) {
                        return false;
                    }
                    ConnectedRight = true;
                    return true;
            }
            return false;
        }

        public Component Clone() => new() {
            Position = Position,
            Type = Type,
            ConnectedUp = ConnectedUp,
            ConnectedDown = ConnectedDown,
            ConnectedLeft = ConnectedLeft,
            ConnectedRight = ConnectedRight,
            ItemStorageWorkCooldown = ItemStorageWorkCooldown,
            StoredItem = StoredItem.Clone()
        };

        public override string ToString() => $"X:{X} | Y:{Y} | Type:{Type} | " +
            $"Connected:{(ConnectedUp ? "Up" : "")} {(ConnectedDown ? "Down" : "")} {(ConnectedLeft ? "Left" : "")}{(ConnectedRight ? "Right" : "")} | " +
            $"ItemType:{StoredItem.type}";
    }
}
