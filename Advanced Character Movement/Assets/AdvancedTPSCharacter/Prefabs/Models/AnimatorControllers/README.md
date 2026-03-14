# RigController layer mask assignment

`RigController.controller` uses two layers:

- **Base Layer**: no avatar mask (`m_Mask: {fileID: 0}`) so full-body locomotion remains authored here.
- **RecoilLayer**: assigned to `UpperBody_Recoil.mask` to isolate recoil / weapon actions (aim, fire, reload) to the upper body.

`UpperBody_Recoil.mask` enables humanoid Body, Head, Arms, Fingers, and Hand IK, while disabling Root, Legs, and Foot IK. This keeps hips/legs driven by locomotion on Base Layer while upper-body weapon poses play on RecoilLayer.
