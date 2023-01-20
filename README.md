# Distributed Spanning Forest
![Demo GIF](demo.gif)

This project is an implementation of the article "Maintaining a Distributed Spanning Forest in Highly Dynamic Networks" by Matthieu Barjon, Arnaud Casteigts, Serge Chaumette,
Colette Johnen, and Yessin M. Neggaz.

The goal of this project is to emulate the performance of a group of UAVs through the usage of the distributed spanning forest algorithm, which aims to maintain a connected network despite node failures and new nodes joining the network.

## Getting Started
These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

## Prerequisites
Unity version 2021.3.15 or higher
Visual Studio or any other code editor

## Installing
1. Clone the repository to your local machine
    ```
    git clone https://github.com/ypavie/Distributed-Spanning-Forest
    ```
2. Open the project in Unity
3. Open the "UAV_Behaviour" script and adjust the parameters in the "Start" method to match your desired simulation
5. Press the play button to start the simulation

## Running the simulation
The simulation starts with a random number of UAVs (Unmanned Aerial Vehicles) being spawned in the scene. Each UAV is assigned a unique ID and starts as a single root (red color) with a token. The UAVs move randomly in the scene and broadcast their status and ID to their neighbors.
The UAVs use the information received to maintain a distributed spanning forest in the network. When a UAV loses its token, it becomes a leaf in the tree (black color) and the token is passed to one of its children. The tree structure is dynamic and adapts to the changes in the network.
The logic for the simulation is implemented in the "Ground" object, which can be found in the scene hierarchy. Simulation parameters can be adjusted in the "Ground" object's inspector. The relationship between the UAVs, as shown in the gif above, are only visualized in the scene and are not reflected in-game.

## Built With
- Unity
- C#

## Authors
* Yann Pavie 
* Axel Jacotey https://github.com/35000axel

## License
This project is licensed under the MIT License

## References
Matthieu Barjon, Arnaud Casteigts, Serge Chaumette, Colette Johnen, Yessin M Neggaz. Maintaining
a Distributed Spanning Forest in Highly Dynamic Networks. The Computer Journal, 2019