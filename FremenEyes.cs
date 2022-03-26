using System;
using System.Collections;
using UnityEngine;
using ThunderRoad;
using Chabuk.ManikinMono;

namespace EyesOfIbad
{
    public class FremenEyes : LevelModule
    {
        public string userSettingsFile = "StreamingAssets/Mods/FremenEyes/user_settings.json";
        public static Settings settings;
        public string[] materialAddress = { "Fisher.Material.Fremen_InnerEye", "Fisher.Material.Fremen_OuterEye" };
        public bool player_only = false;
        Material[] fremenEyes;
        Color eyeRGB;
        Color eyeEmission;
        float intensityHDR;
        const string player_str = "PlayerDefault";
        const string outer_mat_str = "Human_OuterEye";
        const string inner_mat_str = "Human_InnerEye";
        const string lod0_str = "Eyes_LOD0";
        const string lod1_str = "Eyes_LOD1";

        public override void OnUnload()
        {
            EventManager.onCreatureSpawn -= EventManager_onCreatureSpawn;
        }

        public override IEnumerator OnLoadCoroutine()
        {
            if (settings == null) settings = Settings.ReadFromDisk(userSettingsFile);
            eyeRGB = settings.GetColor();
            eyeEmission = new Color(eyeRGB.r * intensityHDR, eyeRGB.g * intensityHDR, eyeRGB.b * intensityHDR, eyeRGB.a);
            intensityHDR = settings.glowIntensityHDR;
            Debug.Log($"[Fremen-Eyes][{Time.time}] Mod Settings Loaded:\n{settings.ToString()}");

            fremenEyes = new Material[2];
            Catalog.LoadAssetAsync<Material>(materialAddress[0], mat =>
            {
                fremenEyes[0] = new Material(mat);
                Debug.Log($"[Fremen-Eyes][{Time.time}] Loaded Material: {materialAddress[0]}");
            },
            materialAddress[0]);
            Catalog.LoadAssetAsync<Material>(materialAddress[1], mat =>
            {
                fremenEyes[1] = new Material(mat);
                Debug.Log($"[Fremen-Eyes][{Time.time}] Loaded Material: {materialAddress[1]}");
            },
            materialAddress[1]);
            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;  // TODO: Renderers break with pooled creatures
            return base.OnLoadCoroutine();
        }

        void EventManager_onCreatureSpawn(Creature creature)
        {
            if (player_only && !creature.name.Contains(player_str)) return;
            try
            {
                ManikinGroupPart[] parts = creature.ragdoll.GetComponentsInChildren<ManikinGroupPart>();
                foreach (ManikinGroupPart mgp in parts)
                    foreach (ManikinGroupPart.PartLOD lod in mgp.partLODs)
                        foreach (Renderer r in lod.renderers)
                        {
                            SkinnedMeshRenderer sr = r as SkinnedMeshRenderer;
                            if (r.name.Contains(lod0_str))
                                for (int i = 0; i < sr.materials.Length; i++)
                                    TranscribeThunderRoadShader(sr.materials[i], fremenEyes[i]);
                            else if (r.name.Contains(lod1_str))
                                TranscribeThunderRoadShader(sr.material, fremenEyes[0]);
                        }
            }
            catch(NullReferenceException e)
            {
                Debug.LogError($"[Fremen-Eyes][ERROR][{Time.time}] NullReferenceException: {e.Message}");
            }
        }

        void TranscribeThunderRoadShader(Material target, Material reference)
        {
            target.SetTexture("_BaseMap", reference.GetTexture("_BaseMap"));
            target.SetColor("_BaseColor", eyeRGB);
            target.SetColor("_Tint0", eyeRGB);
            target.SetColor("_Tint1", eyeRGB);
            target.SetColor("_Tint2", eyeRGB);
            target.SetColor("_EmissionColor", eyeEmission);
            target.EnableKeyword("_EMISSIONMAP_ON");
            target.DisableKeyword("_COLORMASK_ON");
        }
    }
}
