# Risk Of Options Extender

Source for the [Risk Of Options Extender](RiskOfOptionsExtender/Thunderstore/README.md) Risk of Rain 2 mod.

## Building

Requires the .NET SDK (6 or later) and an r2modman profile with [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) installed. Game and BepInEx references come from NuGet (`NuGet.config` adds the BepInEx feed). Risk Of Options is referenced from the profile, publicized so its internal members are reachable.

```
dotnet build RiskOfOptionsExtender.sln
```

The solution has two projects: the plugin, and `RiskOfOptionsExtender.Patcher`, a BepInEx preloader patcher that watches for config files being created. Building the plugin builds the patcher too.

Every build copies both into the r2modman profile (`plugins/` and `patchers/`). A Release build (`-c Release`) also writes a Thunderstore package to `dist/`, with the patcher in its `patchers/` folder.

The default profile is `Main`. To use another, create `Directory.Build.props.user` next to the solution:

```xml
<Project>
  <PropertyGroup>
    <ProfileName>Default</ProfileName>
  </PropertyGroup>
</Project>
```
