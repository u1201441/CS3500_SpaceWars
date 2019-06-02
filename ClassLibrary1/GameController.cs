// Written by Christopher Jones and John Jacobson

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpaceWars
{
    /// <summary>
    /// This class is responsible for handling all information recieved from the Network Controller,
    /// communicating with the world and the view, and deciding what messages are sent to the server.
    /// </summary>
    public class GameController
    {
        public World theWorld = new World();
        private string stringToSend;
        SocketState ss;

        // Bools to track once JSons will be sent to the controller
        private bool connectedID = false;
        private bool connectedWorldSize = false;

        public int worldSize;
        private bool isMario = false;
        private int playerID;
        private string playerName;

        /// <summary>
        /// Default constructor for a GameController object
        /// </summary>
        public GameController()
        {
        }

        /// <summary>
        /// This method allows the user to connect to the server from anywhere a Game Controller object
        /// exsits. It gives the Network Controller an IP address and a player name to send to the server.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="userName"></param>
        public void ConnectToServer(string hostname, string userName)
        {
            playerName = userName;
            if (playerName == "Mario" || playerName == "mario")
                isMario = true;
            Network.ConnectToServer(SendName, hostname);
        }

        /// <summary>
        /// This method is a delegate method called by the Network Controller to send the player's name to
        /// the server, update the delegate to the next step, and request more data from the server through
        /// the Network Controller.
        /// </summary>
        /// <param name="ss"></param>
        public void SendName(SocketState ss)
        {
            if(playerName[playerName.Length - 1] != '\n')
            {
                playerName = playerName + "\n";
            }

            ss.callMe = FirstContact;
            Network.Send(ss.theSocket, playerName);
            Network.GetData(ss);
        }

        /// <summary>
        /// This method is a delegate method called by the Network Controller to process the initial data 
        /// sent by the server. It initializes the player's ID and world size, updates the delegate to the 
        /// next step, and requests more data from the server through the Network Controller.
        /// </summary>
        /// <param name="ss"></param>
        public void FirstContact(SocketState ss)
        {
            this.ss = ss;
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

                if (!connectedID)
                {
                    playerID = int.Parse(p);
                    connectedID = true;
                    ss.sb.Remove(0, p.Length);
                    theWorld.setPlayerID(playerID);
                    theWorld.EnableMarioMode(isMario);
                    continue;
                }

                if (!connectedWorldSize)
                {
                    worldSize = int.Parse(p);
                    connectedWorldSize = true;
                    ss.sb.Remove(0, p.Length);
                    continue;
                }

                ss.sb.Remove(0, p.Length);
            }

            ss.callMe = ProcessData;
            Network.GetData(ss);
        }

        /// <summary>
        /// This method is the final delegate method given to the Network Controller to process the 
        /// data from the server. It gets all server information about the world and updates both the
        /// view and the world.
        /// </summary>
        /// <param name="ss"></param>
        public void ProcessData(SocketState ss)
        {
            lock (this)
            {
                this.ss = ss;
                string totalData = ss.sb.ToString();
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
                JObject blankJSonObject;

                string shipToken = "thrust";
                string projectileToken = "owner";
                string starToken = "mass";

                // Loop until we have processed all messages.
                // We may have received more than one.
                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;

                    blankJSonObject = JObject.Parse(p);

                    JToken token = blankJSonObject[shipToken];

                    if (token != null)
                    {
                        Ship newShip = JsonConvert.DeserializeObject<Ship>(p);
                        theWorld.addShip(newShip);
                    }

                    token = blankJSonObject[projectileToken];

                    if (token != null)
                    {
                        Projectile newProjectile = JsonConvert.DeserializeObject<Projectile>(p);
                        if (!newProjectile.GetAlive())
                        {
                            theWorld.removeProjectile(newProjectile);
                        }
                        else
                        {
                            theWorld.addProjectile(newProjectile);
                        }
                    }

                    token = blankJSonObject[starToken];

                    if (token != null)
                    {
                        Star newStar = JsonConvert.DeserializeObject<Star>(p);
                        theWorld.addStar(newStar);

                    }
                    // Then remove it from the SocketState's growable buffer
                    ss.sb.Remove(0, p.Length);

                }

                // Call event to refresh view.
                UpdateArrived();
                if (stringToSend != null)
                {
                    UserInput(stringToSend);
                }
                Network.GetData(ss);
            }

        }

        public delegate void ServerUpdateHandler();
        private event ServerUpdateHandler UpdateArrived;

        /// <summary>
        /// Event handler that updates the view.
        /// </summary>
        /// <param name="h"></param>
        public void RegisterServerUpdateHandler(ServerUpdateHandler h)
        {
            UpdateArrived += h;
        }

        /// <summary>
        /// This method is called to send data to the server through the Network Controller.
        /// </summary>
        /// <param name="s"></param>
        public void UserInput(string s)
        {
            stringToSend = s;
            Network.Send(ss.theSocket, stringToSend + "\n");
        }

        public int GetPlayerID()
        {
            return playerID;
        }

        public bool GetIsMario()
        {
            return isMario;
        }
    }
}
