// Written by Christopher Jones and John Jacobson

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWars
{
    /// <summary>
    /// This class contains the star object that is contained within the world
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Star
    {    
      [JsonProperty(PropertyName = "star")]
        private int ID;

        [JsonProperty]
        private Vector2D loc;

        [JsonProperty]
        private double mass;

        private int hp;
        private int respawnDelay;
        private int respawnTimer;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Star() { }

        /// <summary>
        /// Constructor to set all star properties.
        /// </summary>
        /// <param name="ID_"></param>
        /// <param name="loc_"></param>
        /// <param name="mass_"></param>
        [JsonConstructor]
        public Star(int ID_, Vector2D loc_, double mass_)
        {
            ID = ID_;
            loc = loc_;
            mass = mass_;
            hp = 100;
            respawnDelay = 300;
            respawnTimer = 0;
        }

        /// <summary>
        /// Return current hp of this star (boss mode only.)
        /// </summary>
        /// <returns></returns>
        public int GetHP() { return hp; }

        /// <summary>
        /// Set current hp of this star (boss mode only.)
        /// </summary>
        /// <param name="health"></param>
        public void SetHP(int health) { hp = health; }

        /// <summary>
        /// Returns ID of this star.
        /// </summary>
        /// <returns>int</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        /// Returns location.
        /// </summary>
        /// <returns>Vector2D</returns>
        public Vector2D GetLocation()
        {
            return loc;
        }

        /// <summary>
        /// Returns mass of this star.
        /// </summary>
        /// <returns>double</returns>
        public double GetMass()
        {
            return mass;
        }

        /// <summary>
        /// Reduce the health of this star by the provided damage amount.
        /// </summary>
        /// <param name="damage">Damage to deal to this star.</param>
        public void Damage(int damage)
        {
            hp = hp - damage;
        }

        /// <summary>
        /// Set the respawn delay to be used by this star.
        /// </summary>
        /// <param name="rd"></param>
        public void SetRespawnDelay(int rd) { this.respawnDelay = rd; }
        
        /// <summary>
        /// "Tick" the respawn timer of this star by one frame, if it is dead.
        /// </summary>
        public void RespawnTick()
        {
            if (respawnTimer > 0)
            {
                respawnTimer = (respawnTimer + 1) % respawnDelay;
            }
        }

        /// <summary>
        /// Start the respawn timer for this star once it dies.
        /// </summary>
        public void Died()
        {
            respawnTimer++;
        }

        /// <summary>
        /// True if this star is ready to be spawned (respawn timer has been reached,) false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool ReadyToRespawn() { return respawnTimer == 0; }
        
    }
}
