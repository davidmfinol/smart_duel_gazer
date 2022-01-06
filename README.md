# Smart Duel Gazer

Smart Duel Gazer (SDG) is a Unity project for enhancing the experience of playing card games using augmented reality. It's currently only used to visualise the actions performed in the [Smart Duel Disk][Smart Duel Disk].

## How does it work?

Let's say you're playing a Yu-Gi-Oh! duel and you summon a Blue-Eyes White Dragon (BEWD) in the Smart Duel Disk (SDD). The SDD will then send an event to the [Smart Duel Server][Smart Duel Server] (SDS) to let it know that a BEWD has been summoned. The SDS will then send an event to the SDG, informing it that a BEWD has been summoned in a specific zone on the field. To keep it short: the user performs an action using the SDD and the SDG will visualise it using 3D models and/or VFX.

The SDD runs on an Android/iOS device and is something you wear on your arm, like a duel disk. The SDG runs on AR glasses / an AR headset / ... which is something you wear on your head.

## Do I need AR glasses to be able to use the SDG?

No, it's not necessary, but it is recommended. You can also install the app on an Android phone or iPhone, but that means you'll need to hold your phone to watch a duel. Wearing AR glasses is definitely a more fun experience.

## Which AR glasses / AR headsets are supported?

We're currently working on adding support for the [Nreal Light][Nreal Light] AR glasses. They're some of the first AR glasses to hit the market.
In 2022 we're also going to look into adding support for the [Oculus Quest 2][Oculus Quest 2].

A lot of people have asked us to look into Google Cardboard as well, but it was deprecated in 2021, so we're not going to do that.

## How to install the app?

iPhone users:
 - Download & install Apple's TestFlight (https://apps.apple.com/en/app/testflight/id899247664)
 - Download the Smart Duel Gazer via TestFlight: https://testflight.apple.com/join/3eAV5YYt

Android users: https://play.google.com/store/apps/details?id=com.crowncorp.duelgazer

## I'm a Unity developer, can I help?

Yes! If you're interested in helping out, send me a message on [our Discord server][Discord link].

## Getting started

1. Download Unity Version 2020.2.3f1 at [unity3d.com][Unity download]
2. Install the Android module if you want to make a build for Android / the Nreal Light
3. Install the iOS module if you want to make a build for iOS
4. Build and run the app

[Smart Duel Disk]: https://github.com/BramDC3/smart_duel_disk
[Smart Duel Server]: https://github.com/BramDC3/smart_duel_server
[Nreal Light]: https://www.nreal.ai/light
[Oculus Quest 2]: https://www.oculus.com/quest-2/
[Discord link]: https://discord.gg/XCcfcbBcjE
[Unity download]: https://unity3d.com/get-unity/download
