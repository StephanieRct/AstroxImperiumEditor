using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ExecuteAlways]
public class GalaxyRender : MonoBehaviour
{
    public AstroxEditor.SaveGame CurrentSaveGame;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(CurrentSaveGame != null)
        {
            foreach(var ss in CurrentSaveGame.Sectors)
            {
                var sector = ss.Value;
                var pos0 = ss.Value.Position.Value;
                foreach (var wg in sector.WarpGates)
                {
                    var otherSector = wg.GetExitSector(CurrentSaveGame);
                    if (otherSector != null)
                    {
                        var pos1 = otherSector.Position.Value;
                        
                        Color c = Color.white;
                        if(sector.WarpGates.Count <= 2 || otherSector.WarpGates.Count <= 2)
                        {
                            c = Color.red;
                        }
                        UnityEngine.Debug.DrawLine(pos0.ToV3(), pos1.ToV3(), c);
                    } 
                    //else
                    //{
                    //    AstroxEditor.AstroxEditor.LogWarning($"Warp Gate ({wg.Id.Value}) in sector ({sector.Id.Value})'{sector.Name.Value}' points to unknown sector ({wg.ExitSector.Value})");
                    //}
                }
            }
        }
        //UnityEngine.Debug.Log("Galaxy Update [" + Time.time + "]");
    }
}
