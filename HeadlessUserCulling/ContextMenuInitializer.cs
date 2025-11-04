using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeContextMenu(User user, Slot CullingRoot, Slot UserCullingSlot)
    {
        Slot ThisUserRoot = user.Root.Slot;

        // Sets up context menu
        Slot RootMenuSlot = ThisUserRoot.AddSlot("HeadlessCullingContextMenu", false);
        Slot DistanceMenuSlot = RootMenuSlot.AddSlot("CullingDistance", false);

        Slot DistVarSlot = CullingRoot.GetChildrenWithTag("DistanceVar").First();
        if (!DistVarSlot.IsDestroyed)
        {
            var DistVar = DistVarSlot.GetComponent<DynamicValueVariable<float>>();

            // Root Context Menu Button
            var RootItemSource = RootMenuSlot.AttachComponent<ContextMenuItemSource>();
            RootItemSource.Label.Value = "User Culling Settings";
            RootItemSource.Color.Value = colorX.Cyan;

            var RootItem = RootMenuSlot.AttachComponent<RootContextMenuItem>();
            RootItem.ExcludeOnTools.Value = true;
            RootItem.Item.Target = RootItemSource;

            var RootSubmenu = RootMenuSlot.AttachComponent<ContextMenuSubmenu>();
            RootSubmenu.ItemsRoot.Target = RootMenuSlot;

            // Culling Distance Button
            var DistanceItemSource = DistanceMenuSlot.AttachComponent<ContextMenuItemSource>();
            DistanceItemSource.Color.Value = colorX.Yellow;

            var StringDriver = DistanceMenuSlot.AttachComponent<MultiValueTextFormatDriver>();
            StringDriver.Sources.Add(DistVar.Value);
            StringDriver.Format.Value = "Culling Distance: {0}";
            StringDriver.Text.Target = DistanceItemSource.Label;

            // Sets up a list with values that will be put into the context menu
            // but inserts the default distance from the config in order
            List<float> DistanceList = new List<float>();
            DistanceList.Add(float.PositiveInfinity);
            DistanceList.Add(20F);
            DistanceList.Add(10F);
            DistanceList.Add(5F);
            DistanceList.Add(3F);
            DistanceList.Add(2F);

            if (!DistanceList.Contains(Config!.GetValue(DefaultDistance)))
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Config.GetValue(DefaultDistance) < DistanceList[i] &&
                        Config.GetValue(DefaultDistance) > DistanceList[i + 1])
                    {
                        DistanceList.Insert(i + 1, Config.GetValue(DefaultDistance));
                        break;
                    }
                    else DistanceList.Add(Config.GetValue(DefaultDistance));
                }
            }

            var ButtonCycle = DistanceMenuSlot.AttachComponent<ButtonValueCycle<float>>();
            ButtonCycle.TargetValue.Target = DistVar.Value;
            for (int i = 0; i < 7; i++)
            {
                ButtonCycle.Values.Add(DistanceList[i]);
            }
        }

        // Sets up the context menu to destroy itself when
        // the user's culling root is destroyed
        UserCullingSlot.Destroyed += d =>
        {
            if (!user.IsDestroyed && !ThisUserRoot.IsDestroyed && !RootMenuSlot.IsDestroyed) RootMenuSlot.Destroy();
        };

        // Regenerates the context menu if it's directly destroyed
        RootMenuSlot.Destroyed += d =>
        {
            if (!user.IsDestroyed && !CullingRoot.IsDestroyed && !UserCullingSlot.IsDestroyed && !ThisUserRoot.IsDestroyed)
            {
                InitializeContextMenu(user, CullingRoot, UserCullingSlot);
            }
        };
    }
}
