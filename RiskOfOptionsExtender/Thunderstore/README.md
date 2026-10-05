# Risk Of Options Extender

An add-on for [Risk Of Options](https://thunderstore.io/package/Rune580/Risk_Of_Options/) that puts every mod's settings
in its in-game menu, and makes the mod list easier to get around.

I play with a pretty extensive modlist, and a lot of those mods either don't show up in Risk Of Options at all or only list
some of their settings. I got tired of searching through r2modman's config list and editing settings manually. With as many
mods as I have, finding the one I wanted in the list was super annoying. Especially for things like OST Mods that have a
volume slider that you need to use instead of the default game's music slider. So I made this to help with all of that.

![The Mod Options menu with pinned mods, the All Tabs bar and a filled-in mod page](https://raw.githubusercontent.com/Delfofthebla/RiskOfOptionsExtender/master/docs/screenshots/FeatureOverview.png)

## What it does

### Every setting in the menu

Mods with no Risk Of Options support get a page built from their config files. Mods that do support it but only list
some of their settings get the rest added to their page. Those added settings are marked restart required, since the mod
might only read them at startup. Settings spread across extra config files are included too.

Risk Of Options can't show every setting type directly. Settings like Double, Int64, KeyCode and Vector2 show up as the
closest control it has: a number field, a key bind or a text box. Mods without an icon or description get the ones from
their Thunderstore package.

Files a mod keeps for its own use, like `.Backup.cfg` files or files marked "DO NOT MODIFY", are left out.

### Restart-required settings and long names

Settings that only take effect after a restart show a restart icon before their name, and say so at the end of their
description. Risk Of Options' own "Restart Required!" banner still appears when you change one.

Long setting names wrap onto more lines instead of running under the setting's control, and checkboxes line up with the
other controls.

### The mod list

- **Search:** type in the box above the list to filter mods by name.
- **Sort:** the button next to the search box switches between A-Z and Z-A.
- **Pin:** click the star on a mod's row to keep it at the top of the list.
- **Hide empty mods:** mods with no settings to show are hidden.

![Searching the mod list for "risky"](https://raw.githubusercontent.com/Delfofthebla/RiskOfOptionsExtender/master/docs/screenshots/SearchShowcase.png)

### All Tabs picker

When a mod has more tabs than fit on one page, an **All Tabs** button appears at the start of the tab bar. It opens a
list of every tab, grouped by config file when the settings come from several.

![The All Tabs picker open on RiskyMod, grouped by config file](https://raw.githubusercontent.com/Delfofthebla/RiskOfOptionsExtender/master/docs/screenshots/AllTabNavigation.png)

## Don't want some of it?

Everything is a setting on this mod's own page, or in its config file:

- _Visual Tweaks → Mod List Tweaks_ and _Option Page Tweaks_ turn off the mod list and option page changes. Both take
  effect right away, and a switched-off feature doesn't hook into Risk Of Options at all.
- _Mod List → Hide Empty Mods_ and _Sort Order_.
- _Fill In Missing Options → Never Fill In Missing Options_ stops adding settings for every mod. _Skip (mod name)_
  will skip it for only the skipped mods, and is listed for each mod that has settings missing from the menu. Both
  take effect after a restart.

## Compatibility

**Supersedes [OptionGenerator](https://thunderstore.io/package/6thmoon/OptionGenerator/).** You shouldn't install both,
but they _can_ technically be installed together. When they are, OptionGenerator fills in the mods that don't use
Risk Of Options at all, and this mod will then only fill in the ones that do but leave some settings out.

I wouldn't recommend keeping both unless this mod starts breaking in the future and OptionGenerator doesn't. In that
case you can also turn this mod's autofill off entirely with "Never Fill In Missing Options".

If a game update or Risk Of Options change breaks part of this mod, that part _should_ turn itself off for the session and
log one error, while everything else keeps working. Please report the error from `LogOutput.log` on the GitHub if you see
one.
