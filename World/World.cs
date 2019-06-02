// Written by Christopher Jones and John Jacobson

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWars
{
    /// <summary>
    /// This class contains the world object, which tracks, updates, and stores
    /// all ship, projectile, and star objects.
    /// </summary>
    public class World
    {
        // Data structures for managing the objects within the world.
        private Dictionary<int, Ship> shipDictionary;
        private Dictionary<int, Projectile> projectileDictionary;
        private Dictionary<int, Star> starDictionary;

        // Data structures for holding temporary objects waiting for respawn
        private Dictionary<int, Ship> deadShipDictionary;
        private Dictionary<int, Star> deadStarDictionary;

        // Lists of respawned items used for cleaning up deadDictionaries after respawn.
        private List<int> shipsToRespawn;
        private List<int> starsToRespawn;

        // Used for maintaining unique star/projectile IDs.
        private int starIDs;
        private static int projIDs;

        // Holds dimensions of the world.
        private int worldSize;
        
        Random rand;

        // Projectile velocity in distance per frame.
        public double ProjVelocity { get; set; }

        // Modifiable variables for ship properties
        public int FireDelay { get; set; }
        public double ShipAccel { get; set; }
        public int ShipTurn { get; set; }
        public int ShipRespawnRate { get; set; }
        public int ShipSize { get; set; }
        public int ShipHealth { get; set; }
        

        // Modifiable variables for star properties
        public int StarSize { get; set; }

        // variables for managing "boss mode"
        public int BossHealth { get; set; }
        private int starFireCounter;
        private Vector2D starFireDir;
        private int starFireDelay;
        private bool clockwise = true;

        // using these for thread locks when parsing these data structures.
        public static object shipLock = new object();
        public static object projLock = new object();
        public static object starLock = new object();

        // Used by client to manage Mario Mode bonus feature.
        private int curPlayerID;
        private bool marioMode;

        /// <summary>
        /// The default constructor for the world
        /// </summary>
        public World()
        {
            // Initialize data structures.
            shipDictionary = new Dictionary<int, Ship>();
            projectileDictionary = new Dictionary<int, Projectile>();
            starDictionary = new Dictionary<int, Star>();
            deadShipDictionary = new Dictionary<int, Ship>();
            shipsToRespawn = new List<int>();
            deadStarDictionary = new Dictionary<int, Star>();
            starsToRespawn = new List<int>();

            rand = new Random();
            worldSize = 750;

            // initialize ID counters.
            projIDs = 0;
            starIDs = 0;

            // initialize boss mode variables
            starFireCounter = 0;
            starFireDir = new Vector2D(1, 0);
            starFireDir.Rotate(rand.NextDouble() * 360); // randomize starting fire location.
            starFireDelay = 2;
        }

        /// <summary>
        /// Returns a copy of the starDictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Star> GetDeadStars()
        {
            lock (deadStarDictionary)
            {
                Dictionary<int, Star> returnDictionary = new Dictionary<int, Star>(deadStarDictionary);

                return returnDictionary;
            }
        }

        /// <summary>
        /// Returns a copy of the shipDictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Ship> GetDeadPlayers()
        {
            lock (deadShipDictionary)
            {
                Dictionary<int, Ship> returnDictionary = new Dictionary<int, Ship>(deadShipDictionary);

                return returnDictionary;
            }
        }

        /// <summary>
        /// Set the fire delay to be used by all ships, measured in frames between shots.
        /// (i.e. SetFireDelay(10) only allows a ship to fire a projectile 10 or more frames after its previous shot.)
        /// </summary>
        /// <param name="Delay"></param>
        public void SetFireDelay(int Delay)
        {
            FireDelay = Delay;
        }

        /// <summary>
        /// Sets the dimensions of the world.
        /// </summary>
        /// <param name="size"></param>
        public void SetUniverseSize(int size)
        {
            worldSize = size;
        }

        /// <summary>
        /// This method will either add a new ship to the world, or update
        /// a current ship with a new ship depending on the ship's ID.
        /// </summary>
        /// <param name="newShip"></param>
        public void addShip(Ship newShip)
        {
            int shipId = newShip.GetID();
            lock (shipDictionary)
            {
                if (shipDictionary.ContainsKey(shipId))
                {
                    shipDictionary.Remove(shipId);
                    shipDictionary.Add(shipId, newShip);
                }
                else
                {
                    shipDictionary.Add(shipId, newShip);
                }
            }
        }

        /// <summary>
        /// This method will either add a new projectile to the world, or
        /// update a current projectile with a new projectile depending on
        /// the projectile's ID.
        /// </summary>
        /// <param name="newProjectile"></param>
        public void addProjectile(SpaceWars.Projectile newProjectile)
        {
            int projId = newProjectile.GetID();

            lock (projectileDictionary)
            {
                if (projectileDictionary.ContainsKey(projId))
                {
                    projectileDictionary.Remove(projId);
                    projectileDictionary.Add(projId, newProjectile);
                }
                else
                {
                    projectileDictionary.Add(projId, newProjectile);
                }
            }
        }

        /// <summary>
        /// Returns the dimensions of the world (world is defined to be square.)
        /// </summary>
        /// <returns>int</returns>
        public int GetWorldSize()
        {
            return worldSize;
        }

        /// <summary>
        /// This method will either add a new star to the world, or
        /// update a current star with a new star depending on
        /// the projectile's ID.
        /// </summary>
        /// <param name="newStar"></param>
        public void addStar(Star newStar)
        {
            int starId = newStar.GetID();

            lock (starDictionary)
            {
                if (starDictionary.ContainsKey(starId))
                {
                    starDictionary.Remove(starId);
                    starDictionary.Add(newStar.GetID(), newStar);
                }
                else
                {
                    starDictionary.Add(newStar.GetID(), newStar);
                }
            }
        }

        /// <summary>
        /// This method removes a projectile from the world.
        /// </summary>
        /// <param name="proj"></param>
        public void removeProjectile(Projectile proj)
        {
            int projId = proj.GetID();
            lock (projectileDictionary)
            {
                projectileDictionary.Remove(projId);
            }
        }

        /// <summary>
        /// This method removes a ship from the world.
        /// </summary>
        public void RemoveShip(int ship)
        {
            lock (shipDictionary)
            {
                shipDictionary.Remove(ship);
            }
        }

        /// <summary>
        /// This method removes a star from the world.
        /// </summary>
        public void RemoveStar(int star)
        {
            lock (starDictionary)
            {
                starDictionary.Remove(star);
            }
        }

        /// <summary>
        /// This method "kills" a ship and adds it to a list to respawn.
        /// </summary>
        /// <param name="ship"></param>
        public void KillShip(Ship ship)
        {
            lock (shipDictionary)
            {
                this.RemoveShip(ship.GetID());
                ship.Died();
                deadShipDictionary.Add(ship.GetID(), ship);
            }
        }

        /// <summary>
        /// This method "kills" a star and adds it to a list to respawn (for boss mode only).
        /// </summary>
        /// <param name="ship"></param>
        public void KillStar(Star star)
        {
            lock (starDictionary)
            {
                this.RemoveStar(star.GetID());
                star.Died();
                deadStarDictionary.Add(star.GetID(), star);
            }
        }

        /// <summary>
        /// Moves all projectiles a distance equal to their current velocity, one time.
        /// </summary>
        public void updateProjectiles()
        {
            lock (projectileDictionary)
            {
                foreach (KeyValuePair<int, Projectile> proj in projectileDictionary)
                {
                    proj.Value.Move();
                }
            }
        }

        /// <summary>
        /// Move all ships by one frame, and tick fire-cooldown timers.
        /// </summary>
        public void updateShips()
        {
            lock (shipDictionary)
            {
                foreach (KeyValuePair<int, Ship> ship in shipDictionary)
                {
                    ship.Value.Move();

                    ship.Value.CooldownTick();
                }
            }
        }

        /// <summary>
        /// Applies the mass ("gravity") generated by each star to each ship within the world for one frame.
        /// Adds each stars "mass" to each ships velocity, oriented from the ship towards the star, one time.
        /// </summary>
        public void ApplyStarGravity()
        {
            double xAccel, yAccel;
            double starMass;
            Vector2D starLoc, accelDir, shipLoc;

            foreach (KeyValuePair<int, Star> star in starDictionary)
            {
                starMass = star.Value.GetMass();
                starLoc = new Vector2D(star.Value.GetLocation());

                lock (shipDictionary)
                {
                    foreach (KeyValuePair<int, Ship> ship in shipDictionary)
                    {
                        shipLoc = new Vector2D(ship.Value.GetLocation());
                        accelDir = starLoc - shipLoc;
                        accelDir.Normalize();
                        xAccel = accelDir.GetX() * starMass;
                        yAccel = accelDir.GetY() * starMass;

                        ship.Value.Accelerate(xAccel, yAccel);
                    }
                }
            }
        }

        /// <summary>
        /// Tick respawn timers for Ships (and Stars in boss mode) by 1 frame, and respawn if ready.
        /// </summary>
        public void updateRespawnCounter()
        {
            shipsToRespawn.Clear();

            lock (deadShipDictionary)
            {
                foreach (KeyValuePair<int, Ship> ship in deadShipDictionary)
                {
                    ship.Value.RespawnTick();
                    if (ship.Value.ReadyToRespawn())
                    {
                        RespawnShip(ship.Value);
                        shipsToRespawn.Add(ship.Value.GetID());
                    }
                }

                foreach (int shipID in shipsToRespawn)
                {
                    deadShipDictionary.Remove(shipID);
                }
            }


            starsToRespawn.Clear();

            lock (deadStarDictionary)
            {
                foreach (KeyValuePair<int, Star> star in deadStarDictionary)
                {
                    star.Value.RespawnTick();
                    if (star.Value.ReadyToRespawn())
                    {
                        respawnStar(star.Value);
                        starsToRespawn.Add(star.Value.GetID());
                    }
                }

                foreach (int starID in starsToRespawn)
                {
                    deadStarDictionary.Remove(starID);
                }
            }
        }

        /// <summary>
        /// Creates a new ship at a random world location, and adds this ship to the world.
        /// </summary>
        /// <param name="id">id of this ship</param>
        /// <param name="name">name of the player controlling this ship</param>
        public void SpawnShip(int id, string name)
        {
            // This probably won't work as ships can spawn on a center star, but for now should make ships randomly initialize.
            // Create new ship for this client.
            Vector2D newLoc = new Vector2D(rand.Next(-worldSize / 2, worldSize / 2), rand.Next(-worldSize / 2, worldSize / 2));
            Vector2D newOrient = new Vector2D(0, -1);
            Ship newShip = new Ship(id, newLoc, newOrient, false, name, ShipHealth, 0);

            // Set ship's modifiable variables
            newShip.setAccelRate(ShipAccel);
            newShip.SetRespawnDelay(ShipRespawnRate);
            newShip.SetFireDelay(FireDelay);
            newShip.setWorldSize(this.worldSize);

            this.addShip(newShip);
        }

        /// <summary>
        /// Spawns a ship using the properties of an existing ship. Used for respawning existing ships which have been killed. Spawn location is randomized.
        /// </summary>
        /// <param name="ship">ship to be respawned</param>
        public void RespawnShip(Ship ship)
        {
            Vector2D newLoc = new Vector2D(rand.Next(-worldSize / 2, worldSize / 2), rand.Next(-worldSize / 2, worldSize / 2));
            Vector2D newOrient = new Vector2D(0, -1);
            Ship newShip = new Ship(ship.GetID(), newLoc, newOrient, false, ship.GetName(), ShipHealth, ship.GetScore());

            // Set ship's modifiable variables
            newShip.setAccelRate(ShipAccel);
            newShip.SetRespawnDelay(ShipRespawnRate);
            newShip.SetFireDelay(FireDelay);
            newShip.setWorldSize(this.worldSize);

            this.addShip(newShip);
        }

        /// <summary>
        /// Spawns a star using the properties of an existing star. Used for respawning existing stars which have been killed (boss mode only.) Spawn location is retained from initial star.
        /// </summary>
        /// <param name="star">star to be respawned</param>
        public void respawnStar(Star star)
        {
            Vector2D newLoc = new Vector2D(star.GetLocation());
            Star newStar = new Star(star.GetID(),newLoc, star.GetMass());

            // Set ship's modifiable variables
            newStar.SetHP(BossHealth);

            this.addStar(newStar);
        }

        /// <summary>
        /// Creates a new star with the provided properties, and adds it to the world.
        /// </summary>
        /// <param name="starX">x coordinate of this stars location</param>
        /// <param name="starY">y coordinate of this stars location</param>
        /// <param name="starMass">mass of this star</param>
        public void SpawnStars(double starX, double starY, double starMass)
        {
            Vector2D starLocation = new Vector2D(starX, starY);

            Star newStar = new Star(starIDs++, starLocation, starMass);

            newStar.SetHP(BossHealth);

            this.addStar(newStar);
        }

        /// <summary>
        /// This method returns a dictionary copy of the player list
        /// keyed to the ship IDs.
        /// </summary>
        /// <returns></returns>
        public IDictionary getPlayers()
        {
            lock (shipDictionary)
            {
                Dictionary<int, Ship> returnDictionary = new Dictionary<int, Ship>(shipDictionary);

                return returnDictionary;
            }

        }

        /// <summary>
        /// This method returns a dictionary copy of the projectile list
        /// keyed to the ship IDs.
        /// </summary>
        /// <returns></returns>
        public IDictionary getProjs()
        {
            lock (projectileDictionary)
            {
                Dictionary<int, Projectile> returnDictionary = new Dictionary<int, Projectile>(projectileDictionary);

                return returnDictionary;
            }
        }

        /// <summary>
        /// This method returns a dictionary copy of the star list
        /// keyed to the ship IDs.
        /// </summary>
        /// <returns></returns>
        public IDictionary getStars()
        {
            lock (starDictionary)
            {
                Dictionary<int, Star> returnDictionary = new Dictionary<int, Star>(starDictionary);

                return returnDictionary;
            }
        }

        /// <summary>
        /// Creates a projectile with the same location and orientation of the ship which fired it, and adds it to the world. Then tells the ship that is projectile has been fired.
        /// </summary>
        /// <param name="currentShip">ship which fired this projectile</param>
        public void SpawnProjectile(Ship currentShip)
        {
            if (currentShip.ReadyToFire())
            {
                // Need to adjust spawn location Y-axis
                Vector2D projLocation = new Vector2D(currentShip.GetLocation().GetX(), currentShip.GetLocation().GetY());
                Vector2D projDir = new Vector2D(currentShip.GetOrientation());
                Vector2D projVeloc = new Vector2D(currentShip.GetOrientation().GetX() * ProjVelocity, currentShip.GetOrientation().GetY() * ProjVelocity);

                Projectile newProj = new Projectile(projIDs, projLocation, projDir, true, currentShip.GetID());   // May want to shift projectile
                newProj.SetVelocity(projVeloc);
                this.addProjectile(newProj);
                projIDs++;
                
                lock (shipDictionary)
                {
                    this.shipDictionary[currentShip.GetID()].FireProjectile();
                }

            }
        }

        /// <summary>
        /// Creates a projectile with the same location as the star which fired it, and adds it to the world. Orientation must be provided as stars do not have an orientation.
        /// </summary>
        /// <param name="currentStar">star which fired this projectile</param>
        /// <param name="dir">orientation of the projectile</param>
        public void SpawnProjectile(Star currentStar, Vector2D dir)
        {
            // Need to adjust spawn location Y-axis
            Vector2D projLocation = new Vector2D(currentStar.GetLocation().GetX(), currentStar.GetLocation().GetY());
            Vector2D projDir = new Vector2D(dir);
            Vector2D projVeloc = new Vector2D(dir.GetX() * ProjVelocity / 3, dir.GetY() * ProjVelocity / 3);

            Projectile newProj = new Projectile(projIDs, projLocation, projDir, true, int.MaxValue); // hard coding this, bad design but... yeah...
            newProj.SetVelocity(projVeloc);
            this.addProjectile(newProj);
            projIDs++;
        }

        /// <summary>
        /// Method for firing projectiles from a star, for Boss mode.
        /// Generates a single line of projectiles rotating clockwise around the star.
        /// TODO: need to modify fire counter and fire delay to be star properties so that boss mode works with multiple stars.
        /// </summary>
        /// <param name="currentStar">Star firing this projectile.</param>
        public void bossFireMode1(Star currentStar)
        {
            if (starFireCounter % starFireDelay == 0)
            {
                starFireDir.Rotate(2.5);
                SpawnProjectile(currentStar, this.starFireDir);
            }
            starFireCounter++;
        }

        /// <summary>
        /// Method for firing projectiles from a star, for Boss mode.
        /// Generates 4 lines of projectiles rotating counter-clockwise aruond each star.
        /// TODO: need to modify fire counter and fire delay to be star properties so that boss mode works with multiple stars.
        /// </summary>
        /// <param name="currentStar">Star firing this projectile.</param>
        public void bossFireMode2(Star currentStar)
        {
            if (starFireCounter % (starFireDelay * 3) == 0)
            {
                starFireDir.Rotate(-1.5);
                SpawnProjectile(currentStar, this.starFireDir);

                starFireDir.Rotate(90.0);
                SpawnProjectile(currentStar, this.starFireDir);

                starFireDir.Rotate(90.0);
                SpawnProjectile(currentStar, this.starFireDir);

                starFireDir.Rotate(90.0);
                SpawnProjectile(currentStar, this.starFireDir);

                starFireDir.Rotate(90.0); // reset orientation for next iteration.
            }
            starFireCounter++;
        }

        /// <summary>
        /// Method for firing projectiles from a star, for Boss mode.
        /// Generates 2 lines of projectiles rotating around each star, with direction changing at various intervals.
        /// TODO: need to modify fire counter and fire delay to be star properties so that boss mode works with multiple stars.
        /// </summary>
        /// <param name="currentStar">Star firing this projectile.</param>
        public void bossFireMode3(Star currentStar)
        {
            if (starFireCounter % starFireDelay == 0)
            {
                if ((starFireDir.ToAngle() >= 0 && starFireDir.ToAngle() < 2.0))
                {
                    clockwise = !clockwise;
                }

                if (clockwise)
                {
                    starFireDir.Rotate(2.0);
                }
                else
                {
                    starFireDir.Rotate(-2.0);
                }

                SpawnProjectile(currentStar, this.starFireDir);

                starFireDir.Rotate(180.0);
                SpawnProjectile(currentStar, this.starFireDir);

                starFireDir.Rotate(180.0); // reset orientation for next iteration.
            }
            starFireCounter++;

        }

        /// <summary>
        /// Returns true if ship and star are within collision distance, otherwise false.
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="star"></param>
        /// <returns></returns>
        public bool HasCollidedShipStar(Ship ship, Star star)
        {
            Vector2D shipLoc = ship.GetLocation();
            Vector2D starLoc = star.GetLocation();
            Vector2D distanceVector = shipLoc - starLoc;
            double distanceLength = distanceVector.Length();
            if (distanceLength <= StarSize)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if projectile and star are within collision distance, otherwise false.
        /// </summary>
        /// <param name="star"></param>
        /// <param name="proj"></param>
        /// <returns></returns>
        public bool HasCollidedProjStar(Star star, Projectile proj)
        {
            Vector2D starLoc = star.GetLocation();
            Vector2D projLoc = proj.GetLocation();
            Vector2D distanceVector = starLoc - projLoc;
            double distanceLength = distanceVector.Length();
            if (distanceLength <= StarSize)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if ship and projectile are within collision distance, otherwise false.
        /// </summary>
        /// <param name="ship"></param>
        /// <param name="proj"></param>
        /// <returns></returns>
        public bool HasCollidedShipProj(Ship ship, Projectile proj)
        {
            Vector2D shipLoc = ship.GetLocation();
            Vector2D projLoc = proj.GetLocation();
            Vector2D distanceVector = shipLoc - projLoc;
            double distanceLength = distanceVector.Length();
            if (distanceLength <= ShipSize && (ship.GetID() != proj.GetOwner()))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enables mario mode if player name is mario, otherwise does nothing. For client only.
        /// </summary>
        /// <param name="isMario">Flag for determining if current player name is "mario"</param>
        public void EnableMarioMode(bool isMario)
        {
            marioMode = isMario;
        }

        /// <summary>
        /// Sets the player id for this client provided by the server.
        /// </summary>
        /// <param name="playerID"></param>
        public void setPlayerID(int playerID)
        {
            curPlayerID = playerID;
        }

        /// <summary>
        /// Returns true if mario mode is enabled, else false.
        /// </summary>
        /// <returns>bool</returns>
        public bool GetMarioMode()
        {
            return marioMode;
        }

        /// <summary>
        /// Returns player ID of this client.
        /// </summary>
        /// <returns>int</returns>
        public int GetCurrentPlayer()
        {
            return curPlayerID;
        }
    }
}
