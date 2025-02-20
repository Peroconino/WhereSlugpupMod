using BepInEx;
using System;
using UnityEngine;
//needed for IL hook : 
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Random = UnityEngine.Random;
using System.Linq;

namespace WhereSlugpupMod
{

    [BepInPlugin("prismsoup.whereslugpupmod", "Where Slugpup ?", "1.0.2")]
    public class WhereSlugpupMain : BaseUnityPlugin
    {
        AbstractCreature pupNPC;
        AbstractRoom pupRoom;
        bool isNewPupFound;
        bool isCycleStart;
        bool init;
        bool isPupDead;

        SlugpupMarker foundPupMarker;

        WhereSlugpupOptions whereSlugpupOptions;
        static WhereSlugpupMain WSInstance;


        public WhereSlugpupMain()
        {
                this.isNewPupFound = false;
                this.isCycleStart = false;
                this.init = false;
                this.isPupDead = false;
                this.whereSlugpupOptions = new WhereSlugpupOptions();

            if (!WSInstance)
            {
                WSInstance = this;
            }
            else
            {
                base.Logger.LogWarning("WhereSlugpup WSInstance duplicate has been destroyed");
                Destroy(this);
                
            }
        }

        public void Awake()
        {
            IL.World.SpawnPupNPCs += World_SpawnPupNPCs;
            On.RainWorldGame.ctor += RainWorldGame_ctor;
            On.RainWorldGame.Update += RainWorldGame_Update;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RegionGate.NewWorldLoaded += RegionGate_NewWorldLoaded;
            On.AbstractCreature.Die += DidMyPupJustDieAbstract;
            //On.AbstractCreatureAI.Moved += DidMySlugpupJustMove;
            
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (this.init)
            {
                return;
            }
            this.init = true;
            try
            {
                MachineConnector.SetRegisteredOI("prismsoup.whereslugpupmod", this.whereSlugpupOptions);
            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex);
            }
        }

        private void DidMySlugpupJustMove(On.AbstractCreatureAI.orig_Moved orig, AbstractCreatureAI self)
        {
            orig(self);
            if (self.world.game.IsStorySession && this.isNewPupFound && self.parent.ID == this.pupNPC.ID)
            {
                print("WhereSlugpup : pup was at : " + self.lastRoom + " and moved at :" + self.parent.pos.room);
                this.foundPupMarker?.PupMoved(self.parent.pos.room);
            }

        }

        //Checks if the spawned slugpup has died
        private void DidMyPupJustDieAbstract(On.AbstractCreature.orig_Die orig, AbstractCreature self)
        {
            try
            {
                if (self.world.game.IsStorySession && this.isNewPupFound && self.ID == this.pupNPC.ID)
                {
                    this.isPupDead = true;
                    this.foundPupMarker?.PupDied();
                }
            }
            catch(Exception e)
            {
                base.Logger.LogError(e);
                throw;
            }
            orig(self);

        }

        //Is called whenever the player crosses to a new region via a gate
        //Resets isNewPupFound, isCycleStart and isPupDead to their default values
        private void RegionGate_NewWorldLoaded(On.RegionGate.orig_NewWorldLoaded orig, RegionGate self)
        {
            orig(self);
            try
            {
                Debug.Log("whereslugpup : regiongateloaded");
                this.isNewPupFound = false;
                this.isCycleStart = true; //not a new cycle but respawns message
                this.isPupDead = false;
            }
            catch(Exception e)
            {
                base.Logger.LogError(e);
                throw;
            }
            
        }

        //Gets the slugpup's color from its ID and returns it
        private Color SlugpupColor()
        {
            Random.State state = Random.state;
            Random.InitState(this.pupNPC.ID.RandomSeed);

            //values needed for slugpup color
            Random.Range(0f, 1f); //meant to waste a value  
            float met = Mathf.Pow(Random.Range(0f, 1f), 1.5f);
            float stealth = Mathf.Pow(Random.Range(0f, 1f), 1.5f);
            Random.Range(0f, 1f); //meant to waste a value
            Random.Range(0f, 1f); //meant to waste a value
            float hue = Mathf.Lerp(Random.Range(0.15f, 0.58f), Random.value, Mathf.Pow(Random.value, 1.5f - met));
		    float saturation = Mathf.Pow(Random.Range(0f, 1f), 0.3f + stealth * 0.3f);
		    bool dark = (Random.Range(0f, 1f) <= 0.3f + stealth * 0.2f);
		    float luminosity = Mathf.Pow(Random.Range(dark ? 0.9f : 0.75f, 1f), 1.5f - stealth);
            //float eyeColor = Mathf.Pow(Random.Range(0f, 1f), 2f - this.Stealth * 1.5f);

            Random.state = state;

            //Debug.Log("whereslugpup : hue "+hue+" - saturation "+saturation+" - dark "+dark+" - luminosity "+luminosity);
            return RWCustom.Custom.HSL2RGB(hue, saturation, Mathf.Clamp(dark ? (1f - luminosity) : luminosity, 0.01f, 1f), 1f);
            
        }

        //
        public void SlugpupFound(AbstractCreature pupNPC, AbstractRoom pupRoom)
        {
            Debug.Log("whereslugpup : slugpup found ! - "+pupNPC.ID.RandomSeed + " - "+ pupRoom.name);
            this.pupNPC = pupNPC;
            this.pupRoom = pupRoom;
            this.isNewPupFound = true; //note : abstract sluppies can get through rooms they shouldnt be going through at times it seems ? make slup tracker if possible ??
        }

        //ILHook
        //Gets local values (Abstract Creature and Abstract Room) when a slugpup is spawned (the pup itself and
        //the room it spawns in respectively) then calls the SlugpupFound method with these values as parameters
        public static void World_SpawnPupNPCs(ILContext il)
        {
            try
            {
                ILCursor cursor = new ILCursor(il);
                cursor.GotoNext(
                    x => x.MatchCallvirt<AbstractRoom>("AddEntity")
                    );
                
                cursor.Index -= 1;
                cursor.Emit(OpCodes.Ldloc,6);
                cursor.Emit(OpCodes.Ldloc,13);

                cursor.EmitDelegate<Action<AbstractRoom, AbstractCreature>>((pupRoom, pupNPC) => {
                    //base.Logger.LogInfo("whereslugpup : IL hook ! pup " + pupNPC + " room " + pupRoom);
                    WSInstance.SlugpupFound(pupNPC, pupRoom);
                });

            }catch(Exception e)
            {
                //base.Logger.LogError(e);
                throw;
            }
            
        }

        //The RainWolrdGame ctor should be called at each beginning of a cycle, allowing isCycleStart, isNewPupFound and isPupDead to be 
        //reset at their default values
        private void RainWorldGame_ctor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            try
            { 
                this.isCycleStart = true;
                this.isNewPupFound = false;
                this.isPupDead = false;
                Debug.Log("whereslugpup RW CTOR");
            }
            catch(Exception e)
            {
                base.Logger.LogError(e);
                throw;
            }
        }

        //Spawns a message, a map marker and plays a sound effect if a pup has been found during a Campaign or Expedition
        private void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            try
            { 
                if (self.IsStorySession && self.cameras[0].hud != null && this.isNewPupFound && this.isCycleStart)
                {
                    string text = "A slugpup has spawned";
                    if (this.whereSlugpupOptions.wantsPupID.Value) text += " (" + this.pupNPC.ID.RandomSeed + ")";
                    if (this.whereSlugpupOptions.wantsPupRoom.Value) text += " in " + this.pupRoom.name;
                    //self.cameras[0].hud.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom); this only plays when it's not the beginning of a cycle it seems
                    Debug.Log("whereslugpup : message appears");
                    self.cameras[0].hud.textPrompt.AddMessage(text, 10, 450, false, true);
                    //self.cameras[0].hud.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom); doesnt seem to work for some reason when placed after the message
                    //self.cameras[0].room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f); //similarly, this only works when not right at the beginning of a cycle

                    self.cameras[0].room.AddObject(new PupPing(self.cameras[0].room));

                    // for later -> idk check how expedition does it but also I dont know how it does it -> custom icon and custom text
                    isCycleStart = false;
                    

                    if (this.whereSlugpupOptions.wantsPupMap.Value)
                    {
                        //add moving circle around ? like swarm ones ?
                        //add slup symbol
                        Vector2 markerPosition = self.cameras[0].hud.map.mapData.ShelterMarkerPosOfRoom(this.pupRoom.index);
                        markerPosition.y += 120;
                        this.foundPupMarker = new SlugpupMarker(self.cameras[0].hud.map, this.pupRoom.index, markerPosition, this.SlugpupColor());
                        self.cameras[0].hud.map.mapObjects.Add(this.foundPupMarker);
                        //Debug.Log("whereslugpup: map marker added");
                    }
                }
            }
            catch(Exception e)
            {
                base.Logger.LogError(e);
                throw;

            }
            
        }

    }
}
