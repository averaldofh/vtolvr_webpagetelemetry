using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using HtmlAgilityPack;


namespace OBSTelemetry
{
    public class DataGetters
    {
        public static VTOLMOD mod;
        public Main DataLogger { get; set; }

        public DataGetters(Main dataLogger)
        {
            this.DataLogger = dataLogger;
        }

        public void GetData()
        {
            Actor playeractor = FlightSceneManager.instance.playerActor;
            GameObject currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();
            string[] lines = { "Alt: XXX", "Speed: XXX", "HDG: XXX", "Vspd: XXX", "Força G: XXX", "Health: XXX" };
            string temp1;
            string temp2;
            int tmp;

            var doc = new HtmlDocument();
            doc.Load(Globals.path + "test.html");

            //Get altitude in ft
                temp1 = Math.Round((playeractor.flightInfo.altitudeASL * 3.28), 0).ToString();
                temp2 = $"Alt: {temp1} ft";
                var node = doc.GetElementbyId("altitude");
                node.InnerHtml = temp2.ToString();
                lines[0] = temp2;

            //Get Airspeed in kt
                temp1 = Math.Round((playeractor.flightInfo.airspeed * 1.943), 0).ToString();
                temp2 = $"Speed: {temp1} kt";
                node = doc.GetElementbyId("speed");
                node.InnerHtml = temp2.ToString();
                lines[1] = temp2;

            //Get heading
                temp1 = Math.Round(playeractor.flightInfo.heading, 0).ToString();
                temp2 = $"HDG: {temp1}";
                node = doc.GetElementbyId("speed");
                node.InnerHtml = temp2.ToString();
                lines[2] = temp2;

            //Get Vertical speed in ft/min
                temp1 = Math.Round((playeractor.flightInfo.verticalSpeed * 197), 0).ToString();
                temp2 = $"Vspd: {temp1} ft/min";
                node = doc.GetElementbyId("vspd");
                node.InnerHtml = temp2.ToString();
                lines[3] = temp2;

            //Get G force
                temp1 = Math.Round(playeractor.flightInfo.playerGs, 1).ToString();
                temp2 = $"G Force: {temp1}";
                node = doc.GetElementbyId("gforce");
                node.InnerHtml = temp2.ToString();
                lines[4] = temp2;

            //Get Health Level
                Health health = Traverse.Create(playeractor).Field("h").GetValue() as Health;
                temp1 = Math.Round(health.currentHealth,0).ToString();
                temp2 = $"Health: {temp1}%";
                node = doc.GetElementbyId("health");
                node.InnerHtml = temp2.ToString();
                lines[5] = temp2;

            //Get Fuel Level
                tmp = getFuelPercentage(currentVehicle);
                temp2 = $"Fuel: {tmp}%";
                node = doc.GetElementbyId("fuel");
                node.SetAttributeValue("value", tmp.ToString());
                lines[6] = temp2;

            //Get missile detected
            var miss = getMissileDetected(currentVehicle);
            node = doc.GetElementbyId("missile");
            if (miss == "0")
            {
                node.SetAttributeValue("src", "missileoff.png");
            }
            else
            {
                node.SetAttributeValue("src", "missileon.png");
            }

            //Write each value to a line in status.txt
            File.WriteAllLines(Globals.path + "status.txt", lines);
            doc.Save(Globals.path + "test.html");
        }

        public static int getFuelPercentage(GameObject player)
        {
            int tmp;
            try
            {
                FuelTank tank = player.GetComponentInChildren<FuelTank>();
                tmp = (int)Math.Round(tank.totalFuel, 0);
                VTOLVehicles acvehicle = VTOLAPI.GetPlayersVehicleEnum();
                switch (acvehicle)
                {
                    case VTOLVehicles.AV42C: //3200
                        tmp = map(tmp, 0, 3200, 0, 100);
                        break;
                    case VTOLVehicles.AH94: //870??
                        tmp = map(tmp, 0, 870, 0, 100);
                        break;
                    case VTOLVehicles.F45A: //7600
                        tmp = map(tmp, 0, 7600, 0, 100);
                        break;
                    case VTOLVehicles.FA26B: //7100
                        tmp = map(tmp, 0, 7100, 0, 100);
                        break;
                }
                return tmp;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                //return "Fuel: XXX";
                return -1;
            }
        }

        public static string getMissileDetected(GameObject vehicle)
        {
                MissileDetector md = vehicle.GetComponentInChildren<MissileDetector>();
                return md.missileDetected.ToString();
        }

        public void clearData()
        {
            string[] lines = { "Alt: XXX", "Speed: XXX", "HDG: XXX", "Vspd: XXX", "G Force: XXX", "Health: XXX"};
            try
            {
                File.WriteAllLines(Globals.path + "status.txt", lines);
            }
            catch (Exception ex)
            {
                File.WriteAllText(Globals.path + "exceptions.txt", ex.ToString());
            }
        }

        private static int map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }
    }
}