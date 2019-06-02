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
    /// This class contains the projectile object contained within the world.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {    
      [JsonProperty(PropertyName = "proj")]
        private int ID;

        [JsonProperty]
        private Vector2D loc;

        [JsonProperty]
        private Vector2D dir;

        [JsonProperty]
        private bool alive;

        [JsonProperty]
        private int owner;

        private Vector2D acceleration;
        private Vector2D velocity;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Projectile() { }

        /// <summary>
        /// The constructor for a projectile object with the object's id, location,
        /// direction, whether it is alive, and its owner.
        /// </summary>
        /// <param name="ID_"></param>
        /// <param name="loc_"></param>
        /// <param name="dir_"></param>
        /// <param name="alive_"></param>
        /// <param name="owner_"></param>
        [JsonConstructor]
        public Projectile(int ID_, Vector2D loc_, Vector2D dir_, bool alive_, int owner_)
        {
            ID = ID_;
            loc = loc_;
            dir = dir_;
            alive = alive_;
            owner = owner_;
            acceleration = new Vector2D(0,0);
            velocity = new Vector2D(0,0);
        }

        ///// <summary>
        ///// Jerks (modifies acceleration of) this projectile by a provided input of acceleration.
        ///// </summary>
        //public void Jerk(double xJerk, double yJerk)
        //{
        //    double newX = this.acceleration.GetX() + xJerk;
        //    double newY = this.acceleration.GetY() + yJerk;

        //    this.acceleration = new Vector2D(newX, newY);
        //}

        /// <summary>
        /// Accelerates (modifies velocity of) this projectile by its current attributes.
        /// </summary>
        public void Accelerate(double xAccel, double yAccel)
        {
            double newX = this.velocity.GetX() + xAccel;
            double newY = this.velocity.GetY() + yAccel;

            this.velocity = new Vector2D(newX, newY);
        }

        /// <summary>
        /// Moves (modifies position of) this projectile by its current attributes.
        /// </summary>
        public void Move()
        {
            double newX = this.loc.GetX() + this.velocity.GetX();
            double newY = this.loc.GetY() + this.velocity.GetY();

            this.loc = new Vector2D(newX, newY);
        }

        /// <summary>
        /// Manually set the velocity of this projectile.
        /// </summary>
        /// <param name="newVel"></param>
        public void SetVelocity(Vector2D newVel)
        {
            this.velocity = newVel;
        }

        /// <summary>
        /// Returns ID of this projectile.
        /// </summary>
        /// <returns>int</returns>
        public int GetID()
        {
            return ID;
        }

        /// <summary>
        /// Returns location of this projectile.
        /// </summary>
        /// <returns>Vector2D</returns>
        public Vector2D GetLocation()
        {
            return loc;
        }

        /// <summary>
        /// Returns orientation of this projectile.
        /// </summary>
        /// <returns>Vector2D</returns>
        public Vector2D GetOrientation()
        {
            return dir;
        }

        /// <summary>
        /// Returns alive status of this projectile.
        /// </summary>
        /// <returns>bool</returns>
        public bool GetAlive()
        {
            return alive;
        }

        /// <summary>
        /// Set the alive status of this projectile to false.
        /// </summary>
        public void Kill()
        {
            this.alive = false;
        }

        /// <summary>
        /// Return ID of ship which fire this projectile.
        /// </summary>
        /// <returns>int</returns>
        public int GetOwner()
        {
            return owner;
        }
    }
}
