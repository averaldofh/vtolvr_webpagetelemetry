using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

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
            if (Main.playerActor == null) { Main.playerActor = FlightSceneManager.instance.playerActor; Debug.Log("player was null"); }
            if (Main.playerVehicle == null){ Main.playerVehicle = VTOLAPI.GetPlayersVehicleGameObject(); Debug.Log("vehicle was null"); }
            if (Main.md == null) {Main.md = Main.playerActor.GetComponentInChildren<MissileDetector>(); Debug.Log("md was null"); }
            if (Main.tank == null){ Main.tank = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<FuelTank>(); Debug.Log("tank was null"); }
            if (Main.rWR == null) {Main.rWR = Main.playerActor.GetComponentInChildren<ModuleRWR>(); Debug.Log("rwr was null"); }
            if (Main.dashRWR == null){ Main.dashRWR = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<DashRWR>(); Debug.Log("dashrwr was null"); }

            rwrGetter = new rwr(this);

            string[] lines = { "Alt: XXX", "Speed: XXX", "HDG: XXX", "Vspd: XXX", "Força G: XXX", "Health: XXX", "Fuel : XXX"};
            string temp1, temp2;
            int tmp;
            var doc = new HtmlDocument();
            HtmlNode fNode;
            HtmlTextNode hNode;

            if (rwrEn) { doc.Load(Globals.path + "270x675TelemetryRWR.html");/* Debug.Log("Loaded rwr.html"); */}
            else { doc.Load(Globals.path + "270x440Telemetry.html"); /*Debug.Log("Loaded NOT rwr.html");*/ }

            //Get altitude in ft
            temp1 = Math.Round((Main.playerActor.flightInfo.altitudeASL * 3.28), 0).ToString();
            temp2 = $"Alt: {temp1} ft";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='altitude']//text()") as HtmlTextNode;
            lines[0] = temp2;
            hNode.Text = temp2;

            //Get Airspeed in kt
            temp1 = Math.Round((Main.playerActor.flightInfo.airspeed * 1.943), 0).ToString();
            temp2 = $"Speed: {temp1} kt";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='speed']//text()") as HtmlTextNode;
            lines[1] = temp2;
            hNode.Text = temp2;

            //Get heading
            temp1 = Math.Round(Main.playerActor.flightInfo.heading, 0).ToString();
            temp2 = $"HDG: {temp1}";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='hdg']//text()") as HtmlTextNode;
            lines[2] = temp2;
            hNode.Text = temp2;

            //Get Vertical speed in ft/min
            temp1 = Math.Round((Main.playerActor.flightInfo.verticalSpeed * 197), 0).ToString();
            temp2 = $"Vspd: {temp1} ft/min";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='vspd']//text()") as HtmlTextNode;
            lines[3] = temp2;
            hNode.Text = temp2;

            //Get G force
            temp1 = Math.Round(Main.playerActor.flightInfo.playerGs, 2).ToString();
            temp2 = $"G Force: {temp1}";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='gforce']//text()") as HtmlTextNode;
            lines[4] = temp2;
            hNode.Text = temp2;

            //Get Health Level
            Health health = Traverse.Create(Main.playerActor).Field("h").GetValue() as Health;
            temp1 = Math.Round(health.currentHealth, 0).ToString();
            temp2 = $"Health: {temp1}%";
            hNode = doc.DocumentNode.SelectSingleNode("//td[@id='health']//text()") as HtmlTextNode;
            lines[5] = temp2;
            hNode.Text = temp2;

            //Get Fuel Level
            tmp = Mathf.RoundToInt(Main.tank.fuelFraction * 100);
            temp2 = $"Fuel: {tmp}%";
            fNode = doc.GetElementbyId("fuel");
            lines[6] = temp2;
            fNode.SetAttributeValue("value", tmp.ToString());

            Debug.Log($"data {lines}");

            //Get missile detected
            var miss = (Main.md.detectIncoming && Main.md.missileIncomingDetected);
                fNode = doc.GetElementbyId("missile");
            if (miss == true)
            {
                //fNode.ReplaceClass("msslon", "mssloff");
                fNode.SetAttributeValue("src", "res/missileon.png");
                Debug.Log("MISSILE INCOMING");
            }
            else
            {
                //fNode.ReplaceClass("mssloff", "msslon");
                fNode.SetAttributeValue("src", "res/missileoff.png");
            }

            //get lock
            var lockinfo = Main.rWR.isLocked;
            fNode = doc.GetElementbyId("lock");
            if (lockinfo == true)
            {
                //fNode.ReplaceClass("lockon", "lockoff");
                fNode.SetAttributeValue("src", "res/lockon.png");
                Debug.Log("LOCK DETECTED");
            }
            else
            {
                //fNode.ReplaceClass("lockoff", "lockon");
                fNode.SetAttributeValue("src", "res/lockoff.png");
            }

            //get RWR
            if (rwrEn) { 
                var sector = rwrGetter.getRWR();
                fNode = doc.GetElementbyId("rwr");
                switch (sector)
                {
                    case 0: fNode.SetAttributeValue("src", "res/OFF.webp"); /*File.WriteAllText(Globals.path + "270x675TelemetryRWR.html", Regex.Replace(File.ReadAllText(Globals.path + "270x675TelemetryRWR.html"), @"(?:setInterval.\d.\d.\d.\d.)", "setInterval(draw(0, 0, 0, 0), 200)")); */break;
                    case 1: fNode.SetAttributeValue("src", "res/NE.webp"); /*File.WriteAllText(Globals.path + "270x675TelemetryRWR.html", Regex.Replace(File.ReadAllText(Globals.path + "270x675TelemetryRWR.html"), @"(?:setInterval.\d.\d.\d.\d.)", "setInterval(draw(1, 0, 0, 0), 200)")); */break;
                    case 2: fNode.SetAttributeValue("src", "res/SE.webp");  /* File.WriteAllText(Globals.path + "270x675TelemetryRWR.html", Regex.Replace(File.ReadAllText(Globals.path + "270x675TelemetryRWR.html"), @"(?:setInterval.\d.\d.\d.\d.)", "setInterval(draw(0, 0, 1, 0), 200)"));*/ break;
                    case 3: fNode.SetAttributeValue("src", "res/SW.webp"); /*File.WriteAllText(Globals.path + "270x675TelemetryRWR.html", Regex.Replace(File.ReadAllText(Globals.path + "270x675TelemetryRWR.html"), @"(?:setInterval.\d.\d.\d.\d.)", "setInterval(draw(0, 0, 0, 1), 200)")); */break;
                    case 4: fNode.SetAttributeValue("src", "res/NW.webp"); /*File.WriteAllText(Globals.path + "270x675TelemetryRWR.html", Regex.Replace(File.ReadAllText(Globals.path + "270x675TelemetryRWR.html"), @"(?:setInterval.\d.\d.\d.\d.)", "setInterval(draw(0, 1, 0, 0), 200)")); */break;
                    default: fNode.SetAttributeValue("src", "res/OFF.webp"); /*File.WriteAllText(Globals.path + "270x675TelemetryRWR.html", Regex.Replace(File.ReadAllText(Globals.path + "270x675TelemetryRWR.html"), @"(?:setInterval.\d.\d.\d.\d.)", "setInterval(draw(0, 0, 0, 0), 200)")); */break;
                }
            }
            
            //Write each value to a line in status.txt
            File.WriteAllLines(Globals.path + "status.txt", lines);
            if (rwrEn) { doc.Save(Globals.path + "270x675TelemetryRWR.html"); }
            else { doc.Save(Globals.path + "270x440Telemetry.html"); }
        }
         public void clearData(bool rwrEn)
        {
            string[] lines = { "Alt: XXX", "Speed: XXX", "HDG: XXX", "Vspd: XXX", "G Force: XXX", "Health: XXX", "Fuel: XXX"};
                File.WriteAllLines(Globals.path + "status.txt", lines);
            var doc = new HtmlDocument(); HtmlNode fNode; HtmlTextNode hNode;
            if (rwrEn)
            {
                doc.Load(Globals.path + "270x675TelemetryRWR.html");
                fNode = doc.GetElementbyId("rwr");
                fNode.SetAttributeValue("src", "OFF.png");
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
                fNode.SetAttributeValue("value", "0");
                fNode = doc.GetElementbyId("lock");
                //fNode.ReplaceClass("lockoff", "lockon");
                fNode.SetAttributeValue("src", "lockoff.png");
                fNode = doc.GetElementbyId("missile");
                //fNode.ReplaceClass("mssloff", "msslon");
                fNode.SetAttributeValue("src", "missileoff.png");
            }
            else
            {
                doc.Load(Globals.path + "270x440Telemetry.html");
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
                fNode.SetAttributeValue("value", "0");
                fNode = doc.GetElementbyId("lock");
                //fNode.ReplaceClass("lockoff", "lockon");
                fNode.SetAttributeValue("src", "lockoff.png");
                fNode = doc.GetElementbyId("missile");
                //fNode.ReplaceClass("mssloff", "msslon");
                fNode.SetAttributeValue("src", "missileoff.png");
            }
        }
    }
}
