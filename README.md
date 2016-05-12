# CMSC425
Title: .....................Insert.Title....................

Team Members: Andrew Cleary, George Cleary

Overview:
Top-down 2-d adventure game. Main mechanic that allows the player to “buffer” actions/inputs in a paused game state for limited then after the buffer period the character executes the actions popping them off a stack.  This mechanic is a type of a turn-based system which allows the player to buffer some moves per buffer period and on release advance the game-state the specified amount of turns, executing the actions of the player, executing the actions of the enemy’s AI, and advancing any non-player elements ahead in their cycles (projectiles that fly).  Grid-Based map with enemies that you fight that have predictable (read learnable) AI’s so that you can anticipate their movements/actions and buffer a stack of actions that will defeat them. Kill the enemies and exit the level to advance to the next. How many levels can you clear?  

Source Files: ..................Dropbox link of zip of project (eliminate unnecessary files, such as Library, Temp, and executables.)

Video Link: ...................A link to location to download a overview video

//Game Features//
Turn Based Mechanics:
-Player has limited time to create a queue of a number of set moves
-Then the game plays out each move
-Repeat until level won or player dead

Player:
-Moves in 4 directions
-Melee Attack
-Range Attack
-Keyboard Controls for up, down, left, right, melee attack, ranged attack

Enemies:
-Ranged enemy: Tries to keep distance from player, ranged attacks
-Melee enemy: Tries to run up to player, melee attacks if close to player

Level Generation:
-level generated randomly with more enemies as level gets higher

Animations/Pixel Art:
-Sprites: projectile, wall, blocks, floor tiles, health potion, UI icons, etc.
-Animations: idle, attack, hit for player and enemies
-Very time consuming A+ art created using the image editor GIMP.

UI:
-Intuitive interface to see the move queue
-Health globe
-Looks nice
-Auto-scales to different sizes on different resolutions

Other:
-Object oriented in a way to easily add other enemies or bosses into a level
-Move queue can be specified to different values (like 1-10) and the UI will adjust accordingly
-Distributed/tested among family and friends(12 people total) and feedback/comments utilized


External resources:
Used tutorial from unity3d.com/learn/tutorials/projects/2d-roguelike-tutorial to help setup the game, board and placing objects.
