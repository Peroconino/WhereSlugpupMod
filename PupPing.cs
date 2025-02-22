namespace WhereSlugpupMod;
class PupPing : UpdatableAndDeletable
{
    int counter;
    readonly int untilSound;

    public PupPing(Room pupRoom)
    {
        room = pupRoom;
        counter = 0;
        untilSound = 40;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        counter++;
        if (counter >= untilSound)
        {
            room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
            Debug.Log("WhereSlugpup : PupPing has played !");
            Destroy();
        }

    }

}

