// Written by Christopher Jones and John Jacobson

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpaceWars
{
    /// <summary>
    /// This class contains the ship object contained within the world.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Ship : IComparable
    {
        [JsonProperty(PropertyName = "ship")]
        private int ID { get; set; }

        [JsonProperty]
        private Vector2D loc { get; set; }

        [JsonProperty]
        private Vector2D dir { get; set; }

        [JsonProperty]
        private bool thrust { get; set; }

        [JsonProperty]
        private string name { get; set; }

        [JsonProperty]
        private int hp { get; set; }

        [JsonProperty]
        private int score { get; set; }


        private string queuedCommands;
        private Vector2D acceleration;
        private Vector2D velocity;
        private int fireDelay;
        private int cooldownTimer;
        private double accelRate;
        private int worldSize;
        private int respawnTimer;
        private int respawnDelay;
        private int turningRate;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Ship() { }

        /// <summary>
        /// Constructor to set all ship properties.
        /// </summary>
        /// <param name="_ID"></param>
        /// <param name="_loc"></param>
        /// <param name="_dir"></param>
        /// <param name="_thrust"></param>
        /// <param name="_name"></param>
        /// <param name="_hp"></param>
        /// <param name="_score"></param>
        [JsonConstructor]
        public Ship(int _ID, Vector2D _loc, Vector2D _dir, bool _thrust, string _name, int _hp, int _score)
        {
            ID = _ID;
            loc = _loc;
            dir = _dir;
            thrust = _thrust;
            name = _name;
            hp = _hp;
            score = _score;

            queuedCommands = "";
            fireDelay = 6;
            respawnDelay = 300;      // READ FROM FILE
            cooldownTimer = 0;
            respawnTimer = 0;
            accelRate = 0.08;        // READ FROM FILE
            turningRate = 2;

            acceleration = new Vector2D(0, 0);
            velocity = new Vector2D(0, 0);

        }

        /// <summary>
        /// Set the size of the world the ship inhabits.
        /// </summary>
        /// <param name="size"></param>
        public void setWorldSize(int size) { this.worldSize = size; }

        ///// <summary>
        ///// Jerks (modifies acceleration of) this ship by a provided input of acceleration.
        ///// </summary>
        //public void Jerk(double xJerk, double yJerk)
        //{
        //    double newX = this.acceleration.GetX() + xJerk;
        //    double newY = this.acceleration.GetY() + yJerk;

        //    this.acceleration = new Vector2D(newX, newY);
        //}

        /// <summary>
        /// Accelerates (modifies velocity of) this ship by its current attributes.
        /// </summary>
        public void Accelerate(double xAccel, double yAccel)
        {
            double newX = this.velocity.GetX() + xAccel;
            double newY = this.velocity.GetY() + yAccel;

            this.velocity = new Vector2D(newX, newY);
        }

        /// <summary>
        /// Moves (modifies position of) this ship by its current attributes.
        /// </summary>
        public void Move()
        {
            double newX = this.loc.GetX() + this.velocity.GetX();
            double newY = this.loc.GetY() + this.velocity.GetY();

            // Wrap to opposite edge when out of bounds in X-axis.
            if (Math.Abs(newX) > worldSize / 2)
            {
                newX = -worldSize / 2 * Math.Sign(newX);
            }

            // Wrap to opposite edge when out of bounds in Y-axis.
            if (Math.Abs(newY) > worldSize / 2)
            {
                newY = -worldSize / 2 * Math.Sign(newY);
            }

            this.loc = new Vector2D(newX, newY);
        }

        /// <summary>
        /// Manually set the velocity of this ship.
        /// </summary>
        /// <param name="newVel"></param>
        public void SetVelocity(Vector2D newVel)
        {
            this.velocity = newVel;
        }


        /// <summary>
        /// Returns ID of this ship.
        /// </summary>
        /// <returns>int</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        /// Returns location of this ship.
        /// </summary>
        /// <returns>Vector2D</returns>
        public Vector2D GetLocation()
        {
            return loc;
        }

        /// <summary>
        /// Process the current commands contained in this ship.
        /// "T": Thrust ship once.
        /// "F": Fire projectile from ship, if not on cooldown. This is not processed here, but handled by the world (who has control of the projectiles.)
        /// "L": Turn ship left once.
        /// "R": Turn ship right once.
        /// 
        /// Commands queued for this ship will be cleared once this is called! Process "F" in the world before calling this.
        /// </summary>
        public void ProcessCommands()
        {
            // if ship is not currently thrusting, let the view know not to draw thrust.
            if (!queuedCommands.Contains("T"))
            {
                thrust = false;
            }

            string[] parts = Regex.Split(queuedCommands, "");

            foreach (char c in queuedCommands)
            {
                switch (c)
                {
                    case 'L':
                        this.TurnLeft();
                        break;
                    case 'T':
                        this.Thrust();
                        break;
                    case 'R':
                        this.TurnRight();
                        break;
                    case 'F':
                        // Not my job! The World handles this.
                        break;
                    default:
                        break;
                }
            }

            this.ClearCommands();
        }

        /// <summary>
        /// Returns orientation of this ship.
        /// </summary>
        /// <returns>Vector2D</returns>
        public Vector2D GetOrientation()
        {
            return dir;
        }

        /// <summary>
        /// Returns thrust of this ship.
        /// </summary>
        /// <returns>bool</returns>
        public bool GetThrust()
        {
            return thrust;
        }

        /// <summary>
        /// Returns velocity of this ship.
        /// </summary>
        /// <returns></returns>
        public Vector2D GetVelocity() { return this.velocity; }

        /// <summary>
        /// Returns name of the player controlling this ship.
        /// </summary>
        /// <returns>string</returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Returns hp of this ship.
        /// </summary>
        /// <returns>int</returns>
        public int GetHP()
        {
            return hp;
        }

        /// <summary>
        /// Returns score of this ship.
        /// </summary>
        /// <returns>int</returns>
        public int GetScore()
        {
            return score;
        }

        /// <summary>
        /// The logic that dictates how ships are compared to each other.
        /// Ships are sorted based first on their score, then by their Id.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            Ship otherShip = (Ship)obj;

            if (this.score - otherShip.score == 0)
            {
                return this.ID - otherShip.ID;
            }
            else
            {
                return otherShip.score - this.score;
            }
        }

        /// <summary>
        /// Turns ship left/counter-clockwise by the default turning rate one time.
        /// </summary>
        public void TurnLeft()
        {
            this.dir.Rotate(-turningRate);
        }

        /// <summary>
        /// Turns ship right/clockwise by the default turning rate one time
        /// </summary>
        public void TurnRight()
        {
            this.dir.Rotate(turningRate);
        }
        
        /// <summary>
        /// Accelerate the ship (change its velocity) by the default acceleration rate in the direction it is currently facing.
        /// Also changes "thrust" flag for the view.
        /// </summary>
        public void Thrust()
        {
            thrust = true;
            this.Accelerate(this.GetOrientation().GetX() * accelRate, this.GetOrientation().GetY() * accelRate);
        }

        /// <summary>
        /// Clear the currently queued commands from this ship.
        /// </summary>
        public void ClearCommands()
        {
            queuedCommands = "";
        }

        /// <summary>
        /// Get the currently queued commands for this ship.
        /// </summary>
        /// <returns></returns>
        public string GetCommands()
        {
            return queuedCommands;
        }

        /// <summary>
        /// Add commands to this ships queue.
        /// </summary>
        /// <param name="userInput"></param>
        public void queueCommands(string userInput)
        {
            queuedCommands = userInput;
        }

        /// <summary>
        /// Set the acceleration rate of this ship.
        /// </summary>
        /// <param name="acceleration"></param>
        public void setAccelRate(double acceleration)
        {
            accelRate = acceleration;
        }

        /// <summary>
        /// Returns true if this ship is ready to fire, false if it is waiting on cooldown from previously firing.
        /// </summary>
        /// <returns></returns>
        public bool ReadyToFire() { return cooldownTimer == 0; }

        /// <summary>
        /// Sets the cooldown / fire delay between projectiles.
        /// </summary>
        /// <param name="cd"></param>
        public void SetFireDelay(int cd) { this.fireDelay = cd; }

        /// <summary>
        /// If ship is ready to fire, does nothing. Otherwise, steps the cooldown timer for firing one time.
        /// </summary>
        public void CooldownTick()
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer = (cooldownTimer + 1) % fireDelay;
            }
        }

        /// <summary>
        /// Sets the ship respawn delay (number of frames a ship must wait to respawn after death.)
        /// </summary>
        /// <param name="rd"></param>
        public void SetRespawnDelay(int rd) { this.respawnDelay = rd; }

        /// <summary>
        /// If ship is ready to be spawned, does nothing. Otherwise, steps the respawn timer up one time.
        /// </summary>
        public void RespawnTick()
        {
            if (respawnTimer > 0)
            {
                respawnTimer = (respawnTimer + 1) % respawnDelay;
            }
        }

        /// <summary>
        /// Adds one to respawn timer. Used to initiate the timer when the ship dies, then RespawnTick should be used.
        /// </summary>
        public void Died()
        {
            respawnTimer++;
        }

        /// <summary>
        /// Returns true if this ship is ready to be spawned, false if the ship is waiting on a respawn timer.
        /// </summary>
        /// <returns></returns>
        public bool ReadyToRespawn() { return respawnTimer == 0; }

        /// <summary>
        /// Adds one to fire delay timer. Used to initiate the timer when the ship fires a projectile, then CooldownTick should be used.
        /// </summary>
        /// <returns></returns>
        public bool FireProjectile()
        {
            if (ReadyToFire())
            {
                cooldownTimer++;
                return true;
            }

            return false; // This should never happen, indicates an error (ship does not manage projectiles, so this wil throw timers off if we are firing when not cooled down)
        }

        /// <summary>
        /// Reduce the health of this ship by the provided damage amount.
        /// </summary>
        /// <param name="damage">Damage to deal to this ship.</param>
        public void Damage(int damage)
        {
            hp = hp - damage;
        }

        /// <summary>
        /// Add one point to the player controlling this ship.
        /// </summary>
        public void PointScored()
        {
            score++;
        }
    }
}
