// Written by Christopher Jones and John Jacobson

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace SpaceWars
{
    /// <summary>
    /// The game server for Spacewars. This server keeps track of and informs
    /// clients about the game world, and receives user-input.
    /// </summary>
    class Server
    {
        // Instance of the world to be utilized by the server.
        int worldSize;
        World theWorld;

        // Data structures for managing the clients connected to the server.
        List<int> clientsToRemove;
        Dictionary<int, SocketState> clients;

        // Counter for maintaining unique ship id's as players are connected,  killed, or respawned
        int shipIDCounter;

        // Variables for managing world update frequency. 
        int msPerFrame;
        Stopwatch timer = new Stopwatch();

        // Data structures handling the objects within the world.
        // These will hold snapshots of the worlds objects for every frame.
        Dictionary<int, Ship> ships;
        Dictionary<int, Projectile> projectiles;
        Dictionary<int, Star> stars;

        // Data structures used for managing "Dead" objects. These aid in either removing, 
        // and/or re-adding objects to the world for respawns, as necessary.
        // These will hold snapshots of the worlds objects for every frame.
        Dictionary<int, Ship> deadShips;
        List<Projectile> deadProjs;
        Dictionary<int, Star> deadStars;

        // "Boss Mode" data.
        private bool bossModeEnabled;
        private int bossMaxHP;

        /// <summary>
        /// This method sets everything up, starts an event loop for accepting connections,
        /// and starts the frame loop.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Server svr = new Server();
            Network.ServerAwaitingClientLoop(svr.HandleNewClient); // Beging client connection loop.
            Console.WriteLine("Awaiting connections.");
            svr.timer.Start();

            // Busy loop until at least one client has connected.
            while (svr.clients.Count == 0) { }

            // Update the world until the application is terminated.
            while (true)
            {
                svr.UpdateWorld(svr);
            }
        }

        private void UpdateWorld(Server svr)
        {
            // Busy loop to maintain a "frame rate", which can be adjusted in the settings file.
            while (timer.ElapsedMilliseconds < msPerFrame)
            { }

            // Restart the timer here so that it begins counting while processing data.
            // This should allow for more consistent frame rate.
            timer.Restart();

            // Create snapshot copies of the worlds objects to parse.
            ships = (Dictionary<int, Ship>)theWorld.getPlayers();
            projectiles = (Dictionary<int, Projectile>)theWorld.getProjs();
            stars = (Dictionary<int, Star>)theWorld.getStars();
            deadShips = (Dictionary<int, Ship>)theWorld.GetDeadPlayers();
            deadStars = (Dictionary<int, Star>)theWorld.GetDeadStars();

            // How does this affect performance? Unsure...
            lock (clients) 
            {
                
                theWorld.updateShips();

                // Apply star mass to all ships for this frame.
                theWorld.ApplyStarGravity();

                // Tick all active respawn counters by one frame.
                theWorld.updateRespawnCounter();
                
                foreach (KeyValuePair<int, Ship> ship in ships)
                {
                    // Kill ships for clients that are disconnected.
                    if (clientsToRemove.Contains(ship.Value.GetID()))
                    {
                        ship.Value.Damage(5);
                        theWorld.KillShip(ship.Value);
                    }
                    // Process "fire" commands by spawning the requisite projectile
                    if (ship.Value.GetCommands().Contains('F'))
                    {
                        theWorld.SpawnProjectile(ship.Value);
                    }

                    // Process queued ship commands for this frame.
                    ship.Value.ProcessCommands();
                }

                // Update projectile physics for this frame.
                theWorld.updateProjectiles();

                // Kill projectiles
                foreach (KeyValuePair<int, Projectile> proj in projectiles)
                {
                    // Kill projetiles out of bounds
                    if (Math.Abs(proj.Value.GetLocation().GetX()) > worldSize / 2 ||
                        Math.Abs(proj.Value.GetLocation().GetY()) > worldSize / 2)
                    {
                        proj.Value.Kill();
                    }
                }

                // Detect Collision with ships and projectiles
                foreach (KeyValuePair<int, Ship> ship in ships)
                {
                    foreach (KeyValuePair<int, Projectile> proj in projectiles)
                    {
                        if (theWorld.HasCollidedShipProj(ship.Value, proj.Value))
                        {
                            if (bossModeEnabled) // in boss mode, ships can only be collided with star projectiles.
                            {
                                if (proj.Value.GetOwner() == int.MaxValue)
                                {
                                    ship.Value.Damage(1);
                                    proj.Value.Kill();

                                    if (ship.Value.GetHP() < 1)
                                    {
                                        theWorld.KillShip(ship.Value);
                                        break;
                                    }

                                }
                            }
                            else // in normal circumstances, ships are affected by all projectiles.
                            {
                                ship.Value.Damage(1);
                                proj.Value.Kill();

                                if (ship.Value.GetHP() < 1)
                                {
                                    // If-else statement to ensure that a player will score their point even if they're dead.
                                    if (ships.ContainsKey(proj.Value.GetOwner()))
                                    {
                                        ships[proj.Value.GetOwner()].PointScored();
                                    }
                                    else
                                    {
                                        deadShips[proj.Value.GetOwner()].PointScored();
                                    }

                                    theWorld.KillShip(ship.Value);
                                    break;
                                }
                            }
                        }
                    }
                }

                // Detect Collision with ships and stars
                foreach (KeyValuePair<int, Ship> ship in ships)
                {
                    foreach (KeyValuePair<int, Star> star in stars)
                    {
                        if (theWorld.HasCollidedShipStar(ship.Value, star.Value))
                        {
                            ship.Value.Damage(ship.Value.GetHP());
                            theWorld.KillShip(ship.Value);
                        }
                    }
                }

                // Detect Collision with stars and projectiles
                foreach (KeyValuePair<int, Star> star in stars)
                {
                    foreach (KeyValuePair<int, Projectile> proj in projectiles)
                    {
                        if (proj.Value.GetOwner() < int.MaxValue && theWorld.HasCollidedProjStar(star.Value, proj.Value))
                        {
                            if (bossModeEnabled)
                            {
                                star.Value.Damage(1);
                                if (star.Value.GetHP() < 1)
                                {
                                    // If-else statement to ensure that a player will score their point even if they're dead.
                                    if (ships.ContainsKey(proj.Value.GetOwner()))
                                    {
                                        ships[proj.Value.GetOwner()].PointScored();
                                        ships[proj.Value.GetOwner()].PointScored();
                                        ships[proj.Value.GetOwner()].PointScored();
                                        ships[proj.Value.GetOwner()].PointScored();
                                        ships[proj.Value.GetOwner()].PointScored();
                                    }
                                    else
                                    {
                                        deadShips[proj.Value.GetOwner()].PointScored();
                                        deadShips[proj.Value.GetOwner()].PointScored();
                                        deadShips[proj.Value.GetOwner()].PointScored();
                                        deadShips[proj.Value.GetOwner()].PointScored();
                                        deadShips[proj.Value.GetOwner()].PointScored();
                                    }
                                    theWorld.KillStar(star.Value);
                                }
                            }
                            proj.Value.Kill();
                        }
                    }
                }

                // Spawn boss mode projectiles
                if (bossModeEnabled)
                {
                    foreach (KeyValuePair<int, Star> star in stars)
                    {
                        if (star.Value.GetHP() >= 2 * (bossMaxHP / 3)) { theWorld.bossFireMode1(star.Value); }
                        else if (star.Value.GetHP() >= bossMaxHP / 3) { theWorld.bossFireMode2(star.Value); }
                        else if (star.Value.GetHP() > 0) { theWorld.bossFireMode3(star.Value); }
                    }
                }

                foreach (KeyValuePair<int, SocketState> ss in clients)
                {

                    // What happens if disconnected mid-send / mid-receive? DONT KNOW!
                    if (!ss.Value.theSocket.Connected)
                    {
                        clientsToRemove.Add(ss.Value.id);
                        continue;
                    }

                    string sendString = "";
                    // Send all ships to client.
                    foreach (KeyValuePair<int, Ship> ship in ships)
                    {
                        sendString = sendString + JsonConvert.SerializeObject(ship.Value) + "\n";
                    }
                    if (sendString.Length > 0)
                    {
                        Network.Send(ss.Value.theSocket, sendString);
                    }

                    sendString = "";
                    // Send all projectiles to client.
                    foreach (KeyValuePair<int, Projectile> proj in projectiles)
                    {
                        sendString = sendString + JsonConvert.SerializeObject(proj.Value) + "\n";
                    }
                    if (sendString.Length > 0)
                    {
                        Network.Send(ss.Value.theSocket, sendString);
                    }

                    sendString = "";
                    // Send all stars to client.
                    foreach (KeyValuePair<int, Star> star in stars)
                    {
                        sendString = sendString + JsonConvert.SerializeObject(star.Value) + "\n";
                    }
                    if (sendString.Length > 0)
                    {
                        Network.Send(ss.Value.theSocket, sendString);
                    }
                }

                // Remove disconnected clients.
                foreach (int ssId in clientsToRemove)
                {
                    clients.Remove(ssId);
                }

                // Remove dead projectiles.
                foreach (KeyValuePair<int, Projectile> proj in projectiles)
                {
                    if (!proj.Value.GetAlive())
                    {
                        theWorld.removeProjectile(proj.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Constructor for the Server.
        /// </summary>
        public Server()
        {
            theWorld = new World();
            clients = new Dictionary<int, SocketState>();
            clientsToRemove = new List<int>();
            worldSize = theWorld.GetWorldSize();
            deadProjs = new List<Projectile>();

            // Reads from the settings.xml file and initializes attributes provided therein.
            InitializeSettings();

            //fireCooldown = 6; // Need to read this from file.
            //shipIDCounter = 0;
            //theWorld.SetFireDelay(fireCooldown);

            //// Set "Boss Mode"
            //bossModeEnabled = true;
            //bossMaxHP = 100;
        }

        /// <summary>
        /// This method is the delegate callback passed to the network 
        /// to handle a new client connecting.
        /// </summary>
        /// <param name="ss"></param>
        public void HandleNewClient(SocketState ss)
        {
            Console.WriteLine("New client detected.");

            ss.callMe = ReceiveName;

            Network.GetData(ss);
        }

        /// <summary>
        /// This is the first callback when handling a new client, used to handle initial handshake.
        /// Adds the client to the client list, receives and sets the name provided by the client, and sends the players ID and world size.
        /// </summary>
        /// <param name="ss"></param>
        private void ReceiveName(SocketState ss)
        {
            //Console.WriteLine("Receiving name"); // for testing, to ensure this callMe method is reached.

            // Should this be added after receiving a full name?
            // Assuming that we should be removing this when we catch a closed connection
            // in any case, so believe it is safe to create this here.
            lock (clients)
            {
                if (!clients.ContainsValue(ss))
                {
                    clients.Add(shipIDCounter, ss);
                    ss.id = shipIDCounter;
                    shipIDCounter++;
                }
            }


            string totalData = ss.sb.ToString();

            //Console.WriteLine(totalData); // for testing, to ensure name is received correctly from client.

            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                theWorld.SpawnShip(ss.id, p.Substring(0, p.Length - 1));

                // Remove ship name from message buffer.
                ss.sb.Remove(0, p.Length);

                Network.Send(ss.theSocket, "" + ss.id + "\n" + 750 + "\n");

                // Handshake complete, move to game communication
                ss.callMe = HandleDataFromClient;
                //Console.WriteLine("Sent the ID and World Size"); // for testing (did we reach this point?)
            }

            // for testing, when we send data to the professors client we begin receiving feedback.
            //Network.Send(ss.theSocket, "{ \"ship\":0,\"loc\":{ \"x\":-210.55169931100295,\"y\":278.64037767770776},\"dir\":{ \"x\":0.0,\"y\":-1.0},\"thrust\":false,\"name\":\"testName\",\"hp\":5,\"score\":0}\n{ \"star\":0,\"loc\":{ \"x\":0.0,\"y\":0.0},\"mass\":0.01}\n");

            Network.GetData(ss);

            //ss.callMe(ss); // this is for testing, should not be necessary for receiving data.
        }

        /// <summary>
        /// Callback used for most game communication.
        /// This receives data from the client and passes it to the model for processing.
        /// </summary>
        /// <param name="ss"></param>
        public void HandleDataFromClient(SocketState ss)
        {
            string totalData = ss.sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");


            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                ProcessUserInput(ss.id, p.Substring(1, p.Length - 2)); // Manually removing encasing parentheses.
                ss.sb.Remove(0, p.Length);
            }


            Network.GetData(ss);
        }

        /// <summary>
        /// Sanitizes data provided by the client and passes it to the clients ship to be processed in the next frame.
        /// </summary>
        /// <param name="shipID"></param>
        /// <param name="userInput"></param>
        private void ProcessUserInput(int shipID, string userInput)
        {
            Ship currentShip = (Ship)theWorld.getPlayers()[shipID];
            string commandString = "";

            // Normalize user input.
            if (userInput.Contains("L"))
                commandString += "L";
            if (userInput.Contains("R"))
                commandString += "R";
            if (userInput.Contains("F"))
                commandString += "F";
            if (userInput.Contains("T"))
                commandString += "T";

            // Used for preventing dead ships from receiving user input
            if (currentShip != null)
            {
                currentShip.queueCommands(commandString);
            }
        }

        /// <summary>
        /// Reads data from settings.xml file (hard coded file path) and initializes the settings of the server, world, and its objects.
        /// UniverseSize: The length of one edge of the square world.
        /// MSPerFrame: How many milliseconds to wait between processing the next frame of actions in the world. FrameRate.
        /// FramesPerShot: How many frames must be processed between a ships "fire" commands, i.e. a fire rate cooldown.
        /// FireSpeed: Distance to move a projectile in each frame.
        /// RespawnRate: How many frames must be processed between a ships death and recreation, i.e. a respawn time. 
        /// Boss: controls for enabling boss mode.
        ///     enabled: 1 for enabled, 0 for disabled.
        ///     health: health of boss star if mode is enabled (relative to projectile damage of 1.)
        /// Ship: initializes default settings for all ships
        ///     accel: acceleration rate of ships, in distance per frame squared.
        ///     turn: turn rate of ships, in degrees.
        ///     health: maximum health of ships (relative to projectile damage of 1)
        ///     size: hitbox, or collision distance, of ships.
        /// Star: creates a star in the world.
        ///     x: x-coordinate of stars location
        ///     y: y-coordinate of stars location
        ///     mass: mass of star (affects acceleration imparted on ships in the world.)
        ///     size: hitbox, or collision distance, of star.
        /// </summary>
        private void InitializeSettings()
        {
            string settingsFile = @"..\..\..\Resource\settings.xml";
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(settingsFile);

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case "UniverseSize":
                        //Console.WriteLine("(node.Name)" + node.Name + " is " + "(node.InnerText)" + node.InnerText);
                        theWorld.SetUniverseSize(int.Parse(node.InnerText));
                        worldSize = int.Parse(node.InnerText);
                        break;
                    case "MSPerFrame":
                        //Console.WriteLine(node.Name + " is " + node.InnerText);
                        msPerFrame = int.Parse(node.InnerText);
                        break;
                    case "FramesPerShot":
                        //Console.WriteLine(node.Name + " is " + node.InnerText);
                        theWorld.FireDelay = int.Parse(node.InnerText);
                        break;
                    case "FireSpeed":
                        //Console.WriteLine(node.Name + " is " + node.InnerText);
                        theWorld.ProjVelocity = int.Parse(node.InnerText);
                        break;
                    case "RespawnRate":
                        //Console.WriteLine(node.Name + " is " + node.InnerText);
                        theWorld.ShipRespawnRate = int.Parse(node.InnerText);
                        break;
                    case "Ship":
                        //Console.WriteLine("Ship found!");
                        foreach (XmlNode shipNode in node)
                        {
                            //Console.WriteLine("\t" + shipNode.Name + " " + shipNode.InnerText);
                            if (shipNode.Name.Equals("accel"))
                            {
                                theWorld.ShipAccel = double.Parse(shipNode.InnerText);
                            }
                            else if (shipNode.Name.Equals("turn"))
                            {
                                theWorld.ShipTurn = int.Parse(shipNode.InnerText);
                            }
                            else if (shipNode.Name.Equals("health"))
                            {
                                theWorld.ShipHealth = int.Parse(shipNode.InnerText);
                            }
                            else
                            {
                                theWorld.ShipSize = int.Parse(shipNode.InnerText);
                            }
                        }
                        break;
                    case "Star":
                        //Console.WriteLine("Star found!");
                        double starX = 0.0;
                        double starY = 0.0;
                        double starMass = 0.0;

                        foreach (XmlNode starNode in node)
                        {
                            //Console.WriteLine("\t" + starNode.Name + " " + starNode.InnerText);
                            if (starNode.Name.Equals("x"))
                            {
                                starX = int.Parse(starNode.InnerText);
                            }
                            else if (starNode.Name.Equals("y"))
                            {
                                starY = int.Parse(starNode.InnerText);
                            }
                            else if (starNode.Name.Equals("mass"))
                            {
                                starMass = double.Parse(starNode.InnerText);
                            }
                            else
                            {
                                theWorld.StarSize = int.Parse(starNode.InnerText);
                            }
                        }

                        theWorld.SpawnStars(starX, starY, starMass);

                        break;
                    case "Boss":
                        //Console.WriteLine("Boss found!");
                        foreach (XmlNode bossNode in node)
                        {
                            //Console.WriteLine("\t" + bossNode.Name + " " + bossNode.InnerText);
                            if (bossNode.Name.Equals("enabled") && (int.Parse(bossNode.InnerText) == 1))
                            {
                                this.bossModeEnabled = true;
                            }
                            else
                            {
                                if (bossModeEnabled)
                                {
                                    this.bossMaxHP = int.Parse(bossNode.InnerText);
                                    theWorld.BossHealth = this.bossMaxHP;
                                }
                            }
                        }
                        break;
                    default:
                        //Console.WriteLine("I'm in default, what is this? " + node.InnerText);
                        break;
                }
            }
        }
    }
}
