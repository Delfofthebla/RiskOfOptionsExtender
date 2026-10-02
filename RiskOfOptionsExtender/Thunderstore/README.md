# Risk Of Options Extender

An add-on for [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) that puts every mod's settings in its in-game menu, and makes the mod list easier to get around.

## Every setting in the menu

- Mods with no Risk Of Options support get a page built from their config files.
- Mods that support Risk Of Options but only list some of their settings get the rest added to their page. Those added settings are marked "restart required", since the mod may only read them at startup.
- Settings in extra config files are included, such as mods that split their config across several files.
- Setting types Risk Of Options can't show directly (Double, Int64, KeyCode, Vector2 and others) are shown as the closest control it has: a number field, a key bind or a text box.
- Mods without an icon or description get the ones from their Thunderstore package.

Settings that affect gameplay usually only matter on the host's machine.

## Restart-required settings

Settings that only take effect after a restart show a restart icon before their name, and say so at the end of their description. Risk Of Options' own "Restart Required!" banner still appears when you change one.

Long setting names wrap onto more lines, and their row grows taller, instead of running under the setting's control.

## Mod list

- **Search:** type in the box above the list to filter mods by name.
- **Sort:** the button next to the search box switches between A-Z and Z-A.
- **Pin:** click the star on a mod's row to keep it at the top of the list.
- **Hide empty mods:** mods with no settings to show are hidden. Turn this off in this mod's own page.

## Tab picker

When a mod has more tabs than fit on one page, an "All Tabs" button appears at the start of the tab bar. It opens a list of every tab. If the mod's settings come from several config files, the list is grouped by file. The page arrows move together to the end of the bar.

## Settings

| Setting | Default | |
|-|-|-|
| Sort Order | Alphabetical | Order of the mod list. |
| Hide Empty Mods | On | Hide mods with no settings to show. |
| Never Fill In Missing Options | Off | Don't add missing settings for any mod. Takes effect after a restart. |
| Skip *mod name* | Off | Don't add missing settings for that mod. Listed in game only for mods that have settings missing from the menu. Saved to the config file only once ticked, as a GUID in `Skipped Mods`. Takes effect after a restart. |

## Compatibility

Replaces [OptionGenerator](https://thunderstore.io/package/6thmoon/OptionGenerator/). Remove or disable OptionGenerator; this mod won't load while it is installed.
