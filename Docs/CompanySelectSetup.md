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
- `Assets/Scenes/CompanySelect.unity` - The new start screen scene

## Unity Editor Setup Instructions

### 1. Create CompanySlot Prefab

1. In Unity Editor, create a new empty GameObject
2. Add the following components/children:
   - Add `CompanySlot` component
   - Create child TextMeshPro for company name (`m_CompanyNameText`)
   - Create child TextMeshPro for "Bonuses:" label (`m_BonusesLabel`)
   - Create child empty GameObject as bonuses container (`m_BonusesContainer`)
     - Add HorizontalLayoutGroup component
   - Create child TextMeshPro for "Dangers:" label (`m_DangersLabel`)
   - Create child empty GameObject as dangers container (`m_DangersContainer`)
     - Add HorizontalLayoutGroup component
   - Create child Image for selection highlight (`m_SelectionHighlight`)
     - Set color to yellow/gold
     - Initially disabled
   - Assign references in CompanySlot inspector

3. Save as prefab: `Assets/Prefabs/UI/CompanySlot.prefab`

### 2. Set Up CompanySelect Scene

1. Open `Assets/Scenes/CompanySelect.unity`
2. Delete the existing GameOverScreen GameObject
3. Create the following hierarchy:

```
CompanySelect (Canvas)
├── Title (TextMeshPro) - "Select Your Company"
├── CompanySlotsContainer (Empty GameObject with VerticalLayoutGroup)
└── EventSystem
```

4. Add `CompanySelectUI` component to the Canvas
5. Configure CompanySelectUI component:
   - Assign Title TextMeshPro to `m_TitleText`
   - Assign CompanySlotsContainer to `m_CompanySlotsContainer`
   - Assign CompanySlot prefab to `m_CompanySlotPrefab`
   - Assign CollectiblePreview prefab to `m_CollectiblePreviewPrefab` (from `Assets/Prefabs/UI/CollectiblePreview.prefab`)
   - Set `m_GameplaySceneName` to "GameplayScene"

### 3. Layout Configuration

#### CompanySlotsContainer
- Add VerticalLayoutGroup:
  - Child Alignment: Middle Center
  - Spacing: 20
  - Child Force Expand: Width and Height

#### Each CompanySlot (in prefab)
- Use VerticalLayoutGroup for main layout
- Add Image component for background
- Layout each section:
  - Company name at top (large text)
  - Bonuses section with horizontal layout
  - Dangers section with horizontal layout
  - Selection highlight as overlay

### 4. Testing the Flow

The complete flow should be:
1. Game starts → CompanySelect scene loads
2. Player navigates with up/down arrows
3. First company highlighted by default
4. Press Enter/Space to select company
5. Game starts with selected company's bonuses and dangers
6. On game over → Returns to CompanySelect scene

## Code Integration Points

### GameManager Methods
- `GenerateCompanyOptions()` - Creates 3 companies with random collectibles
- `SetSelectedCompany(CompanyData)` - Sets active bonuses/dangers for selected company
- `CompanyName` property - Stores the selected company name

### Scene Flow
- CompanySelect → GameplayScene (via StartNewGame)
- GameOver → CompanySelect (updated in GameOverScreen.cs)

## Notes

- The CompanySelect scene is now the first scene in EditorBuildSettings
- CollectiblePreview prefab is reused to show bonuses and dangers
- Each company gets 3 random bonuses and 2 random dangers (same logic as InitializeCollectibles)
- Navigation uses the same input system as other UI scenes (UI action map)
