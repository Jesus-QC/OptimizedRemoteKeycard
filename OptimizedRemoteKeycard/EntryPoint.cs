using System;
using Exiled.API.Features;
using HarmonyLib;

namespace OptimizedRemoteKeyCard;

public class EntryPoint : Plugin<PluginConfig>
{
    public const string OptimizedRemoteKeyCardVersion = "1.0.0.1";
    
    public override string Author { get; } = "Jesus-QC";
    public override string Name { get; } = "OptimizedRemoteKeycard";
    public override string Prefix { get; } = "optimized_remote_keycard";
    public override Version Version { get; } = new (OptimizedRemoteKeyCardVersion);

    private Harmony _harmony;
    
    public override void OnEnabled()
    {
        _harmony = new Harmony("com.jesusqc.remotekeycard");
        _harmony.PatchAll();
        
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        _harmony.UnpatchAll(_harmony.Id);
        _harmony = null;
        
        base.OnDisabled();
    }
}