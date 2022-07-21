namespace Automatic
{
    public class ResourceManager : ModSystem
    {
        internal static Asset<Texture2D> Pipe;
        internal static Asset<Texture2D> PipeTransparent;
        internal static Asset<Texture2D> PipeTotallyTransparent;
        internal static Asset<Texture2D> Extractor;
        internal static Asset<Texture2D> Depositor;
        internal static Asset<Texture2D> ItemStorage;

        public override void PostSetupContent() {
            if (!Main.dedServ) {
                Pipe = ModContent.Request<Texture2D>("Automatic/Assets/Pipe");
                PipeTransparent = ModContent.Request<Texture2D>("Automatic/Assets/Pipe_Transparent");
                PipeTotallyTransparent = ModContent.Request<Texture2D>("Automatic/Assets/Pipe_TotallyTransparent");
            }
        }
    }
}
