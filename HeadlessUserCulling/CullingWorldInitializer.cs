using FrooxEngine;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeWorld(World world)
    {
        world.RunSynchronously(() =>
        {
            if (Config!.GetValue(Enable))
            {
                // Create and setup culling system root slot
                Slot CullingRoot = world.RootSlot.AddSlot("HeadlessCullingSystem", false);
                CullingRoot.ParentReference.DriveFromRef(CullingRoot.ParentReference, false, false, true);
                CullingRoot.OrderOffset = long.MinValue;
                Slot DynVarSlot = CullingRoot.AddSlot("DynVars", false);
                Slot UsersSlot = CullingRoot.AddSlot("Users", false);
                CullingRoot.Tag = "HeadlessCullingRoot";
                UsersSlot.Tag = "HeadlessCullingUsers";

                // Setup CullingDistance variable
                Slot DistanceVarSlot = DynVarSlot.AddSlot("CullingDistance", false);
                DistanceVarSlot.Tag = "DistanceVar";

                var DistanceDynVar = DistanceVarSlot.AttachComponent<DynamicValueVariable<float>>();
                DistanceDynVar.VariableName.Value = "World/CullingDistance";

                var DistanceOverride = DistanceVarSlot.AttachComponent<ValueUserOverride<float>>();
                DistanceOverride.Default.Value = Config!.GetValue(DefaultDistance);
                DistanceOverride.CreateOverrideOnWrite.Value = true;
                DistanceOverride.Target.Target = DistanceDynVar.Value;

                // Sets up this world to set up users when they join
                world.UserSpawn += InitializeUser;

                // Regenerates the world's culling slots when destroyed
                CullingRoot.Destroyed += d =>
                {
                    world.UserSpawn -= InitializeUser;
                    InitializeWorld(world);
                };
            }
        });
    }
}
