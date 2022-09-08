using System;
using System.Threading;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;

namespace AMODUS_MultiHack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //offsets
        Memory.Mem AU_Memory = new Memory.Mem();

        //GameAssembly.dll+1BE1DC4 is the playerControllAddress

        string SpeedPointer = "GameAssembly.dll+1BE1DC4,5c,4,14"; //float
        string EmergencyCoolDownPointer = "GameAssembly.dll+1BE1DC4,5c,0,48"; //4 bytes

        byte[] NoShadowFogOriginal = { 0x8B, 0x49, 0x2C, 0xD3, 0xE0 };
        byte[] NoShadowFogOriginal2 = { 0x80, 0x7F, 0x29, 0x00, 0x74 };
        byte[] NoShadowFog = { 0x90, 0x90, 0x90 };
        string NoShadowFogPointer1 = "UnityPlayer.dll+93DDD0"; //byte[] (NoShadowFog)
        string NoShadowFogPointer2 = "GameAssembly.dll+E905DB"; //byte[] (NoShadowFog)
        string NoClipPointer = "UnityPlayer.dll+9B97B7"; //byte (0x0F 0x85 = on, 0x0F 0x84 = off)


        //reveal/hide Impostor && show/hide ghosts and ghost chats
        byte[] RevealImpBytes = { 0x90, 0x90, 0x90, 0x90 };
        byte[] StopRevealImpBytes = { 0x80, 0x78, 0x2C, 0x00 };
        byte[] showGhostsBytes = { 0x80, 0x78, 0x2D, 0x01 };
        byte[] hideGhostsBytes = { 0x80, 0x78, 0x2D, 0x00 };
        byte[] showGhostChatsBytes = { 0x0F, 0x83 };
        byte[] hideGhostChatsBytes = { 0x0F, 0x84 };

        //pointers
        byte[] KillOtherImpostor = { 0x0F, 0x82, 0x59, 0x01, 0x00, 0x00 };
        byte[] KillOtherImpostorReset = { 0x0F, 0x85, 0x59, 0x01, 0x00, 0x00 };

        byte[] InfiniteKillRangeBytes = { 0xC7, 0x44, 0x06, 0x10, 0x00, 0x00, 0x80, 0x7F, 0xF3, 0x0F, 0x10, 0x44, 0x06, 0x10 };
        byte[] InfiniteKillRangeReset = { 0xF3, 0x0F, 0x10, 0x44, 0x86, 0x10 };

        string killOtherImpostorPointer = "GameAssembly.dll+657ADF";
        string IsImpostorPointer = "GameAssembly.dll+1BE1DC4,5c,0,34,2C"; //byte
        string IsDeadPointer = "GameAssembly.dll+1BE1DC4,5c,0,34,2D"; //byte
        string KillCoolDownPointer = "GameAssembly.dll+1326843,5c,4,20"; //bytes (0xF3, 0x0F, 0x11, 0x45, 0x0C - bypass, 0x0F, 0x83, 0xA3, 0x00, 0x00, 0x00 - reset)
        string KillDistancePointer = "GameAssembly.dll+1BE1DC4,5c,4,40"; //4 bytes (2 = long, 1 = medium, 0 = short)
        string CrewMaxVisionPointer = "GameAssembly.dll+1BE1DC4,5c,4,18"; //float
        string ImpostorMaxVisionPointer = "GameAssembly.dll+1BE1DC4,5c,4,1c"; //float
        string InvisibilityPointer = "GameAssembly.dll+1BE1DC4,5c,0,30"; //byte
        string SatelliteCameraPointer1 = "UnityPlayer.dll+0145A24C,C,18,44,7C"; //int ---> 0,1
        string SatelliteCameraPointer2 = "UnityPlayer.dll+0145A24C,C,18,44,78"; //float 0,0.6
        string SatelliteCameraPointer3 = "UnityPlayer.dll+0145A24C,C,18,44,80"; //float 4,1
        string SatelliteCameraPointer4 = "UnityPlayer.dll+0145A24C,C,18,44,84"; //float 4,1
        string ghost = "GameAssembly.dll+673AA2"; // (On = 80 7E 29 01   Off: 80 7E 29 00)
        string ghostChats = "GameAssembly.dll+657EDC"; // (On = 0F 83    Off: 0F 84)

        dynamic jsonfile;

        //modifying save file for removing ban
        static string PathFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
            "\\Appdata\\LocalLow\\Innersloth\\Among Us");
        static string path = Path.Combine(PathFolder, "playerStats2");
        Stream saveFile = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);

        //methods------------------------------------------------------------------------------------------------------------------------------------

        public void enableOverrideEmergencyCooldown()
        {

            AU_Memory.WriteMemory("GameAssembly.dll+1BE1DC4,5c,4,34", "int", "0");
        }

        public void disableOverrideEmergencyCooldown()
        {

            AU_Memory.WriteMemory("GameAssembly.dll+1BE1DC4,5c,4,34", "int", "15");
        }
        public void enableOverrideEmergencyCount()
        {

            AU_Memory.WriteMemory(EmergencyCoolDownPointer, "int", "100");
        }

        public void disableOverrideEmergencyCount()
        {

            AU_Memory.WriteMemory(EmergencyCoolDownPointer, "int", "15");
        }
        public void changeSpeed(string speed)
        {
            AU_Memory.WriteMemory(SpeedPointer, "float", speed);
        }
        public void ForceImpostor()
        {

            AU_Memory.WriteMemory(IsImpostorPointer, "byte", "1");
        }

        public void ForceCrewmate()
        {

            AU_Memory.WriteMemory(IsImpostorPointer, "byte", "0");
        }

        public void revivePlayer()
        {

            AU_Memory.WriteMemory(IsDeadPointer, "byte", "0");
        }

        public void killPlayer()
        {

            AU_Memory.WriteMemory(IsDeadPointer, "byte", "1");
        }

        public void enableOverrideKillCooldown()
        {
            byte[] KillCooldownBypass = { 0xF3, 0x0F, 0x11, 0x45, 0x0C };
            AU_Memory.WriteMemory(KillCoolDownPointer, "bytes", "F3 0F 11 45 0C");
        }

        public void showGhosts()
        {
            AU_Memory.WriteBytes(ghost, showGhostsBytes);
        }
        public void hideGhosts()
        {
            AU_Memory.WriteBytes(ghost, hideGhostsBytes);
        }

        public void showGhostChats()
        {
            AU_Memory.WriteBytes(ghostChats, showGhostChatsBytes);
        }
        public void hideGhostChats()
        {
            AU_Memory.WriteBytes(ghostChats, hideGhostChatsBytes);
        }

        public void disableOverrideKillCooldown()
        {

            AU_Memory.WriteMemory(KillCoolDownPointer, "bytes", "0F 83 A3 00 00 00");
        }

        public void showImpostors()
        {
            AU_Memory.WriteBytes("GameAssembly.dll+1C241C4", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+8edba8", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+8edba9", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+8edbaa", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+8edbab", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+8edbac", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+8edbad", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+CAD551", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+CAD552", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+CAD553", RevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+CAD554", RevealImpBytes);
        }

        public void hideImpostors()
        {
            AU_Memory.WriteBytes("GameAssembly.dll+1C241C4", StopRevealImpBytes);
            //AU_Memory.WriteBytes("GameAssembly.dll+8edba8", StopRevealImpBytes);
        }

        
        void enableRainbowColorHack()
        {
            AU_Memory.WriteMemory(jsonfile["isRainbowHackColor"].ToString("X"), "byte", "1");
        }

        void disableRainbowColorHack()
        {
            AU_Memory.WriteMemory(jsonfile["isRainbowHackColor"].ToString("X"), "byte", "0");
        }
        

        public void enableInfiniteKillDistance()
        {
            string Infrange1 = "GameAssembly.dll+657A0A";
            AU_Memory.CreateCodeCave(Infrange1, InfiniteKillRangeBytes, 6);
            //AU_Memory.WriteBytes("GameAssembly.dll+657C13", KillThroughWalls);
        }

        public void disableInfiniteKillDistance()
        {
            string Infrange1 = "GameAssembly.dll+657A0A";
            AU_Memory.WriteBytes(Infrange1, InfiniteKillRangeReset);
            //AU_Memory.WriteBytes("GameAssembly.dll+657C13", KillThroughWallsReset);
        }

        void enableSatelliteView()
        {
            AU_Memory.WriteMemory(SatelliteCameraPointer1, "int", "0");
            AU_Memory.WriteMemory(SatelliteCameraPointer2, "float", "0.6");
            AU_Memory.WriteMemory(SatelliteCameraPointer3, "float", "2");
            AU_Memory.WriteMemory(SatelliteCameraPointer4, "float", "2");
        }

        void disableSatelliteView()
        {
            AU_Memory.WriteMemory(SatelliteCameraPointer1, "int", "1");
            AU_Memory.WriteMemory(SatelliteCameraPointer2, "float", "0");
            AU_Memory.WriteMemory(SatelliteCameraPointer3, "float", "1");
            AU_Memory.WriteMemory(SatelliteCameraPointer4, "float", "1");
        }

        public void enableMaxVisionCrew()
        {

            AU_Memory.WriteMemory(CrewMaxVisionPointer, "float", "5");
        }
        public void disableMaxVisionCrew()
        {

            AU_Memory.WriteMemory(CrewMaxVisionPointer, "float", "1.5");
        }
        public void enableMaxVisionImpostor()
        {

            AU_Memory.WriteMemory(ImpostorMaxVisionPointer, "float", "5");
        }
        public void disableMaxVisionImpostor()
        {

            AU_Memory.WriteMemory(ImpostorMaxVisionPointer, "float", "1.75");
        }
        public void enableInvisibility()
        {

            AU_Memory.WriteMemory(InvisibilityPointer, "byte", "1");
        }

        public void disableInvisibility()
        {

            AU_Memory.WriteMemory(InvisibilityPointer, "byte", "0");
        }
        
        
        public void fakeMedBayAnim()
        {

            AU_Memory.WriteMemory(jsonfile["isScanner"].ToString("X"), "byte", "1");
        }

        public void fakeTrashAnim()
        {

            AU_Memory.WriteMemory(jsonfile["isTrash"].ToString("X"), "byte", "1");
        }

        public void fakeWeaponsAnim()
        {

            AU_Memory.WriteMemory(jsonfile["isWeapons"].ToString("X"), "byte", "1");
        }
        

        public void enableNoClip()
        {

            AU_Memory.WriteMemory(NoClipPointer, "bytes", "0F 85");
        }

        public void disableNoClip()
        {

            AU_Memory.WriteMemory(NoClipPointer, "bytes", "0F 84");
        }
        public void EnableNoShadow()
        {

            AU_Memory.WriteBytes(NoShadowFogPointer1, NoShadowFog);
            AU_Memory.WriteBytes(NoShadowFogPointer2, NoShadowFog);
        }

        public void disableNoShadow()
        {

            AU_Memory.WriteBytes(NoShadowFogPointer1, NoShadowFogOriginal);
            AU_Memory.WriteBytes(NoShadowFogPointer2, NoShadowFogOriginal2);
        }
        public void removeBan()
        {
            saveFile.Position = 140;
            saveFile.WriteByte(0x00);
            saveFile.Position = 141;
            saveFile.WriteByte(0x00);
        }

        public void enableKillOtherImpostor()
        {
            AU_Memory.WriteBytes(killOtherImpostorPointer, KillOtherImpostor);
        }

        public void disableKillOtherImpostor()
        {
            AU_Memory.WriteBytes(killOtherImpostorPointer, KillOtherImpostorReset);
        }

        public void enableInfiniteSabotage()
        {
            AU_Memory.WriteMemory("GameAssembly.dll+1F1407E", "bytes", "C7 47 08 00 00 00 00");
            AU_Memory.WriteMemory("GameAssembly.dll+1F140B8", "bytes", "C7 47 08 00 00 00 00");
            AU_Memory.WriteMemory("GameAssembly.dll+1F140F6", "bytes", "C7 47 08 00 00 00 00");
            AU_Memory.WriteMemory("GameAssembly.dll+1F14130", "bytes", "C7 47 08 00 00 00 00");
            AU_Memory.WriteMemory("GameAssembly.dll+1F1419C", "bytes", "C7 47 08 00 00 00 00");
            AU_Memory.WriteMemory("GameAssembly.dll+1F1419C", "bytes", "C7 47 38 00 00 00 00");
        }

        public void disableInfiniteSabotage()
        {
            AU_Memory.WriteMemory("GameAssembly.dll+1F1407E", "bytes", "C7 47 08 00 00 F0 41");
            AU_Memory.WriteMemory("GameAssembly.dll+1F140B8", "bytes", "C7 47 08 00 00 F0 41");
            AU_Memory.WriteMemory("GameAssembly.dll+1F140F6", "bytes", "C7 47 08 00 00 F0 41");
            AU_Memory.WriteMemory("GameAssembly.dll+1F14130", "bytes", "C7 47 08 00 00 F0 41");
            AU_Memory.WriteMemory("GameAssembly.dll+1F1419C", "bytes", "C7 47 08 00 00 F0 41");
            AU_Memory.WriteMemory("GameAssembly.dll+1F1419C", "bytes", "C7 47 38 00 00 F0 41");
        }

        
        public void sabotageLights()
        {
            AU_Memory.WriteMemory(jsonfile["isLightSabotaged"].ToString("X"), "byte", "1");
        }

        public void sabotageCommunications()
        {
            AU_Memory.WriteMemory(jsonfile["isCommunicationSabotaged"].ToString("X"), "byte", "1");
        }

        public void sabotageO2()
        {
            AU_Memory.WriteMemory(jsonfile["is02Sabotaged"].ToString("X"), "byte", "1");
        }

        public void sabotageReactor()
        {
            AU_Memory.WriteMemory(jsonfile["isReactorSabotaged"].ToString("X"), "byte", "1");
        }
        

        public void executeInjector()
        {
            Process.Start(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\AMODUS_INJECTOR");
        }
        
        public void refreshPlayerStats()
        {
            string[] playerNamePointers = { "GameAssembly.dll+1BE1DC4,5C,8,8,34,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,14,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,18,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,1C,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,20,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,24,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,28,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,2C,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,30,34,C,C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,10,34,C,C"
            };

            string[] playerRolePointers = { "GameAssembly.dll+1BE1DC4,5C,8,8,34,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,14,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,18,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,1C,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,20,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,24,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,28,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,2C,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,30,34,2C",
                "GameAssembly.dll+1BE1DC4,5C,8,8,10,34,2C"
            };

            string[] playerDeadOrAlive = { "GameAssembly.dll+1BE1DC4,5C,8,8,34,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,14,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,18,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,1C,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,20,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,24,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,28,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,2C,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,30,34,2D",
                "GameAssembly.dll+1BE1DC4,5C,8,8,10,34,2D"
            };

            string[] playerColorID = { "GameAssembly.dll+1BE1DC4,5C,8,8,34,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,14,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,18,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,1C,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,20,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,24,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,28,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,2C,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,30,34,14",
                "GameAssembly.dll+1BE1DC4,5C,8,8,10,34,14"
            };


            Border[] statBorderColors = { p1Color,
                p2Color ,
                p3Color ,
                p4Color ,
                p5Color ,
                p6Color ,
                p7Color ,
                p8Color ,
                p9Color ,
                p10Color
            };

            Border[] statBorders = { P1Stat,
                P2Stat,
                P3Stat ,
                P4Stat ,
                P5Stat ,
                P6Stat ,
                P7Stat ,
                P8Stat ,
                P9Stat ,
                P10Stat
            };

            TextBlock[] playerNames = { Player1Name, 
                Player2Name, 
                Player3Name , 
                Player4Name , 
                Player5Name , 
                Player6Name , 
                Player7Name , 
                Player8Name , 
                Player9Name , 
                Player10Name 
            };

            TextBlock[] playerRoles = { Player1Role,
                Player2Role ,
                Player3Role ,
                Player4Role ,
                Player5Role ,
                Player6Role ,
                Player7Role ,
                Player8Role ,
                Player9Role ,
                Player10Role
            };

            int playerStatCount = 0;

            for (int i = 0; i < 10; i++)
            {
                playerNames[i].Text = "??????????";
                playerRoles[i].Text = "????????";
                playerRoles[i].Foreground = Brushes.Black;
                statBorders[i].Visibility = Visibility.Hidden;
            }

            for (int i = 0; i < 10; i++)
            {
                int playerRole = AU_Memory.ReadInt(playerRolePointers[i]);
                int isPlayerDead = AU_Memory.ReadByte(playerDeadOrAlive[i]);
                int playerColor = AU_Memory.ReadInt(playerColorID[i]);
                byte[] unicodeName = AU_Memory.ReadBytes(playerNamePointers[i].ToString(), 20);
                if (unicodeName == null)
                {
                    playerNames[playerStatCount].Text = "??????????";
                    playerRoles[playerStatCount].Text = "????????";
                    playerRoles[playerStatCount].Foreground = Brushes.Black;
                    continue;
                }
                string dirtyName = Encoding.Unicode.GetString(unicodeName);
                var endOfStringPosition = dirtyName.IndexOf("\0");
                string cleanName = endOfStringPosition == -1 ? dirtyName : dirtyName.Substring(0, endOfStringPosition);
                
                statBorders[playerStatCount].Visibility = Visibility.Visible;

                playerNames[playerStatCount].Text = cleanName;

                if (playerRole != 0)
                {
                    playerRoles[playerStatCount].Text = "Impostor";
                    playerRoles[playerStatCount].Foreground = Brushes.Red;
                }
                else
                {
                    playerRoles[playerStatCount].Text = "Crewmate";
                    playerRoles[playerStatCount].Foreground = Brushes.LightGreen;

                }

                if (isPlayerDead != 0)
                {
                    playerRoles[playerStatCount].Text = "Dead";
                    playerRoles[playerStatCount].Foreground = Brushes.Black;
                }

                if (playerColor == 0)
                {
                    statBorderColors[playerStatCount].Background = Brushes.Red;
                }
                else if (playerColor == 1)
                {
                    statBorderColors[playerStatCount].Background = Brushes.Blue;
                }
                else if (playerColor == 2)
                {
                    statBorderColors[playerStatCount].Background = Brushes.DarkGreen;
                }
                else if (playerColor == 3)
                {
                    statBorderColors[playerStatCount].Background = Brushes.HotPink;
                }
                else if (playerColor == 4)
                {
                    statBorderColors[playerStatCount].Background = Brushes.OrangeRed;
                }
                else if (playerColor == 5)
                {
                    statBorderColors[playerStatCount].Background = Brushes.Yellow;
                    //playerNames[playerStatCount].Foreground = Brushes.Black;
                }
                else if (playerColor == 6)
                {
                    statBorderColors[playerStatCount].Background = Brushes.Black;
                }
                else if (playerColor == 7)
                {
                    statBorderColors[playerStatCount].Background = Brushes.White;
                }
                else if (playerColor == 8)
                {
                    statBorderColors[playerStatCount].Background = Brushes.Purple;
                }
                else if (playerColor == 9)
                {
                    statBorderColors[playerStatCount].Background = Brushes.SaddleBrown;
                }
                else if (playerColor == 10)
                {
                    statBorderColors[playerStatCount].Background = Brushes.Cyan;
                }
                else if (playerColor == 11)
                {
                    statBorderColors[playerStatCount].Background = Brushes.Lime;
                }

                playerStatCount += 1;
            }
        }


        //events-------------------------------------------------------------------------------------------------------------------------------------------------------
        public MainWindow()
        {
            executeInjector();
            InitializeComponent();

            try
            {
                AU_Memory.OpenProcess(Process.GetProcessesByName("Among Us").FirstOrDefault().Id);
            }
            catch (Exception)
            {
                MessageBox.Show("Please start Among Us first");
                Application.Current.Shutdown();
            }
            TabControl.SelectedIndex = 0;
            AU_Memory.OpenProcess(Process.GetProcessesByName("Among Us").FirstOrDefault().Id);
            
            //init json
            string jsonPath = Path.GetDirectoryName(Directory.GetCurrentDirectory().ToString()) + "\\AMODUS V6Addresses.json";
            jsonfile = JsonConvert.DeserializeObject(File.ReadAllText(jsonPath));
        }

        private void TitleWindowDrag(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CallGeneralTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 0;
        }

        private void CallPlayerTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 1;
        }

        private void CallAppearanceTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 2;
        }

        private void CallMapTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 3;
        }
        private void CallGameTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 4;
        }

        private void CallStatsTab(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 5;
        }

        private void UI_Loaded(object sender, RoutedEventArgs e)
        {
            Thread PlayerStatsUpdater = new Thread(statsCheckerCall);
            PlayerStatsUpdater.Start();
        }

        private void statsCheckerCall()
        {
            Thread.Sleep(1000);
            while (true)
            {
                Application.Current.Dispatcher.Invoke(refreshPlayerStats);
                Thread.Sleep(1000);
            }
        }


        //general Tab-------------------------------------------------------------------------------------------------------------
        private void Speed_slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            changeSpeed(Speed_slider.Value.ToString());
            Speed_Textbox.Text = Speed_slider.Value.ToString();
        }

        private void Speed_Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Speed_Textbox.Text != "")
            {
                if (Int32.Parse(Speed_Textbox.Text.ToString()) > 30)
                {
                    changeSpeed(Speed_Textbox.Text.ToString());
                    Speed_slider.Value = Int32.Parse(Speed_slider.Maximum.ToString());
                }
                else
                {
                    changeSpeed(Speed_Textbox.Text.ToString());
                    Speed_slider.Value = Int32.Parse(Speed_Textbox.Text.ToString());
                }
            }
        }

        private void EmergencyCountToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableOverrideEmergencyCount();
        }

        private void EmergencyCountToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableOverrideEmergencyCount();
        }

        private void EmergencyCooldownToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableOverrideEmergencyCooldown();
        }

        private void EmergencyCooldownToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableOverrideEmergencyCooldown();
        }

        private void SatelliteViewToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableSatelliteView();
            enableMaxVisionCrew();
            enableMaxVisionImpostor();
            EnableNoShadow();
        }

        private void SatelliteViewToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableSatelliteView();
            disableNoShadow();
        }

        private void changePlayerName(object sender, RoutedEventArgs e)
        {
            char[] splitted = setName_textBox.Text.ToCharArray();
            foreach (char ch in splitted)
            {
                string charactbyte = ((byte)ch).ToString("X");

                if (splitted.Length >= 1)
                {
                    if (ch == splitted[0])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+6434", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+6435", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+6434", "byte", "00");
                }
                if (splitted.Length >= 2)
                {
                    if (ch == splitted[1])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+6436", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+6437", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+6436", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+6437", "byte", "00");
                }

                if (splitted.Length >= 3)
                {
                    if (ch == splitted[2])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+6438", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+6439", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+6438", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+6439", "byte", "00");
                }

                if (splitted.Length >= 4)
                {
                    if (ch == splitted[3])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+643A", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+643B", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+643A", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+643B", "byte", "00");
                }
                if (splitted.Length >= 5)
                {
                    if (ch == splitted[4])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+643C", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+643D", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+643C", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+643D", "byte", "00");
                }
                if (splitted.Length >= 6)
                {
                    if (ch == splitted[5])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+643E", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+643F", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+643E", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+643F", "byte", "00");
                }
                if (splitted.Length >= 7)
                {
                    if (ch == splitted[6])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+6440", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+6441", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+6440", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+6441", "byte", "00");
                }
                if (splitted.Length >= 8)
                {
                    if (ch == splitted[7])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+6442", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+6443", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+6442", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+6443", "byte", "00");
                }
                if (splitted.Length >= 9)
                {
                    if (ch == splitted[8])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+6444", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+6445", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+6444", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+6445", "byte", "00");
                }
                if (splitted.Length == 10)
                {
                    if (ch == splitted[9])
                    {
                        AU_Memory.WriteMemory("AMODUS.dll+6446", "byte", charactbyte);
                        AU_Memory.WriteMemory("AMODUS.dll+6447", "byte", "00");
                    }
                }
                else
                {
                    AU_Memory.WriteMemory("AMODUS.dll+6446", "byte", "00");
                    AU_Memory.WriteMemory("AMODUS.dll+6447", "byte", "00");
                }
            }

            AU_Memory.WriteMemory(jsonfile["isChangeNameTriggered"].ToString("X"), "byte", "1"); //----------------------------------------------------------------------------------------------------
        }

        private void changeEveryoneName(object sender, RoutedEventArgs e)
        {
            char[] splitted = ChangeLobbyNamesButton_Textbox.Text.ToCharArray();
            int MoonCounter = 0;
            /*
            if (splitted.Length > 10)
            {
                for (int U = splitted.Length - 1; U > 9; --U)
                {
                    int indexToRemove = U;
                    splitted = splitted.Where((source, index) => index != indexToRemove).ToArray();
                }
            }
            */
            for (int B = 0; B < ((splitted.Length * 2) + 1); B += 2)
            {
                string address_for_clean_up = (int.Parse(jsonfile["name_container4"].ToString("X"), System.Globalization.NumberStyles.HexNumber) + B).ToString("X");
                AU_Memory.WriteMemory(address_for_clean_up, "byte", "00");
            }
            foreach (char ch in splitted)
            {
                string charactbyte = ((byte)ch).ToString("X");
                string full_address_of_name_container = (int.Parse(jsonfile["name_container4"].ToString("X"), System.Globalization.NumberStyles.HexNumber) + MoonCounter).ToString("X");
                AU_Memory.WriteMemory(full_address_of_name_container, "byte", charactbyte);
                MoonCounter += 2;
            }

            AU_Memory.WriteMemory(jsonfile["isEveryoneSameName"].ToString("X"), "byte", "1");
        }

        //Player Tab-------------------------------------------------------------------------------------------------------------
        private void ForceImpostorToggle_Checked(object sender, RoutedEventArgs e)
        {
            ForceImpostor();
            ForceCrewmateToggle.IsChecked = false;
        }

        private void ForceImpostorToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ForceCrewmate();
            ForceCrewmateToggle.IsChecked = true;
        }

        private void RevealImpostorsToggle_Checked(object sender, RoutedEventArgs e)
        {
            showImpostors();
        }

        private void RevealImpostorsToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            hideImpostors();
        }

        private void ForceCrewmateToggle_Checked(object sender, RoutedEventArgs e)
        {
            ForceCrewmate();
            ForceImpostorToggle.IsChecked = false;
        }

        private void ForceCrewmateToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ForceImpostor();
            ForceImpostorToggle.IsChecked = true;
        }

        private void KillOtherImpostor_Checked(object sender, RoutedEventArgs e)
        {
            enableKillOtherImpostor();
        }

        private void KillOtherImpostor_Unchecked(object sender, RoutedEventArgs e)
        {
            disableKillOtherImpostor();
        }

        private void InvisibilityToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableInvisibility();
        }

        private void InvisibilityToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableInvisibility();
        }

        private void KillCooldownToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableOverrideKillCooldown();
        }

        private void KillCooldownToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableOverrideKillCooldown();
        }

        private void KillDistanceToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableInfiniteKillDistance();
        }

        private void KillDistanceToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableInfiniteKillDistance();
        }

        private void KillPlayerToggle_Checked(object sender, RoutedEventArgs e)
        {
            killPlayer();
            RevivePlayerToggle.IsChecked = false;
        }

        private void KillPlayerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            revivePlayer();
            RevivePlayerToggle.IsChecked = true;
        }

        private void RevivePlayerToggle_Checked(object sender, RoutedEventArgs e)
        {
            revivePlayer();
            KillPlayerToggle.IsChecked = false;
        }

        private void RevivePlayerToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            killPlayer();
            KillPlayerToggle.IsChecked = true;
        }

        private void OverrideCrewVisionToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableMaxVisionCrew();
        }

        private void OverrideCrewVisionToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableMaxVisionCrew();
        }

        private void OverrideImposterVisionToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableMaxVisionImpostor();
        }

        private void OverrideImposterVisionToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableMaxVisionImpostor();
        }

        private void ShowGhosts_Checked(object sender, RoutedEventArgs e)
        {
            showGhosts();
        }

        private void ShowGhosts_Unchecked(object sender, RoutedEventArgs e)
        {
            hideGhosts();
        }

        private void ShowGhostChats_Checked(object sender, RoutedEventArgs e)
        {
            showGhostChats();
        }

        private void ShowGhostChats_Unchecked(object sender, RoutedEventArgs e)
        {
            hideGhostChats();
        }






        //Appearance Tab-------------------------------------------------------------------------------------------------------------

        private void RainbowHackToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableRainbowColorHack();
        }

        private void RainbowHackToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableRainbowColorHack();
        }

        private void Red_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "0");
            MessageBox.Show(AU_Memory.ReadInt("int", jsonfile["targetPlayerColorID"].ToString("X")).ToString());
        }

        private void Blue_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "1");
            MessageBox.Show(AU_Memory.ReadInt("int", jsonfile["targetPlayerColorID"].ToString("X")).ToString());
        }

        private void DGreen_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "2");
        }

        private void Pink_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "3");
        }

        private void Orange_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "4");
        }

        private void Yellow_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "5");
        }

        private void Black_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "6");
        }

        private void White_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "7");
        }

        private void Purple_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "8");
        }

        private void Brown_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "9");
        }

        private void Cyan_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "10");
        }

        private void LGreen_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["targetPlayerColorID"].ToString("X"), "int", "11");
        }

        private void ChangeLobbyColorButton_Click(object sender, RoutedEventArgs e)
        {
            AU_Memory.WriteMemory(jsonfile["isEveryoneSameColor"].ToString("X"), "byte", "1");
        }






        //Map Tab-------------------------------------------------------------------------------------------------------------
        private void NoClipToggle_Checked(object sender, RoutedEventArgs e)
        {
            enableNoClip();
        }

        private void NoClipToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableNoClip();
        }

        private void NoShadowToggle_Checked(object sender, RoutedEventArgs e)
        {
            EnableNoShadow();
        }

        private void NoShadowToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            disableNoShadow();
        }

        private void MedbayScanButton_Click(object sender, RoutedEventArgs e)
        {
            fakeMedBayAnim();
        }

        private void TrashButton_Click(object sender, RoutedEventArgs e)
        {
            fakeTrashAnim();
        }

        private void WeaponsButton_Click(object sender, RoutedEventArgs e)
        {
            fakeWeaponsAnim();
        }

        private void InfSabotage_Checked(object sender, RoutedEventArgs e)
        {
            enableInfiniteSabotage();
        }

        private void InfSabotage_Unchecked(object sender, RoutedEventArgs e)
        {
            disableInfiniteSabotage();
        }

        
        private void SabotageLights_Click(object sender, RoutedEventArgs e)
        {
            sabotageLights();
        }

        private void SabotageCommunications_Click(object sender, RoutedEventArgs e)
        {
            sabotageCommunications();
        }

        private void SabotageO2_Click(object sender, RoutedEventArgs e)
        {
            sabotageO2();
        }

        private void SabotageReactor_Click(object sender, RoutedEventArgs e)
        {
            sabotageReactor();
        }




        //Game Tab-------------------------------------------------------------------------------------------------------------
        private void RemoveBan_Click(object sender, RoutedEventArgs e)
        {
            removeBan();
        }
    }
}
