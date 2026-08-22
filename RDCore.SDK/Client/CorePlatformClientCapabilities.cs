namespace RDCore.SDK.Client;

public abstract record class CorePlatformClientCapability;

/// <summary>
/// Specifies the supported core platform capabilities of this assembly.
/// </summary>
/// <typeparam name="TCapability"></typeparam>
[AttributeUsage(AttributeTargets.Assembly)]
public class ProvidesCorePlatformClientCapabilityAttribute<TCapability> : Attribute
    where TCapability : CorePlatformClientCapability { }

/// <summary>
/// Enables the language server to request a parse result containing the full syntax tree of a specified workspace document.
/// </summary>
public record class ParseFullDocument : CorePlatformClientCapability;
