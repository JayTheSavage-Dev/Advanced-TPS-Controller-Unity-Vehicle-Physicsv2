# QA Runtime Checklist (Deterministic Manual Checks)

## Test Bed / Setup

- **Scene:** `Assets/AdvancedTPSCharacter/Scenes/Playground.unity`
- **Player prefab in scene hierarchy:** `CharacterControllerPackedPref` (instance)
- **Weapon pickup objects in scene hierarchy:** `PickUpPistol`, `PickUpSMG`, `PickUpShotGun`, `PickUpKnife`, `PickUpSniper`, `PickUpRifle`, `PickUpAxe`
- **Source prefabs (for reference):**
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/Character/CharacterControllerPackedPref.prefab`
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/PickUpPistol.prefab`
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/PickUpSMG.prefab`
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/PickUpShotGun.prefab`
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/PickUpKnife.prefab`
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/PickUpSniper.prefab`
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/PickUpRifle.prefab`
  - `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/PickUpAxe.prefab`

### Controls used in this checklist (keyboard/mouse)

- Fire: `Left Mouse Button`
- Reload: `R`
- Aim: `Right Mouse Button`
- Holster/Equip toggle: `F`
- Crouch toggle: `C`
- Movement: `W/A/S/D`
- Sprint: `Left Shift`
- Drop/remove current weapon: `Q`

> Note: weapon cycling (`MoveThrough` / `MoveBack`) is bound for gamepad shoulder buttons in the input asset. For keyboard/mouse validation, use nearby pickup pedestals to force deterministic weapon-slot transitions.

---

## 1) Reload Spam Prevention

### Goal
Repeated reload input while a reload is in progress should not re-trigger the reload animation/logic.

### Steps
1. Open `Playground.unity` and press Play.
2. Move to `PickUpPistol` and ensure pistol is equipped.
3. Fire until ammo is below max clip (but greater than 0).
4. Press `R` once to begin reload.
5. During the same reload animation window, spam `R` quickly (10-15 presses).

### Expected
- Only one reload cycle runs.
- No extra detach/drop/attach loops start before the current cycle ends.
- Ammo is refilled exactly once at reload completion.

---

## 2) Auto-Reload at 0 Ammo

### Goal
When ammo reaches 0 on a firearm, reload should trigger automatically without pressing `R`.

### Steps
1. In Play Mode, equip `PickUpSMG` (or `PickUpRifle`).
2. Hold fire until magazine reaches `0`.
3. Do **not** press `R`.

### Expected
- Reload animation starts automatically after hitting 0 ammo.
- Weapon returns to a full clip when reload completes.
- No second reload starts unless ammo is spent again.

---

## 3) Reload Event Sequence Correctness

### Goal
Validate event order during reload: `detach_magazine` -> `drop_magazine` -> `refill_magazine` -> `attach_magazine`.

### Steps
1. In Play Mode, equip `PickUpRifle` (best visible magazine behavior).
2. Spend at least one bullet.
3. Press `R` once.
4. Observe the weapon + left-hand magazine behavior frame-by-frame (use slow playback if needed).

### Expected
- Magazine detaches from weapon first.
- A dropped magazine clone appears and falls.
- Hand-held magazine is re-enabled/refilled.
- Final magazine is attached back to weapon and clip value is restored.

---

## 4) Aim / Recoil / Reload Interaction

### Goal
Verify aim state and recoil do not break reload transitions.

### Steps
1. Equip `PickUpPistol` or `PickUpSMG`.
2. Hold `Right Mouse Button` to aim.
3. Fire short bursts to apply recoil.
4. While still aimed, press `R` to reload.
5. Repeat with: (a) hip-fire reload, (b) release aim mid-reload, (c) re-press aim right after reload completes.

### Expected
- Reload starts reliably in both aimed and hip-fire cases.
- Recoil settles naturally; no stuck camera kick.
- Character returns to valid post-reload pose and can shoot immediately after reload completion.

---

## 5) Equip / Holster While Moving and Crouching

### Goal
Holster/equip toggles should stay stable while locomoting and crouched.

### Steps
1. Equip any firearm.
2. Hold `W` (forward movement) and toggle holster/equip with `F` repeatedly.
3. While moving, press `C` to crouch and continue toggling `F`.
4. Repeat while strafing (`A` or `D`) and while sprinting (`Left Shift`, then toggle `F` after sprint release).

### Expected
- Holster/equip toggles work consistently without soft-lock.
- No permanent animation blend issues (character should return to normal locomotion).
- Weapon visibility/hand state matches holstered vs equipped state each time.

---

## 6) Each Weapon Slot Flow (7 weapon set)

### Goal
Validate full runtime flow for all slots (SMG, Pistol, Axe, Rifle, Shotgun, Sniper, Knife).

### Deterministic loop
For each pickup in this order:
`PickUpSMG` -> `PickUpPistol` -> `PickUpAxe` -> `PickUpRifle` -> `PickUpShotGun` -> `PickUpSniper` -> `PickUpKnife`

Run the same mini-flow:
1. Pick up weapon.
2. Perform primary action (`LMB`) once or burst.
3. For firearms only (SMG/Pistol/Rifle/Shotgun/Sniper):
   - press `R` reload once,
   - then empty clip to verify auto-reload at 0.
4. Toggle `F` holster and unholster.
5. Press `Q` to remove current weapon, then pick up the next one.

### Expected
- Every slot equips and updates behavior correctly.
- Melee slots (Axe, Knife) do not trigger firearm reload logic.
- Firearm slots support both manual and automatic reload paths.
- Removing and replacing weapons does not corrupt subsequent slot behavior.

---

## Optional: Editor PlayMode Test Scaffolding (Reload Trigger Guard)

If you want lightweight automation for reload-spam guard behavior, add PlayMode tests that:

1. Instantiate `CharacterControllerPackedPref.prefab` in a test scene.
2. Equip a firearm prefab (for example `WeaponPistol.prefab`).
3. Set ammo to a partial value (`ClipSize - 1`).
4. Invoke reload trigger attempts repeatedly over several frames.
5. Assert:
   - only one reload trigger becomes active while reload is in progress,
   - ammo refills once at completion,
   - additional reload attempts are ignored until reload exits.

Suggested location:
- `Assets/AdvancedTPSCharacter/Tests/PlayMode/ReloadGuardPlayModeTests.cs`

Suggested focus:
- Treat `CanTriggerReload` guard behavior as the unit-under-test contract from runtime perspective.
