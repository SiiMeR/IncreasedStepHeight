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
        public const string IncreasedStepHeightAttributeKey = "IncreasedStepHeightValue";
        
        public ICoreServerAPI ServerApi;
        public ICoreClientAPI ClientApi;
        public IClientNetworkChannel ClientNetworkChannel;
        public IServerNetworkChannel ServerNetworkChannel;

        public ServerConfig ServerConfig;
        
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

            ServerConfig = ModConfig.ReadConfig(api);
            
            api.Event.PlayerNowPlaying += (player) =>
            {
                var stepHeight = GetStepHeight(player);
                ServerNetworkChannel.SendPacket(new SetStepHeightMessage(stepHeight), player);
            };
        }

        private void OnToggleStepHeightMessage(IServerPlayer fromPlayer, ToggleStepHeightMessage packet)
        {
            var currentValue = fromPlayer.Entity.WatchedAttributes.GetFloat(IncreasedStepHeightAttributeKey, 0.6f);
            var nextValue = GetNextStepHeight(currentValue, ServerConfig.DoubleBlockStepAllowed);
            
            fromPlayer.Entity.WatchedAttributes.SetFloat(IncreasedStepHeightAttributeKey, nextValue);
            
            var behavior = GetControlledPhysicsBehavior(fromPlayer);
            if (behavior == null)
            {
                return;
            }
            
            behavior.stepHeight = nextValue;
            ServerNetworkChannel.SendPacket(new SetStepHeightMessage(nextValue), fromPlayer);
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
            return player.Entity.WatchedAttributes.GetFloat(IncreasedStepHeightAttributeKey, 0.6f);
        }

        private float GetNextStepHeight(float currentStepHeight, bool isDoubleBlockStepAllowed)
        {
            return currentStepHeight switch
            {
                0.6f => 1.2f,
                1.2f when isDoubleBlockStepAllowed => 2.2f,
                1.2f => 0.6f,
                2.2f => 0.6f,
                _ => 0.6f 
            };
        }
    }
}