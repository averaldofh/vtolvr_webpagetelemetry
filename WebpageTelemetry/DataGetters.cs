using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using HtmlAgilityPack;


namespace WebpageTelemetry
{
    public class DataGetters
    {
        public rwr rwrGetter;

        public Main DataLogger { get; set; }

        public DataGetters(Main dataLogger)
        {
            this.DataLogger = dataLogger;
        }

        public void GetData(bool rwrEn)
        {
            rwrGetter = new rwr(this);
            Actor playeractor = FlightSceneManager.instance.playerActor;
            GameObject currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();
            string[] lines = { "Alt: XXX", "Speed: XXX", "HDG: XXX", "Vspd: XXX", "Força G: XXX", "Health: XXX"};
            string temp1;
            string temp2;
            int tmp; var doc = new HtmlDocument(); HtmlNode fNode; HtmlTextNode hNode;

            if (rwrEn) { doc.Load(Globals.path + "270x675TelemetryRWR.html"); }
            else { doc.Load(Globals.path + "270x440Telemetry.html"); }

            //Get altitude in ft
            temp1 = Math.Round((playeractor.flightInfo.altitudeASL * 3.28), 0).ToString();
            temp2 = $"Alt: {temp1} ft";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='altitude']//text()") as HtmlTextNode;
            lines[0] = temp2;
            hNode.Text = temp2;

            //Get Airspeed in kt
            temp1 = Math.Round((playeractor.flightInfo.airspeed * 1.943), 0).ToString();
            temp2 = $"Speed: {temp1} kt";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='speed']//text()") as HtmlTextNode;
            lines[1] = temp2;
            hNode.Text = temp2;

            //Get heading
            temp1 = Math.Round(playeractor.flightInfo.heading, 0).ToString();
            temp2 = $"HDG: {temp1}";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='hdg']//text()") as HtmlTextNode;
            lines[2] = temp2;
            hNode.Text = temp2;

            //Get Vertical speed in ft/min
            temp1 = Math.Round((playeractor.flightInfo.verticalSpeed * 197), 0).ToString();
            temp2 = $"Vspd: {temp1} ft/min";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='vspd']//text()") as HtmlTextNode;
            lines[3] = temp2;
            hNode.Text = temp2;

            //Get G force
            temp1 = Math.Round(playeractor.flightInfo.playerGs, 1).ToString();
            temp2 = $"G Force: {temp1}";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='gforce']//text()") as HtmlTextNode;
            lines[4] = temp2;
            hNode.Text = temp2;


            //Get Health Level
            Health health = Traverse.Create(playeractor).Field("h").GetValue() as Health;
            temp1 = Math.Round(health.currentHealth, 0).ToString();
            temp2 = $"Health: {temp1}%";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='health']//text()") as HtmlTextNode;
            lines[5] = temp2;
            hNode.Text = temp2;

            //Get Fuel Level
            tmp = getFuelPercentage(currentVehicle);
            temp2 = $"Fuel: {tmp}%";
            fNode = doc.GetElementbyId("fuel");
            fNode.SetAttributeValue("value", tmp.ToString());

            //Get missile detected
            var miss = getMissileDetected(currentVehicle);
            fNode = doc.GetElementbyId("missile");

            if (miss == true)
            {
                fNode.SetAttributeValue("src", "missileon.png");
            }
            else
            {
                fNode.SetAttributeValue("src", "missileoff.png");
            }

            //get lock
            var lockinfo = getLock(currentVehicle);
            fNode = doc.GetElementbyId("lock");
            if (lockinfo == true)
            {
                fNode.SetAttributeValue("src", "lockon.png");
            }
            else
            {
                fNode.SetAttributeValue("src", "lockoff.png");
            }

            //get RWR
            if (rwrEn) { 
                var sector = rwrGetter.getMPos();
                fNode = doc.GetElementbyId("rwr");
                switch (sector)
                {
                    case 0: fNode.SetAttributeValue("src", "OFF.webp"); break;
                    case 1: fNode.SetAttributeValue("src", "NE.webp"); break;
                    case 2: fNode.SetAttributeValue("src", "SE.webp"); break;
                    case 3: fNode.SetAttributeValue("src", "SW.webp"); break;
                    case 4: fNode.SetAttributeValue("src", "NW.webp"); break;
                    default: fNode.SetAttributeValue("src", "OFF.webp"); break;
                }
            }
                //Write each value to a line in status.txt
                File.WriteAllLines(Globals.path + "status.txt", lines);
            try
            {
                if (rwrEn) { doc.Save(Globals.path + "270x675TelemetryRWR.html"); }
                else { doc.Save(Globals.path + "270x440Telemetry.html"); }

            }
            catch (Exception ex)
            {
                Debug.LogError("SAVE HTML:" + ex);
            }
        }

        public static int getFuelPercentage(GameObject player)
        {
            int tmp;
            try
            {
                FuelTank tank = player.GetComponentInChildren<FuelTank>();
                tmp = Mathf.RoundToInt(tank.fuelFraction*100);

                if (tmp <= 0)
                {
                    tmp = 0;
                    //tmp = (int)tank.totalFuel;
                    //tmp = map(tmp, 0, (int)tank.maxFuel, 0, 100);
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

        public static bool getLock(GameObject player)
        {
            ModuleRWR ac = player.GetComponentInChildren<ModuleRWR>();
            return ac.isLocked;
        }

        public static bool getMissileDetected(GameObject player)
        {
                MissileDetector md = player.GetComponentInChildren<MissileDetector>();
                if(md.detectIncoming && md.missileIncomingDetected)
            {
                return true;
            }

            return false;
        }

        public void clearData(bool rwrEn)
        {
            string[] lines = { "Alt: XXX", "Speed: XXX", "HDG: XXX", "Vspd: XXX", "G Force: XXX", "Health: XXX"};
                File.WriteAllLines(Globals.path + "status.txt", lines);
            var doc = new HtmlDocument(); HtmlNode fNode; HtmlTextNode hNode;
            if (rwrEn) { doc.Load(Globals.path + "270x675TelemetryRWR.html");
                fNode = doc.GetElementbyId("rwr");
                fNode.SetAttributeValue("src", "OFF.png");
            }
            else { doc.Load(Globals.path + "270x440Telemetry.html");
                hNode = doc.DocumentNode.SelectSingleNode("//td[@id='altitude']//text()") as HtmlTextNode;
                hNode.Text = "XXX";
                hNode = doc.DocumentNode.SelectSingleNode("//td[@id='speed']//text()") as HtmlTextNode;
                hNode.Text = "XXX";
                hNode = doc.DocumentNode.SelectSingleNode("//td[@id='hdg']//text()") as HtmlTextNode;
                hNode.Text = "XXX";
                hNode = doc.DocumentNode.SelectSingleNode("//td[@id='vspd']//text()") as HtmlTextNode;
                hNode.Text = "XXX";
                hNode = doc.DocumentNode.SelectSingleNode("//td[@id='gforce']//text()") as HtmlTextNode;
                hNode.Text = "XXX";
                hNode = doc.DocumentNode.SelectSingleNode("//td[@id='health']//text()") as HtmlTextNode;
                hNode.Text = "XXX";
                fNode = doc.GetElementbyId("fuel");
                fNode.SetAttributeValue("value","0");
                fNode = doc.GetElementbyId("lock");
                fNode.SetAttributeValue("src", "lockoff.png");
                fNode = doc.GetElementbyId("missile");
                fNode.SetAttributeValue("src", "missileoff.png");
            }

        }

        private static int map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
        {
            return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
        }
    }
}