using System;
using stepheight.Network;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace stepheight
{
    public class StepHeightModSystem : ModSystem
    {
        public ICoreServerAPI ServerApi;
        public ICoreClientAPI ClientApi;
        public IClientNetworkChannel ClientNetworkChannel;
        public IServerNetworkChannel ServerNetworkChannel;
        public bool IsEnabled;
        
        public override double ExecuteOrder() => 2;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            ClientApi = api;
            var config = ModConfig.ReadConfig(api);
            IsEnabled = config.Enabled;

            ClientNetworkChannel = api.Network.RegisterChannel("increasedstepheight")
                .RegisterMessageType<ToggleStepHeightMessage>()
                .RegisterMessageType<SetStepHeightMessage>()
                .SetMessageHandler<SetStepHeightMessage>(OnSetStepHeightMessage);

            api.Input.RegisterHotKey("stepheight", "Toggle increased step height", GlKeys.I,
                HotkeyType.CharacterControls, true);
            api.Input.SetHotKeyHandler("stepheight", (_) => ToggleStepHeight(api));
        }

        private void OnSetStepHeightMessage(SetStepHeightMessage packet)
        {
            var behavior = ClientApi.World.Player.Entity.GetBehavior<EntityBehaviorControlledPhysics>();
            if (behavior == null)
            {
                ClientApi.Logger.Debug($"{ClientApi.World.Player.PlayerUID} has no EntityBehaviorControlledPhysics");
                return;
            }

            behavior.stepHeight = packet.StepHeight;

            if (packet.IsInitialSync)
            {
                ClientNetworkChannel.SendPacket(new ToggleStepHeightMessage(IsEnabled, true));
            }
        }

        private bool ToggleStepHeight(ICoreClientAPI api)
        {
            IsEnabled = !IsEnabled;
            ClientNetworkChannel.SendPacket(new ToggleStepHeightMessage(IsEnabled));
            ModConfig.WriteConfig(api, new ClientConfig{Enabled = IsEnabled});
            
            api.ShowChatMessage("Increased step height " + (IsEnabled ? "on": "off"));
            
            return true;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            ServerNetworkChannel = api.Network.RegisterChannel("increasedstepheight")
                .RegisterMessageType<ToggleStepHeightMessage>()
                .RegisterMessageType<SetStepHeightMessage>()
                .SetMessageHandler<ToggleStepHeightMessage>(OnToggleStepHeightMessage);

            api.Event.PlayerNowPlaying += (player) =>
            {
                var behavior = player.Entity.GetBehavior<EntityBehaviorControlledPhysics>();
                if (behavior == null)
                {
                    ServerApi.Logger.Debug($"{player.PlayerUID} has no EntityBehaviorControlledPhysics");
                    return;
                }

                ServerNetworkChannel.SendPacket(new SetStepHeightMessage(behavior.stepHeight, true), player);
            };
        }

        private void OnToggleStepHeightMessage(IServerPlayer fromplayer, ToggleStepHeightMessage packet)
        {
            var behavior = fromplayer.Entity.GetBehavior<EntityBehaviorControlledPhysics>();
            if (behavior == null)
            {
                ServerApi.Logger.Debug($"{fromplayer.PlayerUID} has no EntityBehaviorControlledPhysics");
                return;
            }

            var stepHeight = Math.Clamp(packet.IsEnabled 
                ? behavior.stepHeight + 0.6f 
                : behavior.stepHeight - 0.6f, 0.6f, 2.4f);
            
            behavior.stepHeight = stepHeight;


            if(!packet.IsInitialSync)
            {
                ServerNetworkChannel.SendPacket(new SetStepHeightMessage(stepHeight), fromplayer);
            }
        }
    }
}