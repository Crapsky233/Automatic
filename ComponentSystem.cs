namespace Automatic
{
    public class ComponentSystem : ModSystem
    {
        public static List<Component> component;

        public override void OnWorldLoad() {
            component = new List<Component>();

            // MP进服通过发Request包来建立元件List
            if (Main.netMode is NetmodeID.MultiplayerClient) {
                Debugger.ConsoleWriteLine("Requested from " + Main.LocalPlayer.whoAmI);
                ModPacket packet = Automatic.Instance.GetPacket();
                packet.Write(NetID.RequestSyncAllComponents); // ID
                packet.Send(-1, -1);
            }
            Debugger.ConsoleWriteLine("Load world in: " + Main.netMode);
        }

        public override void SaveWorldData(TagCompound tag) {
            if (component.Count != 0) {
                tag["component"] = component.Select(info => new TagCompound {
                    ["type"] = info.Type,
                    ["connectedUp"] = info.ConnectedUp,
                    ["connectedDown"] = info.ConnectedDown,
                    ["connectedLeft"] = info.ConnectedLeft,
                    ["connectedRight"] = info.ConnectedRight,
                    ["pos"] = ModUtils.CoordsToPos(info.Position),
                    ["item"] = info.StoredItem,
                }).ToList();
            }
        }

        public override void LoadWorldData(TagCompound tag) {
            bool[,] hasComponent = new bool[Main.maxTilesX, Main.maxTilesY];
            List<Component> list = new();
            foreach (var entry in tag.GetList<TagCompound>("component")) {
                if (!entry.TryGet("type", out byte type) || type == ComponentID.None) {
                    continue;
                }
                if (!entry.TryGet("pos", out int pos)) {
                    continue;
                }
                Point position = ModUtils.PosToCoords(pos);
                if (hasComponent[position.X, position.Y]) {
                    continue;
                }
                hasComponent[position.X, position.Y] = true;

                list.Add(new Component() {
                    Type = type,
                    ConnectedUp = entry.GetBool("connectedUp"),
                    ConnectedDown = entry.GetBool("connectedDown"),
                    ConnectedLeft = entry.GetBool("connectedLeft"),
                    ConnectedRight = entry.GetBool("connectedRight"),
                    Position = position,
                    StoredItem = entry.Get<Item>("item")
                });
            }
            component = list;
        }

        public override void OnWorldUnload() {
            component = null;
        }

        public override void PostUpdateEverything() {
            if (Main.netMode == NetmodeID.Server)
                return;

            foreach (var c in component) {
                c.VisualUpdate();
            }
        }

        public override void PostUpdatePlayers() {
            List<int> extractors = new();
            List<int> depositors = new();

            for (int i = 0; i < component.Count; i++) {
                var c = component[i];

                switch (c.Type) {
                    case ComponentID.Extractor:
                        extractors.Add(i);
                        break;
                    case ComponentID.Depositor:
                        depositors.Add(i);
                        break;
                }
            }

            // 提取器更新
            foreach (int extractorIndex in extractors) {
                var c = component[extractorIndex];
                c.UpdateExtractors();
            }

            // 存入器更新
            foreach (int depositorIndex in depositors) {
                var c = component[depositorIndex];
                c.UpdateDepositors();
            }
        }

        public override void PostUpdateWorld() {
            List<int> shouldBeRemoved = new();
            List<int> pipes = new();
            List<int> itemStorages = new();

            for (int i = 0; i < component.Count; i++) {
                var c = component[i];
                if (c is null || c.Type == ComponentID.None) {
                    shouldBeRemoved.Add(i);
                    continue;
                }

                switch (c.Type) {
                    case ComponentID.Pipe:
                        pipes.Add(i);
                        break;
                    case ComponentID.ItemStorage:
                        itemStorages.Add(i);
                        break;
                }
            }

            // 删除错误component
            foreach (int remove in shouldBeRemoved) {
                component.RemoveAt(remove);
            }

            // 常规更新
            foreach (var c in component) {
                c.Update();
            }

            // 物品存储器更新
            foreach (int storageIndex in itemStorages) {
                var c = component[storageIndex];
                c.UpdateItemStorage();
            }

            // 物品通过更新
            foreach (var p in from c in component
                              where c.StoredItem is not null && !c.StoredItem.IsAir
                              && c.StoredItem.TryGetGlobalItem<StoredItemGlobal>(out var item) && item.ItemPassCooldown <= 0
                              select c) {
                p.UpdateItemPass();
            }
        }
    }
}
