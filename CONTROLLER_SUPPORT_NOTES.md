# Controller Support for Crafting UI - Implementation Notes

This document describes the controller and keyboard support implementation for the Crafting UI.

## Code Changes
The CraftingUI.cs script has been updated to support controller and keyboard navigation in addition to mouse input.

## Required Unity Editor Setup

Since Unity UI objects and their meta files should not be created via code, the following visual elements need to be configured in the Unity Editor:

### CollectiblePreview Prefab
The CollectiblePreview prefab needs a **Selection Outline** Image component for visual feedback:

1. Open the CollectiblePreview prefab in the Unity Editor
2. Add a child Image component named "SelectionOutline" (if not already present)
3. Assign this Image component to the `m_SelectionOutline` field in the CollectiblePreview script
4. Configure the outline appearance (e.g., bright border color, slightly larger than the icon)
5. The outline should be disabled by default (will be enabled when selected)

### How the Navigation Works

The new controller support implements a cursor-based selection system similar to the GridCursor used in gameplay:

1. **Two Areas**: 
   - Collectibles area (top): Browse available collectibles
   - Slots/Buttons area (bottom): Crafting slots, Craft button, and Close button

2. **Navigation**:
   - **Horizontal (Left/Right)**: Navigate within the current area
   - **Vertical (Up/Down)**: Switch between collectibles area and slots/buttons area
   - **Submit (Space/Enter/Gamepad A)**: Select/activate the currently highlighted item
   - **Cancel (Esc/Gamepad B)**: Close the crafting UI

3. **Visual Feedback**:
   - The `SetSelected(true/false)` method on CollectiblePreview shows/hides the selection outline
   - This provides clear visual feedback about which item is currently selected

## Input Bindings
The implementation uses the existing UI action map from GameControls.inputactions:
- `UI.Navigate`: Arrow keys, WASD, D-pad, or analog stick
- `UI.Submit`: Space, Enter, or gamepad South button (A/Cross)
- `UI.Cancel`: Escape or gamepad East button (B/Circle)

These bindings are already configured in the GameControls.inputactions file and work with both keyboard and gamepad.

## Testing Recommendations

1. **Keyboard**: Use arrow keys or WASD to navigate, Space/Enter to select, Esc to close
2. **Gamepad**: Use D-pad or left stick to navigate, A button to select, B button to close
3. **Mouse**: Existing click functionality is preserved and works alongside controller input

The system gracefully handles switching between input methods - you can navigate with controller and click with mouse interchangeably.
