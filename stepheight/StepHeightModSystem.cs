using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace stepheight
{
    public class StepHeightModSystem : ModSystem
    {
        public override double ExecuteOrder() => 2.0d;

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Event.PlayerEntitySpawn += player =>
            {
                var behavior = player.Entity.GetBehavior<EntityBehaviorControlledPhysics>();
                if (behavior == null)
                {
                    api.Logger.Debug($"{player.PlayerUID} has no EntityBehaviorControlledPhysics");
                    return;
                }

                behavior.stepHeight = 1.2f;
            };
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Event.PlayerNowPlaying += (serverPlayer) =>
            {
                var behavior = serverPlayer.Entity.GetBehavior<EntityBehaviorControlledPhysics>();
                if (behavior == null)
                {
                    api.Logger.Debug($"{serverPlayer.PlayerUID} has no EntityBehaviorControlledPhysics");
                    return;
                }

                behavior.stepHeight = 1.2f;
            }; ;
        }
    }
}