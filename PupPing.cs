using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace WhereSlugpupMod
{
    class PupPing : UpdatableAndDeletable
    {
        int counter;
        int untilSound;

        public PupPing(Room room)
        {
            this.room = room;
            this.counter = 0;
            this.untilSound = 40;
        }

        public override void Update(bool b)
        {
            base.Update(b);
            this.counter++;
            if(this.counter >= this.untilSound)
            {
                this.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
                Debug.Log("WhereSlugpup : PupPing has played !");
                this.Destroy();
            }

        }

    }
}
