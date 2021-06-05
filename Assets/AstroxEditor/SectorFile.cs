using System;
using System.Collections.Generic;
using System.Text;

namespace AstroxEditor
{

    public class SectorFile : AstroxFile
    {
        public const string k_SECTOR = "SECTOR";
        public const string k_SECTOR_ID = "id";
        public const string k_SECTOR_Name = "name";


        // WARPGATE; Warpgate id ; Sector filename ; Exit Warpgate id ; X position ; Y position ; Z position;Toll ; Faction ; Level ;
        public const string k_WARPGATE = "WARPGATE";
        public const int k_WARPGATE_ColId = 1;
        public const int k_WARPGATE_ColExitSector = 2;
        public const int k_WARPGATE_ColExitWarpgateId = 3;
        public const int k_WARPGATE_ColPosition = 4;
        public const int k_WARPGATE_ColToll = 7;
        public const int k_WARPGATE_ColFaction = 8;
        public const int k_WARPGATE_ColLevel = 9;


        // SECTORXYZ; location; -4859.000; 2996.000; 3677.000
        public const string k_SECTORXYZ = "SECTORXYZ";
        public const int k_SECTORXYZ_PositionCol = 2;
        public SectorFile(SaveGame sg, string filename)
            : base(sg, filename)
        {
        }
        public struct WarpGate
        {
            public LineRefInt Id;
            public LineRefString ExitSector;
            public LineRefInt ExitWarpgateId;
            public LineRefDouble3 Position;
            public LineRefDouble Toll;
            public LineRefString Faction;
            public LineRefDouble Level;
            public SectorFile GetExitSector(SaveGame sg)
            {
                if (sg.Sectors.TryGetValue(ExitSector.Value, out var sf)) return sf;
                return null;
            }
            public WarpGate(Entry e)
            {
                e.TryGetColumnRef(k_WARPGATE_ColId, out Id);
                e.TryGetColumnRef(k_WARPGATE_ColExitSector, out ExitSector);
                e.TryGetColumnRef(k_WARPGATE_ColExitWarpgateId, out ExitWarpgateId);
                e.TryGetColumnRef(k_WARPGATE_ColPosition, out Position);
                e.TryGetColumnRef(k_WARPGATE_ColToll, out Toll);
                e.TryGetColumnRef(k_WARPGATE_ColFaction, out Faction);
                e.TryGetColumnRef(k_WARPGATE_ColLevel, out Level);
            }
        }
        public LineRefString Name;
        public LineRefInt Id;
        public LineRefDouble3 Position;
        public List<WarpGate> WarpGates;
        public override void Load()
        {
            WarpGates = new List<WarpGate>();
            base.Load();
            foreach (var e in Entries)
            {
                switch (e.Tag)
                {
                    case k_SECTOR:
                        LoadLineSector(e);
                        break;
                    case k_WARPGATE:
                        WarpGates.Add(new WarpGate(e));
                        break;
                    case k_SECTORXYZ:
                        e.TryGetColumnRef(k_SECTORXYZ_PositionCol, out Position);
                        break;
                }
            }

        }
        public bool LoadLineSector(Entry e)
        {
            if (!e.TryGetColumnValue(1, out string col1)) return false;
            switch (col1)
            {
                case k_SECTOR_ID:
                    Id = new LineRefInt(e, 2);
                    break;
                case k_SECTOR_Name:
                    Name = new LineRefString(e, 2);
                    break;

            }
            return true;
        }
        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Sector({Id.Value}) = \"{Name.Value}\"");
            foreach (var wg in WarpGates)
            {
                sb.AppendLine($"\tWarpGate to {wg.ExitSector} at position {wg.Position}");
            }
            return sb.ToString();
        }
    }
}