This folder intentionally exists as a guard rail.

If a full duplicate of the project is accidentally imported under Assets/Assets,
Unity will otherwise compile every script twice and emit CS0101/CS0111 duplicate-type errors.

The asmdef in this folder is intentionally disabled via an unsatisfied define constraint,
so any accidental duplicate tree under Assets/Assets is excluded from compilation.

Do not place gameplay scripts here.
