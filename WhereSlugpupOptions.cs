using Menu.Remix.MixedUI;
using UnityEngine;

namespace WhereSlugpupMod;

class WhereSlugpupOptions : OptionInterface
{
    public Configurable<bool> wantsPupID;
    public Configurable<bool> wantsPupRoom;
    public Configurable<bool> wantsPupMap;
    private readonly CustomLogger Logger;
    public WhereSlugpupOptions(CustomLogger logger)
    {
        Logger = logger;
        wantsPupID = config.Bind("WhereSlugpup_wantsPupID", true);
        wantsPupRoom = config.Bind("WhereSlugpup_wantsPupRoom", true);
        wantsPupMap = config.Bind("WhereSlugpup_wantsPupMap", true);
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
                new OpLabel(80, 400, "Show the found pup's spawn shelter on map", false)
        ];

        optionTab.AddItems(UIArrayElements);

    }


}


