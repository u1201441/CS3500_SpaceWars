// Written by Christopher Jones and John Jacobson


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceWars
{
    /// <summary>
    /// This class contains the window form and all of its functionality.
    /// </summary>
    public partial class Form1 : Form
    {
        // The controller handles updates from the "server"
        // and notifies us via an event
        private GameController theController;

        // World is a simple container for ships, stars, and projectiles
        private World theWorld;

        // The string storing which keys are pressed on a given frame
        private string keys;

        // List used for sorting players based on their ranks.
        List<Ship> playerRanks;

        // Dictionary storing player name labels for scoreboard.
        Dictionary<int, Label> labelDict;

        // Dictionary storing player healthbars for scoreboard.
        Dictionary<int, Rectangle> healthDict;

        // List used for storing display ship sprites
        List<Bitmap> shipSprites;

        // Size of display sprites
        readonly Size DISPLAY_SIZE;

        // The panel that displays the world.
        DrawingPanel drawingPanel;

        // The sprite for the special "Mario Mode"
        private Bitmap marioShip;

        // Indicates that the client window has been properly resized to match
        // the world.
        private bool hasResized;

        /// <summary>
        /// Initializes the form and contains the event listeners.
        /// </summary>
        /// <param name="ctl"></param>
        public Form1(GameController ctl)
        {
            // First initialize all the fields and components
            InitializeComponent();

            theController = ctl;
            theWorld = theController.theWorld;
            labelDict = new Dictionary<int, Label>();
            healthDict = new Dictionary<int, Rectangle>();
            playerRanks = new List<Ship>();
            shipSprites = new List<Bitmap>();
            keys = "";
            DISPLAY_SIZE = new Size(20, 20);

            // Next set up all the event listeners.
            theController.RegisterServerUpdateHandler(UpdateWorld);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.KeyUp += new KeyEventHandler(Form1_KeyUp);
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            this.KeyPreview = true;

            // Then draw the client and drawing panel at their default size
            ClientSize = new Size(950, 800);
            drawingPanel = new DrawingPanel(theWorld);
            drawingPanel.Location = new Point(0, 50);
            drawingPanel.Size = new Size(750, 750);
            drawingPanel.BackColor = Color.Black;
            this.Controls.Add(drawingPanel);

            // Finally, Initialize the sprite list and Mario ship
            string[] filePaths = Directory.GetFiles(@"..\..\..\Resource\Assets\", "*.*", SearchOption.TopDirectoryOnly);

            foreach (String fileName in filePaths)
            {
                if (fileName.Contains("coast"))
                {
                    Bitmap ship = new Bitmap(fileName);
                    ship = new Bitmap(ship, DISPLAY_SIZE);

                    shipSprites.Add(ship);
                }

                if (fileName.Contains("MarioScoreSprite"))
                {
                    Bitmap mShip = new Bitmap(fileName);
                    mShip = new Bitmap(mShip, DISPLAY_SIZE);

                    marioShip = mShip;
                }
            }
        }

        /// <summary>
        /// This method keeps the scoreboard accurate and properly updated.
        /// </summary>
        private void UpdateScoreboard()
        {
            Label playerNameLabel;
            Rectangle healthbar;

            int nextLabelX = theController.worldSize;
            int nextLabelY = 50;

            Dictionary<int, Ship> players = (Dictionary<int, Ship>)theWorld.getPlayers();

            // Start sorting the players according to their scores.
            foreach (KeyValuePair<int, Ship> ship in players)
            {
                playerRanks.Add(ship.Value);
            }

            playerRanks.Sort();
            players.Clear();

            foreach (Ship ship in playerRanks)
            {
                players.Add(ship.GetID(), ship);
            }

            playerRanks.Clear();

            // Now add or update the player names, scores, and health bars.
            foreach (KeyValuePair<int, Ship> ship in players)
            {
                if (!labelDict.ContainsKey(ship.Value.GetID()))
                {
                    // First create new label
                    playerNameLabel = new Label();
                    playerNameLabel.Font = new Font("Arial", 14, FontStyle.Regular);
                    playerNameLabel.Name = "playerLabel" + ship.Value.GetID();
                    playerNameLabel.Location = new Point(nextLabelX, nextLabelY);
                    playerNameLabel.Text = "" + ship.Value.GetName() + ": " + ship.Value.GetScore();
                    playerNameLabel.Size = new Size(185, 25);
                    playerNameLabel.AutoSize = true;

                    labelDict.Add(ship.Value.GetID(), playerNameLabel);

                    this.Controls.Add(labelDict[ship.Value.GetID()]);

                    // Next create new healthbar
                    healthbar = new Rectangle();
                    healthbar.Height = 25;
                    healthbar.Width = 125;
                    healthbar.Location = new Point(nextLabelX, nextLabelY + 25);

                    healthDict.Add(ship.Value.GetID(), healthbar);

                    nextLabelY += 75;
                }
                // If the player name and score are already on the screen, update their value and location.
                else
                {
                    Label updateLabel = new Label();
                    labelDict.TryGetValue(ship.Value.GetID(), out updateLabel);
                    updateLabel.Text = "" + ship.Value.GetName() + ": " + ship.Value.GetScore();
                    updateLabel.Location = new Point(nextLabelX, nextLabelY);
                    nextLabelY += 75;
                }
            }
        }

        /// <summary>
        /// This method tells the window to redraw all of its components.
        /// </summary>
        private void UpdateWorld()
        {
            // Don't try to redraw if the window doesn't exist yet.
            if (!IsHandleCreated)
                return;

            // If the client has connected to the server, received a valid world size, and the
            // window has not resized, draw the new window and world size.
            if (!serverField.Enabled && theController.worldSize > 0 && !hasResized)
            {
                MethodInvoker mn = new MethodInvoker(() =>
                {
                    ClientSize = new Size(theController.worldSize + 200, theController.worldSize + 50);
                    drawingPanel.Size = new Size(theController.worldSize, theController.worldSize);

                    if(theWorld.GetMarioMode()) { drawingPanel.SetMarioBG(); }

                   // Bitmap bgImage = drawingPanel.getBG();
                   // bgImage = new Bitmap(bgImage, new Size(theController.worldSize, theController.worldSize));
                    //drawingPanel.BackgroundImage = bgImage;
                });
                this.Invoke(mn);

                hasResized = true;
            }

            MethodInvoker scoreInvoker = new MethodInvoker(this.UpdateScoreboard);
            this.Invoke(scoreInvoker);

            // Invalidate this form and all its children
            // This will cause the form to redraw as soon as it can
            MethodInvoker m = new MethodInvoker(() =>
            {
                this.Invalidate(true);
            });
            this.Invoke(m);
        }

        /// <summary>
        /// This method is called when the user tries to connect to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serverBtn_Click(object sender, EventArgs e)
        {
            theController.ConnectToServer(serverField.Text, nameField.Text);

            serverBtn.Enabled = false;
            nameField.Enabled = false;
            serverField.Enabled = false;
        }

        /// <summary>
        /// This is the event that runs when a key is pressed down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.Left:
                    keys = keys.Insert(0, "L");
                    break;
                case Keys.Right:
                    keys = keys.Insert(0, "R");
                    break;
                case Keys.Space:
                case Keys.F:
                    keys = keys.Insert(0, "F");
                    break;
                case Keys.Up:
                case Keys.T:
                    keys = keys.Insert(0, "T");
                    break;
            }

            if (!serverField.Enabled)
            {
                theController.UserInput("(" + keys + ")");
            }


        }

        /// <summary>
        /// This is the event that runs when a key is released.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    keys = keys.Replace("L", "");
                    break;
                case Keys.Right:
                    keys = keys.Replace("R", "");
                    break;
                case Keys.Space:
                case Keys.F:
                    keys = keys.Replace("F", "");
                    break;
                case Keys.Up:
                case Keys.T:
                    keys = keys.Replace("T", "");
                    break;
            }

            if (!serverField.Enabled)
            {
                theController.UserInput("(" + keys + ")");
            }

        }

        /// <summary>
        /// Method that runs when the user tries to close the application. Checks if any changes have been made without saving and displays a text
        /// box that asks if the user wants to exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!serverField.Enabled)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to disconnect from the current game?", "Quit?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// This method handles drawing the health bar rectangles on every frame.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            this.DoubleBuffered = true;
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Dictionary<int, Ship> players = (Dictionary<int, Ship>)theWorld.getPlayers();

            int nextLabelX = theController.worldSize;
            int nextLabelY = 50;

            foreach (KeyValuePair<int, Ship> ship in players)
            {
                playerRanks.Add(ship.Value);
            }

            playerRanks.Sort();

            players.Clear();

            foreach (Ship ship in playerRanks)
            {
                players.Add(ship.GetID(), ship);
            }

            playerRanks.Clear();

            Dictionary<int, Rectangle> healthbars = new Dictionary<int, Rectangle>(healthDict);

            foreach (KeyValuePair<int, Ship> ship in players)
            {
                using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
                using (System.Drawing.Pen blackPen = new Pen(Color.Black))
                {
                    Rectangle updatedHealthBar = new Rectangle();
                    healthDict.TryGetValue(ship.Value.GetID(), out updatedHealthBar);
                    updatedHealthBar.Location = new Point(nextLabelX, nextLabelY + 25);
                    e.Graphics.DrawRectangle(blackPen, updatedHealthBar);
                    updatedHealthBar.Width = (ship.Value.GetHP() * 25) + 1;
                    e.Graphics.FillRectangle(greenBrush, updatedHealthBar);

                    // Create and add display ships
                    Bitmap displayShip;
                    if (theWorld.GetMarioMode() && ship.Value.GetID() == theWorld.GetCurrentPlayer())
                    {
                        displayShip = marioShip;
                    }
                    else
                    {
                        displayShip = shipSprites[ship.Value.GetID() % 8];  // There are 8 sprites whose color is directly correlated to the ship's ID
                    }

                    e.Graphics.DrawImage(displayShip, updatedHealthBar.Location.X + 145, updatedHealthBar.Location.Y - 10);

                    nextLabelY += 75;
                }
            }
        }

        private void READMEToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("..\\..\\..\\Resource\\README.txt");
        }
    }
}