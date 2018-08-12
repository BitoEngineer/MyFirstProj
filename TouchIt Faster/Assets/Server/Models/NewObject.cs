using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstServ.Models.TouchItFaster
{
    public enum ObjectType
    {
        Bomb,
        Circle1,
        Circle2,
        Circle3,
        Special
    }

    public class NewObject
    {
        public int ID;
        public float X;
        public float Y;
        public double Size;
        public ObjectType Type;
    }
}
