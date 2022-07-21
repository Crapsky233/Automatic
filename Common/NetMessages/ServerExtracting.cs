namespace Automatic.Common.NetMessages
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServerExtracting
    {
        /// <summary>
        /// 发送向某个客户端同步的请求，只应在Server端执行
        /// </summary>
        /// <param name="toWho">目标客户端</param>
        /// <param name="chestIndex">箱子在<see cref="Main.chest">里的索引</param>
        /// <param name="componentPosition">元件的位置ID</param>
        public static void SendRequest(int toWho, short chestIndex, int componentPosition) {
            ModPacket packet = Automatic.Instance.GetPacket();
            packet.Write(NetID.ServerExtracting); // ID
            packet.Write(chestIndex);
            packet.Write(componentPosition);

            packet.Send(toWho, -1);
        }

        public static void Receive(BinaryReader reader, int fromWho) {
            short chestIndex = reader.ReadInt16();
            int componentPosition = reader.ReadInt32();
            var position = ModUtils.PosToCoords(componentPosition);

            var component = ComponentSystem.component.Find(c => c.Position == position);
            if (component is not null) {
                NetComponent.SendItem(-1, -1, position);
            }
        }
    }
}
