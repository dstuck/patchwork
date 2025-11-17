# Testing Guide for Controller Support in Crafting UI

## Overview
This guide describes how to test the new controller and keyboard navigation support for the crafting UI.

## Prerequisites
1. The CollectiblePreview prefab must have a SelectionOutline Image component configured (see CONTROLLER_SUPPORT_NOTES.md)
2. The game should be running in the Unity Editor or as a build
3. Have a gamepad connected (Xbox, PlayStation, or generic controller) OR use keyboard

## Test Scenarios

### Test 1: Initial Selection State
**Steps:**
1. Start the game
2. Open the crafting UI (default: Escape/Pause button)
3. Observe that the first available collectible is highlighted with a selection outline

**Expected Result:** 
- First valid collectible shows selection outline
- No other items are highlighted

### Test 2: Horizontal Navigation in Collectibles Area
**Input Methods:**
- Keyboard: Arrow keys (Left/Right) or A/D
- Gamepad: D-pad (Left/Right) or Left Stick

**Steps:**
1. With crafting UI open and first collectible selected
2. Press Right/D
3. Observe selection moves to next collectible
4. Press Left/A
5. Observe selection moves back to previous collectible

**Expected Result:**
- Selection outline moves between collectibles
- Only one collectible highlighted at a time
- Navigation wraps or stops at boundaries

### Test 3: Vertical Navigation Between Areas
**Input Methods:**
- Keyboard: Arrow keys (Up/Down) or W/S
- Gamepad: D-pad (Up/Down) or Left Stick

**Steps:**
1. With first collectible selected
2. Press Down/S
3. Observe selection moves to crafting slots area
4. Press Up/W
5. Observe selection returns to collectibles area

**Expected Result:**
- Selection switches between collectibles area and slots/buttons area
- Appropriate item is highlighted in each area

### Test 4: Submit Action on Collectible
**Input Methods:**
- Keyboard: Space or Enter
- Gamepad: A button (Xbox) / Cross button (PlayStation)

**Steps:**
1. Select a valid collectible (not grayed out)
2. Press Submit button
3. Observe collectible is added to next available crafting slot

**Expected Result:**
- Collectible appears in crafting slot
- Same behavior as clicking the collectible with mouse
- Invalid collectibles (grayed out) should not be selectable

### Test 5: Submit Action on Crafting Slot
**Steps:**
1. Navigate to slots area (press Down)
2. Select a filled crafting slot
3. Press Submit button
4. Observe collectible is removed from slot (if allowed by rules)

**Expected Result:**
- Collectible removed from slot following existing removal rules
- Same behavior as clicking the slot with mouse

### Test 6: Cancel Action
**Input Methods:**
- Keyboard: Escape
- Gamepad: B button (Xbox) / Circle button (PlayStation)

**Steps:**
1. With crafting UI open
2. Press Cancel button

**Expected Result:**
- Crafting UI closes
- Game resumes

### Test 7: Craft Button Activation
**Steps:**
1. Fill all three crafting slots with valid collectibles
2. Navigate to slots area (press Down)
3. Continue navigating down to reach Craft button
4. Press Submit

**Expected Result:**
- Crafting operation executes
- Output collectible appears in output slot
- Same behavior as clicking Craft button with mouse

### Test 8: Mouse/Controller Hybrid Use
**Steps:**
1. Open crafting UI
2. Navigate using controller to select a collectible
3. Click a different collectible with mouse
4. Navigate using controller again

**Expected Result:**
- Both input methods work simultaneously
- Last used input method determines selection state
- No conflicts or errors

### Test 9: Navigation Cooldown
**Steps:**
1. Rapidly press navigation buttons (Left/Right or Up/Down)
2. Observe selection movement rate

**Expected Result:**
- Navigation has a slight cooldown (0.15 seconds)
- Prevents accidental rapid movement
- Feels responsive but controlled

### Test 10: Selection Persistence Through Refresh
**Steps:**
1. Select a collectible
2. Add it to a crafting slot (UI will refresh)
3. Observe selection state

**Expected Result:**
- Selection state is maintained after UI refresh
- Selected item remains highlighted if still valid
- If selected item becomes invalid, selection moves to first valid item

## Known Limitations

1. **Unity Editor Setup Required**: The SelectionOutline Image component must be manually added to the CollectiblePreview prefab in Unity Editor. Without this, visual feedback will not work.

2. **Navigation Assumes Grid Layout**: The horizontal navigation assumes collectibles are laid out in a horizontal grid. If layout changes, navigation logic may need adjustment.

3. **Button Navigation**: Navigation to Craft and Close buttons is simplified. More complex button navigation could be added if needed.

## Troubleshooting

### Selection outline not visible
- Check that SelectionOutline Image component is properly configured on CollectiblePreview prefab
- Verify the outline color has sufficient contrast with background

### Controller input not working
- Verify gamepad is properly connected
- Check that GameControls.inputactions has UI action map enabled
- Ensure no other systems are consuming input events

### Navigation feels unresponsive
- Adjust m_NavigationCooldown in CraftingUI.cs (currently 0.15 seconds)
- Lower values = faster navigation, higher values = slower but more controlled

### Selection lost after crafting
- This is expected behavior - selection resets when collectibles list changes
- First valid collectible will be auto-selected

## Success Criteria

All tests should pass with:
- ✅ Keyboard controls work as expected
- ✅ Gamepad controls work as expected  
- ✅ Mouse controls still work (backward compatibility)
- ✅ Visual feedback is clear and consistent
- ✅ No errors or exceptions in console
- ✅ Performance is smooth (no lag during navigation)
