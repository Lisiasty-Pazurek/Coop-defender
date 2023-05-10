# Wild-West
Practice coop twin stick shooter, 5 day marathon
Using Mirror networking, Light Reflective Mirror relay natpunch and LRM lobby.

## Requirements/ User case scenario

#### Lobby
One player hosts the game.
Another player can join the hosted game.
After joining, a timer appears and the gameplay begins.

#### Enemies (NPCs):
Spawn at regular intervals (modifiable in the inspector).
Move and shoot at the closest player. The range of their shots, their speed, health, and damage dealt are all modifiable in the inspector (these variables do not necessarily have to be in one script or on one object).
There cannot be more than 10 enemies on the screen at once (modifiable in the inspector).

#### Players:
Move using WSAD/arrow keys with a modifiable speed in the inspector.
Fire a bullet towards the cursor when the left mouse button is clicked. The damage dealt, firing rate, bullet lifespan, and speed are all modifiable in the inspector (these variables do not necessarily have to be in one script or on one object).
Have 5 health points (modifiable in the inspector).
Do not deal damage to each other.
Earn points by killing NPC enemies.

#### Gameplay
The entire game should be physics-based, with players, enemies, bullets (and any obstacles) colliding/interacting with each other (e.g. players can push each other around).
The goal of the game is to earn as many points as possible in 3 minutes (modifiable in the inspector).
Important information such as remaining time and points earned should be displayed on the UI.
After the game ends, a summary of the players' scores should be displayed, along with a button to start a new game. The game will start again when both players press the button (the readiness state of each player should be shown in the summary).
