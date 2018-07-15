using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    public class TaskTester
    {
        public async void Test()
        {
            //Go();//this is a beautiful example that shows how to use results from multiple tasks.
            MyGoAsync();
        }
        static void MyGo()
        {
            MyGoAsync();
            Console.WriteLine("Finished");
            Thread.Sleep(100000);
            Console.Read();
            Console.ReadLine();
        }

        static async void MyGoAsync()
        {
            List<Task<int>> tasks = new List<Task<int>>();
            for (int i = 0; i < 2; i++)
            {
                Task<int> task = Doubler(i + 1);
                tasks.Add(task);
            }
            //int[] resultInts = await Task.WhenAll(tasks);
            int[] resultInts = new int[2];
            while (tasks.Count > 0)
            {
                var task = Task.WhenAny(tasks).Result;
                tasks.Remove(task);
                int result = task.Result;
                Console.WriteLine($"received result={result}");
            }
        }

        public static async Task<int> Doubler(int val)
        {
            var millisecondsDelay = 10000 - val * 3000;
            Console.WriteLine($"delay {millisecondsDelay}");
            await Task.Delay(millisecondsDelay);
            return val * 2;
        }

        
        public static void Go()
        {
            GoAsync();
            Console.ReadLine();
        }

        public static async void GoAsync()
        {

            Console.WriteLine("Starting");

            Task<int> task1 = Sleep(5000);
            Task<int> task2 = Sleep(3000);
            List< Task < int >> tasks=new List<Task<int>>();
            tasks.Add(task1);
            tasks.Add(task2);
            //int[] result = await Task.WhenAll(task1, task2);//original code
            int[] result = await Task.WhenAll(tasks);
            Console.WriteLine("Slept for a total of " + result.Sum() + " ms");
        }

        private async static Task<int> Sleep(int ms)
        {
            Console.WriteLine($@"Sleeping for {ms} milliseconds");
            await Task.Delay(ms);
            return ms;
        }



    }
}
