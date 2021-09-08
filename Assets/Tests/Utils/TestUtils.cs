using System;
using System.Threading.Tasks;

namespace Tests.Utils
{
    public static class TestUtils
    {
        public static T RunAsyncMethodSync<T>(Func<Task<T>> asyncFunc)
        {
            return Task.Run(async () => await asyncFunc()).GetAwaiter().GetResult();
        }

        public static void RunAsyncMethodSync(Func<Task> asyncFunc)
        {
            Task.Run(async () => await asyncFunc()).GetAwaiter().GetResult();
        }
    }
}