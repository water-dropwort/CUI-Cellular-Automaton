using System;
using System.ComponentModel;

namespace CUICA
{
    public class Program
    {
        const ConsoleColor CC_STATE0 = ConsoleColor.Black;
        const ConsoleColor CC_STATE1 = ConsoleColor.Green;
        const int CELL_ROW_COUNT = 15;
        const int CELL_COLUMN_COUNT = 30;

        public static void Main()
        {
            var app = new App(CELL_ROW_COUNT, CELL_COLUMN_COUNT, CC_STATE0, CC_STATE1);
            app.Run();
        }
    }
}
