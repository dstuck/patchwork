# CompanySelect Scene Setup Guide

This guide explains how to set up the CompanySelect scene in Unity Editor.

## Overview

The CompanySelect scene serves as the start screen where players choose from 3 randomly generated companies, each with different bonuses and dangers.

## Files Created

### Scripts
- `Assets/Scripts/Data/CompanyNameGenerator.cs` - Generates random company names
- `Assets/Scripts/Data/CompanyData.cs` - Data structure for company info
- `Assets/Scripts/UI/CompanySelectUI.cs` - Main scene controller
- `Assets/Scripts/UI/CompanySlot.cs` - Individual company display component
- `Assets/Scripts/Gameplay/GameManager.cs` - Updated with company selection methods

### Scenes
- `Assets/Scenes/CompanySelect.unity` - The new start screen scene (needs UI setup)

## Unity Editor Setup Instructions

### Step 1: Create CompanySlot Prefab

1. In Unity Editor, create a new UI GameObject (right-click in Hierarchy → UI → Panel)
2. Rename it to "CompanySlot"
3. Add the `CompanySlot` component to it
4. Create the following child elements:

#### CompanySlot Children:
```
CompanySlot (Panel with CompanySlot component)
├── CompanyNameText (TextMeshPro - UGUI)
├── BonusesSection
│   ├── BonusesLabel (TextMeshPro - UGUI) - Text: "Bonuses:"
│   └── BonusesContainer (Empty GameObject)
│       └── Add HorizontalLayoutGroup component
│           - Child Alignment: Middle Center
│           - Spacing: 10
│           - Child Force Expand: Width only
├── DangersSection
│   ├── DangersLabel (TextMeshPro - UGUI) - Text: "Dangers:"
│   └── DangersContainer (Empty GameObject)
│       └── Add HorizontalLayoutGroup component
│           - Child Alignment: Middle Center
│           - Spacing: 10
│           - Child Force Expand: Width only
└── SelectionHighlight (Image)
    - Color: Yellow (255, 255, 0, 100)
    - Initially disabled
    - Make it fill the entire CompanySlot (anchor to all corners)
```

5. Configure CompanySlot component references in Inspector:
   - `m_CompanyNameText` → CompanyNameText
   - `m_BonusesContainer` → BonusesContainer (RectTransform)
   - `m_DangersContainer` → DangersContainer (RectTransform)
   - `m_SelectionHighlight` → SelectionHighlight (Image)
   - `m_BonusesLabel` → BonusesLabel
   - `m_DangersLabel` → DangersLabel
   - `m_CollectiblePreviewPrefab` → (Leave empty, will be set by CompanySelectUI)

6. Add VerticalLayoutGroup to the main CompanySlot panel:
   - Child Alignment: Upper Center
   - Spacing: 5
   - Padding: 10 on all sides
   - Child Force Expand: Width and Height

7. Save as prefab: `Assets/Prefabs/UI/CompanySlot.prefab`

### Step 2: Set Up CompanySelect Scene

1. Open `Assets/Scenes/CompanySelect.unity`
2. Delete the existing "GameOverUI" GameObject (or all existing UI except Canvas and EventSystem)
3. Make sure you have:
   - Canvas (with CanvasScaler set to Scale With Screen Size, reference resolution 1920x1080)
   - EventSystem

4. Add `CompanySelectUI` component to the Canvas GameObject

5. Create the following UI hierarchy under Canvas:

```
Canvas (with CompanySelectUI component)
├── Background (Image) - Optional dark background
├── TitleText (TextMeshPro - UGUI)
│   - Text: "Select Your Company"
│   - Font Size: 72
│   - Alignment: Center
│   - Anchor: Top center
├── CompanySlotsContainer (Empty GameObject)
│   - Anchor: Middle center
│   - Add VerticalLayoutGroup:
│     - Child Alignment: Middle Center
│     - Spacing: 30
│     - Child Force Expand: Width only
│   - Add ContentSizeFitter:
│     - Vertical Fit: Preferred Size
└── InstructionsText (TextMeshPro - UGUI) - Optional
    - Text: "Use Arrow Keys to navigate, Enter to select"
    - Font Size: 24
    - Alignment: Center
    - Anchor: Bottom center
```

6. Configure CompanySelectUI component in Inspector:
   - `m_TitleText` → TitleText
   - `m_CompanySlotsContainer` → CompanySlotsContainer (RectTransform)
   - `m_CompanySlotPrefab` → CompanySlot prefab from Assets/Prefabs/UI/
   - `m_CollectiblePreviewPrefab` → CollectiblePreview prefab from Assets/Prefabs/UI/
   - `m_GameplaySceneName` → "GameplayScene"
   - `m_InputCooldown` → 0.15

7. Save the scene

### Step 3: Layout Configuration

#### Canvas Settings
- Canvas Scaler:
  - UI Scale Mode: Scale With Screen Size
  - Reference Resolution: 1920 x 1080
  - Screen Match Mode: Match Width Or Height
  - Match: 0.5

#### TitleText Layout
- Anchor Presets: Top Center (with stretch horizontally if needed)
- Position Y: -100 (from top)
- Font Size: 60-80

#### CompanySlotsContainer Layout
- Anchor Presets: Middle Center
- Width: 1200-1400
- The VerticalLayoutGroup will auto-size height based on children

#### CompanySlot Prefab Layout (within the prefab)
- Recommended size: 400 height x 1200 width (will be controlled by parent layout)
- Background panel color: Semi-transparent dark (e.g., rgba(0,0,0,150))
- CompanyNameText: Large, bold, centered at top
- Sections arranged vertically with small spacing

### Step 4: Testing the Flow

The complete flow should be:
1. Game starts → CompanySelect scene loads
2. Player sees 3 companies with random names
3. Each company shows 3 bonus collectibles and 2 danger collectibles with tooltips
4. First company is highlighted by default (yellow outline)
5. Player navigates with up/down arrows (cursor cycles from bottom to top)
6. Press Enter/Space to select company
7. Game starts with selected company's bonuses and dangers
8. On game over → Returns to CompanySelect scene

### Step 5: Manual Testing Checklist

After setup, test the following:
- [ ] CompanySelect scene loads without errors
- [ ] Three company slots appear with different names
- [ ] Each slot shows 3 collectible icons in bonuses section
- [ ] Each slot shows 2 collectible icons in dangers section
- [ ] Hovering over collectible icons shows tooltips
- [ ] First company is highlighted on load
- [ ] Up arrow moves highlight up (with wrap-around)
- [ ] Down arrow moves highlight down (with wrap-around)
- [ ] Enter key selects highlighted company and starts game
- [ ] Selected company's bonuses/dangers appear during gameplay
- [ ] Game Over returns to CompanySelect scene

## Code Integration Points

### GameManager Methods
- `GenerateCompanyOptions()` - Creates 3 companies with random collectibles (uses same logic as InitializeCollectibles)
- `StartGameWithCompany(CompanyData)` - Sets active bonuses/dangers and starts new game
- `CompanyName` property - Stores the selected company name

### Scene Flow
- CompanySelect → GameplayScene (via StartGameWithCompany)
- GameOver → CompanySelect (updated in GameOverScreen.cs)

### Input System
- Uses the same GameControls input actions as other UI scenes
- Navigate: UI.Navigate (arrow keys or D-pad)
- Submit: UI.Submit (Enter, Space, or gamepad button)

## Troubleshooting

### "CompanySlotPrefab is not assigned" error
- Make sure you've created the CompanySlot prefab and assigned it in CompanySelectUI inspector

### "CollectiblePreview not found" error  
- Verify the CollectiblePreview prefab exists at Assets/Prefabs/UI/CollectiblePreview.prefab
- Make sure it's assigned in CompanySelectUI inspector

### Companies don't show collectibles
- Check that CollectiblePreview prefab has the CollectiblePreview component
- Verify GameManager instance exists (it should persist across scenes)

### Navigation doesn't work
- Verify EventSystem exists in the scene
- Check that GameControls.inputactions has UI.Navigate and UI.Submit actions configured

### Tooltips don't appear
- Make sure TooltipSystem GameObject exists in the scene
- Verify CollectiblePreview prefab has tooltip components

## Notes

- The CompanySelect scene is now the first scene in EditorBuildSettings
- CollectiblePreview prefab is reused to show bonuses and dangers
- Each company gets 3 random bonuses (from: NewSquare, DrawGem, HeartPiece, PristinePaint)
- Each company gets 2 random dangers (from: Spark, GhostSpark, JumpingSpark, Flame)
- Navigation uses the same input system as other UI scenes (UI action map)
- The scene file currently needs to be set up manually in Unity Editor as described above
