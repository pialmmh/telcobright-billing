using System;
using System.Threading.Tasks;

namespace Utils
{
    public class AsyncAwaitDemo
    {
        public async Task<int> DoStuff(int counterStart)
        {
            Task<int> x = null;
            await Task.Run(() =>
            {
                x = LongRunningOperation(counterStart);
            });
            return x.Result;
        }

        private static async Task<int> LongRunningOperation(int counterStart)
        {
            int counter;

            for (counter = counterStart; counter < 5; counter++)
            {
                await Task.Delay(1000);
                Console.WriteLine(counter);
            }

            return counter;
        }
    }
}

