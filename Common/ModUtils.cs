namespace Automatic.Common
{
    public static class ModUtils
    {
        public static bool TryFindChest(Point pos, out Chest chest, out int index) {
            var tile = Framing.GetTileSafely(pos);

            // 获取箱子可能的左上角位置
            int left = pos.X;
            int top = pos.Y;
            if (tile.TileFrameX % 36 != 0) {
                left--;
            }

            if (tile.TileFrameY != 0) {
                top--;
            }

            index = Chest.FindChest(left, top);
            if (index == -1) {
                chest = new Chest();
                return false;
            }
            chest = Main.chest[index];
            return true;
        }

        public static bool IsPlayerInChest(int i, out int playerIndex) {
            playerIndex = -1;
            for (int j = 0; j < 255; j++) {
                if (Main.player[j].chest == i) {
                    playerIndex = j;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a Position ID based on the x,y position.
        public static int CoordsToPos(Point p) => p.X * Main.maxTilesY + p.Y;

        /// <summary>
        /// Gets the coords based on the Position ID.
        public static Point PosToCoords(int p) => new Point(p / Main.maxTilesY, p % Main.maxTilesY);
    }
}
