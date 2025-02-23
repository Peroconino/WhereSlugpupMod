using BepInEx;
using System;
using UnityEngine;
//needed for IL hook : 
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using HUD;

namespace WhereSlugpupMod;


[BepInPlugin(GUID, Name, Version)]
public class WhereSlugpupMain : BaseUnityPlugin
{
    public const string GUID = "prismsoup.whereslugpupmod";
    public const string Name = "Where Slugpup ?";
    public const string Version = "1.0.2";
    bool isInit = false, isCycleStarted = false;
    private readonly WhereSlugpupOptions whereSlugpupOptions;
    private readonly CustomLogger CustomLogger;
    private readonly Dictionary<AbstractCreature, SlugpupData> unTammedPups = [];
    private readonly List<int> uniqueMarkers = [];
    private float StartTime;
    public WhereSlugpupMain()
    {
        CustomLogger = new CustomLogger();
        whereSlugpupOptions = new WhereSlugpupOptions(CustomLogger);
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
            On.AbstractRoom.AddEntity += Hook_AbstractRoom_AddEntity;

            On.AbstractCreature.Die += Hook_On_AbstractCreature_Die;

            On.HUD.Map.Update += Hook_HUD_Map_Update;

            // On.RegionGate.NewWorldLoaded += RegionGate_NewWorldLoaded;

            On.RainWorldGame.ctor += Hook_RainWorldGame_ctor;
            On.RainWorldGame.Update += Hook_RainWorldGame_Update;
            //On.AbstractCreatureAI.Moved += DidMySlugpupJustMove;

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
            var pup = creature;
            Vector2 markerPosition = creature.world.game.cameras[0].hud.map.mapData.ShelterMarkerPosOfRoom(creature.Room.index);
            markerPosition.y += 120;

            var player = creature.world.game.FirstAlivePlayer;

            if (player is not null && player.Room.name != pup.Room.name && !unTammedPups.ContainsKey(pup) && !isCycleStarted)
            {//if they are not in the same shelter we assume they are new pups
                var marker = new SlugpupMarker(pup.world.game.cameras[0].hud.map, self.index, markerPosition, SlugpupColor(pup));
                unTammedPups.Add(pup, new SlugpupData(marker)); // for now all pups here, must be filtered
                CustomLogger.LogInfo($"pup {pup.ID} added!");
                if (!uniqueMarkers.Any(roomIndex => roomIndex == marker.room))
                {
                    CustomLogger.LogInfo($"marker {marker.room} added!");
                    uniqueMarkers.Add(marker.room);
                }
            }
        }
    }
    private void Hook_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager processManager)
    {
        orig(self, processManager);
        isCycleStarted = false;
        StartTime = Time.time; // reset timer every respawn
        unTammedPups.Clear();// reset dictonary every respawn

    }
    private void Hook_RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
    {
        orig(self);

        var newPupsCount = unTammedPups.Count;
        if (!isCycleStarted && newPupsCount > 0 && newPupsCount <= 2)
        {
            List<KeyValuePair<AbstractCreature, SlugpupData>> pups = [.. unTammedPups];
            foreach (KeyValuePair<AbstractCreature, SlugpupData> pairPupData in pups)
            {
                var pupData = pairPupData.Value;
                var pupAbstract = pairPupData.Key;
                pupData.IsNewPup = false;

                string text = "A slugpup has spawned";

                if (whereSlugpupOptions.wantsPupID.Value)
                    text += " (" + pupAbstract.ID.RandomSeed + ")";
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
            self.cameras[0].hud.textPrompt.AddMessage(text, 10, 450, false, true);
            self.cameras[0].room.AddObject(new PupPing(self.cameras[0].room));
            isCycleStarted = true;
        }

        if ((Time.time - StartTime) >= 1.0f) //Enters here if world reset and you have all pups with you already
        {
            isCycleStarted = true;
        }
    }
    // private void DidMySlugpupJustMove(On.AbstractCreatureAI.orig_Moved orig, AbstractCreatureAI self)
    // {
    //     orig(self);
    //     if (self.world.game.IsStorySession && isNewPupFound && self.parent.ID == pupNPC?.ID)
    //     {
    //         print("WhereSlugpup : pup was at : " + self.lastRoom + " and moved at :" + self.parent.pos.room);
    //         foundPupMarker?.PupMoved(self.parent.pos.room);
    //     }
    // }
    private void Hook_HUD_Map_Update(On.HUD.Map.orig_Update orig, Map self)
    {
        orig(self);
        if (whereSlugpupOptions.wantsPupMap.Value && unTammedPups.Count > 0 && uniqueMarkers.Count > 0)
        {
            for (int i = 0; i < uniqueMarkers.Count; i++)
            {
                var pup = unTammedPups.First(pup => pup.Value.FoundPupMarker.room == uniqueMarkers[i]);
                if (!pup.Value.IsMarkedOnTheMap)
                {
                    pup.Key.world.game.cameras[0].hud.map.mapObjects.Add(unTammedPups[pup.Key].FoundPupMarker);
                    pup.Value.IsMarkedOnTheMap = true;
                }
            }
        }
    }
    private void Hook_On_AbstractCreature_Die(On.AbstractCreature.orig_Die orig, AbstractCreature self)
    {
        try
        {
            if (self.world.game.IsStorySession && unTammedPups.ContainsKey(self))
            {
                var pupData = unTammedPups[self];
                pupData.FoundPupMarker.PupDied();
            }
        }
        catch (Exception e)
        {
            CustomLogger.LogError(e);
        }

        orig(self);
    }
    //Gets the slugpup's color from its ID and returns it
    private static Color SlugpupColor(AbstractCreature pupNPC)
    {

        Random.State state = Random.state;
        Random.InitState(pupNPC.ID.RandomSeed);

        //values needed for slugpup color
        Random.Range(0f, 1f); //meant to waste a value  
        float met = Mathf.Pow(Random.Range(0f, 1f), 1.5f);
        float stealth = Mathf.Pow(Random.Range(0f, 1f), 1.5f);
        Random.Range(0f, 1f); //meant to waste a value
        Random.Range(0f, 1f); //meant to waste a value
        float hue = Mathf.Lerp(Random.Range(0.15f, 0.58f), Random.value, Mathf.Pow(Random.value, 1.5f - met));
        float saturation = Mathf.Pow(Random.Range(0f, 1f), 0.3f + stealth * 0.3f);
        bool dark = Random.Range(0f, 1f) <= 0.3f + stealth * 0.2f;
        float luminosity = Mathf.Pow(Random.Range(dark ? 0.9f : 0.75f, 1f), 1.5f - stealth);

        Random.state = state;

        return RWCustom.Custom.HSL2RGB(hue, saturation, Mathf.Clamp(dark ? (1f - luminosity) : luminosity, 0.01f, 1f), 1f);
    }
}

