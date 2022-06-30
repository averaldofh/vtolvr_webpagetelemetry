using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace WebpageTelemetry
{
    public class rwr
    {
        public enum sector
        {
            OFF,
            NE,
            SE,
            SW,
            NW
        }
        public DataGetters rwrLogger { get; set; }
        public rwr(DataGetters rwrLogger)
        {
            this.rwrLogger = rwrLogger;
        }


        public int getRWR()
        {
            if (Main.rWR.isLocked) //when player gets locked 
            {
                ModuleRWR.RWRContact rwrC;

                for (int i = 0; i < Main.rWR.contacts.Length; i++)
                {
                    rwrC = Main.rWR.contacts[i];
                    if (rwrC.active && rwrC.locked && rwrC.radarActor != null) //check which actor is locking the player
                    {
                        Main.lockerActor = rwrC.radarActor;
                    }
                }

                Vector2 lk = Main.dashRWR.WorldToRWRPosition(Main.lockerActor.position);
                if (lk.x > 0 && lk.y > 0) { return (int)sector.NE; }
                if (lk.x < 0 && lk.y > 0) { return (int)sector.NW; }
                if (lk.x < 0 && lk.y < 0) { return (int)sector.SW; }
                if (lk.x > 0 && lk.y < 0) { return (int)sector.SE; }
            }
            else
            {
                return (int)sector.OFF;
            }//islockedIf
        return 0;
        }//getRWR
    }//class
}//namespace
