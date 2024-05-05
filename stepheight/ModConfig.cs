using System.Collections.Generic;
using Vintagestory.API.Common;

namespace stepheight;

public static class ModConfig
{
    private const string ConfigName = "StepHeightServerConfig.json";

    public static ServerConfig ReadConfig(ICoreAPI api)
    {
        ServerConfig config;

        try
        {
            config = LoadConfig(api);

            if (config == null)
            {
                GenerateConfig(api);
                config = LoadConfig(api);
            }
        }
        catch
        {
            GenerateConfig(api);
            config = LoadConfig(api);
        }

        return config;
    }

    public static void WriteConfig<T>(ICoreAPI api, T config)
    {
        api.StoreModConfig<T>(config, ConfigName);
    }
    
    private static ServerConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<ServerConfig>(ConfigName);
    }

    private static void GenerateConfig(ICoreAPI api)
    {
        api.StoreModConfig(new ServerConfig(), ConfigName);
    }
}

public class ServerConfig
{
    public bool DoubleBlockStepAllowed;

    public ServerConfig()
    {
    }

    public ServerConfig(ServerConfig previousConfig)
    {
        DoubleBlockStepAllowed = previousConfig.DoubleBlockStepAllowed;
    }
}