using FrooxEngine;
using FrooxEngine.CommonAvatar;
using FrooxEngine.ProtoFlux;
using ResoniteModLoader;

namespace HeadlessUserCulling;

public partial class HeadlessUserCulling : ResoniteMod
{
    private static void InitializeUser(User user)
    {
        user.World.RunSynchronously(() => 
        {
            if (user != user.World.HostUser)
            {
                // Create and setup user specific culling slots
                Slot CullingRoot = user.World.RootSlot.GetChildrenWithTag("HeadlessCullingRoot").First();
                Slot UserCullingSlot = CullingRoot.AddSlot(user.UserID, false);
                UserCullingSlot.Tag = null;
                Slot DynVarSlot = UserCullingSlot.AddSlot("DynVars", false);
                Slot HelpersSlot = UserCullingSlot.AddSlot("Helpers", false);

                // Sets up the culling behavior via UserDistanceValueDriver and VirtualParent
                var PrimaryDistCheck = UserCullingSlot.AttachComponent<UserDistanceValueDriver<bool>>(true, null);
                PrimaryDistCheck.Node.Value = UserRoot.UserNode.View;
                PrimaryDistCheck.TargetField.Value = user.Root.Slot.ActiveSelf_Field.ReferenceID;
                PrimaryDistCheck.NearValue.Value = true;

                var SecondaryDistCheck = UserCullingSlot.AttachComponent<UserDistanceValueDriver<bool>>(true, null);
                SecondaryDistCheck.Node.Value = UserRoot.UserNode.View;
                SecondaryDistCheck.FarValue.Value = true;

                var PrimaryVirtualParent = UserCullingSlot.AttachComponent<VirtualParent>(true, null);
                PrimaryVirtualParent.OverrideParent.Value = user.Root.HeadSlot.ReferenceID;
                PrimaryVirtualParent.SetVirtualChild(UserCullingSlot, false);

                // Workaround for odd behavior on user initial focus
                // I intend to replace this eventually, but if I cannot
                // find a better method this will stay as is
                var HostOverride = UserCullingSlot.AttachComponent<ValueUserOverride<bool>>(true, null);
                HostOverride.Default.Value = false;
                HostOverride.CreateOverrideOnWrite.Value = true;
                HostOverride.Target.Value = UserCullingSlot.GetComponent<UserDistanceValueDriver<bool>>().FarValue.ReferenceID;
                PrimaryDistCheck.FarValue.Value = true;

                // Sets up dyn vars to be adjustable by the user
                Slot DistanceVarSlot = DynVarSlot.AddSlot("Distance", false);

                var PrimaryDistDriver = DistanceVarSlot.AttachComponent<DynamicValueVariableDriver<float>>(true, null);
                PrimaryDistDriver.VariableName.Value = "HeadlessAvatarCulling/CullingDistance";
                PrimaryDistDriver.Target.Value = PrimaryDistCheck.Distance.ReferenceID;
                SecondaryDistCheck.TargetField.Value = HelpersSlot.ActiveSelf_Field.ReferenceID;

                var SecondaryDistDriver = DistanceVarSlot.AttachComponent<DynamicValueVariableDriver<float>>(true, null);
                SecondaryDistDriver.VariableName.Value = "HeadlessAvatarCulling/CullingDistance";
                SecondaryDistDriver.Target.Value = SecondaryDistCheck.Distance.ReferenceID;

                // Recreates the Audio Output on the user
                // to keep audio working while a user is culled
                var UserVoice = user.Root.Slot.GetComponent<AvatarVoiceInfo>().AudioSource.Value;

                Slot AudioSlot = HelpersSlot.AddSlot("Audio", false);

                var AudioOutput = AudioSlot.AttachComponent<AudioOutput>(true, null);
                AudioOutput.Source.Value = UserVoice;
                AudioOutput.Priority.Value = 0;
                AudioOutput.AudioTypeGroup.Value = AudioTypeGroup.Voice;

                var AudioManager = AudioSlot.AttachComponent<AvatarAudioOutputManager>(true, null);
                AudioManager.AudioOutput.Value = AudioOutput.ReferenceID;
                AudioManager.OnEquip(user.Root.Slot.GetComponentInChildren<AvatarObjectSlot>());

                // This is needed because otherwise, the min scale will
                // be set to Infinity, making the audio output not work
                AudioOutput.MinScale.ActiveLink.ReleaseLink(true);
                AudioOutput.MinScale.Value = 1F;

                // Generates visuals for culled user's head and hands
                Slot VisualSlot = HelpersSlot.AddSlot("Visuals", false);

                var DefaultMaterial = user.World.GetSharedComponentOrCreate("DefaultMaterial", delegate(PBS_Metallic mat) {}, 0, false, false, null);

                Slot HeadVisualSlot = VisualSlot.AddSlot("HeadVisual", false);
                HeadVisualSlot.AttachSphere(0.15F, DefaultMaterial, false);

                Slot LeftHandVisualSlot = VisualSlot.AddSlot("LeftHandVisual", false);
                LeftHandVisualSlot.AttachSphere(0.1F, DefaultMaterial, false);
                var LeftHandVirtualParent = UserCullingSlot.AttachComponent<VirtualParent>(true, null);
                LeftHandVirtualParent.OverrideParent.Value = user.Root.GetHandSlot(Chirality.Left, true).ReferenceID;
                LeftHandVirtualParent.SetVirtualChild(LeftHandVisualSlot, false);

                Slot RightHandVisualSlot = VisualSlot.AddSlot("RIghtHandVisual", false);
                RightHandVisualSlot.AttachSphere(0.1F, DefaultMaterial, false);
                var RightHandVirtualParent = UserCullingSlot.AttachComponent<VirtualParent>(true, null);
                RightHandVirtualParent.OverrideParent.Value = user.Root.GetHandSlot(Chirality.Right, true).ReferenceID;
                RightHandVirtualParent.SetVirtualChild(RightHandVisualSlot, false);
            }
        }, false, null, false);
    }
}