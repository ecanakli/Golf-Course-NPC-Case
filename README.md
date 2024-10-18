# Golf-Course-NPC-Case
Unity 3D Developer Demo Task

<b>Overview of the Game Mechanics</b>
The game is set on a terrain map where an NPC character is tasked with collecting golf balls scattered across the map. The game revolves around the NPC navigating through the environment using NavMesh-based movement, ensuring valid paths for the NPC. Golf balls are distributed randomly at the start of the game, and their positions are validated to be accessible via NavMesh.

The player's control over the game is limited to setting key parameters through the Settings Panel in the main menu. From this panel, the player can adjust the following:

<b>Max Health:</b> Sets the NPC's total health.
<b>Health Decrease Rate:</b> Controls how fast the NPC’s health decreases over time.
<b>Max Ball Count:</b> Specifies the total number of golf balls on the map.
The objective of the NPC is to collect as many golf balls as possible and return them to the golf cart. The game ends when all the balls are collected or the NPC's health reaches zero.

<b>Decision-Making Algorithm</b>
The core of the NPC’s behavior is governed by a decision-making algorithm implemented in the NPCController class. This algorithm dynamically determines which ball the NPC should collect next, considering several factors:

Health-Based Decisions:
The NPC's current health influences how it weighs the time vs. reward trade-off. If the NPC has high health, it prioritizes high-value balls, even if they are farther away. However, with low health, it focuses on closer and easier-to-reach balls, regardless of their point value.

Distance Calculation Using NavMesh:
The NPC uses the NavMesh system to calculate the distance between its position, the ball’s position, and the golf cart’s location. This ensures that only valid paths are used in decision-making.

Dynamic Scoring System:
Each ball has a point value based on its level (Level 1, 2, or 3). The NPC normalizes these values to account for varying distances and adjusts the weight given to time and points using the following formula:

Dynamic Weight Calculation:
When health is high, the NPC prioritizes points by increasing the point weight.
When health is low, time becomes critical, so the time weight increases.
This dynamic scoring ensures that the NPC's behavior adapts based on its current state, attempting to maximize the score efficiently within its health constraints.

Ball Colors Based On Level:
Level 1: Black Golf Ball
Level 2: Silver Golf Ball(White):
Level 3: Gold Golf Ball

Time Constraints:
The algorithm calculates the total time required to collect a ball and return it to the golf cart. If the estimated time exceeds the remaining time based on the NPC's health, that ball is excluded from consideration.

The decision-making process guarantees that the NPC focuses on the most optimal choice, balancing time, points, and health.

Game End Conditions
The game ends when one of the following conditions is met:

All Balls Collected:
The NPC successfully collects and deposits all the golf balls in the cart.
NPC Health Reaches Zero:
The NPC runs out of health, triggering the OnFail event.
The NPCController monitors these conditions and ensures appropriate transitions between states. If the NPC completes its task successfully, the OnSuccessful event is invoked.

Settings Panel Functionality
The Settings Panel in the main menu provides the player with control over key gameplay parameters:

Max Health: Adjusts the NPC's maximum health between 50 and 500.
Health Decrease Rate: Allows the player to modify the health decay rate, ranging from 0.1 to 5 per second.
Max Ball Count: Sets the total number of balls on the terrain, with values between 10 and 200.
