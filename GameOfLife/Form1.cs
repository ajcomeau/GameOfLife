using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class ConwayMain : Form
    {
        Boolean InProgress;

        public ConwayMain()
        {
            InitializeComponent();
        }

        private void ConwayMain_Load(object sender, EventArgs e)
        {
            CreateGridSurface();
            //GetActiveCounts();
        }

        private void CreateGridSurface()
        {
            Point locPoint;
            Cell newCell;
            Random random = new Random();

            // Determine number of rows and columns in grid.
            int rows = (int)(pbGrid.Height / numSSize.Value);
            int cols = (int)(pbGrid.Width / numSSize.Value);
            
            // Create grid with image, graphics object and brush for drawing.
            using(Bitmap bmp = new Bitmap(pbGrid.Width, pbGrid.Height))     
            using(Graphics g = Graphics.FromImage(bmp))                     
            using(SolidBrush cellBrush = new SolidBrush(Color.DarkOrange))  
            {
                // Create black canvas and add it to the picture box.
                g.Clear(Color.Black);               
                pbGrid.Image = (Bitmap)bmp;

                // Clear and rebuild the list of cells, activating 15% of cells at random.
                Cell.gridCells.Clear();

                for(int y = 0; y < rows; y++)
                {
                    for(int x = 0; x < cols; x++)
                    {
                        locPoint = new Point((int)(x * numSSize.Value), (int)(y * numSSize.Value));
                        newCell = new Cell(locPoint, x, y);
                        newCell.IsAlive = (random.Next(100) < 15) ? true : false;
                        Cell.gridCells.Add(newCell);
                    }
                }

                // Plot all the newly created cells to the grid.
                foreach (Cell cell in Cell.gridCells)
                {
                    if (cell.IsAlive)
                    {
                        g.FillRectangle(cellBrush, new Rectangle(cell.Location, 
                            new Size((int)numSSize.Value - 1, (int)numSSize.Value - 1)));
                    }
                }

                pbGrid.Image = (Bitmap)bmp.Clone();
            }
        }

        private void GetActiveCounts()
        {
            cboCells.Items.Clear();
            // Demo function to test LiveAdjacent function.
            foreach (Cell Cell in Cell.gridCells)
            {
                cboCells.Items.Add($"X:{Cell.XPos}, Y:{Cell.YPos}, Count: {Cell.LiveAdjacent()}");
            }

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            CreateGridSurface();
            //GetActiveCounts();
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
            foreach (Cell cell in Cell.gridCells)
            {
                int activeCount = cell.LiveAdjacent();

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
            foreach (Cell cell in Cell.gridCells)
            {
                cell.IsAlive = cell.NextStatus;
            }

            // Create new image and update picture box.
            using (Bitmap bmp = new Bitmap(pbGrid.Width, pbGrid.Height))
            using (Graphics g = Graphics.FromImage(bmp))
            using (SolidBrush cellBrush = new SolidBrush(Color.DarkOrange))
            {
                g.Clear(Color.Black);

                foreach (Cell cell in Cell.gridCells)
                {
                    if (cell.IsAlive)
                    {
                        g.FillRectangle(cellBrush, new Rectangle(cell.Location, 
                            new Size((int)numSSize.Value - 1, (int)numSSize.Value - 1)));
                    }
                }

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
            // Flip the in progress variable.
            InProgress = !InProgress;

            // Change the text in the button.
            btnGo.Text = InProgress ? "Stop" : "Go";

            // While inProgess = true, continue to update the grid.
            while (InProgress)
            {
                GetNextState();
                Application.DoEvents();
            }
        }

        private void ConwayMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            InProgress = false;
            Application.Exit();
        }
    }

    public class Cell
    {
        public static List<Cell> gridCells = new List<Cell>();

        private Point cLocation;
        private int cXPos;
        private int cYPos;
        private Boolean cIsAlive;
        private Boolean cNext;

        public Cell(Point location, int X, int Y)
        {
            this.Location = location;
            this.YPos = Y;
            this.XPos = X;
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


        public int LiveAdjacent()
        {
            // Function to return number of active neighboring cells.

            int liveAdjacent = 0;

            foreach (Cell cell in Cell.gridCells)
            {
                if (cell.Location != this.Location && cell.IsAlive)
                {
                    if (Math.Abs(cell.XPos - this.XPos) < 2 && Math.Abs(cell.YPos - this.YPos) < 2)
                    {
                        liveAdjacent++;
                    }
                }
            }

            return liveAdjacent;
        }

    }
}
