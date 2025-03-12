using BepInEx;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using HUD;
using System.Text;
using System.Globalization;

namespace WhereSlugpupMod;


[BepInPlugin(GUID, Name, Version)]
partial class WhereSlugpupMain : BaseUnityPlugin
{
    public const string GUID = "prismsoup.whereslugpupmod";
    public const string Name = "Where Slugpup ?";
    public const string Version = "1.0.2";
    bool isInit = false, isCycleStarted = false;
    private readonly WhereSlugpupOptions whereSlugpupOptions;
    private readonly CustomLogger CustomLogger;
    private readonly SpawnedPups SpawnedPups;
    private float StartTime;
    public WhereSlugpupMain()
    {
        CustomLogger = new();
        whereSlugpupOptions = new(CustomLogger);
        SpawnedPups = new();
    }

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (isInit) return;

        try
        {
            On.AbstractRoom.AddEntity += Hook_AbstractRoom_AddEntity;//adds the pups to Dictonaries

            On.AbstractCreature.Die += Hook_On_AbstractCreature_Die;//changes the icon to dead slugcat if the pup dies

            On.HUD.Map.Draw += Hook_Map_Draw;//rendering the things on the map

            On.RainWorldGame.ctor += Hook_RainWorldGame_ctor;//reset values
            On.RainWorldGame.Update += Hook_RainWorldGame_Update;// player notification

            MachineConnector.SetRegisteredOI(GUID, whereSlugpupOptions);
            isInit = true;
        }
        catch (Exception ex)
        {
            CustomLogger.LogError(ex);
        }
    }
    private void Hook_AbstractRoom_AddEntity(On.AbstractRoom.orig_AddEntity orig, AbstractRoom self, AbstractWorldEntity abstractWorldEntity)
    {
        orig(self, abstractWorldEntity);
        var creature = abstractWorldEntity as AbstractCreature;
        if (creature?.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
        {
            var unTammedPups = SpawnedPups.unTammedPups;
            var tammedPups = SpawnedPups.tammedPups;
            var uniqueMarkers = SpawnedPups.uniqueMarkers;
            var pup = creature;
            Vector2 markerPosition = pup.world.game.cameras[0].hud.map.mapData.ShelterMarkerPosOfRoom(pup.Room.index);
            markerPosition.y += 120;

            var player = creature.world.game.FirstAlivePlayer;

            if (!isCycleStarted && !unTammedPups.ContainsKey(pup) && !tammedPups.ContainsKey(pup) && player is not null && player.Room.name != pup.Room.name)
            {
                var marker = new SlugpupMarker(pup.world.game.cameras[0].hud.map, self.index, markerPosition, SlugpupColor(pup));
                unTammedPups.Add(pup, new SlugpupData(marker));

                if (!uniqueMarkers.Any(roomIndex => roomIndex == marker.room))
                {
                    uniqueMarkers.Add(marker.room);

                    CustomLogger.LogInfo($"marker {marker.room} added!");
                }

                CustomLogger.LogInfo($"pup {pup.ID} added to unTammed!");
            }
            else if (!isCycleStarted && !unTammedPups.ContainsKey(pup) && !tammedPups.ContainsKey(pup) && player is not null)
            {
                var marker = new SlugpupMarker(pup.world.game.cameras[0].hud.map, self.index, markerPosition, SlugpupColor(pup));
                tammedPups.Add(pup, new SlugpupData(marker, false));

                CustomLogger.LogInfo($"pup {pup.ID} added to tammed!");
            }
        }
    }
    private void Hook_On_AbstractCreature_Die(On.AbstractCreature.orig_Die orig, AbstractCreature self)
    {

        if (self.world.game.IsStorySession && SpawnedPups.ContainsKey(self))
        {
            var pupData = SpawnedPups.unTammedPups[self];
            pupData.FoundPupMarker.PupDied();
        }

        orig(self);
    }
    private void Hook_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager processManager)
    {
        orig(self, processManager);
        isCycleStarted = false;
        StartTime = Time.time;
        SpawnedPups.Clear();// reset dictonaries every respawn
    }
    private void Hook_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        var newPupsCount = SpawnedPups.unTammedPups.Count;
        if (!isCycleStarted && newPupsCount > 0 && newPupsCount <= 2)
        {
            List<KeyValuePair<AbstractCreature, SlugpupData>> pups = [.. SpawnedPups.unTammedPups];
            foreach (KeyValuePair<AbstractCreature, SlugpupData> pairPupData in pups)
            {
                var pupData = pairPupData.Value;
                var pupAbstract = pairPupData.Key;
                pupData.IsNewPup = false;

                string text = "A slugpup has spawned";

                if (whereSlugpupOptions.wantsPupID.Value)
                    text += " (" + pupAbstract.ID + ")";
                if (whereSlugpupOptions.wantsPupRoom.Value)
                    text += " in " + pupAbstract.Room.name;

                self.cameras[0].hud.textPrompt.AddMessage(text, 10, 450, false, true);
                self.cameras[0].room.AddObject(new PupPing(self.cameras[0].room));
                isCycleStarted = true;
            }
        }
        else if (!isCycleStarted && newPupsCount > 0)
        {
            string text = "Many slugpups has spawned!";
            var sb = new StringBuilder(text); // more efficient with stringBuilder
            foreach (var pup in SpawnedPups.unTammedPups.Select(pupPair => pupPair.Key).AsEnumerable().Where(pup => pup is not null))
            {
                if (whereSlugpupOptions.wantsPupID.Value)
                    _ = sb.AppendFormat(CultureInfo.InvariantCulture, " {0}", pup!.ID);
                if (whereSlugpupOptions.wantsPupRoom.Value)
                    _ = sb.AppendFormat(CultureInfo.InvariantCulture, " in {0}", pup!.Room.name);
            }
            text = sb.ToString();
            self.cameras[0].hud.textPrompt.AddMessage(text, 10, 450, false, true);
            self.cameras[0].room.AddObject(new PupPing(self.cameras[0].room));
            isCycleStarted = true;
        }

        if ((Time.time - StartTime) >= 1.0f) //Enters here if world reset and you have all pups with you already
        {
            isCycleStarted = true;
        }
    }
    private void Hook_Map_Draw(On.HUD.Map.orig_Draw orig, Map self, float timeStacker)
    {
        orig(self, timeStacker);

        CreateMovingPupsIcons(self, timeStacker);

        CreateStaticPupsIcons();
    }
}

