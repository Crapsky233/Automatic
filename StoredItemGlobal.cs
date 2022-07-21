namespace Automatic
{
    public class StoredItemGlobal : GlobalItem
    {
        public const int PassCooldown = 4;

        // 为了不使得物品传输互相影响，CD必须放在GlobalItem里面而不是由元件存储
        [CloneByReference]
        public int ItemPassCooldown;

        public override bool IsCloneable => true;
        public override bool InstancePerEntity => true;
    }
}
