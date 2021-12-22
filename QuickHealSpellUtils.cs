using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HealingSpell
{
    public static class QuickHealSpellUtils
    {
        public static GameObject healingOrb;
        public static GameObject healingAoe;

        public static List<T> LoadResources<T>(string[] names, string assetName) where T : class
        {
            // Grabs the .assets file from the Bundles folder in the mod directory
            FileInfo[] files = new DirectoryInfo(Application.streamingAssetsPath + "/Mods/HealingSpell_U10/Bundles").GetFiles(assetName + ".bundle", SearchOption.AllDirectories);

            // Unpacks assets (code heavily shortened here by dotPeek, entirely unsure what this does but it works)
            AssetBundle assetBundle = !AssetBundle.GetAllLoadedAssetBundles().Any() ? AssetBundle.LoadFromFile(files[0].FullName) : (AssetBundle.GetAllLoadedAssetBundles().Count(x => files[0].Name.Contains(x.name)) != 0 ? AssetBundle.GetAllLoadedAssetBundles().First(x => files[0].Name.Contains(x.name)) : AssetBundle.LoadFromFile(files[0].FullName));
            List<T> objList = new List<T>();

            foreach (string allAssetName in assetBundle.GetAllAssetNames())
            {
                foreach (string name in names)
                {
                    if (allAssetName.Contains(name))
                    {
                        objList.Add(assetBundle.LoadAsset(allAssetName) as T);
                        Debug.Log(Assembly.GetExecutingAssembly().GetName() + " loaded asset: " + allAssetName);
                    }
                }
            }

            if (objList.Count != 0)
                return objList;
            Debug.LogError(Assembly.GetExecutingAssembly().GetName() + " found no objects in array. The functions may not work as intended.");
            return null;
        }

        /* Method needed for extract's commented out method in HealingSpell.cs       
         * public static IEnumerator<float> DoActionAfter(float seconds, System.Action action)
        {
            float time = 0;
            while (time < 1f)
            {
                time += Time.fixedDeltaTime / seconds;
                yield return Time.fixedDeltaTime;
            }

            action();
        }*/
    }
}