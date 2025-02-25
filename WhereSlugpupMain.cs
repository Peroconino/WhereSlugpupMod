using BepInEx;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using HUD;
using System.Reflection;
using System.Text;
using System.Globalization;

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
    private readonly Dictionary<AbstractCreature, SlugpupData> tammedPups = [];
    private readonly HashSet<int> uniqueMarkers = []; //hashset is better to guarantee theres no duplicates
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
            var pup = creature;
            Vector2 markerPosition = pup.world.game.cameras[0].hud.map.mapData.ShelterMarkerPosOfRoom(pup.Room.index);
            markerPosition.y += 120;

            var player = creature.world.game.FirstAlivePlayer;

            if (!isCycleStarted && !unTammedPups.ContainsKey(pup) && !tammedPups.ContainsKey(pup) && player is not null && player.Room.name != pup.Room.name)
            {//if they are not in the same shelter we assume they are new pups
                var marker = new SlugpupMarker(pup.world.game.cameras[0].hud.map, self.index, markerPosition, SlugpupColor(pup));
                unTammedPups.Add(pup, new SlugpupData(marker));
                CustomLogger.LogInfo($"pup {pup.ID} added to unTammed!");
                if (!uniqueMarkers.Any(roomIndex => roomIndex == marker.room))
                {
                    CustomLogger.LogInfo($"marker {marker.room} added!");
                    uniqueMarkers.Add(marker.room);
                }
            }
            else if (!isCycleStarted && !unTammedPups.ContainsKey(pup) && !tammedPups.ContainsKey(pup) && player is not null)
            {
                var marker = new SlugpupMarker(pup.world.game.cameras[0].hud.map, self.index, markerPosition, SlugpupColor(pup));
                CustomLogger.LogInfo($"pup {pup.ID} added to tammed!");
                tammedPups.Add(pup, new SlugpupData(marker, false));
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
    private void Hook_RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager processManager)
    {
        orig(self, processManager);
        isCycleStarted = false;
        StartTime = Time.time; // reset timer every respawn
        unTammedPups.Clear();// reset dictonaries every respawn
        tammedPups.Clear();
        uniqueMarkers.Clear();
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
            foreach (var pup in unTammedPups.Select(pupPair => pupPair.Key).ToList())
            {
                if (whereSlugpupOptions.wantsPupID.Value)
                    _ = sb.AppendFormat(CultureInfo.InvariantCulture, " ({0})", pup.ID);
                if (whereSlugpupOptions.wantsPupRoom.Value)
                    _ = sb.AppendFormat(CultureInfo.InvariantCulture, " in {0}", pup.Room.name);
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

        if (whereSlugpupOptions.wantsEnhancedSlugpupAwareness.Value && self.fade > 0f)
        {
            List<AbstractCreature> slugcats = [.. unTammedPups.Keys, .. tammedPups.Keys];

            FieldInfo creatureSymbolsField = typeof(Map).GetField("creatureSymbols", BindingFlags.NonPublic | BindingFlags.Instance);
            List<CreatureSymbol> creatureSymbols = (List<CreatureSymbol>)creatureSymbolsField.GetValue(self);// needed to use reflection here to access creatureSymbols

            foreach (CreatureSymbol creatureSymbol in creatureSymbols)
            {
                if (creatureSymbol.iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                {
                    creatureSymbol.RemoveSprites();
                }
            }

            foreach (AbstractCreature slugcat in slugcats)
            {
                CreateSlugpupSymbol(self, slugcat, creatureSymbols, timeStacker);
            }
            slugcats.Clear();
        }

        if (whereSlugpupOptions.wantsPupMap.Value && unTammedPups.Count > 0 && uniqueMarkers.Count > 0)
        {
            for (int i = 0; i < uniqueMarkers.Count; i++)
            {
                var pup = unTammedPups.First(pup => pup.Value.FoundPupMarker.room == uniqueMarkers.ElementAt(i));
                if (!pup.Value.IsMarkedOnTheMap)
                {
                    pup.Key.world.game.cameras[0].hud.map.mapObjects.Add(unTammedPups[pup.Key].FoundPupMarker);
                    pup.Value.IsMarkedOnTheMap = true;
                    CustomLogger.LogInfo($"begining marker pos: {unTammedPups[pup.Key].FoundPupMarker.inRoomPos}");
                }
            }
        }
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
    private static void CreateSlugpupSymbol(Map self, AbstractCreature slugcat, List<CreatureSymbol> creatureSymbols, float timeStacker)
    {
        if (slugcat.pos.TileDefined)
        {
            CreatureSymbol slugPupSymbol = new(CreatureSymbol.SymbolDataFromCreature(slugcat), self.inFrontContainer);
            slugPupSymbol.Show(true);
            slugPupSymbol.lastShowFlash = 0f;
            slugPupSymbol.showFlash = 0f;
            slugPupSymbol.myColor = SlugpupColor(slugcat);
            slugPupSymbol.symbolSprite.alpha = 0.9f;

            if (slugcat.realizedCreature == null || slugcat.realizedCreature.dead)
            {
                slugPupSymbol.symbolSprite.scale = 0.8f;
                slugPupSymbol.symbolSprite.alpha = 0.7f;
            }
            slugPupSymbol.shadowSprite1.alpha = slugPupSymbol.symbolSprite.alpha;
            slugPupSymbol.shadowSprite2.alpha = slugPupSymbol.symbolSprite.alpha;
            slugPupSymbol.shadowSprite1.scale = slugPupSymbol.symbolSprite.scale;
            slugPupSymbol.shadowSprite2.scale = slugPupSymbol.symbolSprite.scale;
            Vector2 drawPos = self.RoomToMapPos((slugcat.realizedCreature == null) ? (slugcat.pos.Tile.ToVector2() * 20f) : slugcat.realizedCreature.mainBodyChunk.pos, slugcat.Room.index, timeStacker);
            slugPupSymbol.Draw(timeStacker, drawPos);
            creatureSymbols.Add(slugPupSymbol);
        }

    }
}

