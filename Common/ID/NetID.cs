namespace Automatic.Common.ID
{
    public static class NetID
    {
        public const byte ComponentPassItem = 0;
        public const byte SyncComponent = 1;
        /// <summary>
        /// 申请同步元件，只应在MP里Send，在Server里Receive
        /// </summary>
        public const byte RequestSyncAllComponents = 2;
        /// <summary>
        /// 申请同步箱子内所有物品
        /// </summary>
        public const byte SyncChestItems = 3;
        /// <summary>
        /// 服务器端在箱子被玩家打开的情况下提取物品
        /// </summary>
        public const byte ServerExtracting = 4;
    }
}
