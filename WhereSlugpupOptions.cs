using JetBrains.Annotations;
using Menu.Remix.MixedUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WhereSlugpupMod
{
    class WhereSlugpupOptions : OptionInterface
    {
        public Configurable<bool> wantsPupID;
        public Configurable<bool> wantsPupRoom;
        public Configurable<bool> wantsPupMap;

        public WhereSlugpupOptions()
        {
            wantsPupID = this.config.Bind<bool>("WhereSlugpup_wantsPupID", true);
            wantsPupRoom = this.config.Bind<bool>("WhereSlugpup_wantsPupRoom", true);
            wantsPupMap = this.config.Bind<bool>("WhereSlugpup_wantsPupMap", true);
        }

        public override void Initialize()
        {
            OpTab optionTab = new OpTab(this, "Where Slugpup Options !");
            this.Tabs = new OpTab[]
            {
                optionTab
            };

            OpContainer tabContainer = new OpContainer(new Vector2(0, 0));
            optionTab.AddItems(tabContainer);

            UIelement[] UIArrayElements = new UIelement[]
            {
                new OpLabel(0, 550, "Where Slugpup Options !", true),
                new OpCheckBox(wantsPupID, 50, 500),
                new OpLabel(80, 500, "Show the found pup's ID", false),
                new OpCheckBox(wantsPupRoom, 50, 450),
                new OpLabel(80, 450, "Show the found pup's spawn shelter name", false),
                new OpCheckBox(wantsPupMap, 50, 400),
                new OpLabel(80, 400, "Show the found pup's spawn shelter on map", false)
            };

            optionTab.AddItems(UIArrayElements);

        }

        
    }

}
