using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

namespace Canadian_Population
{
    public partial class CanadianPopulationForm : Form
    {

        //Create lists here for data to be stored
        List<PopulationArea> allPopulation = new List<PopulationArea>();
        List<PopulationArea> selectedPopulation = new List<PopulationArea>();

        public CanadianPopulationForm()
        {
            InitializeComponent();

            this.Text = "Canadian Population";

            ReadPopulationData("Population by Geographical Area - Canada.xml");

            // set the form autosize to grow to fit the controls

            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.Text = "Canadian Population";

            // initialize datagridview control with default settings

            InitializeDataGridView();

            // initialize listbox
            InitializeListBoxAreaName();

            //Resets everything to default
            ResetControlsToDefault();

            //Event Listeners
            buttonReset.Click += (s, e) => ResetControlsToDefault();
            checkBox2016Population.CheckedChanged += DisplayData;
            textBoxMax.TextChanged += DisplayData;
            textBoxMin.TextChanged += DisplayData;

        }

        /// <summary>
        /// Populates Datagridview data and label data based on filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayData(object sender = null, EventArgs e = null)
        {
            //Checking Filters
            dataGridViewPopulation.Rows.Clear();
            int max = 0;
            int min = 0;

            if (checkBox2016Population.Checked == true)
            {
                try
                {
                    int.TryParse(textBoxMax.Text, out max);
                    int.TryParse(textBoxMin.Text, out min);
                }
                catch (Exception ex) {
                    MessageBox.Show("Invalid Min or Max entered in Filter by 2016 Population");
                }
            }

            //List of AreaNames
            List<string> selectedAreaName = listBoxAreaName.SelectedItems.OfType<string>().ToList();

            //Query for filtering data
            selectedPopulation = allPopulation.Where(s => selectedAreaName.Contains(s.AreaName)
                                        && CompareWithMaxMin(s.Population2016, max, min)).ToList();


            Console.WriteLine(allPopulation.Count());


            Console.WriteLine("Here" + selectedPopulation.Count());

            var selectedPopulationCount = selectedPopulation.Count();
            var totalDwellings = 0;
            decimal totalLandArea = 0;
            var total2016Population = 0;
            var total2011Population = 0;

            //List to populate selected data based on filters
            foreach (PopulationArea eachPopulationEntry in selectedPopulation)
            {
                dataGridViewPopulation.Rows.Add(
                    eachPopulationEntry.AreaName,
                    eachPopulationEntry.Province,
                    eachPopulationEntry.Population2016,
                    eachPopulationEntry.Population2011,
                    eachPopulationEntry.NumberOfDwellings,
                    eachPopulationEntry.LandArea);
                totalDwellings += eachPopulationEntry.NumberOfDwellings;
                totalLandArea += eachPopulationEntry.LandArea;
                total2016Population += eachPopulationEntry.Population2016;
                total2011Population += eachPopulationEntry.Population2011;
            }

            //Calculations for labels
            labelAreaCount.Text = String.Format("{0:n0}", selectedPopulationCount).ToString();
            labelTotalDwellings.Text = String.Format("{0:n0}", totalDwellings).ToString();
            labelTotalLandArea.Text = String.Format("{0:n}", totalLandArea).ToString();
            labelTotal2016Population.Text = String.Format("{0:n0}", total2016Population).ToString();
            labelTotal2011Population.Text = String.Format("{0:n0}", total2011Population).ToString();


        }

        /// <summary>
        /// Useful method to compare min and max population, was used in the query for filtering
        /// </summary>
        /// <param name="actualSize"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public bool CompareWithMaxMin(int actualSize, double max, double min)
        {
            return (checkBox2016Population.Checked == true && actualSize < max && actualSize > min) || checkBox2016Population.Checked == false;
        }

        /// <summary>
        /// Populating ListBox with AreaNames
        /// </summary>
        private void InitializeListBoxAreaName()
        {
            listBoxAreaName.Items.Clear();

            listBoxAreaName.SelectionMode = SelectionMode.MultiExtended;

            listBoxAreaName.DataSource = allPopulation
                .OrderBy(x => x.AreaName)
                .Select(x => x.AreaName)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Resets everything to the initial state
        /// </summary>
        private void ResetControlsToDefault()
        {
            listBoxAreaName.SelectedIndexChanged -= DisplayData;
            for (int i = 0; i < listBoxAreaName.Items.Count; i++)
            {
                listBoxAreaName.SetSelected(i, true);
            }

            checkBox2016Population.Checked = false;
            textBoxMin.Clear();
            textBoxMax.Clear();

            DisplayData();

            listBoxAreaName.SelectedIndexChanged += DisplayData;

            dataGridViewPopulation.Rows.Clear();

            var ascendingPopulationList = allPopulation.OrderBy(x => x.AreaName).ToList();
            foreach (PopulationArea eachPopulationEntry in ascendingPopulationList)
            {
                dataGridViewPopulation.Rows.Add(
                    eachPopulationEntry.AreaName,
                    eachPopulationEntry.Province,
                    eachPopulationEntry.Population2016,
                    eachPopulationEntry.Population2011,
                    eachPopulationEntry.NumberOfDwellings,
                    eachPopulationEntry.LandArea);

            }
        }

        /// <summary>
        /// Allows users to select the xml file to be read by the program
        /// </summary>
        /// <param name="fileName"></param>
        private void ReadPopulationData(string fileName)
        {
            // open the file for reading
            OpenFileDialog openFileDialogCSV = new OpenFileDialog
            {

                InitialDirectory = Path.GetFullPath(Application.StartupPath + "\\..\\.."),
                Filter = "XML files|*.xml"
            };
            StreamReader populationFile = new StreamReader(fileName);


            // open the filedialog, get a name, and open the file
            if (openFileDialogCSV.ShowDialog() == DialogResult.OK)
            {
                populationFile = File.OpenText(openFileDialogCSV.FileName);
            }
            // create the serializer, note use of typeof
            XmlSerializer populationSerializer =
                new XmlSerializer(typeof(List<PopulationArea>), new XmlRootAttribute("ArrayOfPopulationArea"));   //NOTE TO SELF: MAKE SURE YOU HAVE THE RIGHT ROOT ATTRIBUTE! You were stuck here for 2 hours!

            // deserialize to the list of Population from file, note use of cast

            allPopulation = populationSerializer.Deserialize(populationFile) as List<PopulationArea>;
            populationFile.Close();
            Console.WriteLine($"=> Reading list of cars from {fileName}");

            // display

            foreach (PopulationArea eachPopulationEntry in allPopulation)
            {
                Console.WriteLine($"Area Name: {eachPopulationEntry.AreaName}\nProvince: {eachPopulationEntry.Province}\nPopulation 2016: {eachPopulationEntry.Population2016}\nPopulation 2011: {eachPopulationEntry.Population2011}\nNumber of Dwellings: {eachPopulationEntry.NumberOfDwellings}\nLand Area: {eachPopulationEntry.LandArea}");
            }
        }

        /// <summary>
        /// Set up the GridView control with consistent parameters.
        /// Easier than using the designer.
        /// </summary>
        private void InitializeDataGridView()
        {
            dataGridViewPopulation.Columns.Clear();  // any columns created by the designer, get rid of them
            dataGridViewPopulation.ReadOnly = true;  // no cell editing allowed
            dataGridViewPopulation.AllowUserToAddRows = false;     // no rows can be added or deleted
            dataGridViewPopulation.AllowUserToDeleteRows = false;
            dataGridViewPopulation.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewPopulation.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dataGridViewPopulation.AutoSize = false;        // don't autosize the cells
            dataGridViewPopulation.RowHeadersVisible = false;
            dataGridViewPopulation.Width = 900;

            // right justify everything

            dataGridViewPopulation.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridViewPopulation.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // TASK: add the datagridview columns here

            foreach (PropertyInfo eachPopulationEntry in (new PopulationArea()).GetType().GetProperties())
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn()
                {
                    Name = eachPopulationEntry.Name,
                    ValueType = eachPopulationEntry.GetType()
                };

                dataGridViewPopulation.Columns.Add(column);
            }

        }
    }

    /// <summary>
    /// Class that includes relevant properties for a population area
    /// </summary>
    [Serializable]
    public class PopulationArea
    {
        /// <summary>
        /// Population Area Name
        /// </summary>
        public string AreaName { get; set; } 
        /// <summary>
        /// Name of Province
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 2016 population
        /// </summary>
        public int Population2016 { get; set; }
        /// <summary>
        /// 2011 population
        /// </summary>
        public int Population2011 { get; set; }
        /// <summary>
        /// Number of dwellings (like houses) in the area
        /// </summary>
        public int NumberOfDwellings { get; set; }
        /// <summary>
        /// Land area in square kilometers.
        ///     Used to calculate density.
        /// </summary>
        public decimal LandArea { get; set; }

        public PopulationArea() { }
    }
}
