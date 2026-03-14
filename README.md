# Advanced-TPS-Controller-Unity
A state-of-the-art TPS controller with a large set of gameplay and animation systems for quick, production-ready Unity setup.

## Features
- 7 weapon types: Axe, Knife, Pistol, Shotgun, Rifle, SMG, Sniper.
- Weapon-specific attacks (shooting, swinging), with ammo, reloading, and procedural recoil for guns.
- Advanced foot IK and character movement.
- 8-way locomotion and animation controllers reusable across projects.
- New Input System implementation.
- Xbox, PlayStation, and PC controls.
- Key rebinding with persistent bindings.
- Destructible objects with rigidbody force impulse reactions.
- Aiming with camera zoom and reduced recoil values.
- Recoil pattern configuration.
- Equip and holster abilities.
- Advanced vehicle suspension and car controller.
- Improved map, visuals, and item pedestals.
- Weapon toggling/cycling support.
- Spine rotation based on camera position.
- Advanced crosshair system.
- Customizable toggles (for example, hiding inventory).
- Footstep sound detection system.
- Improved ragdoll and player controls.

## Unity 6 compatibility
This repository has been updated for Unity 6 (6000.0.x).

- Project version has been moved to Unity 6.
- Legacy/deprecated package dependencies were removed from `Packages/manifest.json`.
- Core package versions were updated to Unity 6-compatible releases.

If Unity prompts to reimport assets after upgrade, allow the full reimport once and then reopen the project.

## Final integration checklist
For a quick 5–10 minute validation pass of reload/aim/recoil/equip/holster + prefab references, use `FINAL_INTEGRATION_CHECKLIST.md`.
