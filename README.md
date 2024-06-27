# Idle Plus
Idle Plus is a mod for the game [Idle Clans](https://www.idleclans.com/), designed to enhance 
the experience with features like *Edit Offer* in the Player Market, *Market Value* on items, 
*Claim All* boosts, and more.

## Features
- **Claim all boosts**: Claim all four boosts in a single click.
- **Edit offer**: Edit your market offers without having to cancel, claim and remake them.
- **Lowest/Highest price on offer**: Easily see if your offer is the lowest or highest by just opening it up and looking at the price line, e.g. `Price: 122 (Lowest 121)`.
- **Compact numbers in market**: Write your offer price with compact numbers, e.g. `1k`, `8.52m`, `1kk`, `.5b`.
- **No "you need to open player marker before doing this"**: In the vanilla game, you need to open the player market before being able to create a sell offers from your inventory, this mod removes that limitation, allowing you to create a sell offer instantly after logging in.
- **Market value**: See the market value of items while hovering over them.
- **Item tweak**: Items that can't be sold to the game shop won't display a price when hovered over.
- **And much more planned!**

## Installation
1. Download BepInEx 6, you can find it [here](https://builds.bepinex.dev/projects/bepinex_be/692/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.692%2B851521c.zip). 
*(direct download, if you would rather download it yourself, then you can go [here](https://builds.bepinex.dev/projects/bepinex_be) 
and download it, make sure to get the x86 version for IL2CPP)*
2. Open the game folder (right-click the game in steam > Manage > Browse local files).
3. Optionally, make a backup of Idle Clans (create a copy of the `Idle Clans` folder and rename it to `Idle Clans - Vanilla`).
4. Extract the contents of the BepInEx zip file directly into the game folder (`Idle Clans`).
5. Start the game by either running `Idle Clans.exe` or via Steam. BepInEx 6 will install automatically; it should only take a few seconds.
6. Close the game and download the latest version of the mod from the [releases page](https://github.com/Uraxys/IdlePlus/releases).
7. Place the mod into the `plugins` folder (`Idle Clans/BepInEx/plugins`).
8. Optionally, turn off the BepInEx console by going into the config (`Idle Clans/BepInEx/config/BepInEx.cfg`) and setting `Enabled` under `[Logging.Console]` to `false`.
9. Done, you can now start the game and enjoy Idle Plus!

Updating is even simpler, just download the latest version of the mod and replace the old one in the `plugins` folder.

## Contributing
Want to contribute? Great! Before you can start you must get the DLL files generated by BepInEx 
and move them into the libs folder in the root of the project.

This can be done after installing BepInEx, go into the BepInEx folder and copy both `core` and `interop` 
into the `libs` folder, after that your project should have all the dependencies needed to build
successfully.

Feel free to open a pull request with your changes, I'll be happy to review them when I have the time.

## FAQ
**Q**: Can I be banned for using this mod?
<br>**A**: No, you shouldn't be, all the features are fine according to Temsei, and any new ones will be checked before being added.

**Q**: Can you add X feature?
<br>**A**: Maybe, if you have a feature request, feel free to open an issue [here](https://github.com/Uraxys/IdlePlus/issues) or send me a DM on discord at `uraxys`.

**Q**: Why isn't the mod / X working?
<br>**A**: Make sure you have the latest version of the mod installed, if you do, and it still isn't working, please open an issue [here](https://github.com/Uraxys/IdlePlus/issues).

**Q**: Can I use this mod as a base for my own mod?
<br>**A**: Yes, you can use this mod as a base for your own mod, just remember to follow the license :)

## License
MIT License

Copyright (c) 2024 Uraxys

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.