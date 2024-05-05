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
        public const string IncreasedStepHeightAttributeKey = "IncreasedStepHeightEnabled";
        
        public ICoreServerAPI ServerApi;
        public ICoreClientAPI ClientApi;
        public IClientNetworkChannel ClientNetworkChannel;
        public IServerNetworkChannel ServerNetworkChannel;
        
        public override double ExecuteOrder() => 2;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            ClientApi = api;

            ClientNetworkChannel = api.Network.RegisterChannel("increasedstepheight")
                .RegisterMessageType<ToggleStepHeightMessage>()
                .RegisterMessageType<SetStepHeightMessage>()
                .SetMessageHandler<SetStepHeightMessage>(OnSetStepHeightMessage);

            api.Input.RegisterHotKey("stepheight", "Toggle increased step height", GlKeys.I,
                HotkeyType.CharacterControls, true);
            api.Input.SetHotKeyHandler("stepheight", _ => ToggleStepHeight());
        }

        private void OnSetStepHeightMessage(SetStepHeightMessage packet)
        {
            var behavior = GetControlledPhysicsBehavior(ClientApi.World.Player);
            if (behavior == null)
            {
                return;
            }
            
            behavior.stepHeight = packet.StepHeight;
            ClientApi.ShowChatMessage($"Set step height to {packet.StepHeight}");
        }

        private bool ToggleStepHeight()
        {
            ClientNetworkChannel.SendPacket(new ToggleStepHeightMessage());
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
                var stepHeight = GetStepHeight(player);
                ServerNetworkChannel.SendPacket(new SetStepHeightMessage(stepHeight), player);
            };
        }

        private void OnToggleStepHeightMessage(IServerPlayer fromPlayer, ToggleStepHeightMessage packet)
        {
            var currentValue = fromPlayer.Entity.WatchedAttributes.GetBool(IncreasedStepHeightAttributeKey);
            fromPlayer.Entity.WatchedAttributes.SetBool(IncreasedStepHeightAttributeKey, !currentValue);
                
            var behavior = GetControlledPhysicsBehavior(fromPlayer);
            if (behavior == null)
            {
                return;
            }
            
            var stepHeight = GetStepHeight(fromPlayer);

            behavior.stepHeight = stepHeight;
            ServerNetworkChannel.SendPacket(new SetStepHeightMessage(stepHeight), fromPlayer);
        }

        private EntityBehaviorControlledPhysics GetControlledPhysicsBehavior(IPlayer player)
        {
            var behavior = player.Entity.GetBehavior<EntityBehaviorControlledPhysics>();
            if (behavior == null)
            {
                ServerApi.Logger.Debug($"{player.PlayerUID} has no EntityBehaviorControlledPhysics");
                return null;
            }

            return behavior;
        }

        private float GetStepHeight(IPlayer player)
        {
            var isEnabled = player.Entity.WatchedAttributes.GetBool(IncreasedStepHeightAttributeKey);
            return isEnabled ? 0.6f : 1.2f;
        }
    }
}