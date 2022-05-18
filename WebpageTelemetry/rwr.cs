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
       
        
        public int getMPos()
        {
            Actor player = FlightSceneManager.instance.playerActor;
            ModuleRWR rWR = player.GetComponentInChildren<ModuleRWR>();

            if (rWR.isLocked) //when player gets locked
            {
                Actor locker = new Actor();
                //Vector3 Ppos;
                //Vector3 lockerVector;
                ModuleRWR.RWRContact rwrC;

                for (int i = 0; i < rWR.contacts.Length; i++)
                {
                    rwrC = rWR.contacts[i];
                    if (rwrC.active && rwrC.locked && rwrC.radarActor != null) //check which actor is locking the player
                    {
                        locker = rwrC.radarActor;
                        Debug.Log($"Player {player.name} is locked by {locker.name}");
                    }
                }
                //lockerVector = FloatingOrigin.WorldToGlobalPoint(locker.position).toVector3;
                //Ppos = FloatingOrigin.WorldToGlobalPoint(player.position).toVector3;

                return 0;
            } else { return 0; }//islockedIf
        }//getMPos


    }//class

}//namespace

