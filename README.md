# Home Sweet Home

A RimWorld mod. Colonists notice when somebody leaves, how long they stay gone, and who walks back through the gate.

**RimWorld 1.6** · [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3300829226)

---

## What it does

### While they're away

Everyone left behind carries a thought for each person they're separated from. A partner or a child hurts most, then siblings and parents, then extended family, then close friends and friends. Somebody they can't stand leaving is a quiet relief instead.

The effect grows in four steps as the absence runs on — under three days, three to seven, seven to fifteen, and beyond that. It works in both directions: a colonist out on a caravan misses the people back home exactly the same way.

Nobody misses a stranger. Two colonists have to have actually lived in the colony together before any of this applies to them, easing in over the first few days and counting for nothing before that. A prisoner recruited the week after a caravan rolled out has never met the people on it, so they feel nothing until it comes back — however warmly the game's opinion score happens to rate somebody they've never spoken to. There's a slider for how long "long enough" is.

### Coming back

Walking through the gate after a long haul is the mood boost the mod is named for, scaled by how long the trip took. Everyone at the colony reacts too — warmly if they liked the traveller, considerably less so if they didn't.

Reaching whatever the caravan set out for is worth a smaller boost on its own.

### On the road

Travellers get homesick the longer they're out. Who they're travelling with matters just as much: a caravan of friends holds together, and a caravan carrying two people who hate each other does not. Finishing a trip together also shifts how those people see each other afterwards, up or down.

### Camping

Setting up camp is **not** coming home. The journey keeps running underneath it, homesickness keeps building (a little slower, since a fire and a roof help), and nobody gets a homecoming out of pitching a tent. Vanilla 1.6 camps and Set Up Camp are both handled.

There's a slider for how much a day in camp wears a traveller down compared to a day on the road.

---

## Traits

Around sixty trait profiles decide how hard all of this lands on a given colonist.

Psychopaths feel nothing when someone leaves and very little when they return. Recluses quietly enjoy an emptier colony. Depressives take an absence badly and iron-willed colonists shrug it off. Wanderlusters would rather not go home at all, and world-weary ones can't get back fast enough.

Covered out of the box:

- **Vanilla and all DLC** — 36 profiles across 27 traits, including the full Nerves, NaturalMood, Neurotic and SpeedOffset spectrums
- **Vanilla Traits Expanded** — 25 profiles
- **Combat Extended** — the Bravery spectrum

Profiles are plain XML defs (`Defs/TraitProfileDefs/`). Each one is a set of multipliers where `1` means "this trait has no opinion":

```xml
<HomeSweetHome.TraitProfileDef>
  <defName>HSH_Trait_Psychopath</defName>
  <trait>Psychopath</trait>
  <missing>0</missing>    <!-- someone they care about is away -->
  <relief>0</relief>      <!-- ...and comes back -->
  <gloating>0.4</gloating><!-- someone they can't stand is away -->
  <dread>0.4</dread>      <!-- ...and comes back -->
  <homesick>0.5</homesick><!-- being away themselves -->
  <company>0</company>    <!-- travelling with people they like -->
  <friction>0.3</friction><!-- travelling with people they don't -->
</HomeSweetHome.TraitProfileDef>
```

Traits are matched **by name at load time**, so a profile for a mod that isn't installed is dropped quietly instead of throwing errors. Adding support for another trait mod means one more file in that folder and nothing else — no patch operations, no load order requirements.

---

## Compatibility

The mod works out where colonists are rather than hooking caravan events. That means caravans, vehicles, shuttles, transport pods, outposts, quest maps and modded travel all work without special cases written for any of them.

- **No Harmony patches at all.** Nothing to conflict with.
- Safe to add to a running save. Colonists already in the colony are dated from their "time as colonist" record, so nobody is treated as a stranger on the sweep after you install it.
- Safe to remove — thoughts expire on their own within a few days.
- Tested against RimWorld 1.6.4871 with Core, Royalty, Ideology, Biotech, Anomaly and Odyssey.

Known to play nicely with Vanilla Expanded Framework, Vanilla Vehicles Expanded, Vehicle Framework, Set Up Camp, Yayo's Caravan and Vanilla Outposts Expanded.

---

## Settings

Everything is toggleable from the mod settings menu:

| Setting | Default |
|---|---|
| Miss people who are away | on |
| React when people come home | on |
| Enjoy the absence of people they dislike | on |
| Homesickness | on |
| Homecoming mood boost | on |
| Satisfaction on arrival | on |
| Travelling companions matter | on |
| Journeys change opinions | on |
| Camping still counts as being away | on |
| Small comfort from making camp | on |
| Camp wear factor | 50% |
| Mood effect strength | 100% |
| Ignore absences shorter than | 1 day |
| Strangers need this long together | 4 days |

There's also a button to wipe every thought the mod has handed out, for when you want a clean slate mid-save.

---

## Building

```
dotnet build Source/HomeSweetHome -c Release
```

The output goes straight to `Assemblies/`. References come from the `Krafs.Rimworld.Ref` NuGet package, so no local RimWorld install is needed to compile.

---

## Credits

Original mod by [IceFlopper](https://github.com/IceFlopper). Rebuilt for 1.6 by [Funstab](https://github.com/Funstab).
