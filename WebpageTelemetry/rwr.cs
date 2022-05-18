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
            DashRWR dashRWR = VTOLAPI.GetPlayersVehicleGameObject().GetComponentInChildren<DashRWR>();

            if (rWR.isLocked) //when player gets locked
            {
                Actor locker = new Actor();
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
                Debug.Log($"WorldToWRWPosition {dashRWR.WorldToRWRPosition(locker.position)}");
                Vector2 lk = dashRWR.WorldToRWRPosition(locker.position);
                if (lk.x > 0 && lk.y > 0) { return (int)sector.NE; }
                if (lk.x < 0 && lk.y > 0) { return (int)sector.NW; }
                if (lk.x < 0 && lk.y < 0) { return (int)sector.SW; }
                if (lk.x > 0 && lk.y < 0) { return (int)sector.SE; }

            } else 
            { 
                return 0;
            }//islockedIf
            return 0;
        }//getMPos


    }//class

}//namespace

