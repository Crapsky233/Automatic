namespace Automatic.Common
{
    public static class NetHandler
    {
        public static void HandlePacket(BinaryReader reader, int whoAmI) {
            byte msgType = reader.ReadByte();
            switch (msgType) {
                case NetID.ComponentPassItem:
                    ComponentPassItem.Receive(reader, whoAmI);
                    break;
                case NetID.SyncComponent:
                    NetComponent.Receive(reader, whoAmI);
                    break;
                case NetID.RequestSyncAllComponents:
                    foreach (var pos in from c in ComponentSystem.component where c.Type is not ComponentID.None select c.Position) {
                        NetComponent.Send(whoAmI, -1, pos);
                    }
                    break;
                case NetID.SyncChestItems:
                    ReceivePickupItemStack(reader, whoAmI);
                    break;
                case NetID.ServerExtracting:
                    ServerExtracting.Receive(reader, whoAmI);
                    break;
                default:
                    Automatic.Instance.Logger.WarnFormat("Automatic: Unknown Message type: {0}", msgType);
                    break;
            }
        }


        public static void ReceivePickupItemStack(BinaryReader reader, int fromWho) {
            int stack = reader.ReadInt32();
            if (Main.netMode == NetmodeID.Server) {
                byte slot = reader.ReadByte();
                short chestIndex = reader.ReadInt16();
                SendPickupItemStack(-1, fromWho, slot, chestIndex);
            }
            else if (Main.mouseItem.stack > stack) {
                Main.mouseItem.stack = stack;
            }
        }

        public static void SendPickupItemStack(int toWho, int fromWho, byte slot, short chestIndex) {
            if (Main.netMode == NetmodeID.Server) {
                ModPacket packet = Automatic.Instance.GetPacket();
                packet.Write(NetID.SyncChestItems); // ID
                packet.Write(Main.chest[chestIndex].item[slot].stack);
                packet.Send(toWho, fromWho);
            }
            else {
                ModPacket packet = Automatic.Instance.GetPacket();
                packet.Write(NetID.SyncChestItems); // ID
                packet.Write(-1);
                packet.Write(slot);
                packet.Write(chestIndex);
                packet.Send(toWho, fromWho);
            }
        }
    }
}
