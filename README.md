# 2dGrid-hierarchical-pathfinding-astar

The goal if this project was to generate a massive map and release as many agents on it as possible.
On a Ryzen 5 3600 with 2x8GB DDR4 3600MHZ memory, ~~I'm able to generate a 32x32 map with a grid size of 14 (a little over 200,000 pathfinding nodes), and release 2000 agents that continuously find new random paths from the map. With this setup, the game is running over 60FPS with very little lagspikes.
I recommend a 24x24 map with a grid size of 14, and it should support approx. 2000 units without any lag spikes at all.~~
Update on this, pathfinding now costs so, so little that even an 80x80 grid size map (with 14 chunk size), pathfinding has ZERO impact on performance. Having 5000 AI with the said PC and map setup, I am still at 120FPS. However, all units are rendered at all time (something you the reader can optimize for sure), so going above 5000AI on my rig will impact the performance. Having 9999 AI on said map size will get me at 30FPS.

Feel free to reuse this code. There's plenty of optimizations to be done around here.

!Important pathfinder notes!<br />
The base A* implementation is all thanks to Sebastian Lague (look him up on youtube). Though his code was greatly modified for my needs.<br />
Note that I did not implement his path smoothing algorithm into my own pathfinder.<br />

As for the Hierarchical Pathfinding, big thanks to this paper ( https://webdocs.cs.ualberta.ca/~mmueller/ps/hpastar.pdf ). The implementation loosely came from this.<br />
It should've been easier to implement HPA*, and I'm pretty sure I've over engineered it to hell and beyond.<br />

And lastly, please note that this implementation works with a randomly generated map. ~~I'm also planning to extend the functionality so that the grid/clusters react to changes in the world map.~~ Done. It reacts to map change. Make sure you call the Grid.UpdateNode() function.<br />
It works, but it may take a while to generate a larger map. On the other hand, this code should be able to work with any map size (limited by your computer)<br />

!Important map generation notes!<br />
The way it works is sort of hardcoded, but it is not meant to be a final map generator, it's more like a proof of concept prototype.<br />
Short explanation is, we first generate the Pathfinder grid nodes, with default values (aka. everything is unwalkable, movement penalty is 0, etc..)
We then generate a map, and as we do so, we update the grid nodes.<br />Sebastian's original implementation relies on raycasting to figure out what tile the nodes are on.
This proved to be ineffective, as I would've had to store tile information in the map itself. This way, the generator knows the information instead, and passes it to the grid during generation. Now the map can truly act as nothing more than background.<br />
Also note that the generated mountains are hardcoded to be unwalkable. Again, the generator is only a prototype, I expect you to create your own.<br />

!How to implement/setup!<br />
(The names are not hard coded)<br />
Start a new scene<br />
Add 4 new layers (needed for the current terrain generator). Name them "Plains", "Forests", "Hills" and "Mountains".<br />
Create an empty game object called "Chunk", and make a prefab out of it, then save it somewhere. This should just be empty.<br />
Import 4 sprites to the game, preferably one for each terrain type (plains, forests, hills and mountains). Should be 32x32 pixels for each sprite. Create new GameObjects with Sprite Renderer components, and add your freshly imported sprites for each. Save the game objects as prefabs.<br />

Create an empty GameObject called "A*". Add the Grid, Pathfinding and Path Request Manager components to it.<br />
In the Grid component, set up four (4) elements in the array. Select Plains, Forest, Hills, Mountains, one for each element. Add any terrain penalty value ~~(MAKE SURE EVERY TERRAIN PENALTY IS ABOVE 0)~~<br />

Create an empty GameObject called "Map". Add the Terrain Generator, Chunk Renderer and Chunk Spotter components to it.<br />
In the Terrain Generator component, set up four (4) elements in the array. Drag in your terrain prefabs you've created earlier. For the Update UI script, drag in the Update UI component from the canvas (see below)<br />
In the Chunk Spotter component, drag your main camera into the Cam To Check box. Also drag in the Chunk prefab into the Chunk Instance box. Render Chunks should be 1-1 for starters, feel free to change it later.<br />
In the Chunk Manager component, drag in the Update UI component from the canvas (see below).<br />
Feel free to play around with the numbers. Note that high values may crash Unity.<br />

Create an empty GameObject called "Canvas". Add the Update UI component to it.<br />
In the Update UI component, drag in the below game object (see below (below)).... below.<br />
Create a new empty child (for the Canvas) GameObject, and add a TextMeshPro - Text component to it. It doesn't matter where you place the text, but expect it to say big numbers, such as "10000/10000" at (hopefully) maximum.<br />

Create an empty GameObject called "AI".<br />
Add the Unit and Path Generation components to it.<br />
Note that is Path Generation component is responsible for finding a random path to get to. The Unit component is responsible for executing the movement.<br />
Feel free to create as many duplicates of this as you want to test out. My record that suprisingly still works is 3000.<br />
