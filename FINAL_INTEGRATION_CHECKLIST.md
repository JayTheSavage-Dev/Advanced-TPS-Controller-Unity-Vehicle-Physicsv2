# Final Integration Checklist (5–10 minutes)

Use this as a **last-pass smoke test** after wiring a character variant or moving the TPS setup into a new scene.

## 0) Open baseline scene and prefab (30–60 sec)
- Open `Assets/AdvancedTPSCharacter/Scenes/SampleScene.unity` or your target gameplay scene.
- Select your player prefab (recommended baseline: `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/Character/CharacterControllerPackedPref.prefab`).

---

## 1) Inspector reference sanity check (2–3 min)

### A. `ActiveWeapon` component
Verify these references are assigned on the player root:
- `weaponSlots` array size = 7 (SMG, Pistol, Axe, Rifle, Shotgun, Sniper, Knife order).
- `WeaponLeftGrip` and `WeaponRightGrip`.
- `rigController` (weapon rig animator).
- `tracerRenderer`.
- `ammoWidget`.
- `playerCamera` (`CinemachineFreeLook`).

### B. `ReloadWeapon` component
Verify:
- `rigController` assigned.
- `activeWeapon` assigned.
- Reload overrides list exists (or intentionally empty if using defaults).

### C. `WeaponAiming` component
Verify:
- Camera/FOV refs are assigned.
- Aiming works with both firearm and melee equipped.

### D. Weapon prefabs
For each weapon prefab in `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponsRaw/`:
- `GunController` present.
- `WeaponProceduralRecoil` present for firearms and pattern is non-empty.
- Fire point/muzzle/tracer refs are assigned.

---

## 2) Runtime interaction pass (3–5 min)
Enter Play Mode and validate all flows below:

1. **Equip / switch**
   - Cycle forward/back through inventory.
   - Expected: correct equip animation and correct rig pose per slot.

2. **Holster / unholster**
   - Toggle holster key once each direction.
   - Expected: `Holster_Weapon` behavior visually matches state and blocks weapon firing while holstered.

3. **Aim**
   - Hold aim with pistol/rifle.
   - Expected: camera zoom/FOV transition, movement remains responsive, crosshair state is consistent.

4. **Recoil**
   - Fire a 10–15 round burst with an automatic gun.
   - Expected: procedural recoil pattern applies and calms after release; recoil is reduced while aiming.

5. **Reload**
   - Empty mag, then reload.
   - Expected: reload animation, ammo refill, no firing during reload lock.

6. **Melee edge checks**
   - Switch to Axe and Knife.
   - Expected: attack animations fire correctly and do not use firearm reload/recoil logic.

---

## 3) Pickup + prefab linkage check (1–2 min)
- Drop at least one pickup from `Assets/AdvancedTPSCharacter/Prefabs/MainPrefabs/WeaponPickups/` into scene.
- Trigger pickup in Play Mode.
- Confirm:
  - Weapon is added to inventory.
  - Correct slot receives weapon.
  - Equip/holster/aim/reload behavior still works after pickup.

---

## 4) “Ready to ship” quick gate (30 sec)
Mark done only if all are true:
- [ ] No missing references on player/weapon prefabs.
- [ ] Equip + holster transitions are animation-clean.
- [ ] Aim + recoil + reload behave correctly on at least 2 firearms.
- [ ] Melee weapons (Axe/Knife) still behave correctly.
- [ ] Pickup path works and preserves state.

If any box fails, fix and rerun only the failed section first, then perform one final full pass.
