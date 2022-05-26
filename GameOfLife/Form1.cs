using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class ConwayMain : Form
    {
        Boolean InProgress;
        Grid CellGrid;

        public ConwayMain()
        {
            InitializeComponent();
        }

        private void ConwayMain_Load(object sender, EventArgs e)
        {
            CreateGridSurface(true);
            //GetActiveCounts();
        }

        private void CreateGridSurface(bool RandomCells)
        {
            Cell newCell;
            Random random = new Random();

            // Determine number of rows and columns in grid.
            int rows = (int)(pbGrid.Height / numSSize.Value);
            int cols = (int)(pbGrid.Width / numSSize.Value);

            // Create grid object.
            CellGrid = new Grid(rows, cols);

            // Clear and rebuild the list of cells, activating 15% of cells at random.
            Grid.gridCells.Clear();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    newCell = new Cell(x, y, (int)numSSize.Value);
                    if (RandomCells)
                        newCell.IsAlive = (random.Next(100) < 15) ? true : false;
                    else
                        newCell.IsAlive = false;
                }
            }

            Grid.gridCells = Grid.gridCells.OrderBy(c => c.XPos).OrderBy(c => c.YPos).ToList();

            UpdateGrid(CellGrid);

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            CreateGridSurface(true);
        }

        private void GetNextState()
        {
            //Method to calculate next positions of cells and update the grid.
            /*
            1.Any live cell with fewer than two live neighbours dies, 
            as if by underpopulation.
            2.Any live cell with two or three live neighbours lives on 
            to the next generation.
            3.Any live cell with more than three live neighbours dies, 
            as if by overpopulation.
            4. Any dead cell with exactly three live neighbours becomes a live cell, 
            as if by reproduction.
            */

            // Calculate next status of each cell.
            foreach (Cell cell in Grid.gridCells)
            {
                int activeCount = CellGrid.LiveAdjacent(cell);

                if (cell.IsAlive)
                {
                    if ((activeCount < 2) || (activeCount > 3))
                        cell.NextStatus = false;
                    else
                        cell.NextStatus = true;
                }
                else
                {
                    if (activeCount == 3)
                        cell.NextStatus = true;
                }

            }

            // For each cell, update IsAlive with NextStatus
            foreach (Cell cell in Grid.gridCells)
            {
                cell.IsAlive = cell.NextStatus;
            }

            UpdateGrid(CellGrid);


        }

        private void UpdateGrid(Grid GridDisplay)
        {
            Random random = new Random();

            // Create new image and update picture box.
            using (Bitmap bmp = new Bitmap(pbGrid.Width, pbGrid.Height))
            using (Graphics g = Graphics.FromImage(bmp))
            using (SolidBrush cellBrush = new SolidBrush(Color.DarkOrange))
            {
                g.Clear(Color.Black);

                foreach (Cell cell in Grid.gridCells)
                {
                    if (cell.IsAlive)
                    {
                        g.FillRectangle(cellBrush, cell.CellDisplay);
                    }
                }

                if (pbGrid.Image != null)
                    pbGrid.Image.Dispose();  // Release resources from previous image.

                pbGrid.Image = (Bitmap)bmp.Clone();
            }
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            GetNextState();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            Go();
        }

        private void Go()
        {
            // Flip the in progress variable.
            InProgress = !InProgress;

            // Change the text in the button.
            btnGo.Text = InProgress ? "Stop" : "Go";
            goToolStripMenuItem.Text = InProgress ? "Stop" : "Go";

            // While inProgess = true, continue to update the grid.
            while (InProgress)
            {
                GetNextState();
                Application.DoEvents();

                if (nudDelay.Value > 0)
                {
                    Thread.Sleep((int)nudDelay.Value);
                }
            }
        }

        private void ConwayMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Make sure the program ends when the form is closed.
            InProgress = false;
            Application.Exit();

        }

        private void pbGrid_MouseClick(object sender, MouseEventArgs e)
        {
            int CellIndex;
            // Get cell size from first cell on grid.
            Size cellSize = Grid.gridCells[0].CellSize;

            // Determine grid index from grid location and cell size.
            int XLoc = e.X / cellSize.Width;
            int YLoc = e.Y / cellSize.Height;

            // Get cell list index from grid index.
            CellIndex = (YLoc * CellGrid.Columns) + XLoc;

            // Flip cell status between dead and alive.
            Grid.gridCells[CellIndex].IsAlive = !Grid.gridCells[CellIndex].IsAlive;

            // Update grid.
            UpdateGrid(CellGrid);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            CreateGridSurface(false);
        }

        private void resetGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Reset the grid with a new random pattern.
            CreateGridSurface(true);

        }

        private void clearGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Clear the grid for a blank surface.
            CreateGridSurface(false);

        }

        private void advanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Advance the simulation by one frame.
            GetNextState();
        }

        private void goToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Go();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadPatternToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog fdOpen = new OpenFileDialog())
                {
                    fdOpen.Title = "Select location of file.";
                    fdOpen.Filter = "gam files (*.gam)|*.gam|All files (*.*)|*.*";

                    if (fdOpen.ShowDialog() == DialogResult.OK)
                    {
                        CellGrid.LoadGrid(fdOpen.FileName, pbGrid.Size);
                    }
                }

                UpdateGrid(CellGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem loading the specified file.\n" + ex.Message);
            }

        }

        private void savePatternToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                using (SaveFileDialog fdSave = new SaveFileDialog())
                {
                    fdSave.Title = "Select location for save file.";
                    fdSave.Filter = "gam files (*.gam)|*.gam|All files (*.*)|*.*";

                    if (fdSave.ShowDialog() == DialogResult.OK)
                    {
                        CellGrid.SaveGrid(fdSave.FileName);
                    }
                    MessageBox.Show("File saved.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem saving the specified pattern.\n" + ex.Message);
            }

        }
    }

    public class Grid
    {
        public static List<Cell> gridCells = new List<Cell>();

        private int cRows;
        private int cCols;

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
        }

        public int Rows
        {
            get { return cRows; }
            set { cRows = value; }
        }

        public int Columns
        {
            get { return cCols; }
            set { cCols = value; }
        }

        public int LiveAdjacent(Cell cell)
        {
            // Function to return number of active neighboring cells.
            // Use cell index numbers to search range of cells for active neighbors.

            int liveAdjacent = 0;

            // Get range of cells to be examined for active neighbors.
            int cellIndex = (cell.YPos * Columns) + cell.XPos;
            int startIndex = cellIndex - Columns - 2;
            int endIndex = cellIndex + Columns + 2;

            // Ensure that the start and end indexes don't exceed the grid range.
            startIndex = (startIndex < 0) ? 0 : startIndex;
            endIndex = (endIndex > (Grid.gridCells.Count - 1)) ? Grid.gridCells.Count - 1 : endIndex;

            // Iterate through the defined range and look for active neighbors.
            for (int x = startIndex; x < endIndex; x++)
            {
                if (Math.Abs(cell.XPos - gridCells[x].XPos) < 2 && Math.Abs(cell.YPos - gridCells[x].YPos) < 2)
                {
                    if (Grid.gridCells[x].Location != cell.Location)
                    {
                        if (gridCells[x].IsAlive)
                        {
                            liveAdjacent++;
                        }
                    }
                }
            }

            return liveAdjacent;
        }

        public void LoadGrid(string filePath, Size displaySize)
        {
            string? lineText;
            string startText = "", gridString = "";
            int cellSize = 10, rows, cols, maxSize, cellCount;
            Cell newCell;

            try
            {
                // Clear the list of cells.
                gridCells.Clear();

                // Read file.
                using (StreamReader loadFile = new StreamReader(filePath))
                {
                    while (!loadFile.EndOfStream)
                    {
                        // Get line from file.
                        lineText = loadFile.ReadLine();

                        if (lineText != null)
                        {
                            // Get the first character.
                            startText = lineText.Substring(0, 1);

                            if (lineText.Substring(0, 4) == "Cell")
                            {
                                // If the line starts with "Cell", it's the cell size.
                                int.TryParse(lineText.Substring(lineText.IndexOf(":") + 1), out int result);
                                cellSize = result;
                            }
                            else if (startText == "0" || startText == "1")
                            {
                                // If the line starts with 0 or 1, treat it as part of the grid.
                                gridString += lineText;
                            }
                        }
                    }
                }

                // Determine maximum cell size supported by number of cells and the display size.
                // If it's smaller than the current spec, update the assumed cell size.
                maxSize = (int)Math.Sqrt((displaySize.Width * displaySize.Height) / gridString.Length);
                maxSize = maxSize > 25 ? 25 : maxSize;
                cellSize = (maxSize < cellSize) ? maxSize : cellSize;

                // Determine the number of rows and columns in the grid.
                rows = displaySize.Height / cellSize;
                cols = displaySize.Width / cellSize;

                Rows = rows;
                Columns = cols;

                // Create the cells in the List<Cell> collection. 
                cellCount = 0;
                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        // Create new cell and turn it on or off as specified.
                        // If the number of cells needed exceeds the number of cells specified
                        // in the file, set all the ones after to IsAlive = False.
                        newCell = new Cell(x, y, cellSize);

                        if (cellCount < gridString.Length)
                            newCell.IsAlive = (gridString[cellCount]) == '1' ? true : false;
                        else
                            newCell.IsAlive = false;

                        cellCount++;
                    }
                }

                // Sort the grid at the end.  
                Grid.gridCells = Grid.gridCells.OrderBy(c => c.XPos).OrderBy(c => c.YPos).ToList();
            }
            catch (Exception)
            {
                throw;
            }
 

        }

        public void SaveGrid(string filePath)
        {
            string rowString = "";
            int cellIndex = 0;

            try
            {
                using (StreamWriter saveFile = new StreamWriter(filePath))
                {
                    saveFile.WriteLine($"Cell size: {gridCells[0].CellSize.Width.ToString()} ");
                    saveFile.WriteLine("-- BEGIN GRID --");
                    // Save the current grid to a text file.
                    for (int y = 0; y < Rows; y++)
                    {
                        for (int x = 0; x < Columns; x++)
                        {
                            rowString += gridCells[cellIndex].IsAlive ? "1" : "0";
                            cellIndex++;
                        }
                        saveFile.WriteLine(rowString);
                        rowString = "";
                    }
                    saveFile.WriteLine("-- END GRID --");
                    saveFile.Flush();
                    saveFile.Close();

                }
            }
            catch (Exception)
            {
                throw;
            }

        }

    }


    public class Cell
    {
        private Point cLocation;
        private Rectangle cCellDisplay;
        private Size cCellSize;
        private int cXPos;
        private int cYPos;
        private Boolean cIsAlive;
        private Boolean cNext;

        public Cell(int CellSize)
        {
            Grid.gridCells.Add(this);
            this.CellSize = new Size(CellSize, CellSize);
        }
        public Cell(Point location, int X, int Y)
        {
            int cellSize;
            // Set object settings and add to grid.
            Location = location;
            YPos = Y;
            XPos = X;
            Grid.gridCells.Add(this);
            // If location is not 0, divide pixel location by grid location to get the size of the cell.
            cellSize = (X == 0) ? 0 : location.X / X;
        }

        public Cell(int X, int Y, int CellSize)
        {
            Location = new Point(X * CellSize, Y * CellSize);
            XPos = X;
            YPos = Y;
            this.CellSize = new Size(CellSize, CellSize);
            Grid.gridCells.Add(this);
        }

        public Rectangle CellDisplay
        {
            // Rectangle object for displaying cell when needed.
            get { return cCellDisplay; }
            set { cCellDisplay = value; }
        }

        public Size CellSize
        {
            // Calculated cell size.
            get { return cCellSize; }
            set
            {
                cCellSize = value;
                // Update the display rectangle at the same time.
                CellDisplay = new Rectangle(Location, new Size(value.Width - 1, value.Height - 1));
            }
        }

        public Point Location
        {
            // X,Y Point that specifies cell location.
            get { return cLocation; }
            set { cLocation = value; }
        }

        public int XPos
        {
            // Cell number on X-axis
            get { return cXPos; }
            set { cXPos = value; }
        }

        public int YPos
        {
            // Cell number on Y-axis
            get { return cYPos; }
            set { cYPos = value; }
        }

        public Boolean IsAlive
        {
            // Indicates if cell has been activated.
            get { return cIsAlive; }
            set { cIsAlive = value; }
        }

        public Boolean NextStatus
        {
            get { return cNext; }
            set { cNext = value; }
        }

        public override string ToString()
        {
            //Override the cell ToString to provide location information.
            return $"GridX:{XPos}  GridY:{YPos}  Alive:{IsAlive}  Next:{NextStatus}";
        }

    }
}
