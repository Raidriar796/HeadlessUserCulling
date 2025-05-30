using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    [AutoRegisterConfigKey] public static readonly ModConfigurationKey<bool> Enable =
            new ModConfigurationKey<bool>(
                "Enable",
                "Enable HeadlessAvatarCulling",
                () => true);

    [AutoRegisterConfigKey] public static readonly ModConfigurationKey<bool> AutoGenContextMenu =
            new ModConfigurationKey<bool>(
                "AutoGenContextMenu",
                "Automatically generate context menus for culling settings",
                () => true);
}
