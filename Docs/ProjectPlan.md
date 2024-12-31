# Game Project Plan

## Project Overview
Patchwork will be a 2D deckbuilding game where the player will curate a deck of tile cards of different shapes and abilities that they will need to use to try to patch up different shaped holes on a board. They will need to beat consecutive rounds of board with different challenges that will lead to upgrades to their deck.

## Milestones

### 0.1: MVP
- [x] Fixed deck of tiles
- [x] Fixed board with holes
- [x] Player able to place tiles on board
- [x] Board scoring system

### 0.2: Progress
- [x] Multiple boards
- [x] Cumulative score

### 0.3: Deck expansion
- [x] Tile selection screen
- [x] Able to add new tiles to deck between boards

### 0.4: Tile upgrades
- [x] Create 2 tile scoring upgrades
- [x] Board scoring accounts for upgrades
- [x] Tile upgrade screen
- [x] Select new tile and upgrade one after each round
- [x] Tooltip to explain upgrades

### 0.5: Difficulty scaling
- [ ] Implement draw gems
- [ ] Scale up board size along with draw gems
- [ ] Add timer with scaling rewards

## Design
Main concepts
- Tiles: A tiles shape is composed of connected unit squares and it maybe have modifiers that will be accounted for in scoring and be displayed visually
- Deck: The deck is a collection of tiles that will persist in a given run
- Hand: A hand is a selection of tiles from the players deck
- Board: The board is a grid that players will place tiles on, each square of the grid can have properties that effect scoring. At baseline a hole will score a base number of points if it's covered by a tile and a solid space will penalize points if covered


### Core Game Mode

During the core game mode, movement will be confined to a grid. The board will have grid coordinates and the player will move their curosr around the board up, down, left, and right along the grid.

The cursor will display the selected tile and when the player presses the confirm button, the tile will be placed on the board at the cursor's location. The next tile will then be selected until there are no more tiles to select at which point the board will be scored. There will be a scoring display followed by moving to the next scene.

## Notes
- Obviously the player's tiles will be upgraded as they progress, but maybe the board itself can be upgraded as well. That could be a path for larger scale "boss" upgrades perhaps.

## Brainstorming

### Challenge Increasee
It's hard to increase the challenge by making the board more complex or smaller

- Increase scoring requirement
    - Really relies on tile upgrades maybe too much
- Add timer
    - More hectic
    - More mistakes so more out of things like penalty decreases
- Add moving levels
    - Lose opportunity to to place in spots after they disappear
    - Maybe you draw more tiles as you go to fill in more