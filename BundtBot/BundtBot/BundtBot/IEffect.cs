using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundtBot.BundtBot {
    public interface IEffect {
        float ApplyEffect(float sample);
    }
}
