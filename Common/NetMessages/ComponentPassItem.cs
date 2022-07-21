namespace Automatic.Common.NetMessages
{
    public static class ComponentPassItem
    {
        public static void Send(int toWho, int fromWho, Point fromPosition, Point toPosition, Item item, int fromStack, int toStack) {
            ModPacket packet = Automatic.Instance.GetPacket();

            packet.Write(NetID.ComponentPassItem); // ID

            packet.Write(ModUtils.CoordsToPos(fromPosition));
            packet.Write(ModUtils.CoordsToPos(toPosition));
            packet.Write(fromStack);
            packet.Write(toStack);
            ItemIO.Send(item, packet, true);

            packet.Send(toWho, fromWho);
        }

        public static void Receive(BinaryReader reader, int fromWho) {
            Point fromPosition = ModUtils.PosToCoords(reader.ReadInt32());
            Point toPosition = ModUtils.PosToCoords(reader.ReadInt32());
            int fromStack = reader.ReadInt32();
            int toStack = reader.ReadInt32();
            Item item = ItemIO.Receive(reader, true);

            var fromTarget = ComponentSystem.component.Find(c => c.Position == fromPosition);
            var toTarget = ComponentSystem.component.Find(c => c.Position == toPosition);

            if (fromTarget is null || toTarget is null) {
                Debugger.ConsoleWriteLine($"Pass failed! from: {fromPosition}, to: {toPosition}, item: {item.Clone()}");
                return;
            }

            if (Main.netMode != NetmodeID.Server && Automatic.Config.PipeAnimation) {
                toTarget.Animations.Add(new Component.ItemAnimation(item.Clone(), StoredItemGlobal.PassCooldown, fromPosition));
            }

            fromTarget.StoredItem = item.Clone();
            fromTarget.StoredItem.stack = fromStack;
            toTarget.StoredItem = item.Clone();
            toTarget.StoredItem.stack = toStack;

            if (Main.netMode == NetmodeID.Server) {
                Send(-1, fromWho, fromPosition, toPosition, fromTarget.StoredItem.Clone(), fromStack, toStack);
            }

            Debugger.ConsoleWriteLine($"from: {fromPosition}, to: {toPosition}, item: {item.Clone()}");
        }
    }
}
