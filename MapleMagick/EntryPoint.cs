using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acuerdo.Plugin;

namespace MapleMagick
{
    public class EntryPoint : IPlugin
    {
        public string Name
        {
            get { return "MapleMagick Tweetholicer Helper Library"; }
        }

        public double Version
        {
            get { throw new NotImplementedException(); }
        }

        public void Loaded()
        {
            throw new NotImplementedException();
        }
    }

}
