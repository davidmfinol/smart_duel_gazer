using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.Config.Interface.Providers;

namespace AssemblyCSharp.Assets.Code.Core.Config.Impl.Providers
{
    public class DelayProvider : IDelayProvider
    {
        public Task Wait(int milliSeconds)
        {
            return Task.Delay(milliSeconds);
        }
    }
}