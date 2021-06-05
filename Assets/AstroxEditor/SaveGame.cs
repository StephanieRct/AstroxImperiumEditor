using System;
using System.Collections.Generic;
using System.IO;

namespace AstroxEditor
{
    public class AstroxEditor
    {
        public static void Log(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }
        public static void LogError(string msg)
        {
            UnityEngine.Debug.LogError(msg);
        }
        public static void LogWarning(string msg)
        {
            UnityEngine.Debug.LogWarning(msg);
        }
        
    }
    public class SaveGame
    {
        static readonly string k_FolderSector = "sectors";
        string FolderSave;
        //public List<SectorFile> Sectors;
        public Dictionary<string, SectorFile> Sectors;
        public SaveGame(string folder)
        {
            FolderSave = folder;
        }
        public void SaveTo(string folder)
        {
            var sectorFolder = folder + "\\" + k_FolderSector;
            Directory.CreateDirectory(sectorFolder);

            foreach (var ss in Sectors)
            {
                ss.Value.SaveToFolder(sectorFolder);
            }
        }
        public void Save()
        {
            SaveTo(FolderSave);
        }
        public bool Load()
        {
            if (!Directory.Exists(FolderSave)) return false;

            string sectorFilename = FolderSave + "\\" + k_FolderSector;
            Sectors = new Dictionary<string, SectorFile>();
            var sectorsTxts = Directory.EnumerateFiles(sectorFilename, "*.txt");
            foreach (var s in sectorsTxts)
            {
                var fn = Path.GetFileName(s);
                var f = new SectorFile(this, s);
                f.Load();
                Sectors.Add(fn, f);
            }
            return true;
        }
        public SectorFile GetSector(int id)
        {
            foreach (var ss in Sectors)
            {
                if (ss.Value.Id.Value == id)
                {
                    return ss.Value;
                }
            }
            return null;
        }
        public SectorFile GetSectorByFilename(string fn)
        {
            if (Sectors.TryGetValue(fn, out var f))
            {
                return f;
            }
            return null;
        }
        public SectorFile GetSectorByName(string name)
        {
            foreach (var ss in Sectors)
            {
                if (ss.Value.Name.Value == name)
                {
                    return ss.Value;
                }
            }
            return null;
        }
    }




    public class GalaxyRelaxer
    {

        public static void RelaxSectorAngle(bool verbose, SectorFile sectorA, SectorFile sectorB, SectorFile sectorC, double minAngle, double angleFactor)
        {
            var vec0 = sectorA.Position.Value - sectorB.Position.Value;
            var vec1 = sectorC.Position.Value - sectorB.Position.Value;
            var vec0N = math.normalize(vec0, out var vec0l);
            var vec1N = math.normalize(vec1, out var vec1l);
            var angle = Math.Acos(math.dot(vec0N, vec1N));
            var degreeOld = angle / Math.PI * 180;
            var axis = math.normalize(math.cross(vec0N, vec1N));

            var distDiff = math.dist(vec0l, vec1l) / (vec0l + vec1l);

            //var minAngle = Math.PI / (distDiff+0.001);//sectorB.WarpGates.Count;
            //var minAngle = Math.PI / 3.0f;
            if (angle < minAngle)
            {
                var angleDelta = (minAngle - angle) * 0.5 * angleFactor;
                var qA = quaternion.AxisAngle(axis, -angleDelta);
                var qB = quaternion.AxisAngle(axis, angleDelta);
                var vec0New = math.rotate(qA, vec0);
                var vec1New = math.rotate(qB, vec1);
                var sectorAPos = sectorB.Position.Value + vec0New;
                var sectorCPos = sectorB.Position.Value + vec1New;

                var vec0NewN = math.normalize(vec0New);
                var vec1NewN = math.normalize(vec1New);
                var angleNew = Math.Acos(math.dot(vec0NewN, vec1NewN));
                var degreeNew = angleNew / Math.PI * 180;

                sectorA.Position.Value = sectorAPos;
                sectorC.Position.Value = sectorCPos;

                var vec0After = sectorA.Position.Value - sectorB.Position.Value;
                var vec1After = sectorC.Position.Value - sectorB.Position.Value;
                var vec0lAfter = math.length(vec0After);
                var vec1lAfter = math.length(vec1After);
                var ldiff0 = vec0lAfter - vec0l;
                var ldiff1 = vec1lAfter - vec1l;
                if (verbose) AstroxEditor.Log($"[{sectorA.Id.Value}][{sectorB.Id.Value}][{sectorC.Id.Value}]; \t"
                                     + $"angle={degreeOld:0.###}; \t"
                                     + $"minAngle={(minAngle / Math.PI * 180):0.###}; \t"
                                     + $"newAngle={degreeNew:0.###}; \t"
                                     + $"ldiff0={ldiff0:0.###}; \t"
                                     + $"ldiff1={ldiff1:0.###}");

            }
            //else
            //{
            //
            //    if (verbose) Console.WriteLine($"[{sectorA.Id.Value}][{sectorB.Id.Value}][{sectorC.Id.Value}] angle={degreeOld}");
            //}
        }
        public static void ScaleSectors(SaveGame sg, double scale)
        {
            foreach(var ss in sg.Sectors)
            {
                ss.Value.Position.Value *= scale;
            }
        }
        public static void RelaxSector(SaveGame sg, bool verbose, int iterationCount, double dist0, double dist1, int distMinWGThreshold, double distFactor, double minAngle0, double minAngle1, int angleWGThreshold, double angleFactor)
        {
            if(verbose) AstroxEditor.Log($"Relaxing iterationCount:{iterationCount} dist0:{dist0} dist1:{dist1} distFactor:{distFactor} angleFactor:{angleFactor}...");
            for (int iter = 0; iter != iterationCount; ++iter)
            {

                if (verbose) AstroxEditor.Log($"iteration {iter} ...");
                foreach (var ss in sg.Sectors)
                {
                    var sourceSector = ss.Value;
                    foreach (var wg in sourceSector.WarpGates)
                    {
                        var targetSector = wg.GetExitSector(sg);
                        if (targetSector != null)
                        {
                            var diff = targetSector.Position.Value - sourceSector.Position.Value;
                            var diffN = math.normalize(diff, out var dist);
                            var dist2dist0 = math.dist(dist, dist0);
                            var dist2dist1 = math.dist(dist, dist1);
                            var totWG = sourceSector.WarpGates.Count + targetSector.WarpGates.Count;
                            var targetDist = totWG < distMinWGThreshold ? dist0 : dist1;
                            //var minWGCount = Math.Min(sourceSector.WarpGates.Count, targetSector.WarpGates.Count);
                            //var targetDist = minWGCount <= 2 ? dist0 : dist1;
                            
                            //var targetDist = dist2dist0 < dist2dist1 ? dist0 : dist1;

                            var mov = (targetDist - dist) * distFactor;
                            double targetW = targetSector.WarpGates.Count;

                            double sourceW = sourceSector.WarpGates.Count;
                            targetW = targetW * targetW;
                            sourceW = sourceW * sourceW;
                            var totalW = targetW + sourceW;
                            targetW = 1 - targetW / totalW;
                            sourceW = 1 - sourceW / totalW;

                            targetSector.Position.Value += diffN * mov * targetW;
                            sourceSector.Position.Value -= diffN * mov * sourceW;

                            if (verbose)
                            {
                                var distAfter = math.dist(targetSector.Position.Value, sourceSector.Position.Value);
                                AstroxEditor.Log($"[{sourceSector.Id}]->[{targetSector.Id}]; \t"
                                    + $"w0=({sourceSector.WarpGates.Count}){sourceW:0.###}; \t"
                                    + $"w1=({targetSector.WarpGates.Count}){targetW:0.###}; \t"
                                    + $"dist={dist:0.###}; \t"
                                    + $"distNew={distAfter:0.###}; \t"
                                    + $"delta={(distAfter - dist):0.###}"
                                );
                            }

                        }

                    }
                }
                foreach (var ss in sg.Sectors)
                {
                    var sourceSector = ss.Value;
                    foreach (var wg in sourceSector.WarpGates)
                    {
                        var targetSector = wg.GetExitSector(sg);
                        if (targetSector != null)
                        {
                            foreach (var targetWg in targetSector.WarpGates)
                            {
                                var ThirdSector = targetWg.GetExitSector(sg);
                                if (ThirdSector != null && ThirdSector.Id.Value != sourceSector.Id.Value)
                                {
                                    var wpTotal = sourceSector.WarpGates.Count + targetSector.WarpGates.Count + ThirdSector.WarpGates.Count;
                                    if (wpTotal < angleWGThreshold)
                                    {
                                        RelaxSectorAngle(verbose, sourceSector, targetSector, ThirdSector, minAngle0, angleFactor);
                                    } else
                                    {
                                        RelaxSectorAngle(verbose, sourceSector, targetSector, ThirdSector, minAngle1, angleFactor);
                                    }
                                }
                            }
                        }

                    }
                }
                if (verbose) AstroxEditor.Log($"done");
            }
        }
    }
}