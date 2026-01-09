# flowtrix-game
Temple runner kinda game project by Intern (Mefin)


Flowtrix is a fast-paced 3D endless runner game made with Unity. Dodge obstacles, collect coins, and see how far you can progress while running across vibrant, dynamic tracks.
________________________________________
✨ Features

●	Smooth third-person camera and character controls

●	Procedurally generated platforms with obstacles and coins

●	Coin system with score tracking and UI updates

●	Multiple obstacle types (spinning bars, swinging pendulums, etc.)

●	Simple start, pause, and restart UI

●	Optimized object pooling for coins, obstacles, and platforms

________________________________________
🚀 Installation

1.	Clone the repository.

2.	Open the project folder in Unity Hub.

3.	Ensure you are using the Unity version specified in: ProjectSettings/ProjectVersion.txt

4.	Open the MainMenu scene (located in Assets/Scenes) and click Play.

________________________________________
🎮 How to Play

●	Move Left/Right: A / D or ← / →

●	Jump: W or ↑

●	Slide: S or ↓

●	Collect coins to increase your score

●	Avoid obstacles and stay on the platforms to keep running

________________________________________
🖥️ Scripts Overview

AudioManager.cs ->	Manages audio across scenes

CameraController.cs ->	Smooth camera follow and rotation

Coin.cs & CoinManager.cs ->	Coin animation, collection, and score management

ObstacleManager.cs ->	Handles pooling and spawning of obstacles

PlatformManager.cs ->	Procedural platform spawning and recycling

PlayerController.cs ->	Player movement, input handling, lane logic, obstacle detection

ParkourController.cs ->	Handles jump/slide animations and collider changes

PendulumSwing.cs, SpinningObstacle.cs, SwingObstacle.cs ->	Controls animated obstacles (pendulums, spinning bars, etc.)

UiManager.cs, StartGameMenu.cs ->	UI handling and scene management

TurnTrigger.cs, PlayerTurnController.cs ->	Manages level turns and player rotation

