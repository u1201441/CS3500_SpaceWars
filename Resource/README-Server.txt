/**********************************************************************
 * 
 * SpaceWars Server
 * 
 * Created: 2018-11-24
 * 
 * Authors: Chris Jones (u0980906), John Jacobson (u1201441)
 * 
 * 
**********************************************************************/


Implementation Notes:
	All required functionality implemented.
	Prior to starting the server, check the settings.xml file to customize the game to your liking.
	Afterwards, simply open the server, and connect a client to the relevant address.
	Fight other ships, and avoid running into stars on the map while fighting against their natural gravity. Each kill on an enemy ship will yield a point, fight to get the most points on the server!

	NEW MODE: Introducing "Boss Mode", where you can now team up with your friends to take on the big, bad boss star! Simply change the "enabled" setting in the settings file to "1" to enable this mode. Once enabled, all players must band together (friendly-fire is disabled) to fight the boss star! Dodge the stars attacks and gun it down through 3 phases to achieve victory! Be aware that the boss will continue to respawn after being defeated, so don't let your guard down!
		
		Note: Depending on how the client handles displaying stars, "dead" bosses may or may not dissapear. The star is temporarily removed from the world during its respawn timer, but the professors client does not seem to draw stars with incoming data (though it does manage their presence in the world correctly.) This means that any star created will continue to draw in the position it last received a location even if it is no longer being sent. We could possibly move stars out of the screens boundaries during respawn to resolve this visual bug, but as of this posting we have not taken the time to do so. Also, this has not been implemented for multiple stars yet. We would need to move some variables into the star class and create multiple accompanying methods, so for now there is only ONE boss star. :)
	

Problems:
	Main issue during build was initial network testing. We attempted to fully implement network communication, and test it, prior to implementing any of the model, but found it difficult to test. We were able to verify that the network code was working by testing with the "FancyChatClient" to see that our code received and sent data, but the professor's client would not send any commands to out server unless we were constantly sending data to the client. We were never able to determine why this is the case, but since our server intends to constantly be sending data to the client it does not affect operation of the server under normal conditions. Our client seems very inefficient, and lags with fairly low amount of objects being drawn compared to the professors client. Unfortunately this means Boss Mode does not run well on our own client.


/*************************** CHANGE LOG ***************************/
Note: USER was driver at time of noted changes, partner programming was utilized at all times.

DATE		USER		NOTES

2018-12-06	u0980906	Implmented unit tests for all world methods and all applicable ship, projectile, and star methods.
2018-12-06	u1201441	Primarily refactoring. Added documentation to all methods, made some changes to structure or readability. Added functionality for boss mode and processing multiple stars.
2018-12-05	u0980906	Removed extra send methods, removed extra background for testing
2018-12-03	u1201441	Added mechanics for star death and respawning in boss mode. Updated README file. Did some testing on client functionality and server settings.
2018-12-02	u0980906	Implemented settings file, fixed rare crash with client disconnet, fixed crash with dead ship score updates, added locks to world storage to prevent crashes. 
2018-12-02	u1201441	Added "Boss Mode"mechanics, including modifications to collision methods and star properties. Also locked (and changed existing locks) on changes to main data structures.
2018-12-01	u0980906	Added collision physics, added ship respawning, wrote tests for world, fixed bug with thrust visuals, fixed bug with dead ships receiving user inputs, added error checking for rare disconnection bug.
2018-12-01	u1201441	Added motion physics (velocity and acceleration for both ships and projectiles), fixed some bugs with projectile spawning and despawning, implemented fire delay and ship wraparound when out of world bounds.
2018-11-29	u1201441	Fixed client-disconnect crashes (so that server gracefully handles exiting clients), added busy loop to stall world updates until first client is connected and world is initialized, set up world tick-rate and world update method to send all existing objects to client, imyplemented unit-movement and rotations, as well as implementing all user-input commands (so that ships turn, fire, and thrust on command in client.) 
2018-11-29	u0980906	Implemented the SpawnProjectile method, added a command queue for ships to allow for frame-based inputs, implmented initial "update world" loop with frame timer, fixed bug where server would not remove values from string buffer.
2018-11-29	u1201441	"Completed" network/server communication methods (one unidentified bug still exists,) and added methods for processing user input.
2018-11-24	u0980906	Fixed start-up error with reference to Resource file. Initial implementation of Server's main method, constructor, and infinitely looping data sending.
2018-11-24	u1201441	Created new Server project/class. Added new methods to Network for handling incoming connections, and implemented an initial "ReadName" delegate for the initial server handshake, as well as a stub for the final callback delegate.











/*
	Assignment description
*/

Problem Set 8 - SpaceWars Server

The SpaceWars project is a success! The new client has dramatically lifted the profile of our venture capitalist's startup. The server is now starting to show its age and needs improvements, including adding more interesting game play elements. You are tasked with completing the server in two one-week "sprints" (more on this below).
The Assignment

In your repository, add a new project called Server to the SpaceWars solution. This should be a console application.

Your job is to build a program (i.e., the server) which will allow connections from multiple clients and manage the rules of the game. As always, make sure to read all the specifications here and to pay attention to updates that come via lecture.

Below there will be numerous notes about "A good solution will..." or "An excellent solution will...". Your team is not required to implement every one of these, but for a top score you should work toward as many as possible and you should document which elements you complete in your README.
The Game Experience

Writing a game is hard work. Making a game fun is even more hard work. The basic rules of SpaceWars are well defined, but the small "tunable" characteristics of the game can make or break it. For example: the respawn rate of ships. If it's too frequent, there are no consequences of getting destroyed. If it's too slow, the game is boring.

This tuning will all take place in the server (PS8), not in the client.

For excellent work on this project, your team must construct your game engine so that it supports the basic requirements, and also makes it easy to tune parameters that determine how the game plays. You should also detail game play design decisions in your README.
Partners

You must again work on this project with a partner. Additionally, you are required to use the pair programming technique. 

Unless there is a documented partnership collapse, you and your partner will receive identical grades on this assignment. If you have problems with your partner, please notify us immediately.
Repository and Team Declaration

We will of course store your code in a GitHub repository as usual. Additionally: before the due date you should use the submission area on this web page (see below) to hand in a plain text file called "partnership".
Partnership Documentation

The partnership file described below should be handed in via canvas on this assignment. Name this file: partnership_recap.

The partnership documentation is used for two purposes. First to make it easy for us to identify who worked together (and to make sure both students realize they are working together). Second, to comment on anything we should know about how well the partnership worked (or didn't work).

The partnership file should contain the following information in the following format:

Partner1 Name  // P1: Daniel Kopta   (please put in alphabetical order by First name)
Partner2 Name  // P2: Peter Jensen

Repository     // Repository: URL to GitHub repository
Contribution   // My Contribution:  50%  (or 20% → I didn't do much, or 80% → I did almost all...)

Two to three paragraphs on the experience.  Consider: 1) How well the partnership worked.
2) Which techniques worked well, which did not.  3) What was the division of work on the
project
  

Both members of a partnership must submit their own individual partnership file.

Partners are free to (encouraged to) discuss all aspects of the project, all code, work together at the same time, etc.
README Requirements

Your project README should document all of your design decisions, as well as detailing any features you wish the graders to be aware of.

This file will be the "first stop" when your work is being evaluated. Set the tone by doing a good job describing what works and doesn't work, as well as listing interesting things and features that you would like the graders to be aware of.

You should make sure to document any parts of the program that are not working properly, any efforts you have made to remedy the problem, and what partial code you have.
Program Requirements
The Client

Unless otherwise agreed upon by the entire class, you should not modify your client in any way to adjust to your server functionality.

One of the requirements below is to add a new game mode in the game. Your new game mode must be designed in a way that any client can successfully play this new mode without any modifications.
The Server

The server maintains the state of the world, computes all game mechanics, and communicates to the clients what is going on. The client's only job is to display the state of the world to the player and send requests to the server. You should strive to make your server efficient (without going overboard), modular (i.e., easily tested), and re-use as much of your SpaceWars model and networking code as possible.
Structure Requirements

The program should continue to be structured following an MVC (Model View Controller) architecture, and use separation of concerns. Note that this includes PS7 for a complete picture, but you will not need to redo the PS7 parts!

    Model - This should store the "world" (ships, projectiles, and stars), and implement the game mechanics. You should build on top of your existing model from PS7. This will involve adding new functionality such as moving objects, and collision detection. These new updates to the model will only be used by the server, and the old parts of the model should continue to support the client from PS7.

    NetworkController - This should store any socket code. Reuse your existing network code from PS7, and add any server-related functionality to it.

    Resources - This should contain your server settings file, README, any sprite images, etc. The images will probably not change from PS7, but the README should.

    View - This will contain your GUI code. This will most likely not change from PS7.

    Server - This code should implement the game mechanics and update the clients.

Note: the Resources project should not be set to compile into anything.

For more info, see below.
Models

The models represent the game logic and state for SpaceWars. They include the World and the Ship, Projectile, and Star classes. The World is responsible for storing the current state of the game, including the status and location of every object in the game. In your client (PS7), all of the game state came from the server. In PS8, you will be adding to your existing Model classes to create and manipulate that state directly, and send it to the clients.

It should be clear that the World, Ship, Projectile, and Star models can be used by both the client and the server. It should also be clear that Server's needs are more than what the client needed. The client was "passive", and simply storing and drawing state. The server is "active", creating and modifying the state.
The Model Classes

The world needs at least the following new functionality. Please note that some of the functionality will be optional for those who are looking for an "excellent" category grade. Good solutions will not require it.

When I refer to a "Vector" below, I am referring to the Vector2D class provided in PS7. When I refer to a "unit" below, I am referring to one unit of the world's size. In other words, if the world is 750 x 750, then it is 750 "units" wide and 750 units high. Think of them as meters if you wish.

Modifications for the Server
Basic Data

    Starting hit points - How many "hit points" do ships start with? The default is 5.
    Projectile speed - How fast do projectiles travel? The default is 15 units per frame.
    Engine strength - How much acceleration do the ships' engines apply? The default is 0.08 units per frame.
    Turning rate - How many degrees can a ship rotate per frame? The default is 2 degrees.
    Ship size - How much area does a ship occupy? The default is a circle with radius 20 units. Note: this is not necessarily tied to the visual size of a ship displayed by the client. This is used for the purposes of detecting collisions with projectiles. If a projectile comes within 20 units of a ship, it hits.
    Star size - How much area does a star occupy? The default is a circle with radius 35. This is used for detecting collisions with other objects. If an object comes within 35 units of a star, it hits.

The above items are all hard-coded by the sample server, but a better solution would make them settings in an xml file. This would be fairly trivial to implement.

The below items are modifiable settings in the sample server xml file, and must be modifiable in your solution.

    Universe size - The number of units on each side of the square universe. The default is 750.
    Time per frame - How often does the server attempt to update the world? The default is 16 milliseconds.
    Projectile firing delay - How many frames must a ship wait between firing projectiles? The default is 6 frames. This is not in units of time. It is in units of frames.
    Respawn delay - How many frames must a ship wait before respawning? The default is 300 frames. This is not in units of time. It is in units of frames.

Mechanics

    Motion - Every object except for stars can potentially move on every frame. An object at rest should not move unless acted upon by a force. An object in motion should retain that motion unless acted upon by a force. There are two forces in SpaceWars: gravity and engine thrust. See the "Motion" section below.
    Wraparound - Ships that reach one edge of the world should be "teleported" to the opposite edge. This does not apply to projectiles.
    Collision - The server should detect when a projectile collides with a ship or star, and when a ship collides with a star. See "Ship size" and "Star size" above.
    Command requests - Clients can request the following actions for their player: rotate left, rotate right, fire projectile, engine thrust. The server should make every effort to comply and apply these commands fairly to every client. The commands should be applied on the next frame after receiving them. If the server receives multiple identical or conflicting commands from the same client within one frame window, such as (L) (L) or (L) (R), the server should arbitrarily pick one of them to apply.
    New player / Respawn - The server can add new players to the world at any time, and existing players can respawn at any time. Your world model should provide functionality for finding an unoccupied part of the world to add the new ship. It should try to avoid creating a ship that is about to run into a star. New ship locations and directions should be random, to the extent allowed by the above constraint. Note that the sample client always creates ships facing "up" (the Vector 0, -1). This can be easily improved.
    Cleanup - Projectiles that get too far away from the center of the world should be destroyed. The sample server uses a radius, but your server can use simple square bounds detection.
    Scoring - When a ship is destroyed due to a projectile, the ship that fired that projectile gains one score point.

Objects

The server must track all necessary information about objects to send a complete JSON string to the clients. See the section titled "JSON (JavaScript Object Notation)" from PS7 for a full list of the information that the server must send to clients for each type of object.

Your server should contain all necessary helper methods and data to enable the above functionality, and send the world to all clients on every frame.
Motion

See Lecture 22 for more details on motion and vectors.

In this section, I refer to vectors with values using angle brackets, such as <x, y>. I refer to mathematical operations on vectors using terms like "add" and "subtract". These operators are defined in the provided Vector2D class. The terms "location", "velocity", "acceleration", "orientation" all refer to Vector2D objects.

On each frame, the server should update every object's location (except for stars) based on its current velocity, and it should update its current velocity based on its instantaneous acceleration. This is described below.

Ships

Every ship should be created with an initial velocity of <0, 0>, and an arbitrary unit-length (normalized) orientation vector. On each frame, update the ship's motion:

    Compute the sum total instantaneous acceleration applied to the ship caused by stars and engine thrust.
    Add this acceleration to the velocity.
    Add the velocity to the location.

To compute the acceleration caused by a star:

1. Find the vector from the ship to the star:

Vector2D g = star.Location - ship.Location;

2. Turn the vector in to a unit-length direction by normalizing it:

g.Normalize();

3. Adjust the "strength" (length) of the vector by multiplying by the star's mass:

g = g * star.Mass;

To compute the acceleration caused by engine thrust:

1. Start with the ship's orientation vector. This vector should already be normalized if your server is implemented properly. As long as it is initially normalized, and the only operation ever applied to it is "Rotate", then it will always be normalized.

Vector2D t = new Vector2D(ship.Orientation);

Important: Do not modify the ship's copy of orientation when computing the engine thrust. This is why the above code creates a copy with "new".

2. Adjust the length of the vector by multiplying by the engine strength:

t = t * EngineStrength;

To compute the total acceleration, add the acceleration caused by all stars to the acceleration caused by engine thrust.

To update a ship's orientation when the client sends an "L" or "R" command, simply rotate the orientation vector by the desired number of degrees, positive for "R" (clockwise), or negative for "L" (counterclockwise). This assumes the orientation vector is already normalized (which it should be).

Projectiles

Projectiles have a constant velocity (no acceleration). Projectiles are created with a velocity in the direction of the ship's orientation, with length equal to server's definition of projectile speed (default of 15).

On each frame, update the projectile's motion:

    Add the constant velocity to the location.

The Server

The server is a standalone program that can run on a separate machine from any client. The server program contains a world, and uses appropriate methods to keep it up-to-date on every frame. It is up to the server to determine how often frames "tick". The server's required functionality is as follows:

    Read an XML settings file to determine the rules and settings of the game. At a minimum, this file should contain the required settings specified in the "Data" section above. See the provided "settings.xml" in the sample server. Excellent solutions for full credit will add additional interesting settings, but these settings must constitute a SpaceWars game that any existing client can play (see below for examples).
    Start an event-loop that listens for TCP socket connections from clients. When a new client connects, the server must implement the server side of the handshake in the communication protocol described in PS7. After receiving the player name from the client, the server should create a new Ship for the player, and send the client a unique player ID and the world size. After this point, the server begins sending the new client the world state on each frame. The server must allow for additional clients to connect at any time, and add them to its list of clients.
    Start a thread that infinitely loops and updates the world every time a new frame must be computed. Each iteration, the server should wait the appropriate amount of time since finishing the previous frame, then apply all the necessary updates to the world, and then send this new world to every client connected. See the protocol in PS7 for sending the world state. Note: do not use a System.Timer for this. See Lecture 22.
    Handle disconnects. When a client disconnects, the server must handle it gracefully, i.e. not crash or get in a corrupted state. The server should stop trying to send data to the disconnected client on each frame.
    Since all of the server functionality is handled by separate threads, you will need something to prevent the Main thread from immediately exiting after creating those threads, such as a Console.Read() statement.

Network Controller

The game logic of this program should take you about a week (give or take) to get mostly working. The network server code should take less time than this.

The networking code needs to be updated to create a server listening event loop. The entire point of this loop is to listen for clients which want to connect and then to give them their own socket and specific listening thread.

You should continue to program in your NetworkController project. Some of the code, such as receiving and sending messages, should remain basically the same.

Here is a list of new required methods:

    void ServerAwaitingClientLoop(callMe)

    This is the heart of the server code. It should start a TcpListener for new connections and pass the listener, along with the callMe delegate, to BeginAcceptSocket as the state parameter. Note: you should create a new type of state class to hold a TcpListener plus the delegate. This is not the same as a SocketState. Upon a connection request coming in the OS should invoke the AcceptNewClient as the callback method (see below).

    Although this method is called a "Loop", it is not a traditional loop, but an "event loop" (i.e., this method sets up the connection listener, which, when a connection occurs, continues listening for another connection).
    void AcceptNewClient(IAsyncResult ar)

    This is the callback that BeginAcceptSocket should use. This code will be invoked by the OS when a connection request comes in. It should:
            Extract the state containing the TcpListener and the callMe delegate from "ar"
            Create a new socket by using listener.EndAcceptSocket
            Save the socket in a new SocketState
            Call the callMe method and pass it the new SocketState. This is how your server will get a copy of the SocketState corresponding to this client. Your callMe method (from the server) should save the SocketState in a list of clients.
            Await a new connection request (continue the event loop) with BeginAcceptSocket. Note that this means the networking code assumes we want to always continue the event loop. This is a little different from the other networking callbacks you wrote in PS7. This is OK, because the name of the entry method (1. above) implies that it is a loop.

Network Protocol

The protocol for communication is the same as described in PS7. You will have to implement the server's end of that protocol. See the serverdiagram

. It can be summarized as:

    Wait for clients to connect on port number 11000.
    Upon connection, wait for a single '\n' terminated string representing the player's name.
    Upon receiving the name, send the startup data. The server must not send any world data to a client before sending the startup data.
    Continually send the current state of the world to the client.
    Continually listen on the socket for command requests from the client.

Carefully re-read the networking protocol from PS7 - the above is just a summary! If you are uncertain about any part of the protocol, examine the data sent by the sample server, and slow it down so you can see it.

Communication Protocol Errors

If bad data comes from the client that does not match the protocol, you should terminate the connection. Should the client terminate the connection (bytes received == 0) the server should gracefully handle this situation by removing the player socket from the list of connected sockets. The player's ship should be destroyed when the player disconnects.

Gracefully handling disconnects should be one of the last things you do. For a good grade it is enough to stop sending packets to a closed socket. An excellent solution will clean up the list of connections as soon as a send or receive fails.
Server Example

Any functionality that is present in the sample server should be preserved even if not mentioned in this specification. That being said, if there is functionality you wish to modify as part of your additional game mode, you may do so but must document it in the README. Any additional features or functionality should be enabled with a setting in the XML file.
Locking

It should go without saying, but there will be many threads all accessing the same world object at the same time, as well as multiple threads accessing a "list" of connected sockets. Any code that accesses shared data needs to be protected by a lock.
View

As stated above, the Client GUI should not be changed for this project.
JSON (JavaScript Object Notation)

In your client you are deserializing your objects. The server will need to use the symmetric serialize code.
Testing

Your Model should be unit tested to as close to 100% of code coverage as possible. You should document your testing strategy in your README (a single paragraph detailing where you focused your testing efforts and how effective you believe you were). You may be thinking "how can we unit test code that is driven by network communication?" The better you separate concerns, the easier unit testing will be.

I also suggest you work with another team to test their client on your server and vice versa.
Extras

To receive full credit, your server will need to implement an extra game mode. This mode should have additional game mechanics that are not possible by just tweaking the default server settings. For example, an extra mode might be:

    Moving stars - Stars apply gravity to eachother, or stars are pushed when hit by projectiles, ...
    Mass projectiles - Projectiles are affected by gravity
    Team mode - Designate certain players to be on a team with eachother; their projectiles don't harm one another.
    ... use your imagination!

There are many other possibilities such as these that don't require modifying the client.

Your extra mode should be enabled with a setting in the settings.xml file, and documented in your README.
Sprint 1

Modern work environments often split work into "sprints". Sprints are usually 1-3 weeks of labor to accomplish a "deliverable". I suggest you break this project into two one-week sprints.

Over the first week, I suggest you:

    Get the networking code working such that you can have multiple copies of your client connecting to the server. At the least you should be able to handle a single client.
    Have your server create random ship/projectile/star objects and be able to send them across the network.
    Have your server code accept the engine thrust requests and move the ship in a constant direction one unit.
    Write and/or augment basic test codes for the model.
    Ignore most of the game mechanics, such as motion and collision (except the hacky motion suggested in step 3 above)

Sprint 2

During the second week of the project I suggest you complete the main game mechanics of the project in an orderly fashion:

    Implement motion; get engine thrust with no stars working first, then get gravity with stars working.
    Implement collision detection and projectile cleanup.
    Implement the remaining command requests.
    Add an extra game mode feature.
    Clean up and polish code, add any documentation that you forgot to add while coding. Fix bugs, test, etc.

Tips

    At this point, it should be clear that you are only working on the SERVER. The client is already done. Use your client (or the sample clients) to test your server. 
    Both the server and the client will contain their own world. They are not the same instance of the world! They can't be the same instance, since they are potentially running on different computers, but they will have the same class type. The client and server worlds will contain mostly the same data. The client's world is "passive", and is strictly received from the server. The server's world is "active" and updated with game mechanics logic. This means that the server will use many world methods that the client will never need to invoke.
    The server will likely contain methods such as (not necessarily with these names):
        Main - set everything up and start an event loop for accepting connections, and start the frame loop (this one is not an event loop).
        Handle a new client - this is the delegate callback passed to the networking class to handle a new client connecting. Change the callback for the socket state to a new method that receives the player's name, then ask for data.
        Receive player name - this is a "callMe" delegate that implements the server's part of the initial handshake. Make a new Ship with the given name and a new unique ID, and set the SocketState's ID. Then change the callback to a method that handles command requests from the client. Then send the startup info to the client (ID and world size). Then add the client's socket to a list of all clients. Then ask the client for data. Note: it is important that the server sends the startup info before adding the client to the list of all clients. This guarantees that the startup info is sent before any world info. Remember that the server is running a loop on a separate thread that may send world info to the list of clients at any time.
        Handle data from client - this is a callMe delegate for processing client direction commands. Process the command, then ask for more data. Note: In order to know which client the request came from, the SocketState must contain the player's ID. This is what the ID is for in the example socket state class.
        Update - this is the method invoked every iteration through the frame loop. Update the world, then send it to each client.

Misc Game Expectations

Note: Our VC may continue to identify issues and features for the game. Students are responsible for re-checking the project requirements before the due date. Note: it is unlikely that major changes will take place, but you never know what marketing will demand goes into the project.

We will try to keep a summary of the important ones below:

    You are free to modify the signatures of the Networking methods in a minor way to help support handling disconnected clients gracefully. These modifications should not be SpaceWars specific.

Rubric
PS8 - CS3500 - Fall 2017
PS8 - CS3500 - Fall 2017
Criteria 	Ratings 	Pts
This criterion is linked to a Learning Outcome Partnership Documentation
Did you submit the required partnership information and was your analysis insightful?
	
10.0 pts
Full Marks
	
5.0 pts
Partial
	
0.0 pts
No Marks
	
10.0 pts
This criterion is linked to a Learning Outcome Model Unit Tests
Your model code (the world) should have unit tests where reasonable. You should be able to exercise most of this code without using the real entry points (like networking code).
	
10.0 pts
Full Marks
	
0.0 pts
No Marks
	
10.0 pts
This criterion is linked to a Learning Outcome Software Engineering
Did you follow good software engineering guidelines? Is your README informative and discusses design decisions? Is your code well-commented? Did you use appropriate separation of concerns, and write modular code?
	
30.0 pts
Full Marks
	
0.0 pts
No Marks
	
30.0 pts
This criterion is linked to a Learning Outcome Networking Library
Your networking code should be self contained in a general purpose, standalone library.
	
10.0 pts
Full Marks
	
0.0 pts
No Marks
	
10.0 pts
This criterion is linked to a Learning Outcome Basic Functionality
Does your server allow multiple clients to connect and play the game? Does it update the world properly, and send the correct data to each client? Does it accept and process command requests correctly?
	
65.0 pts
Full Marks
	
0.0 pts
No Marks
	
65.0 pts
This criterion is linked to a Learning Outcome Settings File
Can your server correctly load a default settings file and use those settings?
	
10.0 pts
Full Marks
	
0.0 pts
No Marks
	
10.0 pts
This criterion is linked to a Learning Outcome Extra Game Mode
Does your server implement an additional game mode that existing clients can play?
	
10.0 pts
Full Marks
	
0.0 pts
No Marks
	
10.0 pts
This criterion is linked to a Learning Outcome Stability, Asynchronous Networking
Does your program properly handle race conditions and the unpredictable nature of asynchronous networking?
	
40.0 pts
Full Marks
	
0.0 pts
No Marks
	
40.0 pts
This criterion is linked to a Learning Outcome Graceful Disconnect
Does your program correctly handle clients connecting and disconnecting at any time during the lifetime of the server?
	
15.0 pts
Full Marks
	
0.0 pts
No Marks
	
15.0 pts
Total Points: 200.0