using System;
using System.Collections.Generic;

namespace CUICA
{
    public class ChangeTarget
    {
        public readonly int Row;
        public readonly int Column;
        public readonly bool NextState;

        public ChangeTarget(int row, int col, bool nextState)
        {
            Row = row;
            Column = col;
            NextState = nextState;
        }
    }

    public class TwoStateCellularAutomaton
    {
        // properties
        public bool[,] Cells
        {
            get
            {
                return mCells;
            }
        }
        public int RowCount
        {
            get
            {
                return miRowCount;
            }
        }
        public int ColumnCount
        {
            get
            {
                return miColumnCount;
            }
        }

        // fields
        private readonly bool[,] mCells;
        private readonly int miRowCount;
        private readonly int miColumnCount;
        private readonly Func<bool,int,bool> mfRule;

        // constructor
        public TwoStateCellularAutomaton(int rowCount, int columnCount, Func<bool,int,bool> rule)
        {
            miRowCount = rowCount;
            miColumnCount = columnCount;
            mCells = new bool[miRowCount,miColumnCount];
            mfRule = rule;
        }

        public TwoStateCellularAutomaton(int rowCount, int columnCount):this(rowCount,columnCount,LifeGame)
        {
        }

        // methods
        public static bool LifeGame(bool isLive, int numOfNeighbourTrueCell)
        {
            if(isLive == false && numOfNeighbourTrueCell == 3)
            {
                return true;
            }
            if(isLive == true && (numOfNeighbourTrueCell == 2 || numOfNeighbourTrueCell == 3))
            {
                return true;
            }
            if(isLive == true && numOfNeighbourTrueCell <= 1)
            {
                return false;
            }
            if(isLive == true && numOfNeighbourTrueCell >= 4)
            {
                return false;
            }
            return isLive;
        }

        public List<ChangeTarget> Next()
        {
            var listOfChangeTarget = new List<ChangeTarget>();
            for(int i = 0; i < miRowCount; i++)
            {
                for(int j = 0; j < miColumnCount; j++)
                {
                    var numOfNeighbourTrueCell = CountNeighbourTrueCell(i, j);
                    var nextState = mfRule(mCells[i,j], numOfNeighbourTrueCell);
                    if (nextState != mCells[i,j])
                    {
                        listOfChangeTarget.Add(new ChangeTarget(i,j,nextState));
                    }
                }
            }

            foreach(var ct in listOfChangeTarget)
            {
                mCells[ct.Row,ct.Column] = ct.NextState;
            }

            return listOfChangeTarget;
        }

        // private methods
        private int CountNeighbourTrueCell(int i, int j)
        {
            int left   = (j == 0)? (miColumnCount - 1) : (j - 1);
            int right  = (j == miColumnCount - 1)? 0: (j + 1);
            int top    = (i == 0)? (miRowCount - 1) : (i - 1);
            int bottom = (i == miRowCount - 1)? 0 : (i + 1);

            int count = 0;
            foreach(int r in new int[]{top,i,bottom})
            {
                foreach(int c in new int[]{left,j,right})
                {
                    if(r != i || c != j)
                    {
                        count += mCells[r,c]? 1 : 0;
                    }
                }
            }
            return count;
        }
    }
}
