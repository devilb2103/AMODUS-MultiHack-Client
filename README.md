# AMODUS-MultiHack-Client

An old Among-Us internal I wrote back in 2021 when I had nothing better to do than inject c# into my bloodstream.
This repo is more like a covid lockdown digital souvenir than something that i will constantly update for people to compile and use.

This WPF project uses confuserEx for code obfuscation and Costura.Fody for merging all c# assemblies into one file.

This project is also dependent 2 other (compiled) files to function as expected, from:
- https://github.com/devilb2103/AMODUS_INJECTOR
- https://github.com/devilb2103/AMODUS_internal_dll

make sure to whitelist the folder path or all the files used to prevent your antivirus from deleting project dependencies

## Features
  ### General
    - Change player speed
    - Change player name (synced across all clients)
    - Override emergency count
    - Override emergency cooldown
    - Satellite view (zoom out)
  
  ### Player
    - Force impostor (make self impostor)
    - Reveal impostor (red name highlight)
    - Force crewmate (make self crewmate)
    - Invisibility (move around after entering vents)
    - Override kill cooldown
    - Override kill distance (kill players from far and through walls)
    - Kill player (kill self)
    - Revive player (Revive self)
    - Override crew vision (removes vignette as a crew member)
    - Override impostor vision (removes vignette as an impostor)
  
  ### Appearance hacks
    - Rainbow hack toggle (cycles player between all colors) (synced across all clients) (Internal Feature)
    - Change lobby color (HOST ONLY) (Changes colors of all lobby players to the selected color from the color palette) (Internal Feature)
  
  ### Map
    - No clip (move through walls)
    - No shadow / fog (removes raycasted shadows from walls)
    - Medbay Scan (Plays the medbay scan animation, works even as impostor) (synced across all clients) (Internal Feature)
    - Empty Garbage (Plays the garbage disposal animation, works even as impostor) (synced across all clients) (Internal Feature)
    - Weapons (Plays the Weapons minigame machine gun animation, works even as impostor) (synced across all clients) (Internal Feature)
   
   ### Stats
    - View current players in the lobby (updates automatically)

## Warning
  - all pointers are outdated
  - external libraries used are most probably outdated so manual updation is required before you build it into an exe
  - 2021 me did not write light speed efficient code
  - UI libraries are not updated

If at all you want to try it out and assuming this still works provided you've updated all the pointers correctly, this program uses a custom injector that you can find in my public repos or just click on [this](https://github.com/devilb2103/AMODUS_INJECTOR)

Link to the latest available [public build](https://www.unknowncheats.me/forum/among-us/431341-amodus-multihack-internal.html)

Happy cheating :D

![](https://i.postimg.cc/Ssw9yxn1/image-2022-09-08-122736390.png)
