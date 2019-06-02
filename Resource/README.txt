/**********************************************************************
 * 
 * SpaceWars Client
 * 
 * Created: 2018-11-03
 * 
 * Authors: Chris Jones (u0980906), John Jacobson (u1201441)
 * 
 * 
**********************************************************************/


Implementation Notes:
	All required functionality implemented.
	Enter a server name and player name, then press the "Enter Server" button. (Try using the name "Mario" for a fun easter egg!)
	Use [Left]/[Right]/[Up] (or [T] instead of [Up]) arrow keys to control your ship, and [Space] or [F] to fire projectiles.
	Added artwork for multiple stars.
	

Problems:
	Attempted to implement explosions, but were unable to get animations working correctly within assignment deadline. Attempted to set up a timer to render animations at 60 FPS (or any FPS), but couldn't figure out how to implement in the existing form easily.
	Has some issues with adding background images as they were causing other controls not to draw, and adding display lag. Resizing the image before setting it seemed to resolve this.

Note:
	All included artwork was publicly available and free for non-commercial use. 



/*************************** CHANGE LOG ***************************/
Note: USER was driver at time of noted changes, partner programming was utilized at all times.

DATE		USER		NOTES

2018-11-15	u1201441	Implemented sorting of players on scoreboard and began implementation of ship explodions.
2018-11-15	u0980906	Completed form control implementation, fixed network communication with TA help, added documentation across entire solution, added some error handling.
2018-11-14	u0980906	Adjustments to projectiles and removing them as necessary. Fixed multiple bugs, including window resize crash and race conditions in network communication delegate.
2018-11-14	u1201441	Completed implementation of object geometry, and modified positioning and placements of form elements. Began implementation of adding controls to form for player names, health, score.
2018-11-10	u0980906	Implemented Form, added locks, tuning to view sizes and positioning to get to a testable state.
2018-11-10	u1201441	Created accessor methods and related documentations for model objects, and implemented image loading and storage. Got program communicating with network and drawing on screen.
2018-11-08	u0980906	Began implementation of game drawing panel.
2018-11-08	u1201441	Modified data structures used in the model.
2018-11-07	u0980906	Finished initial implementation of game controller.
2018-11-07	u1201441	Finished initial implementation of network controller, began game controller implementation.
2018-11-03	u0980906	Began network implementation.
2018-11-03	u1201441	Created skeleton solution, synced to GIT. Created initial file structure, documentation, method stubs.













Descriptions:

	The Game

	SpaceWars can be described succinctly as:

		Each player controls a ship that can fly around freely in space and fire projectiles.
		If the ship reaches the edge of space, it wraps around to the other side. Projectiles do not warp around.
		The space can include zero or more stars, which apply a gravitational force to every ship. If a ship impacts a star, it is destroyed.
		A ship can sustain 5 projectile hits before destruction.
		A destroyed ship will respawn randomly after a certain amount of time.
		The player that fired the last projectile that destroys a ship gains one score point.

	There are a lot of details in this assignment. Read it all first. Then discuss it with your partner. Then read it again. Then begin.

	If these specifications aren't clear, use the provided executables to gain an understanding of how the game works.
	The Assignment

	We will start by remaking the client only.

	We will test our client by connecting to the existing server executable. Once we have made a new client from scratch, we will then make a new server in PS8. If you run the server on your machine, you can connect to it at the address "localhost".

	Create a solution called SpaceWars in VS and upload it to your team's GitHub repository.
	Partners

	You must work on this project with a partner, and you must use pair programming techniques as you work on the project. See PS6 for instructions on adding your partner to your github team.

	Unless there is a documented partnership collapse, you and your partner will receive identical grades on this assignment. If you have problems with your partner, please inform us as soon as the problem arises.
	Repository and Team Declaration

	You will of course store your code in a GitHub repository as usual. Additionally: fill out the partner declaration survey for the final project.

	See PS6 for instructions on giving your partner access to your repository.
	Partnership Documentation

	The partnership file described below should be handed in via canvas on this assignment. Name this file: partnership_recap

	The partnership documentation is used for two purposes: first, to make sure both students realize they are working together, second, to comment on anything we should know about how well the partnership worked (or didn't work).

	The partnership file should contain the following information in the following format:

	Partner1 Name  // P1: Daniel Kopta   (please put in alphabetical order by First name)
	Partner2 Name  // P2: Peter Jensen

	Repository     // Repository URL to GitHub repository
	Contribution   // My Contribution:  50%  (or 20% → I didn't do much, or 80% → I did almost all...)

	Two to three paragraphs on the experience. Consider: 
	1) How well the partnership worked.
	2) Which techniques worked well, which did not.
  

	Both members of a partnership must submit their own individual partnership file.

	Partners are free to (encouraged to) discuss all aspects of the project, all code, work together at the same time, etc. Partnerships are not encouraged (nor permitted) to divide the project up and work separately.
	README Requirements

	Your project README should document all of your design decisions, as well as detailing any features you wish the graders to be aware of.

	This file will be the "first stop" when your work is being evaluated. Set the tone by doing a good job describing what works and doesn't work, as well as listing interesting things (i.e., features) that you would like the graders to be aware of.
	Program Requirements
	Client/Server

	This project will revolve around a classic client/server architecture. We will make our own server in PS8, but for now we will focus on the client. The server will maintain the state of the world and communicate to the clients what is going on. The clients will display the state of the world to the player and send control requests to the server. You should strive to make your client GUI look good and easily support the player while playing the game.

	PLEASE get something basic working before adding bells and whistles!

	The server is provided for you as an executable, and your client must use the exact communication protocol as used by the server, described below. Your client must communicate with the server via sockets using the given port number described below. Again, DO NOT try to write any server code in this assignment. Your client will interact with an existing server.

	The client is mostly passive, and has three main jobs:

		Allow the user to connect to a server at a certain address, and provide the player's name.
		Draw the state of the world, as described to it by the server.
		Send control commands to the server. These are commands input by the player, such as turning or accelerating.

	The client's job is NOT:

		To make any game mechanics decisions, such as whether a projectile hits another ship.
		To anticipate the movement/update of any object in the world. Even if the client sends a control request to the server, the server is free to ignore it. The client cannot assume its requests will affect the game.

	Software Structure Requirements

	Put your client in a solution called SpaceWars.

	You must employ separation of concerns rigorously in this assignment. You are expected to use all of the same engineering/coding standards that we have expected from you throughout the semester. The program should be structured following the MVC (Model View Controller) architecture. There is no explicitly required structure for your projects, but your code must:

		Separate the model from the view and controller. For example, there should be a project that represents the "world", one that represents a ship, etc... Each project should be self-contained, and represent one "concern" of the overall system. The interfaces between individual components should be well-designed and documented.

		Avoid duplicating code. You may be tempted to write two different versions of a "world" class - one for the server, and one for the client, since they have slightly different needs. However, a single world class can encapsulate all the functionality needed for both. The client may not use all of the "world" functionality that the server does, and vice versa, and this is OK. In PS7, the model code is mostly "passive", i.e., it is populated by filling in data from the server. In PS8, you will add the game mechanics that update the model, such as moving objects, collision detection, etc.

		Resources project - This should contain your README, any graphics files (sprites), any support libraries, etc.

		View project - This will contain your GUI code. The "view" parts should be in a separate project from the logic parts. For example, your logic to parse the information received by the server should not be in the Form class. Your view code should not contain any model code, and vice versa.
		Controller project - This should contain logic for parsing the data received by the server, updating the model accordingly, and anything else you think belongs here. It is OK to put key press handlers either here or in the view.

	Note that the Resources project should not be set to compile into anything.

	To see a high-level diagram of how you may structure your Client (including how elements will communicate with each other), see the client Networking Diagram
	.
