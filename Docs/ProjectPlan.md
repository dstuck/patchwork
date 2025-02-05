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
- [x] Implement draw gems
- [x] Scale up board size along with draw gems
- [x] Add timer with scaling rewards

### 0.6: Boss boards
- [x] Make boss level with moving board
- [x] Make boss level explanation screen
- [x] Add boss rewards

### 0.7: Danger
- [ ] Add spark tiles that must be covered
- [ ] Keep track of "danger" and trigger game over
- [ ] Add fire that spreads after each placement


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

- What if the game is more about collecting various gems on the board?
    - It's more of an excavating game where tiles modify the value of collected gems

- What if the core mechanic is about "defusing bombs" (or map that to some less violent concept)
    - basic bombs just need a tile covering it
    - some bombs are paired and need a single tile to cover them both
    - some bombs have timers making you work fast
    - some bombs spawn other bombs so need to be defused first
    - big bombs could cover multiple squares (maybe just combine them automatically if they're next to each other)
- Maybe we could go back to the original idea of patching up electrical equipment/toys headless_run_game
    - each spark needs to be covered up
    - some sparks spawn other sparks
    - maybe paired sparks make sense
- Whether sparks or bombs, we still need to figure out ways that could actually trigger a loss
    - Players are driven to go fast for bonus points but can always slow down if they need to not lose...
    - timed and spawning bombs could potentially overwhelm the player but not likely
    - maybe the only way you really lose it on a boss fight, the intermediate ones just matter because it lets you build up resources by doing them fast
    - So need to make sure that resources are required to beat the bosses consistently
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
- Add gems that must be covered by tiles or you lose, maybe we'll call it bomb for now
    - Maybe we start with one and build them up til there are lots of them
- Could have little moving things/enemies that move around and lose you points
    - There could be upgrades that squash them and give bonus points

### Upgrade Routes
- Higher multipliers but lose precision somehow?
    - Narratively a more AI-like build where we're scoring faster but with poorer quality
- More squares
    - Maybe bigger tiles
    - Allow multiple scoring rather than losing points for overlap
    - Narratively, this would be upgrading to do a safer job
- More bombs, but higher multiplier


### Boss Boards
Large moving level:
- the board moves to the left
- tiles need to be carefully saved to hit the draw gems to maximize points

Connection puzzle:
- the board contains two points that need to be continually connected by tiles

Fog of war:
- Can only see a small space in the center and one square around the edge of each tile you play

## Story

### Ideas
- You take a low paying job that involves patching up exposed electrical wiring
    - You find out soon that the company has a big showy AI that it's using to build its devices and your job is to clean up the messes that it makes. You need to go fast under threat of being outsourced, you don't have enough materials to fully patch things up, and you as the QA team are blamed when the public gets mad at the shoddy work and accidents it causes
    - In each level you never have enough squares to cover everything
- Ending could be based on upgrade path and the Gervais Principle
    - If you use lots of materials to cover wiring you just get canned and find a better job
    - If you have high coverage throughout the game you are a loser 
    - If you use the high-multiplier, low accuracy path you are clueless and get promoted to middle management
    - If you do the minimum amount to get by, you get promoted to upper management as a sociopath


## Todos (followups from previous versions)
- [ ] Add bomb tiles
