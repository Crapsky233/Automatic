namespace Automatic.Common.NetMessages
{
    public static class NetComponent
    {
        public static void SendItem(int toWho, int fromWho, Point position) {
            ModPacket packet = Automatic.Instance.GetPacket();

            packet.Write(NetID.SyncComponent); // ID
            packet.Write((byte)1); // Sub ID
            packet.Write(ModUtils.CoordsToPos(position));
            var c = ComponentSystem.component.Find(c => c.Position == position);
            ItemIO.Send(c.StoredItem, packet, true);

            packet.Send(toWho, fromWho);
        }

        public static void ReceiveItem(BinaryReader reader, int fromWho) {
            var position = ModUtils.PosToCoords(reader.ReadInt32());
            var target = ComponentSystem.component.Find(t => t.Position == position);
            if (target is not null) {
                target.StoredItem = ItemIO.Receive(reader, true);
            }

            if (Main.netMode == NetmodeID.Server) {
                SendItem(-1, fromWho, position);
            }
        }

        public static void Send(int toWho, int fromWho, Point position) {
            ModPacket packet = Automatic.Instance.GetPacket();

            packet.Write(NetID.SyncComponent); // ID
            packet.Write((byte)0); // Sub ID

            packet.Write(ModUtils.CoordsToPos(position));

            var c = ComponentSystem.component.Find(c => c.Position == position);
            packet.Write(c.Type);
            packet.Write(c.ConnectedUp);
            packet.Write(c.ConnectedDown);
            packet.Write(c.ConnectedLeft);
            packet.Write(c.ConnectedRight);
            ItemIO.Send(c.StoredItem, packet, true);

            packet.Send(toWho, fromWho);
        }

        public static void Receive(BinaryReader reader, int fromWho) {
            if (ComponentSystem.component is null) {
                ComponentSystem.component = new List<Component>();
            }

            byte receiveType = reader.ReadByte();
            switch (receiveType) {
                case 1:
                    ReceiveItem(reader, fromWho);
                    return;
            }

            Component c = new() {
                Position = ModUtils.PosToCoords(reader.ReadInt32()),
                Type = reader.ReadByte(),
                ConnectedUp = reader.ReadBoolean(),
                ConnectedDown = reader.ReadBoolean(),
                ConnectedLeft = reader.ReadBoolean(),
                ConnectedRight = reader.ReadBoolean(),
                StoredItem = ItemIO.Receive(reader, true)
            };

            var target = ComponentSystem.component.Find(t => t.Position == c.Position);
            if (target is not null) {
                // 请勿直接将target赋值
                target.Position = c.Position;
                target.Type = c.Type;
                target.ConnectedUp = c.ConnectedUp;
                target.ConnectedDown = c.ConnectedDown;
                target.ConnectedLeft = c.ConnectedLeft;
                target.ConnectedRight = c.ConnectedRight;
                target.StoredItem = c.StoredItem;
            }
            else {
                ComponentSystem.component.Add(c);
            }

            if (Main.netMode == NetmodeID.Server) {
                Send(-1, fromWho, c.Position);
            }

            Debugger.ConsoleWriteLine("Received one in " + ComponentSystem.component[^1].Position.ToString());
        }
    }
}
