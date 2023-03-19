using System;
using System.ComponentModel;

namespace CUICA
{
    public class App
    {
        // Cell's color
        private readonly ConsoleColor mccState0;
        private readonly ConsoleColor mccState1;
        // Cell's row/column count
        private readonly int mCellRowCount;
        private readonly int mCellColumnCount;
        // Instances
        private readonly TwoStateCellularAutomaton mCA;
        private readonly BackgroundWorker mWorker;
        // Char of cell
        private readonly char SQUARE = '■';
        // Byte-size of square
        private readonly int SQUARE_WIDTH = 2;
        // Cursor's positions
        private readonly int mAppCursorTop;
        private readonly int mAppCursorBottom;
        private readonly int mCellCursorTop;
        private readonly int mCellCursorLeft;
        // If worker is running, this value is true.
        private bool mIsRunningCA = false;
        // Store cursor position when worker started.
        private int mCursorLeftBackup = 0;
        private int mCursorTopBackup = 0;

        // Constructor
        public App(int cellRowCount, int cellColumnCount, ConsoleColor ccState0, ConsoleColor ccState1)
        {
            mCellRowCount = cellRowCount;
            mCellColumnCount = cellColumnCount;
            mccState0 = ccState0;
            mccState1 = ccState1;

            mCA = new TwoStateCellularAutomaton(mCellRowCount, mCellColumnCount);
            mWorker = new BackgroundWorker();
            mWorker.WorkerSupportsCancellation = true;
            mWorker.DoWork += DoWork_Worker;
            mWorker.RunWorkerCompleted += Completed_Worker;

            var posns = InitializeCUI();
            mAppCursorTop = posns.Item1;
            mAppCursorBottom = posns.Item2;
            mCellCursorTop = posns.Item3;
            mCellCursorLeft = posns.Item4;

            // Move the cursor to top-left corner.
            Console.SetCursorPosition(mCellCursorLeft, mCellCursorTop);

            Console.CancelKeyPress += CancelKeyPress;
        }

        // Start application
        public void Run()
        {
            try
            {
                bool exitFlag = false;
                while(true)
                {
                    if(exitFlag == true) break;

                    var cki = Console.ReadKey(true);
                    switch(cki.Key)
                    {
                        // Run
                        case ConsoleKey.R:
                            RunCA();
                            break;
                        // Cancel
                        case ConsoleKey.C:
                            CancelCA();
                            break;
                        // Move the cursor
                        case ConsoleKey.UpArrow:
                            if(mIsRunningCA == false)
                                MoveCursor(-1, 0);
                            break;
                        case ConsoleKey.DownArrow:
                            if(mIsRunningCA == false)
                                MoveCursor(+1, 0);
                            break;
                        case ConsoleKey.LeftArrow:
                            if(mIsRunningCA == false)
                                MoveCursor(0, -1);
                            break;
                        case ConsoleKey.RightArrow:
                            if(mIsRunningCA == false)
                                MoveCursor(0, +1);
                            break;
                        // Update the cell's state
                        case ConsoleKey.U:
                            if(mIsRunningCA == false)
                                UpdateCurrentCell();
                            break;
                        // Exit
                        case ConsoleKey.E:
                            CancelCA();
                            exitFlag = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            finally
            {
                Console.ResetColor();
                Console.SetCursorPosition(0, mAppCursorBottom);
            }
        }

        // Write cell's and border.
        private Tuple<int,int,int,int> InitializeCUI()
        {
            int appCursorTop = 0;
            int appCursorBottom = 0;
            int limitCellCursorTop = 0;
            int limitCellCursorLeft = 0;

            WriteVerticalLine();

            for(int i = 0; i < mCellRowCount; i++)
            {
                Console.ResetColor();
                Console.Write("｜");
                limitCellCursorLeft = Console.CursorLeft;
                for(int j = 0; j < mCellColumnCount; j++)
                {
                    Console.ForegroundColor = mCA.Cells[i,j] ? mccState1 : mccState0;
                    Console.Write(SQUARE);
                }
                Console.ResetColor();
                Console.WriteLine("｜");
            }

            WriteVerticalLine();
            Console.Write("Press command key:");

            appCursorTop = Console.CursorTop - mCellRowCount - 2; // 2 = two vertical lines.
            appCursorBottom = Console.CursorTop;
            limitCellCursorTop = appCursorTop + 1; // 1 = a vertical line.
            return new Tuple<int,int,int,int>(appCursorTop, appCursorBottom, limitCellCursorTop, limitCellCursorLeft);
        }

        // Write a vertical line in the initialization.
        private void WriteVerticalLine()
        {
            Console.ResetColor();
            Console.Write("＋");
            for(int j = 0; j < mCellColumnCount; j++)
                Console.Write("―");
            Console.WriteLine("＋");
        }

        // Convert cursor's position to index.
        private Tuple<int,int> FromCursorPos(int cursorTop, int cursorLeft)
        {
            int row = cursorTop - mCellCursorTop;
            int col = (cursorLeft - mCellCursorLeft) / SQUARE_WIDTH;
            return new Tuple<int,int>(row, col);
        }

        // Convert index to cursor's position.
        private Tuple<int,int> ToCursorPos(int row, int col)
        {
            int top = mCellCursorTop + row;
            int left = mCellCursorLeft + (col * SQUARE_WIDTH);
            return new Tuple<int,int>(top, left);
        }

        // Start worker thread.
        private void RunCA()
        {
            if(mIsRunningCA == false)
                mWorker.RunWorkerAsync();
        }

        // Stop worker thread.
        private void CancelCA()
        {
            if(mIsRunningCA == true)
                mWorker.CancelAsync();
        }

        // Move cursor by the specified amount.
        private void MoveCursor(int deltaRow, int deltaCol)
        {
            var indexes = FromCursorPos(Console.CursorTop, Console.CursorLeft);
            int row = indexes.Item1;
            int col = indexes.Item2;
            int movedRow;
            int movedCol;
            // Row
            if(row + deltaRow < 0)
                movedRow = mCellRowCount + (row + deltaRow);
            else if(row + deltaRow > mCellRowCount - 1)
                movedRow = row + deltaRow - mCellRowCount;
            else
                movedRow = row + deltaRow;
            // Column
            if(col + deltaCol < 0)
                movedCol = mCellColumnCount + (col + deltaCol);
            else if(col + deltaCol > mCellColumnCount - 1)
                movedCol = col + deltaCol - mCellColumnCount;
            else
                movedCol = col + deltaCol;
            // Set position
            var posns = ToCursorPos(movedRow, movedCol);
            Console.SetCursorPosition(posns.Item2, posns.Item1);
        }

        // Change the state of current position.
        private void UpdateCurrentCell()
        {
            var indexes = FromCursorPos(Console.CursorTop, Console.CursorLeft);
            int row = indexes.Item1;
            int col = indexes.Item2;
            mCA.Cells[row,col] = !mCA.Cells[row,col];
            WriteCell(Console.CursorTop, Console.CursorLeft, mCA.Cells[row,col]);
        }

        // Write a square to specified position.
        private void WriteCell(int top, int left, bool state)
        {
            Console.ForegroundColor = state? mccState1 : mccState0;
            Console.SetCursorPosition(left, top);
            Console.Write(SQUARE);
            Console.SetCursorPosition(left, top);
        }

        // Worker
        private void DoWork_Worker(object sender, EventArgs e)
        {
            mIsRunningCA = true;
            mCursorLeftBackup = Console.CursorLeft;
            mCursorTopBackup = Console.CursorTop;
            Console.CursorVisible = false;
            var caller = sender as BackgroundWorker;
            while(true)
            {
                if(caller.CancellationPending)
                    return;

                // Update mCA.Cells and get list of changed cell.
                var listOfChangeTarget = mCA.Next();
                // Overwrite cells in cui with the list.
                foreach(var ct in listOfChangeTarget)
                {
                    var posns = ToCursorPos(ct.Row, ct.Column);
                    WriteCell(posns.Item1, posns.Item2, ct.NextState);
                }
                System.Threading.Thread.Sleep(500);
            }
        }

        // Worker completed.
        private void Completed_Worker(object sender, RunWorkerCompletedEventArgs e)
        {
            mIsRunningCA = false;
            Console.SetCursorPosition(mCursorLeftBackup, mCursorTopBackup);
            Console.CursorVisible = true;
        }

        // Disable Ctrl+C and Ctrl+break to stop application loop with E key safely.
        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
