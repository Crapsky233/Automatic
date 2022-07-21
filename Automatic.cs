global using Automatic.Common;
global using Automatic.Common.ID;
global using Automatic.Common.NetMessages;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.IO;
global using System.Linq;
global using Terraria;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.ID;
global using Terraria.ModLoader;
global using Terraria.ModLoader.Config;
global using Terraria.ModLoader.IO;
global using Terraria.Utilities;

namespace Automatic
{
    public class Automatic : Mod
    {
        public static Configuration Config { get; set; }
        public static Automatic Instance { get; set; }

        public override void Load() {
            Instance = this;
        }

        public override void Unload() {
            Config = null;
            Instance = null;
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            NetHandler.HandlePacket(reader, whoAmI);
        }
    }
}