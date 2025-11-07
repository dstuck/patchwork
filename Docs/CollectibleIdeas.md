# Collectible Upgrades

## Crafting rules

Crafting will require 3 collectibles, 2 as sources that will be removed and 1 as a target
that will persist after the upgrade. All upgrades have a sign (negative or positive) and a level. Here are the rules:

1. All three inputs must be the same sign (for now, in the future maybe we'll have rules about targets off the opposite sign)
2. Both sources must have the same level, either both 1 or 2
3. The output will be one level higher than the sources and of the same sign
4. If the target is the same type as one of the sources, the output will be that same type (i.e. fire + spark + fire = fire)
5. Otherwise, the output will match one of the 3 inputs chosen at random


## Brainstorming
### Costs
 - sparks
 - flames
 - point penalties
 - multiplier penalties
 - time penalties

### Benefits
 - multiplier
 - tile points
 - gems
 - hearts
 - company scrip

### Mixed
 - permanently delete tile


## Negative Collectibles

### Spark (-1)
Basic danger, cost life if not picked up
1. 1 danger
2. 2 danger
3. 4 danger

### Jumping Spark (-1)
Cost life if not picked up. Moves N squares away on tile placement
1. 1 squares
2. 2 squares, 2 danger
3. 4 squares, 3 danger

### Sneaky Spark (-1)
Cost life if not picked up. Sprite is hidden until N tiles have been placed
1. 1 tile
2. 2 tiles, 2 danger
3. 3 tiles, 2 danger

### Flame (-2)
Spreading flame danger, cost 1 life if not picked up. Spreads to N squares on each tile placement
1. 1 squares
2. 2 squares
3. 4 squares

### Poison (-2)
Discard N tiles from hand if picked up
1. 1 tile
2. 2 tiles
3. 4 tiles

## Positive Collectibles

### Draw Gem (+2)
Collect to draw a bonus tile, increases size of board by 5
1. 1 tile
2. 2 tiles
3. 4 tiles

### Heart Piece (+2)
Collect to add N to max hearts
1. .25 hearts
2. .5 hearts
3. 1 heart

### Multiplier (+2)
Collect to increase multiplier by N
1. x0.5
2. x1
3. x2

### Extra square (+2)
1. Add 1 square to the tile
2. Add 2
3. Add 4 (!?)

### Point Bonus (+2)
Collect to increase tile points by N
1. +10
2. +20
3. +40

### Pristine Gem (+2)
Adds Pristine upgrade to tile
1. +5
2. +10
3. +20


## Tile Upgrades

### Pristine Bonus
Adds 5 points if the tile is not overlapping with anything

### Lenient Bonus
No penalty for overlapping with anything

### Healing Bonus
Heal one heart if the tile is placed without collecting any collectibles