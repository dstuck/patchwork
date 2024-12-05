# Chat Session - Tile Scoring Implementation

Q: let's start working towards board scoring. The tiles themselves should own the responsibility for computing a score for a given state of the board and other tiles. They should score based on each square they cover and whether it's shared with a hole or another tile neither, or both. Over a hole should score 10, over another tile should be -2, over no hole should be -5.

A: Let's add scoring logic to the PlacedTile class. Each tile will calculate its own score based on its position relative to holes and other tiles: 

Q: Now let's calculate the full score when the enter key is pressed

A: We'll add scoring methods to both Board and GridCursor classes:

And in GridCursor:

Q: When a score is computed let's have each tile display the score on it's center point

A: We'll add a TextMeshPro component to display the score:

And update the score display in CalculateScore:

Q: The text is just showing up as a T when the tile is placed

A: The issue is with the TextMeshPro configuration. Let's update the settings to make it more visible and properly layered:

Q: It did not. There are just T's showing up. How is that text value updated and displayed? It should initally not show anything until we trigger a board score

A: Let's fix the text visibility logic. We'll start with the text disabled and only enable it when calculating scores:

1. Start with text disabled (already set)
2. Only enable and show text when calculating score
3. Made text larger and properly layered above tiles

The key changes are:
- Text starts disabled with empty string
- Only enabled when score is calculated
- Proper sorting order to show above tiles
- Black text color for visibility
- Increased size for readability

This ensures scores only appear after calculating the board score and are clearly visible.