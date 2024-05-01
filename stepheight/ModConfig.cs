using System.Collections.Generic;
using Vintagestory.API.Common;

namespace stepheight;

public static class ModConfig
{
    private const string ConfigName = "StepHeightConfig.json";

    public static ClientConfig ReadConfig(ICoreAPI api)
    {
        ClientConfig config;

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
    
    private static ClientConfig LoadConfig(ICoreAPI api)
    {
        return api.LoadModConfig<ClientConfig>(ConfigName);
    }

    private static void GenerateConfig(ICoreAPI api)
    {
        api.StoreModConfig(new ClientConfig(), ConfigName);
    }
}

public class ClientConfig
{
    public bool Enabled;

    public ClientConfig()
    {
    }

    public ClientConfig(ClientConfig previousConfig)
    {
        Enabled = previousConfig.Enabled;
    }
}