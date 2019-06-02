// Written by Christopher Jones and John Jacobson

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceWars
{
    /// <summary>
    /// This class contains the panel on which the world in draw in the window.
    /// </summary>
    public class DrawingPanel : Panel
    {
        private readonly Size SHIP_SIZE = new Size(35, 35);
        private readonly Size PROJECTILE_SIZE = new Size(15, 15);
        private readonly Size STAR_SIZE = new Size(55, 55);

        private World theWorld;
        private List<Bitmap> shipSprites = new List<Bitmap>();
        private List<Bitmap> shipSpritesThrust = new List<Bitmap>();
        private List<Bitmap> projSprites = new List<Bitmap>();
        private List<Bitmap> starSprites = new List<Bitmap>();
        private Bitmap bgImage, marioBG;
        private Bitmap marioStar;
        private Bitmap marioShip;
        private Bitmap marioFireball;


        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// The constructor for the drawing panel.
        /// </summary>
        /// <param name="w"></param>
        public DrawingPanel(World w)
        {
            DoubleBuffered = true;
            theWorld = w;

            DrawSprites();
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
            return (int)w + size / 2;
        }

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // Perform the transformation
            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            e.Graphics.TranslateTransform(-x, -y);
            // Draw the object 
            drawer(o, e);
            // Then undo the transformation
            e.Graphics.ResetTransform();
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ShipDrawer(object o, PaintEventArgs e)
        {
            Ship ship = o as Ship;
            Bitmap shipSprite;
            int x, y;
            Point p;

            x = WorldSpaceToImageSpace(this.Size.Width, (int)ship.GetLocation().GetX() - (SHIP_SIZE.Width / 2));
            y = WorldSpaceToImageSpace(this.Size.Width, (int)ship.GetLocation().GetY() - (SHIP_SIZE.Height / 2));
            p = new Point(x,y);

            if(theWorld.GetMarioMode() && ship.GetID() == theWorld.GetCurrentPlayer())
            {
                shipSprite = marioShip;
            }
            else
            {
                if (ship.GetThrust())
                    shipSprite = shipSpritesThrust[ship.GetID() % shipSprites.Count];
                else
                    shipSprite = shipSprites[ship.GetID() % shipSprites.Count];
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(shipSprite, p);

        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            Projectile proj = o as Projectile;
            Bitmap projSprite;
            int x, y;
            Point p;

            x = WorldSpaceToImageSpace(this.Size.Width, (int)proj.GetLocation().GetX() - (PROJECTILE_SIZE.Width / 2));
            y = WorldSpaceToImageSpace(this.Size.Width, (int)proj.GetLocation().GetY() - (PROJECTILE_SIZE.Height / 2));
            p = new Point(x, y);

            if (theWorld.GetMarioMode() && proj.GetOwner() == theWorld.GetCurrentPlayer())
                projSprite = marioFireball;
            else
                projSprite = projSprites[proj.GetOwner() % projSprites.Count];

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(projSprite, p);

        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void StarDrawer(object o, PaintEventArgs e)
        {
            Star star = o as Star;
            Bitmap starSprite;
            int x, y;
            Point p;

            x = WorldSpaceToImageSpace(this.Size.Width, (int)star.GetLocation().GetX());
            y = WorldSpaceToImageSpace(this.Size.Width, (int)star.GetLocation().GetY());
            p = new Point(x - (STAR_SIZE.Width / 2), y - (STAR_SIZE.Height / 2));

            if(theWorld.GetMarioMode())
            {
                starSprite = marioStar;
            }
            else
            {
                starSprite = starSprites[star.GetID() % starSprites.Count];
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(starSprite, p);
        }


        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            lock (World.shipLock)
            {
                // Draw the ships
                foreach (KeyValuePair<int, Ship> pair in (Dictionary<int, Ship>)theWorld.getPlayers())
                {
                    if (pair.Value.GetHP() > 0)
                    {
                        DrawObjectWithTransform(e, pair.Value, this.Size.Width, pair.Value.GetLocation().GetX(), pair.Value.GetLocation().GetY(), pair.Value.GetOrientation().ToAngle(), ShipDrawer);
                    }
                }
            }

            lock (World.projLock)
            {
                // Draw the projectile
                foreach (KeyValuePair<int, Projectile> pair in (Dictionary<int, Projectile>)theWorld.getProjs())
                {
                    if (pair.Value.GetAlive())
                    {
                        DrawObjectWithTransform(e, pair.Value, this.Size.Width, pair.Value.GetLocation().GetX(), pair.Value.GetLocation().GetY(), pair.Value.GetOrientation().ToAngle(), ProjectileDrawer);
                    }
                }
            }

            lock (World.starLock)
            {
                // Draw the stars
                foreach (KeyValuePair<int, Star> pair in (Dictionary<int, Star>)theWorld.getStars())
                {
                    DrawObjectWithTransform(e, pair.Value, this.Size.Width, pair.Value.GetLocation().GetX(), pair.Value.GetLocation().GetY(), 0.0, StarDrawer);
                }
            }

            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

        /// <summary>
        /// Draws the objects based on which sprite they are to be represented by.
        /// </summary>
        private void DrawSprites()
        {
            string[] filePaths = Directory.GetFiles(@"..\..\..\Resource\Assets\", "*.*", SearchOption.TopDirectoryOnly);

            foreach (String fileName in filePaths)
            {
                if (fileName.Contains("coast"))
                {
                    Bitmap ship = new Bitmap(fileName);
                    ship = new Bitmap(ship, SHIP_SIZE);

                    shipSprites.Add(ship);
                }

                if (fileName.Contains("thrust"))
                {
                    Bitmap shipThrust = new Bitmap(fileName);
                    shipThrust = new Bitmap(shipThrust, SHIP_SIZE);

                    shipSpritesThrust.Add(shipThrust);
                }

                if (fileName.Contains("shot"))
                {
                    Bitmap proj = new Bitmap(fileName);
                    proj = new Bitmap(proj, PROJECTILE_SIZE);

                    projSprites.Add(proj);
                }

                if (fileName.Contains("star"))
                {
                    Bitmap star = new Bitmap(fileName);
                    star = new Bitmap(star, STAR_SIZE);

                    starSprites.Add(star);
                }

                if (fileName.Contains("Background"))
                {
                    Bitmap bg = new Bitmap(fileName);

                    bgImage = bg;
                }

                if (fileName.Contains("MarioStar"))
                {
                    Bitmap mStar = new Bitmap(fileName);
                    mStar = new Bitmap(mStar, STAR_SIZE);

                    marioStar = mStar;
                }

                if (fileName.Contains("MarioShip"))
                {
                    Bitmap mShip = new Bitmap(fileName);
                    mShip = new Bitmap(mShip, SHIP_SIZE);

                    marioShip = mShip;
                }

                if (fileName.Contains("MarioFireball"))
                {
                    Bitmap mFire = new Bitmap(fileName);
                    mFire = new Bitmap(mFire, PROJECTILE_SIZE);

                    marioFireball = mFire;
                }

                if (fileName.Contains("MarioBG"))
                {
                    Bitmap mBG = new Bitmap(fileName);

                    marioBG = mBG;
                }
            }
        }

        public void SetMarioBG()
        {
            bgImage = marioBG;
        }

        public Bitmap getBG() { return bgImage; }
        
    }
}
