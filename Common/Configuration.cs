namespace Automatic.Common
{
    [Label("$Mods.Automatic.Config.Label")]
    public class Configuration : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        public override void OnLoaded() => Automatic.Config = this;

        [Label("$Mods.Automatic.Config.PipeTransparent.Label")]
        [Tooltip("$Mods.Automatic.Config.PipeTransparent.Tooltip")]
        [DefaultValue(PipeTransparent.Opaque)]
        [DrawTicks]
        public PipeTransparent PipeTransparent;

        [Label("$Mods.Automatic.Config.PipeAnimation.Label")]
        [Tooltip("$Mods.Automatic.Config.PipeAnimation.Tooltip")]
        [DefaultValue(true)]
        public bool PipeAnimation;
    }

    public enum PipeTransparent
    {
        Opaque,
        Translucent,
        Transparent
    }
}
