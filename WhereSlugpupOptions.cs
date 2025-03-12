using Menu.Remix.MixedUI;
using UnityEngine;

namespace WhereSlugpupMod;

class WhereSlugpupOptions : OptionInterface
{
    public Configurable<bool> wantsPupID;
    public Configurable<bool> wantsPupRoom;
    public Configurable<bool> wantsPupMap;
    public Configurable<bool> wantsEnhancedSlugpupAwareness;
    private readonly CustomLogger CustomLogger;
    public WhereSlugpupOptions(CustomLogger logger)
    {
        CustomLogger = logger;
        wantsPupID = config.Bind("wantsPupID", true);
        wantsPupRoom = config.Bind("wantsPupRoom", true);
        wantsPupMap = config.Bind("wantsPupMap", true);
        wantsEnhancedSlugpupAwareness = config.Bind("wantsEnhancedSlugpupAwareness", true);
    }

    public override void Initialize()
    {
        OpTab optionTab = new(this, "Where Slugpup Options !");
        this.Tabs =
        [
                optionTab
        ];

        OpContainer tabContainer = new(new Vector2(0, 0));
        optionTab.AddItems(tabContainer);

        UIelement[] UIArrayElements =
        [
                new OpLabel(0, 550, "Where Slugpup Options !", true),
                new OpCheckBox(wantsPupID, 50, 500),
                new OpLabel(80, 500, "Show the found pup's ID", false),
                new OpCheckBox(wantsPupRoom, 50, 450),
                new OpLabel(80, 450, "Show the found pup's spawn shelter name", false),
                new OpCheckBox(wantsPupMap, 50, 400),
                new OpLabel(80, 400, "Show the found pup's spawn shelter on map", false),
                new OpCheckBox(wantsEnhancedSlugpupAwareness, 50, 350),
                new OpLabel(80, 350, "Enhanced capability of detecting pups in nearby rooms", false),
                new OpLabel(80, 320, "(This option requires pup map markers)", false)
        ];

        optionTab.AddItems(UIArrayElements);

    }


}


