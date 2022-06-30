using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Harmony;
using System.Reflection;
using System.Collections;
using System.Net.Sockets;
using UnityEngine.Events;

namespace WebpageTelemetry
{
      
    static class Globals
    {

        public static string projectName = "Webpage Telemetry";
        public static string projectAuthor = "Averaldo";
        public static string projectVersion = "v1.3";
        public static string path = "VTOLVR_ModLoader\\mods\\Webpage_Telemetry\\";

    }
    public class Main : VTOLMOD
    {
        public bool runlogger;
        public VTOLAPI vtolmod_api;
        public DataGetters dataGetter;
        public rwr rwrGetter;
        public int iterator;
        public Settings settings;
        public UnityAction<bool> rwrChanged;
        public UnityAction<bool> dataChanged;
        public bool rwrEnabled = true;
        public bool dataEnabled = true;
        public static Actor playerActor;
        public static Actor lockerActor;
        public static GameObject playerVehicle;
        public static ModuleRWR rWR;
        public static DashRWR dashRWR;
        public static FuelTank tank;
        public static MissileDetector md;

        // This method is run once, when the Mod Loader is done initialising this game object
        public override void ModLoaded()
        {
            //This is an event the VTOLAPI calls when the game is done loading a scene
            VTOLAPI.SceneLoaded += SceneLoaded;
            base.ModLoaded();

            settings = new Settings(this);

            dataChanged += dataEnabledChange;
            settings.CreateCustomLabel("Enable data getter");
            settings.CreateBoolSetting("Enable", dataChanged, dataEnabled);

            rwrChanged += rwrEnabledChange;
            settings.CreateCustomLabel("enable RWR[notworking]");
            settings.CreateBoolSetting("Enable", rwrChanged, rwrEnabled);

            VTOLAPI.CreateSettingsMenu(settings);

            Debug.Log($"{Globals.projectName} - Mod {Globals.projectVersion} by {Globals.projectAuthor} loaded!");

        }

        public void dataEnabledChange(bool newval)
        {
            dataEnabled = newval;
        }
        public void rwrEnabledChange(bool newval)
        {
            rwrEnabled = newval;
        }

        public void ResetLogger()
        {
            runlogger = false;
            dataGetter.clearData(rwrEnabled);
            Start();
        }

        public void Start()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Telemetry.logger.logger");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            vtolmod_api = VTOLAPI.instance;

            System.IO.File.Create(Globals.path + "status.txt");
            dataGetter = new DataGetters(this);
        }

        //This method is called every frame by Unity. Here you'll probably put most of your code
        void Update()
        {
        }

        //This method is like update but it's framerate independent. This means it gets called at a set time interval instead of every frame. This is useful for physics calculations
        void FixedUpdate()
        {
            if (iterator < 12)
            {
                iterator++;
            }
            else
            {
                iterator = 0;

                if (runlogger)
                {
                    if (dataEnabled)
                    {
                        dataGetter.GetData(rwrEnabled);
                    }

                    VTOLScenes sceneLoop = VTOLAPI.currentScene;
                    switch (sceneLoop)
                    {
                        case VTOLScenes.Akutan:
                        case VTOLScenes.CustomMapBase:
                            break;
                        default:
                            ResetLogger();
                            break;
                    }
                }
            }
        }

        public void startLog()
        {
            runlogger = true;
            playerActor = FlightSceneManager.instance.playerActor;
            rWR = playerActor.GetComponentInChildren<ModuleRWR>();
            dashRWR = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<DashRWR>();
            playerVehicle = VTOLAPI.GetPlayersVehicleGameObject();
            md = playerActor.GetComponentInChildren<MissileDetector>();
            tank = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<FuelTank>();
        }

        //This function is called every time a scene is loaded. this behaviour is defined in Awake().
        private void SceneLoaded(VTOLScenes scene)
        {
            //If you want something to happen in only one (or more) scenes, this is where you define it.

            //For example, lets say you're making a mod which only does something in the ready room and the loading scene. This is how your code could look:
            switch (scene)
            {
                case VTOLScenes.ReadyRoom:
                case VTOLScenes.LoadingScene:
                case VTOLScenes.VehicleConfiguration:
                    ResetLogger();
                    break;
                case VTOLScenes.Akutan:
                case VTOLScenes.CustomMapBase:
                    Debug.Log("Done Waiting map load");
                    startLog();
                    break;
            }
        }
    }
}
