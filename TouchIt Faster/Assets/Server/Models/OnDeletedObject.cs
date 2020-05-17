using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Server.Models
{
    public class OnDeletedObject
    {
        public int CurrentPoints;
        public int MaxTapsInARow;
        public int CurrTapsInARow;
        public float Time;
        public float ExtraTime;
        public int ObjectID;
    }
}
