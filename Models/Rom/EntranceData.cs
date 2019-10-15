﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MMRando.Models
{
    public class Exit
    {
        public Exit(Exit x) {
            if( x != null)
            {
                this.RegionName = x.RegionName;
                this.ExitName = x.ExitName;
                this.ExitIndex = x.ExitIndex;
                this.SpawnName = x.SpawnName;
            }
        }
        public string RegionName { get; set; }
        public string ExitName { get; set; }
        public int ExitIndex { get; set; }
        public string SpawnName { get; set; }
    }

    public class Spawn
    {
        public string SpawnName { get; set; }
        public string RegionName { get; set; }
        public byte SpawnIndex { get; set; }
        public Spawn(string SpawnName, string RegionName, byte SpawnIndex)
        {
            this.SpawnName = SpawnName;
            this.RegionName = RegionName;
            this.SpawnIndex = SpawnIndex;
        }
    }

    public class Entrance
    {
        public string EntranceName { get; set; }
        public string SpawnName { get; set; }
        public string ExitName { get; set; }
        public string ReturnSpawnName { get; set; }
        public string ReturnExitName { get; set; }
        public string Type { get; set; }
        public Entrance( string EntranceName, string SpawnName, string ExitName, string Type)
        {
            this.EntranceName = EntranceName;
            this.SpawnName = SpawnName;
            this.ExitName = ExitName;
            this.Type = Type;
        }
    }

    public class Region
    {
        public string RegionName { get; set; }
        public ushort SceneId { get; set; }
        public ushort ExternalSceneId { get; set; }
        public List<string> Spawns { get; set; }
        public List<string> Exits { get; set; }
    }

    public class EntranceData
    {
        public List<Region> regions;
        public List<Exit> exits;
        public List<Spawn> spawns;
        public List<Entrance> entrances;
        public Dictionary<string, string> internalSceneSwitches;
        public EntranceData(EntranceData Copy)
        {
            if(Copy != null)
            {
                this.regions = new List<Region>();
                foreach (Region r in Copy.regions)
                {
                    this.regions.Add(r);
                }
                this.exits = new List<Exit>();
                foreach (Exit x in Copy.exits)
                {
                    this.exits.Add(new Exit(x));
                }
                this.spawns = new List<Spawn>();
                foreach (Spawn s in Copy.spawns)
                {
                    this.spawns.Add(s);
                }
                this.entrances = new List<Entrance>();
                foreach (Entrance e in Copy.entrances)
                {
                    this.entrances.Add(e);
                }
            }
        }

        public void RenameSpawn(string oldSpawnName, string newSpawnName)
        {
            if(spawns.Find(s => oldSpawnName.Equals(s.SpawnName)) != null)
            {
                foreach (Spawn spawn in spawns.FindAll(s => oldSpawnName.Equals(s.SpawnName)))
                {
                    spawn.SpawnName = newSpawnName;
                }
                foreach (Exit exit in exits.FindAll(x => oldSpawnName.Equals(x.SpawnName)))
                {
                    exit.SpawnName = newSpawnName;
                }
                foreach (Entrance ent in entrances.FindAll(e => oldSpawnName.Equals(e.SpawnName)))
                {
                    ent.SpawnName = newSpawnName;
                }
                foreach (Entrance ent in entrances.FindAll(e => oldSpawnName.Equals(e.ReturnSpawnName)))
                {
                    ent.ReturnSpawnName = newSpawnName;
                }
                foreach (Region region in regions)
                {
                    int i = region.Spawns.FindIndex(s => oldSpawnName.Equals(s));
                    if( i != -1)
                    {
                        region.Spawns[i] = newSpawnName;
                    }
                }
            }
        }

        public void RenameExit(string oldExitName, string newExitName)
        {
            if (exits.Find(s => oldExitName.Equals(s.ExitName)) != null)
            {
                foreach (Exit exit in exits.FindAll(x => oldExitName.Equals(x.ExitName)))
                {
                    exit.ExitName = newExitName;
                }
                foreach (Entrance ent in entrances.FindAll(e => oldExitName.Equals(e.ExitName)))
                {
                    ent.ExitName = newExitName;
                }
                foreach (Entrance ent in entrances.FindAll(e => oldExitName.Equals(e.ReturnExitName)))
                {
                    ent.ReturnExitName = newExitName;
                }
                foreach (Region region in regions)
                {
                    int i = region.Exits.FindIndex(x => oldExitName.Equals(x));
                    if (i != -1)
                    {
                        region.Exits[i] = newExitName;
                    }
                }
            }
        }

        internal bool ConnectEntrance(string from, string to)
        {
            Entrance source = entrances.Find(e => from.Equals(e.EntranceName));
            Entrance dest = entrances.Find(e => to.Equals(e.EntranceName));
            if (source == null || dest == null)
            {
                return false;
            }
            Exit sourceExit = exits.Find(x => source.ExitName.Equals(x.ExitName));
            Exit destReturnExit = exits.Find(x => dest.ReturnExitName.Equals(x.ExitName));
            if ( sourceExit == null || destReturnExit == null )
            {
                return false;
            }

            sourceExit.SpawnName = dest.SpawnName;
            destReturnExit.SpawnName = source.ReturnSpawnName;
            return true;
        }

        internal ushort SpawnAddress( string SpawnName )
        {
            Spawn spawn = spawns.Find(s => SpawnName.Equals(s.SpawnName));
            if (spawn != null)
            {
                Region region = regions.Find(r => spawn.RegionName.Equals(r.RegionName));
                if (region != null)
                {
                    return (ushort)((region.ExternalSceneId << 9) + (spawn.SpawnIndex << 4));
                }
            }
            return 0xFFFF;
        }

        internal int SceneIndex( string RegionName)
        {
            Region region = regions.Find(r => RegionName.Equals(r.RegionName));
            return (region == null) ? -1 : region.SceneId;
        }

        internal void UpdateEntrances()
        {
            Exit t;
            foreach (Entrance e in entrances)
            {
                t = exits.Find(x => e.ExitName.Equals(x.ExitName));
                e.SpawnName = t.SpawnName;
                t = exits.Find(x => e.ReturnExitName.Equals(x.ExitName));
                e.ReturnSpawnName = t.SpawnName;
            }
        }
    }
}
