using RiskOfOptionsExtender.Resilience;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed class ModListGuards(FeatureGuard organizer, FeatureGuard toolbar, FeatureGuard pins)
{
    public FeatureGuard Organizer { get; } = organizer;

    public FeatureGuard Toolbar { get; } = toolbar;

    public FeatureGuard Pins { get; } = pins;
}
