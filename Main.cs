using KitchenData;
using KitchenMods;
using PreferenceSystem;
using System.Collections.Generic;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenVisualPatches
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = $"IcedMilo.PlateUp.{MOD_NAME}";
        public const string MOD_NAME = "Visual Patches";
        public const string MOD_VERSION = "1.0.0";

        static Dictionary<string, Material> _materials;

        internal static PreferenceSystemManager PrefManager;

        public Main()
        {
            //Harmony harmony = new Harmony(MOD_GUID);
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            //PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
        }

        public void PreInject()
        {
        }

        public void PostInject()
        {
            if (_materials == null)
            {
                _materials = new Dictionary<string, Material>();

                Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
                for (int i = 0; i < materials.Length; i++)
                {
                    if (!_materials.ContainsKey(materials[i].name))
                        _materials.Add(materials[i].name, materials[i]);
                }
            }

            string chocolateMaterialName = "Plastic - Dark Red";
            string cakeMaterialName = "Cake";

            Main.LogInfo($"{chocolateMaterialName}: {_materials.TryGetValue(chocolateMaterialName, out Material newChocolateMaterial)}");
            Main.LogInfo($"{cakeMaterialName}: { _materials.TryGetValue(cakeMaterialName, out Material cakeMaterial)}");

            if (newChocolateMaterial != null && cakeMaterial != null)
            {
                Material[] newMaterialArray = new Material[] { cakeMaterial, newChocolateMaterial };

                if (GameData.Main.TryGet(-1354941517, out Item bigCakeTinFlavoured, warn_if_fail: true))
                {
                    GameObject cakePrefab = bigCakeTinFlavoured.Prefab?.transform.Find("Cake")?.gameObject;
                    if (cakePrefab != null)
                    {
                        Main.LogError($"Number of children in {bigCakeTinFlavoured.name} cakePrefab: {cakePrefab.transform.childCount}");
                        for (int i = 0; i < cakePrefab.transform.childCount; i++)
                        {
                            Transform child = cakePrefab.transform.GetChild(i);
                            Main.LogInfo(child.name);
                            if (!child.name.StartsWith("Cake Slice"))
                                continue;
                            ChangeCakeSliceMaterial(child);
                        }
                    }
                    else
                    {
                        Main.LogError($"Failed for find \"Cake\" in prefab for {bigCakeTinFlavoured.name}!");
                    }
                }

                if (GameData.Main.TryGet(-1532306603, out Item bigCakeSlice, warn_if_fail: true))
                {
                    GameObject cakeSlicePrefab = bigCakeSlice.Prefab;
                    if (cakeSlicePrefab != null)
                    {
                        ChangeCakeSliceMaterial(cakeSlicePrefab.transform);
                    }
                    else
                    {
                        Main.LogError($"Failed for find prefab for {cakeSlicePrefab.name}!");
                    }
                }

                void ChangeCakeSliceMaterial(Transform cakeSlice)
                {
                    MeshRenderer renderer = cakeSlice?.Find("Chocolate")?.Find("CakeSlice")?.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.materials = newMaterialArray;
                    }
                    else
                    {
                        Main.LogError($"Failed to find MeshRenderer in {cakeSlice.name}");
                    }
                }
            }
            else
            {
                Main.LogError("Failed to find materials!");
            }
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
