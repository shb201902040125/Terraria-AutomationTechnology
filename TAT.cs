using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;

namespace TAT
{
    public class TAT : Mod
    {
        internal static TAT Ins { get; private set; }
        internal static bool Test = true;
        public TAT()
        {
            Ins = this;
        }
    }
}