# Controller Navigation Flow Diagram

## Visual Layout

```
┌─────────────────────────────────────────────────────────────┐
│                     CRAFTING UI                              │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  COLLECTIBLES AREA (top)                                     │
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐          │
│  │ [C1] │  │  C2  │  │  C3  │  │  C4  │  │  C5  │  ...     │
│  └──────┘  └──────┘  └──────┘  └──────┘  └──────┘          │
│    ▲───────►────────►────────►────────►────────►            │
│    ◄───────◄────────◄────────◄────────◄────────◄            │
│      LEFT/RIGHT navigation within collectibles               │
│                                                               │
│                        ▼ DOWN                                 │
│                        ▲ UP                                   │
│                                                               │
│  SLOTS/BUTTONS AREA (bottom)                                 │
│  ┌──────┐  ┌──────┐  ┌──────┐    ┌──────┐                  │
│  │[Slot1]│  │ Slot2│  │ Slot3│    │Output│                  │
│  └──────┘  └──────┘  └──────┘    └──────┘                  │
│    ▲───────►────────►                                        │
│    ◄───────◄────────◄                                        │
│      LEFT/RIGHT navigation within slots                      │
│                                                               │
│                        ▼ DOWN                                 │
│                                                               │
│             [  Craft Button  ]                               │
│                        ▼ DOWN                                 │
│             [  Close Button  ]                               │
│                                                               │
└─────────────────────────────────────────────────────────────┘

Legend:
  [Item] = Selected (highlighted with outline)
  Item   = Not selected
  ▲▼◄►  = Navigation direction
```

## Navigation States

### State 1: Collectibles Area
```
Current Area: COLLECTIBLES
Selection: m_SelectedCollectibleIndex (0 to N-1)
Actions:
  - LEFT/RIGHT: Move between collectibles
  - DOWN: Switch to Slots Area (slot 0)
  - SUBMIT: Add selected collectible to next slot
```

### State 2: Slots Area
```
Current Area: SLOTS/BUTTONS
Selection: m_SelectedSlotIndex
  - 0, 1, 2: Crafting slots
  - -2: Craft button
  - -3: Close button
Actions:
  - LEFT/RIGHT: Move between slots (0-2)
  - UP: Switch to Collectibles Area
  - DOWN: Move through slots → Craft button → Close button
  - SUBMIT: Remove collectible from slot (if filled)
```

## Input Flow

```
┌──────────────┐
│ Input System │
└──────┬───────┘
       │
       ├─► Navigate.performed
       │   └─► OnNavigate()
       │       ├─► Horizontal? → NavigateLeft/Right()
       │       └─► Vertical?   → NavigateUp/Down()
       │
       ├─► Submit.performed
       │   └─► OnSubmit()
       │       ├─► In Collectibles? → OnCollectibleClicked()
       │       └─► In Slots?        → OnSlotClicked()
       │                             → OnCraftClicked()
       │                             → OnCloseClicked()
       │
       └─► Cancel.performed
           └─► OnCancel() → OnCloseClicked()

After any navigation:
  └─► UpdateSelectionVisuals()
      ├─► Clear all selections
      └─► Highlight current selection
```

## Selection Visual Updates

```
UpdateSelectionVisuals()
│
├─► Clear all existing selections
│   ├─► foreach CollectiblePreview: SetSelected(false)
│   └─► foreach CraftingSlot: SetSelected(false)
│
└─► Highlight current selection
    ├─► If in Collectibles Area:
    │   └─► m_CollectiblePreviews[m_SelectedCollectibleIndex].SetSelected(true)
    │
    └─► If in Slots Area:
        └─► m_CraftingSlots[m_SelectedSlotIndex].SetSelected(true)
        └─► (Buttons: use default Unity hover states)
```

## Code Execution Flow

```
UI Opens (OnEnable)
    └─► InitializeSelection()
        ├─► Set m_IsInCollectiblesArea = true
        ├─► Find first valid collectible
        └─► UpdateSelectionVisuals()

User Presses Navigation Button
    └─► OnNavigate(context)
        ├─► Check cooldown (0.15s)
        ├─► Read Vector2 input
        ├─► Determine direction (horizontal vs vertical)
        └─► Call appropriate Navigate method
            └─► UpdateSelectionVisuals()

User Presses Submit Button
    └─► OnSubmit(context)
        ├─► Check current area
        └─► Execute appropriate action
            ├─► OnCollectibleClicked() - adds to slot
            ├─► OnSlotClicked() - removes from slot
            ├─► OnCraftClicked() - performs crafting
            └─► OnCloseClicked() - closes UI
                └─► RefreshUI() if changes made
                    └─► UpdateSelectionVisuals()

User Presses Cancel Button
    └─► OnCancel(context)
        └─► OnCloseClicked()
            └─► HideUI()
```

## Key Variables

```
m_IsInCollectiblesArea (bool)
  - true:  User is navigating collectibles
  - false: User is navigating slots/buttons

m_SelectedCollectibleIndex (int)
  - -1:     Nothing selected
  - 0..N-1: Index of selected collectible
  - Only used when m_IsInCollectiblesArea = true

m_SelectedSlotIndex (int)
  - -1: Nothing selected
  - 0:  Source slot 1
  - 1:  Source slot 2
  - 2:  Target slot
  - -2: Craft button
  - -3: Close button
  - Only used when m_IsInCollectiblesArea = false

m_LastNavigationTime (float)
  - Time.time of last navigation
  - Used with m_NavigationCooldown (0.15s) to prevent spam
```

## Example Sequence

```
1. User opens crafting UI
   → InitializeSelection()
   → First collectible selected ([C1])

2. User presses RIGHT
   → OnNavigate(RIGHT)
   → NavigateRight()
   → m_SelectedCollectibleIndex = 1
   → UpdateSelectionVisuals()
   → Second collectible selected ([C2])

3. User presses DOWN
   → OnNavigate(DOWN)
   → NavigateDown()
   → m_IsInCollectiblesArea = false
   → m_SelectedSlotIndex = 0
   → UpdateSelectionVisuals()
   → First slot selected ([Slot1])

4. User presses UP
   → OnNavigate(UP)
   → NavigateUp()
   → m_IsInCollectiblesArea = true
   → m_SelectedCollectibleIndex = 1 (restored)
   → UpdateSelectionVisuals()
   → Second collectible selected ([C2])

5. User presses SUBMIT
   → OnSubmit()
   → m_IsInCollectiblesArea = true
   → OnCollectibleClicked(1)
   → Adds C2 to next available slot
   → RefreshUI()
   → UpdateSelectionVisuals()
```
