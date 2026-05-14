# BetterMapHideFarEnemiesFixed

Fixed explore all rooms not working. Original mod from [BetterMapHideFarEnemies](https://thunderstore.io/c/repo/p/Barisbaba/BetterMapHideFarEnemies/).

## Description

- Show Player Locations on Map (Enabled on Default)
- Show Enemy Locations on Map (Enabled on Default)
- Hide Far Enemies from Map (Enabled on Default)
- Explore All Rooms (Disabled on Default)
- Explore Valuables (Disabled on Default)

![image](https://i.imgur.com/2zpJ8vS.png)
 
## Compatibility
- Single-player: ✅
- Multiplayer: ✅

## Installation
1. Download & install [BepInEx](https://github.com/BepInEx/BepInEx).
2. Download the [latest release](https://github.com/Nixeld/R.E.P.O.-BetterMapHideFarEnemiesFixed/releases) of this mod.
3. Extract `Nixeld-BetterMapHideFarEnemiesFixed.zip` into your `BepInEx/plugins` directory.

## Configuration

Settings can be configured in-game using the [REPOConfig mod](https://thunderstore.io/c/repo/p/nickklmao/REPOConfig/), or manually by:

1. Launch the game with mod installed at least once.
2. Edit the `Better.Map.Hide.Far.Enemies.Fixed.cfg` file within your `BepInEx/config` directory.

<details>
<summary>Example Configuration</summary>

```
## Settings file was created by plugin Better Map Hide Far Enemies Fixed v1.0.0
## Plugin GUID: Better.Map.Hide.Far.Enemies.Fixed

[Display Options]

## Display teammate on the map
# Setting type: Boolean
# Default value: true
ShowTeammates = true

## Display enemy on the map
# Setting type: Boolean
# Default value: true
ShowEnemies = true

## Hide enemy unless share room with any player
# Setting type: Boolean
# Default value: true
HideFarEnemies = false

[Gameplay Options]

## Automatically explore all rooms at the start of the game
# Setting type: Boolean
# Default value: false
ExploreAllRooms = true

## All valuables are visible on the map
# Setting type: Boolean
# Default value: false
ExploreValuables = true

```

</details>