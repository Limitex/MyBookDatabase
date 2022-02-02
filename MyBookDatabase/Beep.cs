using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CA1416
namespace MyBookDatabase
{
    public static class Beep
    {
        public static async void Normal()
        {
            await Task.Run(() => 
            {
                Console.Beep(800, 100);
                Console.Beep(1000, 100);
            });
        }

        public static async void Error()
        {
            await Task.Run(() =>
            {
                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
                Console.Beep(1000, 100);
            });
        }
    }
}
