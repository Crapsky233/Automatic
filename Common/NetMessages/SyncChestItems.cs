namespace Automatic.Common.NetMessages
{
    [Obsolete]
    public static class SyncChestItems
    {
        public static void Send(int toWho, int fromWho, short chestIndex) {
            ModPacket packet = Automatic.Instance.GetPacket();

            packet.Write(NetID.SyncChestItems); // ID
            packet.Write(chestIndex);
            var c = Main.chest[chestIndex];
            for (int i = 0; i < Chest.maxItems; i++) {
            }

            packet.Send(toWho, fromWho);
        }

        public static void Receive(BinaryReader reader, int fromWho) {

        }
    }
}
