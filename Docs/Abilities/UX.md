# Ability User Experience

## Overview
---

- Select ability via key binding.
- Ability will display preview of all the *affected* tiles, up until the first *targetable* action.
- *Targetable* actions will cause *targetable* tiles to be re-skinned and responsive to mouse-events.
- Mouse-Over of *targetable* tiles will cause the ability's action chain to be recalculated using the *hovered* tile as the target for the *targetable* action. A preview of the affected tiles will be rendered.
- Mouse-Down of *targetable* tiles will lock the target, render the *targetable* tiles normally, and display a preview of only the affected tiles.
- Pressing *Enter* when ability is in a locked state will cause application of the ability.
- Pressing *Escape* when ability is in a locked state will cause the ability to be unlocked, allowing re-targetting.
- Pressing *Escape* when ability is in an unlocked state will cause deselection of the ability.
- **Note**: abilities without *targetable* actions in their action chain will automatically enter a locked state after ability selection. 