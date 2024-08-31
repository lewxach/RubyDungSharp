using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubyDungSharp
{
    public interface ILevelListener
    {
        void TileChanged(int x, int y, int z);

        void LightColumnChanged(int x, int y, int z, int intensity);

        void AllChanged();
    }
}