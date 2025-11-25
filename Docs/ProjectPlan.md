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
- [x] Add spark tiles that must be covered
- [x] Keep track of "danger" and trigger game over
- [x] Add fire that spreads after each placement

### 0.8: Collectible Modifiers
- [x] Add additional dangers that can be used for upgrades
- [x] Add additional benefits that can be used for upgrades
- [x] Refactor time and multiplier bonuses to be collectibles
- [x] Restructure upgrades to be randomized on each new stage
- [x] Collectible tooltip shows up on button press

### 0.9: Collectible Upgrades
- [x] Create upgraded versions for collectibles (modify sprite with + and ++)
- [x] Create recipes for combining collectibles into higher level
- [x] Create upgrade screen providing upgrade options

### 0.10: SFX
- [x] Add SFX for tile placement
- [x] Add SFX for cursor movement
- [ ] Add SFX for collectible pickup

### 0.11: Game Goals
- [ ] Add score-based game over condition
- [ ] Make clearer score UI
- [ ] Figure out what to do with boss levels

<!-- ### 0.9: CEOs
- [ ] Add boss stage upgrades that come with costs
- [ ] Generate these bonuses randomly
- [ ] Create CEOs that choose bonuses for you based on preferences
- [ ] Add CEO selection screen at start of each run

### 0.10: Badges (rename when we understand how it fits in story better)
- [ ] Implement a badge system that can be equipped on new runs
- [ ] CEOs define badge limits
- [ ] Create 10 initial badges (start with upgraded tiles, allow dropping tiles, extra time...) -->


## Design
Core feeling to capture: "I am so good at my job but it doesn't matter due to the moronic decisions of executives"

You only control your own work and upgrades tied to your deck of patches and limited in scope while large scale upgrade decisions that manage your ability to scale score up and that will add more dangers to the board are chosen by the CEOs that will be determined at the start of each run/company you join.

The CEO will add the collectibles present on your board, but what you can control is how you place them and then how you combine collectibles in a crafting-like step between stages. The recipes for how they combine will be revealed as you make them and will be persistent between runs providing a sense of growth at the outer loop

Main structures
- Tiles: A tiles shape is composed of connected unit squares and it maybe have modifiers that will be accounted for in scoring and be displayed visually
- Deck: The deck is a collection of tiles that will persist in a given run
- Hand: A hand is a selection of tiles from the players deck
- Board: The board is a grid that players will place tiles on, each square of the grid can have properties that effect scoring. At baseline a hole will score a base number of points if it's covered by a tile and a solid space will penalize points if covered
- Collectibles: Items on the board that you can pick up by placing a tile on them. Provide benefits for picking up or penalties for missing them


### Game overview
The core game is about an executive trading off safety by choosing powerups that increase score scaling at the cost of introducing more dangerous elements like sparks, flames, and others. On each run the player will need to exponentially increase their score as they progress to meet 

### Core Game Structure

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

### Collectible management
If all major upgrades are collectible based and chosen by CEOs we have two problems, one we'll have too many for the board eventually and two we won't have control over them. To handle both cases, we should have a collectible combining/upgrading system where you can take say some weaker collectibles and combine them into one stronger one (beneficial and detrimental). That way you're encouraged to reduce the number of them and also have some input in what they are. So combine 2 bad to make a single but stronger bad pickup and maybe add in one good pickup to modify what the outcome is...

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
- What if the meta game is about taking down a set of CEOs whose companies you're working for
    - You'd need some tangential mechanical way to make progress like maybe get enough high combos or intentially tanking at a particular point
    - This could reduce the possible CEOs for future runs
    - Maybe if you "beat" a level by making the company profitable your character becomes a CEO for a future run? Or maybe you're just laid off all the same
    - What if there's a "shadows of mordor" vibe where you make CEOs more powerful or weak based on your performance and instead of making each run a high score attempt, you're intentionally trying to set up things to change power dynamics?

## Todos (followups from previous versions)
- [ ] Add bomb tiles
