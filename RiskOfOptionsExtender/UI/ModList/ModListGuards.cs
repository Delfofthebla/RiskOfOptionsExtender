using RiskOfOptionsExtender.Resilience;

namespace RiskOfOptionsExtender.UI.ModList;

internal sealed record ModListGuards(FeatureGuard Organizer, FeatureGuard Toolbar, FeatureGuard Pins);
