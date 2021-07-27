using System.Threading.Tasks;

namespace Code.Core.Config.Providers
{
    public interface IDelayProvider
    {
        Task Wait(int milliSeconds);
    }
    
    public class DelayProvider : IDelayProvider
    {
        public Task Wait(int milliSeconds)
        {
            return Task.Delay(milliSeconds);
        }
    }
}