using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Fable.Core.JS;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using static System.Net.WebRequestMethods;
using System.Windows.Input;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;


namespace PlantInspector
{
    public partial class Form1 : Form
    {

        struct Standard_Diameter
        {
            public int x1;
            public int y1;
            public int x2;
            public int y2;
            public double diameter;
        }

        struct Bounding_Box
        {
            public Point p1;
            public Point p2;
            public Point p3;
            public Point p4;
            public double area;
        }

        struct Circle_Int
        {
            public Point centre;
            public int radius;
            public double area;
        }

        struct Circle_Double
        {
            public double centre_x;
            public double centre_y;
            public double radius;
            public double area;
        }

        struct Ellipse_Int
        {
            public Point centre;
            public int major_radius;
            public int minor_radius;
            public double angle_major_radius;  // In degrees
            public double area;
        }

        struct Ellipse_Double
        {
            public double centre_x;
            public double centre_y;
            public double major_radius;
            public double minor_radius;
            public double angle_major_radius;  // In degrees
            public double area;
        }

        // Constraints for Dataset_1
        public static double DEFAULT_SCALING_FACTOR_1;          // default scaling factor for converting pixel-based distance into mm-based distances of real scene
        public static string DEFAULT_SOWING_START_DATE_1;       // default date of starting the sowing
        public static string DEFAULT_SCREENING_DATE_1;          // default data of starting the screening
        public static string DEFAULT_CAPTURING_END_DATE_1;      // default date of finishing capturing the series of plant images

        // Constraints for Dataset_2
        public static double DEFAULT_SCALING_FACTOR_2;          // default scaling factor for converting pixel-based distance into mm-based distances of real scene
        public static string DEFAULT_SOWING_START_DATE_2;       // default date of starting the sowing
        public static string DEFAULT_SCREENING_DATE_2;          // default data of starting the screening
        public static string DEFAULT_CAPTURING_END_DATE_2;      // default date of finishing capturing the series of plant images

        // other constraints
        public static int DEFAULT_PLOT_WIDTH;           // default width of output plots
        public static int DEFAULT_PLOT_HEIGHT;          // default height of output plots

        // Bitmaps
        public static Bitmap Input_Mask_Bitmap;       // current mask image to be processed
        public static Bitmap Input_Mask_No_Noise;     // current mask image to be processed, with noise filtered out
        public static Bitmap Input_Mask_With_Stems;   // current mask image to be processed, with stems
        public static Bitmap Input_Segmented_Bitmap;  // current segmented image (only for visualization)
        public static Bitmap Input_Empty_Bitmap;      // empty image (only for visualization)
        // Cache of Rep bitmaps
        public static string cached_class_name = ""; // Name of cached accession (class)
        public static string cached_rep_name = "";   // Name of cached replicate
        public static Dictionary<string, Bitmap> cached_rep_masks = new Dictionary<string, Bitmap>();             // All masks belonging to the current replicate
        public static Dictionary<string, Bitmap> cached_rep_masks_no_noise = new Dictionary<string, Bitmap>();    // All masks belonging to the current replicate after filtering out the noise
        public static Dictionary<string, Bitmap> cached_rep_masks_with_stems = new Dictionary<string, Bitmap>();  // All masks belonging to the current replicate after filtering out the noise and adding stems
        public static Dictionary<string, Bitmap> cached_rep_seg_imgs = new Dictionary<string, Bitmap>();          // All segmented_images belonging to the current replicate
        public static Dictionary<string, Bitmap> cached_rep_empty_imgs =  new Dictionary<string, Bitmap>();       // Empty images of the size of the current replicate's masks.
        public static string current_filename = "";

        // lists for extracted/generated features of currently loaded/processed Class  TODO: need revision
        List<double> DAS_s = new List<double>();
        List<string> date_str = new List<string>();
        List<string> time_str = new List<string>();
        List<List<Point>> rosette_contours = new List<List<Point>>();
        List<double> rosette_areas = new List<double>();
        List<double> rosette_perimeters = new List<double>();
        List<List<Point>> convex_hull_contours = new List<List<Point>>();
        List<double> convex_hull_areas = new List<double>();
        List<double> convex_hull_perimeters = new List<double>();
        List<Circle_Double> convex_hull_equiv_circles = new List<Circle_Double>();
        List<double> convex_hull_inside_equiv_circles = new List<double>();
        List<Standard_Diameter> standard_diameters = new List<Standard_Diameter>();
        List<Bounding_Box> bounding_boxes = new List<Bounding_Box>();
        List<Circle_Int> bounding_circles = new List<Circle_Int>();
        List<Ellipse_Int> bounding_ellipses = new List<Ellipse_Int>();
        List<Ellipse_Double> ellipses_2nd_central_moment = new List<Ellipse_Double>();

        // lists for storing extracted data into the '_Saved_Lists' subfolder  TODO: need revision
        List<int> rep_serialnumbers = new List<int>();
        List<int> rep_lengths = new List<int>();
        List<double> bounding_box_areas = new List<double>();
        List<double> std_diameters = new List<double>();
        List<double> area_growth_rates = new List<double>();
        List<double> convex_hull_growth_rates = new List<double>();
        List<double> circumferences = new List<double>();
        List<double> eccentricities = new List<double>();
        List<double> area_to_perimeter_ratios = new List<double>();
        List<double> compactnesses = new List<double>();
        List<double> extents = new List<double>(); // rectangularities
        List<double> plant_roundnesses = new List<double>();
        List<double> surface_coverages = new List<double>();
        List<double> bounding_box_aspect_ratios = new List<double>();
        List<double> rotational_mass_asymmetries = new List<double>();
        List<double> convex_hull_roundnesses = new List<double>();
        List<double> convex_hull_elongations = new List<double>();
        List<double> bounding_ellipse_circularities = new List<double>();

        //SortedList<string, SortedList<int, List<double>>> data_per_DAS_rosette_areas = new SortedList<string, SortedList<int, List<double>>>();
        // Data per DAS (actually per feature, then per rep, then per DAS = day-after-sowing)
        Dictionary<string, SortedList<string, SortedList<int, List<double>>>> data_per_DAS = new Dictionary<string, SortedList<string, SortedList<int, List<double>>>>();
        string[] feature_names = { 
            "rosette_areas", "perimeters", "convexhull_areas", "convexhull_perimeters", "bounding_box_areas",
            "std_diameters", "circumferences", "eccentricities",
            "area_to_perimeter_ratios", "compactnesses", "extents", "plant_roundnesses", "surface_coverages", "BB_aspect_ratios",
            "rotational_mass_asymmetries", "convex_hull_roundnesses", "convex_hull_elongations", "bounding_ellipse_circularities" }; // Add also "area_growth_rates", "convex_hull_growth_rates" ?

        // saving rep selections into the '...classname/_Saved_Lists' subfolder
        bool m_bRepSelectionLoading;                                // true: loading is activated, false: loading is finished

        // misc.
        public static Boolean Processing_Activated;     // true, if processing one or all classes is started
        List<Color> plot_colors = new List<Color>();    // colors of Rep-related data on plots

        Thread singleThread, multiThread;	//Process class in thread
        bool m_bProcessing;		//False: Class not processing, true: Class processing

        //---------------------------------------------------------------------------------------------------
        public Form1()
        {
            InitializeComponent();

            // constraints for Dataset_1
            DEFAULT_SCALING_FACTOR_1 = 0.13715; // Was 0.13888
            DEFAULT_SOWING_START_DATE_1 = "2022-04-28";  // "4/28/2022";
            DEFAULT_SCREENING_DATE_1 = "2022-05-11";  // DEFAULT_SOWING_START_DATE_1;
            DEFAULT_CAPTURING_END_DATE_1 = "2022-06-07";  // "6/10/2022"

            // constraints for Dataset_2
            DEFAULT_SCALING_FACTOR_2 = 0.14690; // Was 0.15698
            DEFAULT_SOWING_START_DATE_2 = "2022-07-22";  // "07/22/2022";
            DEFAULT_SCREENING_DATE_2 = "2022-08-02";  // DEFAULT_SOWING_START_DATE_2;
            DEFAULT_CAPTURING_END_DATE_2 = "2022-09-15";  // "09/30/2022";


            // other constraints
            DEFAULT_PLOT_WIDTH = 1024;
            DEFAULT_PLOT_HEIGHT = 675;

            // Bitmaps
            Input_Mask_Bitmap = null;
            Input_Mask_No_Noise = null;
            Input_Mask_With_Stems = null;
            Input_Segmented_Bitmap = null;
            Input_Empty_Bitmap = null;

            // controls
            comboBox_Show_Plot.SelectedIndex = 0;
            listView_Show_Data.View = View.Details;
            listView_Show_Data.Hide();
            pictureBox_Source_Image.Width = groupBox_Show_Features.Left - 5 - pictureBox_Source_Image.Left;
            pictureBox_Source_Image.BringToFront();
            pictureBox_Source_Image.Show();

            ClearGUIData();
            ClearFeatures();

            m_bRepSelectionLoading = false;

            Set_ProcessRep_Checked_State(true);

            // misc.
            Processing_Activated = false;
            GetPlotColors();

            SetDataset();

            textBox_Plot_Image_Width.Text = Convert.ToString(DEFAULT_PLOT_WIDTH);
            textBox_Plot_Image_Height.Text = Convert.ToString(DEFAULT_PLOT_HEIGHT);

            //// append the 'Version' to App's window title
            //Assembly MyAsm = Assembly.Load("PlantInspector");
            //AssemblyName aName = MyAsm.GetName();
            //Version ver = aName.Version;
            //this.Text += "   [Version: " + ver + " ]";
            this.CenterToScreen();

        }

        private void SetDataset()
        {
            //ClearGUIData();
            //ClearFeatures();

            DateTime datetime1;
            DateTime datetime2;
            DateTime datetime3;
            if ( radioButton_Dataset_1.Checked==true)
            {
                textBox_Calibration_Data.Text = Convert.ToString(DEFAULT_SCALING_FACTOR_1);

                if (DateTime.TryParse(DEFAULT_SOWING_START_DATE_1, out datetime1))
                {
                    dateTimePicker_Sowing_Start.Value = datetime1;
                }
                if (DateTime.TryParse(DEFAULT_SCREENING_DATE_1, out datetime2))
                {
                    dateTimePicker_Screening_Date.Value = datetime2;
                }
                if (DateTime.TryParse(DEFAULT_CAPTURING_END_DATE_1, out datetime3))
                {
                    dateTimePicker_Capturing_End_Date.Value = datetime3;
                }
            }
            else
            {
                textBox_Calibration_Data.Text = Convert.ToString(DEFAULT_SCALING_FACTOR_2);

                if (DateTime.TryParse(DEFAULT_SOWING_START_DATE_2, out datetime1))
                {
                    dateTimePicker_Sowing_Start.Value = datetime1;
                }
                if (DateTime.TryParse(DEFAULT_SCREENING_DATE_2, out datetime2))
                {
                    dateTimePicker_Screening_Date.Value = datetime2;
                }
                if (DateTime.TryParse(DEFAULT_CAPTURING_END_DATE_2, out datetime3))
                {
                    dateTimePicker_Capturing_End_Date.Value = datetime3;
                }
            }
        }

        private void radioButton_Dataset_CheckedChanged(object sender, EventArgs e)
        {
            SetDataset();
            //ClearGUIData();
            ClearFeatures();
            textBox_Out_FolderName.ResetText();

        }

        private void button_Exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button_Browse_Root_Folder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description =
                "Select the root directory of plant classes.";
            dlg.SelectedPath = textBox_Root_FolderName.Text;
            dlg.ShowNewFolderButton = false;    // prevent the user from creating new folder
            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;

            // accepted by the user - try to load folder and file names
            // REM: the path will be processed by textbox's handler
            textBox_Root_FolderName.Text = dlg.SelectedPath;

        }

        private void textBox_Root_FolderName_TextChanged(object sender, EventArgs e)
        {
            ClearGUIData();
            ClearFeatures();
            textBox_Out_FolderName.ResetText();

            if (textBox_Root_FolderName.Text == "")
                return;

            DirectoryInfo dinfo = new DirectoryInfo(textBox_Root_FolderName.Text);
            if (dinfo.Exists == false)
            {
                // directory does not exist
                textBox_Root_FolderName.ForeColor = Color.Red;
                return;
            }
            DirectoryInfo[] directories = dinfo.GetDirectories();
            if (directories.Count() == 0)
            {
                // selected directory is empty
                textBox_Root_FolderName.ForeColor = Color.Red;
                return;
            }

            // probably acceptable
            textBox_Root_FolderName.ForeColor = Color.Black;

            foreach (DirectoryInfo directory in directories)
            {
                if (directory.Name != "_Bars"
                    && directory.Name != "_Plots"
                    && directory.Name != "_PlotsHtml"
                    && directory.Name != "_Excels"
                    && directory.Name != "_Saved_Lists")
                    listBox_Classes.Items.Add(directory.Name);
            }

            listBox_Classes.SelectedIndex = 0;
        }

        private void button_Browse_Out_Folder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description =
                "Select the output directory of plant classes.";
            dlg.SelectedPath = textBox_Out_FolderName.Text;
            dlg.ShowNewFolderButton = true;    // allow the user to create new folder
            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;

            if (textBox_Out_FolderName.Text != dlg.SelectedPath)
                textBox_Out_FolderName.Text = dlg.SelectedPath;

        }

        private void textBox_Out_FolderName_TextChanged(object sender, EventArgs e)
        {
            //ClearGUIData();
            ClearFeatures();
            listView_Show_Data.Items.Clear();

            if (textBox_Out_FolderName.Text == "")
                return;

            if (listBox_Classes.Items.Count == 0)
                return;
            listBox_Classes.SelectedIndex = 0;

            ReadGlobalParameters();

            // reload the previously created features for current class
            string classname = (string)listBox_Classes.SelectedItem;
            ReloadLists(classname);
        }

        private void listBox_Classes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Classes.Items.Count == 0 || Processing_Activated == true)
                return;

            listView_Reps.Items.Clear();

            string classname = (string)listBox_Classes.SelectedItem;
            string classfoldername = textBox_Root_FolderName.Text + "\\" + classname;
            DirectoryInfo dinfo = new DirectoryInfo(classfoldername);
            if (dinfo.Exists == false)
                return;
            DirectoryInfo[] directories = dinfo.GetDirectories();
            if (directories.Count() == 0)
                return;

            m_bRepSelectionLoading = true;
            foreach (DirectoryInfo directory in directories)
            {
                string rep_item = directory.Name;
                listView_Reps.Items.Add(rep_item);
            }
            m_bRepSelectionLoading = false;

            ClearFeatureLists();

            // reload the previously created features for current class
            ReloadLists(classname);
            pictureBox_Source_Image.Invalidate();

            LoadOrInitializeRepSelections();

            if (listView_Reps.Items.Count > 0)
                listView_Reps.Items[0].Selected = true;
        }

        private void listView_Reps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Classes.Items.Count > 0 && listView_Reps.Items.Count > 0)
            {
                int class_selection_index = listBox_Classes.SelectedIndices.Count == 0 ? 0 : listBox_Classes.SelectedIndices[0];
                string curr_class_name = listBox_Classes.Items[class_selection_index].ToString();
                int rep_selection_index = listView_Reps.SelectedIndices.Count == 0 ? 0 : listView_Reps.SelectedIndices[0];
                string curr_rep_name = listView_Reps.Items[rep_selection_index].Text;
                if (!String.Equals(curr_class_name, cached_class_name) || !String.Equals(curr_rep_name, cached_rep_name))
                {
                    // If the class or the rep changed, we reset the caches
                    cached_class_name = curr_class_name;
                    cached_rep_name = curr_rep_name;
                    cached_rep_masks.Clear();
                    cached_rep_masks_no_noise.Clear();
                    cached_rep_masks_with_stems.Clear();
                    cached_rep_seg_imgs.Clear();
                    cached_rep_empty_imgs.Clear();
                }
            }
            else
            {
                cached_class_name = "";
                cached_rep_name = "";
                cached_rep_masks.Clear();
                cached_rep_masks_no_noise.Clear();
                cached_rep_masks_with_stems.Clear();
                cached_rep_seg_imgs.Clear();
                cached_rep_empty_imgs.Clear();
            }

            if (listView_Reps.Items.Count == 0 || Processing_Activated == true)
                return;

            listBox_Images.Items.Clear();

            listView_Reps.Refresh();
            ListView.SelectedIndexCollection selectedindices = listView_Reps.SelectedIndices;
            if (selectedindices.Count == 0)
                return;

            int selectedindex = selectedindices.Count == 0 ? 0 : listView_Reps.SelectedIndices[0];
            string rep_item = listView_Reps.Items[selectedindex].Text;
            string imagesfoldername = textBox_Root_FolderName.Text + "\\" + listBox_Classes.SelectedItem + "\\" + rep_item + "\\masks";
            DirectoryInfo dinfo = new DirectoryInfo(imagesfoldername);
            if (dinfo.Exists == false)
                return;
            FileInfo[] files = dinfo.GetFiles();
            if (files.Count() == 0)
                return;
            int counter = 0;
            foreach (FileInfo file in files)
            {
                string filename = file.Name;
                counter++;
                string item = counter < 100 ? "0" : "";
                item += counter < 10 ? "0" : "";
                item += Convert.ToString(counter);
                item += "        ";
                item += filename;
                listBox_Images.Items.Add(item);
            }

            listBox_Images.SelectedIndex = 0;
        }

        private void button_Add_All_Reps_Click(object sender, EventArgs e)
        {
            Set_ProcessRep_Checked_State(true);
        }

        private void button_Remove_All_Reps_Click(object sender, EventArgs e)
        {
            Set_ProcessRep_Checked_State(false);
        }

        private void listBox_Images_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_Images.Items.Count == 0 || Processing_Activated == true)
                return;

            string filename = (string)listBox_Images.SelectedItem;
            filename = filename.Remove(0, 11);
            current_filename = filename;
            textBox_Current_Source_Image_File_Name.Text = filename;
        }

        private void textBox_Current_Source_Image_File_Name_TextChanged(object sender, EventArgs e)
        {
            if (Processing_Activated == true)
                return;

            LoadandShowCurrentImage();
        }

        private void pictureBox_Source_Image_MouseLeave(object sender, EventArgs e)
        {
            ShowPixelData(-1, -1);
        }

        private void pictureBox_Source_Image_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
			// When processing class, return
            if (m_bProcessing)
                return;

            if (pictureBox_Source_Image.Image == null || Input_Mask_Bitmap == null || pictureBox_Source_Image.Image == Input_Empty_Bitmap)
            {
                ShowPixelData(-1, -1);
                return;
            }

            Point pos = new Point(e.X, e.Y);
            pos = WindowToImage(pos);

            // check, if outside?
            if (pos.X < 0 || pos.X > (Input_Mask_Bitmap.Width - 1) || pos.Y < 0 || pos.Y > (Input_Mask_Bitmap.Height - 1))
            {
                ShowPixelData(-1, -1);
                return;
            }
            ShowPixelData(pos.X, pos.Y);
        }

        private void ShowImage_Changed(object sender, EventArgs e)
        {
            LoadandShowCurrentImage();
        }

        private void button_Process_Click(object sender, EventArgs e)
        {
            if (Input_Mask_Bitmap == null)
                return;

            if (textBox_Root_FolderName.Text == "")
            {
                System.Windows.Forms.MessageBox.Show("You must fill in an input folder first");
                return;
            }

            if (textBox_Out_FolderName.Text == "")
            {
                System.Windows.Forms.MessageBox.Show("You must fill in an output folder first");
                return;
            }

            // create the output directories under the 'out' directory
            CreateOutputDirectories("\\_Bars");
            CreateOutputDirectories("\\_Excels");
            CreateOutputDirectories("\\_Plots");
            CreateOutputDirectories("\\_PlotsHtml");
            CreateOutputDirectories("\\_Saved_Lists");

            System.Console.WriteLine("START time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            Cursor = System.Windows.Forms.Cursors.WaitCursor;
            Processing_Activated = true;

            listView_Show_Data.Hide();
            pictureBox_Source_Image.Width = groupBox_Show_Features.Left - 5 - pictureBox_Source_Image.Left; // set it anyway
            groupBox_Show_Features_Left_Location = groupBox_Show_Features.Left;
            pictureBox_Source_Image.Show();
            ((Form)this).Refresh();

            string classname = "";
            Boolean bProcessSelectedReps = checkBox_Selected_Reps.Checked;
            if ( radioButton_Process_Selected_Class.Checked)
            {
                // process only the selected class
                classname = (string)listBox_Classes.SelectedItem;
                if (classname == "")
                {
                    Processing_Activated = false;
                    Cursor = System.Windows.Forms.Cursors.Default;
                    return;
                }

                Boolean[] selected_Reps = new Boolean[listView_Reps.Items.Count];
                if (bProcessSelectedReps)
                {
                    for ( int i=0; i< selected_Reps.Length; i++)
                    {
                        selected_Reps[i] = listView_Reps.Items[i].Checked==true ? true: false;
                    }

                    Boolean bNothingToDo = true;
                    for (int i = 0; i < selected_Reps.Length; i++)
                        if (selected_Reps[i] == true)
                            bNothingToDo = false;
                    if (bNothingToDo == true)
                    {
                        MessageBox.Show(" At least one Rep must be selected for processing!");
                        Processing_Activated = false;
                        Cursor = System.Windows.Forms.Cursors.Default;
                        return;
                    }

                }
                else
                {
                    for (int i = 0; i < selected_Reps.Length; i++)
                    {
                        selected_Reps[i] = true;
                    }
                }

                //Process single class in thread
                ProcessSingleClassWorker worker = new ProcessSingleClassWorker(classname, selected_Reps);
                m_bProcessing = true;
                singleThread = new Thread(worker.Run);
                worker.ThreadDone += ProcessSingleClassThreadDone;
                singleThread.Start(this);
            }
            else
            {
                // process all classes
                string excelsfoldername = textBox_Out_FolderName.Text + "\\_Excels";
                string excelfoldername = excelsfoldername + "\\" + classname;
                string excel_filename = excelfoldername + "\\" + "extracted_features.xlsx";
				//Process multiple class in thread
                int nbofclasses = listBox_Classes.Items.Count;
                string[] pclassnames = new string[nbofclasses];
                for (int index = 0; index < nbofclasses; index++)
                {
                    classname = listBox_Classes.Items[index].ToString();
                    pclassnames[index] = classname;
                }


                ProcessMultipleClassWorker worker = new ProcessMultipleClassWorker(excel_filename, pclassnames, bProcessSelectedReps);


                m_bProcessing = true;
                multiThread = new Thread(worker.Run);
                worker.ThreadDone += ProcessMultiClassThreadDone;
                multiThread.Start(this);

            }


            System.Console.WriteLine("END time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        }

        //Called when single class processing is done
        void ProcessSingleClassThreadDone(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (tabControl_Show.SelectedIndex == 3)
                {
                    pictureBox_Source_Image.Hide();
                    listView_Show_Data.Show();
                }
                Processing_Activated = false;
                Cursor = System.Windows.Forms.Cursors.Default;
                pictureBox_Source_Image.Invalidate();
                SetPictureBoxImage();   //pictureBox_Source_Image.Invalidate();
                m_bProcessing = false;
                singleThread = null;
            });

            System.Console.WriteLine("Single thread END time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        //Called when multiple class processing is done
        void ProcessMultiClassThreadDone(object sender, EventArgs e)
        {
            // create the overall growth rate bars
            this.Invoke((MethodInvoker)delegate
            {
                if (tabControl_Show.SelectedIndex == 3)
                {
                    pictureBox_Source_Image.Hide();
                    listView_Show_Data.Show();
                }
                Processing_Activated = false;
                Cursor = System.Windows.Forms.Cursors.Default;
                pictureBox_Source_Image.Invalidate();
                SetPictureBoxImage();
                m_bProcessing = false;
                multiThread = null;
            });

            System.Console.WriteLine("Multithread END time: {0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        }

        // Single class processing worker thread
        class ProcessSingleClassWorker
        {
            string className;
            Boolean[] selected_Reps;

            public event EventHandler ThreadDone;

            public ProcessSingleClassWorker(string pClassName, Boolean[] pSelectedReps) {
                this.className = pClassName;
                selected_Reps = new Boolean[pSelectedReps.Length];
                System.Array.Copy(pSelectedReps, selected_Reps, pSelectedReps.Length);
            }

            public void Run(object sender)
            {
                if (sender is Form1)
                {
                    Form1 fm1 = (Form1)sender;
                    fm1.ProcessClass(this.className, this.selected_Reps);
                }
                if (ThreadDone != null)
                    ThreadDone(this, EventArgs.Empty);
            }
        }
		
		// Multiple class processing worker thread
        class ProcessMultipleClassWorker
        {
            // Switch to your favourite Action<T> or Func<T>
            string[] classNames;
            Boolean bProcessSelectedReps;
            string excelFileName;

            public event EventHandler ThreadDone;

            public ProcessMultipleClassWorker(string pExcelFileName, string[] pClassName, Boolean pProcessSelectedReps)
            {
                this.classNames = new string[pClassName.Length];
                System.Array.Copy(pClassName, classNames, pClassName.Length);
                this.bProcessSelectedReps = pProcessSelectedReps;
                this.excelFileName = pExcelFileName;
            }

            public void Run(object sender)
            {
                // Do a task
                List<List<string>> excel_data = new List<List<string>>();
                string classname;

                for (int index = 0; index < classNames.Length; index++)
                {
                    classname = (string)classNames[index];
                    if (sender is Form1 && sender.GetType() == typeof(Form1))
                    {
                        Form1 fm1 = (Form1)sender;

                        // select the currently processed class
                        fm1.listBox_Classes.Invoke((MethodInvoker)delegate
                        {
                            fm1.listBox_Classes.SelectedIndex = index;
                        });

                        // fill in the list of reps
                        //Invoke in UI thread
                        fm1.listView_Reps.Invoke((MethodInvoker)delegate {
                            fm1.listView_Reps.Items.Clear();
                        });
                        string classfoldername = fm1.textBox_Root_FolderName.Text + "\\" + classname;
                        DirectoryInfo dinfo = new DirectoryInfo(classfoldername);
                        if (dinfo.Exists == false)
                            continue;
                        DirectoryInfo[] directories = dinfo.GetDirectories();
                        if (directories.Count() == 0)
                            continue;
                        foreach (DirectoryInfo directory in directories)
                        {
                            string rep_item = directory.Name;
                            //Invoke in UI thread
                            fm1.listView_Reps.Invoke((MethodInvoker)delegate
                            {
                                fm1.listView_Reps.Items.Add(rep_item);
                            });
                        }
                        //Invoke in UI thread
                        int number_of_reps = 0;
                        fm1.listView_Reps.Invoke((MethodInvoker)delegate
                        {
                            number_of_reps = fm1.listView_Reps.Items.Count;
                        });

                        // load or set the selections
                        string folder_name = fm1.textBox_Out_FolderName.Text + "\\_Saved_Lists\\" + classname;
                        string file_name = folder_name + "\\selected_reps.txt";
                        fm1.m_bRepSelectionLoading = true;
                        for (int i = 0; i < number_of_reps; i++)
                        {
                            //Invoke in UI thread
                            fm1.listView_Reps.Invoke((MethodInvoker)delegate
                            {
                                fm1.listView_Reps.Items[i].Selected = false;
                            });
                        }
                        int nb_of_settings_to_be_loaded = 0;
                        if (System.IO.File.Exists(file_name))
                        {
                            Stream stream = new FileStream(file_name, FileMode.Open, FileAccess.Read);
                            IFormatter formatter = new BinaryFormatter();
                            int[] rep_selections = (int[])formatter.Deserialize(stream);
                            stream.Close();
                            stream.Dispose();

                            nb_of_settings_to_be_loaded = System.Math.Min(number_of_reps, rep_selections.Count());
                            for (int i = 0; i < nb_of_settings_to_be_loaded; i++)
                            {
                                //Invoke in UI thread
                                fm1.listView_Reps.Invoke((MethodInvoker)delegate
                                {
                                    fm1.listView_Reps.Items[i].Checked = rep_selections[i] == 0 ? false : true;
                                });
                            }
                            //Invoke in UI thread
                            fm1.listView_Reps.Invoke((MethodInvoker)delegate
                            {
                                fm1.listView_Reps.Refresh();
                            });

                        }
                        fm1.m_bRepSelectionLoading = false;
                        Boolean[] selected_Reps = new Boolean[nb_of_settings_to_be_loaded];
                        if (bProcessSelectedReps)
                        {
                            for (int i = 0; i < selected_Reps.Length; i++)
                            {
                                selected_Reps[i] = fm1.listView_Reps.Items[i].Checked == true ? true : false;
                            }

                            Boolean bNothingToDo = true;
                            for (int i = 0; i < selected_Reps.Length; i++)
                                if (selected_Reps[i] == true)
                                    bNothingToDo = false;
                            if (bNothingToDo == true)
                            {
                                //MessageBox.Show(" At least one Rep must be selected for processing!");
                                Processing_Activated = false;
                                fm1.Cursor = System.Windows.Forms.Cursors.Default;
                                return;
                            }

                        }
                        else
                        {
                            for (int i = 0; i < selected_Reps.Length; i++)
                            {
                                selected_Reps[i] = true;
                            }
                        }

                        excel_data.AddRange(fm1.ProcessClass(classname, selected_Reps));
                    }
                }

                Excels_EPP.Write_to_Excel_File(excelFileName, excel_data);

                if (ThreadDone != null)
                    ThreadDone(this, EventArgs.Empty);
            }
        }

        private void ShowFeatures_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox_Source_Image.Invalidate();
        }

        private void button_Color_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = false;
            cd.ShowHelp = true;
            cd.Color = button.BackColor;

            if (cd.ShowDialog() == DialogResult.OK)
            {
                button.BackColor = cd.Color;
                pictureBox_Source_Image.Invalidate();
            }

            GetPlotColors();

        }

        private void pictureBox_Source_Image_Paint(object sender, PaintEventArgs e)
        {
            if (Processing_Activated == true)
                return;

            // If mask or segmented image is displayed, paint additional object(s) over the visualized bitmap (overlay)
            if ( tabControl_Show.SelectedIndex==0)
                PaintAddOnsOverInputImage(e.Graphics, pictureBox_Source_Image.Width, pictureBox_Source_Image.Height);
        }

        void PaintAddOnsOverInputImage(Graphics g, int output_width, int output_height)
        {
            if (pictureBox_Source_Image.Image == null)
                return;

            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;

            // transform the positions from image-relative coord. system to the coord. system of visualized image
            double ratio_image = (double)Input_Mask_Bitmap.Width / (double)Input_Mask_Bitmap.Height;
            double ratio_PictureBox = (double)output_width / (double)output_height;
            double ratio;
            double shiftx, shifty;
            if (ratio_image > ratio_PictureBox)
            {
                // the pictureBox's height is bigger than image's one
                ratio = (double)output_width / (double)Input_Mask_Bitmap.Width;
                shiftx = 0;
                shifty = (double)(output_height - ratio * (double)Input_Mask_Bitmap.Height) / 2.0;
            }
            else
            {
                // the pictureBox's width is bigger than image's one
                ratio = (double)output_height / (double)Input_Mask_Bitmap.Height;
                shiftx = (double)(output_width - ratio * (double)Input_Mask_Bitmap.Width) / 2.0;
                shifty = 0;
            }

            // get the index of feature items (computed from 'rep' and 'image')
            if (rep_serialnumbers.Count == 0)
                return;
            int index = 0;

            ListView.SelectedIndexCollection selindices = listView_Reps.SelectedIndices;
            int selected_index = selindices.Count == 0 ? -1 : selindices[0];
            if (selected_index == -1)
                return;
            if (rep_lengths.Count == 0)
                return;
            if (rep_lengths.Count > 0 && rep_lengths[selected_index] == 0)
                return;
            for (int ii = 0; ii < selected_index; ii++)
                index += System.Math.Max(0, rep_lengths[ii]);
            index += listBox_Images.SelectedIndex;

            int nbofpoints = 0;
            int i, x1, y1, x2, y2;
            if (checkBox_Show_Rosette_Area.Checked == true && rosette_contours != null && rosette_contours.Count > index)
            {
                Color plantcontour_color = button_Rosette_Area_Color.BackColor;
                SolidBrush plantcontour_brush = new SolidBrush(plantcontour_color);
                nbofpoints = rosette_contours[index].Count;
                for (i = 0; i < nbofpoints; i++)
                {
                    x1 = (int)(shiftx + ratio * rosette_contours[index][i].X - 0.5);
                    y1 = (int)(shifty + ratio * rosette_contours[index][i].Y - 0.5);
                    g.FillRectangle(plantcontour_brush, x1 - 1, y1 - 1, 3, 3);
                }
                plantcontour_brush.Dispose();
            }

            if ( checkBox_Show_Convex_Hull.Checked== true && convex_hull_contours != null && convex_hull_contours.Count > index)
            {
                Color convexhull_color = button_Convex_Hull_Color.BackColor;
                Pen convexhull_pen = new Pen(convexhull_color, 2);
                nbofpoints = convex_hull_contours[index].Count;
                for (i = 0; i < nbofpoints - 1; i++)
                {
                    x1 = (int)(shiftx + ratio * convex_hull_contours[index][i].X - 0.5);
                    y1 = (int)(shifty + ratio * convex_hull_contours[index][i].Y - 0.5);
                    x2 = (int)(shiftx + ratio * convex_hull_contours[index][i + 1].X - 0.5);
                    y2 = (int)(shifty + ratio * convex_hull_contours[index][i + 1].Y - 0.5);
                    g.DrawLine(convexhull_pen, x1, y1, x2, y2);
                }
                x1 = (int)(shiftx + ratio * convex_hull_contours[index][i].X - 0.5);
                y1 = (int)(shifty + ratio * convex_hull_contours[index][i].Y - 0.5);
                x2 = (int)(shiftx + ratio * convex_hull_contours[index][0].X - 0.5);
                y2 = (int)(shifty + ratio * convex_hull_contours[index][0].Y - 0.5);
                g.DrawLine(convexhull_pen, x1, y1, x2, y2);
                convexhull_pen.Dispose();
            }

            if ( checkBox_Show_Standard_Diameter.Checked == true && standard_diameters != null && standard_diameters.Count > index)
            {
                Color standarddiameter_color = button_Standard_Diameter_Color.BackColor;
                Pen standarddiameter_pen = new Pen(standarddiameter_color, 2);
                x1 = (int)(shiftx + ratio * standard_diameters[index].x1 - 0.5);
                y1 = (int)(shifty + ratio * standard_diameters[index].y1 - 0.5);
                x2 = (int)(shiftx + ratio * standard_diameters[index].x2 - 0.5);
                y2 = (int)(shifty + ratio * standard_diameters[index].y2 - 0.5);
                g.DrawLine(standarddiameter_pen, x1, y1, x2, y2);
                standarddiameter_pen.Dispose();
            }

            if ( checkBox_Show_Bounding_Box.Checked == true && bounding_boxes != null && bounding_boxes.Count > index)
            {
                Color boundingbox_color = button_Bounding_Box_Color.BackColor;
                Pen boundingbox_pen = new Pen(boundingbox_color, 2);
                x1 = (int)(shiftx + ratio * bounding_boxes[index].p1.X - 0.5);
                y1 = (int)(shifty + ratio * bounding_boxes[index].p1.Y - 0.5);
                x2 = (int)(shiftx + ratio * bounding_boxes[index].p2.X - 0.5);
                y2 = (int)(shifty + ratio * bounding_boxes[index].p2.Y - 0.5);
                g.DrawLine(boundingbox_pen, x1, y1, x2, y2);
                x1 = x2;
                y1 = y2;
                x2 = (int)(shiftx + ratio * bounding_boxes[index].p3.X - 0.5);
                y2 = (int)(shifty + ratio * bounding_boxes[index].p3.Y - 0.5);
                g.DrawLine(boundingbox_pen, x1, y1, x2, y2);
                x1 = x2;
                y1 = y2;
                x2 = (int)(shiftx + ratio * bounding_boxes[index].p4.X - 0.5);
                y2 = (int)(shifty + ratio * bounding_boxes[index].p4.Y - 0.5);
                g.DrawLine(boundingbox_pen, x1, y1, x2, y2);
                x1 = x2;
                y1 = y2;
                x2 = (int)(shiftx + ratio * bounding_boxes[index].p1.X - 0.5);
                y2 = (int)(shifty + ratio * bounding_boxes[index].p1.Y - 0.5);
                g.DrawLine(boundingbox_pen, x1, y1, x2, y2);
                boundingbox_pen.Dispose();
            }

            if (checkBox_Show_Bounding_Circle.Checked == true && bounding_circles != null && bounding_circles.Count > index)
            {
                Color boundingcircle_color = button_Bounding_Circle_Color.BackColor;
                Pen boundingcircle_pen = new Pen(boundingcircle_color, 2);
                x1 = (int)(shiftx + ratio * (bounding_circles[index].centre.X - bounding_circles[index].radius) - 0.5);
                y1 = (int)(shifty + ratio * (bounding_circles[index].centre.Y - bounding_circles[index].radius) - 0.5);
                x2 = (int)(ratio * 2.0 * bounding_circles[index].radius + 0.5);
                y2 = (int)(ratio * 2.0 * bounding_circles[index].radius + 0.5);
                g.DrawEllipse(boundingcircle_pen, x1, y1, x2, y2);
                boundingcircle_pen.Dispose();
            }

            if (checkBox_Show_CHull_Equiv_Circle.Checked == true
                && convex_hull_equiv_circles != null
                && convex_hull_equiv_circles.Count > index)
            {
                Color chull_equiv_circle_color = button_CHull_Equiv_Circle_Color.BackColor;
                Pen chull_equiv_circle_pen = new Pen(chull_equiv_circle_color, 2);
                x1 = (int)(shiftx + ratio * (convex_hull_equiv_circles[index].centre_x - convex_hull_equiv_circles[index].radius) - 0.5);
                y1 = (int)(shifty + ratio * (convex_hull_equiv_circles[index].centre_y - convex_hull_equiv_circles[index].radius) - 0.5);
                x2 = (int)(ratio * 2.0 * convex_hull_equiv_circles[index].radius + 0.5);
                y2 = (int)(ratio * 2.0 * convex_hull_equiv_circles[index].radius + 0.5);
                g.DrawEllipse(chull_equiv_circle_pen, x1, y1, x2, y2);
                chull_equiv_circle_pen.Dispose();
            }

            // The ellipse must be drawn the last because the translation/rotation transforms of the Graphics object g affects
            // the positioning for anything following it. It is a bit complex to solve this, so drawing the ellipse at the end is the quickest solution.
            if (checkBox_Show_Bounding_Ellipse.Checked == true && bounding_ellipses != null && bounding_ellipses.Count > index)
            {
                Color boundingellipse_color = button_Bounding_Ellipse_Color.BackColor;
                Pen boundingellipse_pen = new Pen(boundingellipse_color, 2);
                Ellipse_Int curr_ellipse = bounding_ellipses[index];
                x1 = (int)(shiftx + ratio * (curr_ellipse.centre.X - curr_ellipse.major_radius) - 0.5);
                y1 = (int)(shifty + ratio * (curr_ellipse.centre.Y - curr_ellipse.minor_radius) - 0.5);
                x2 = (int)(ratio * 2.0 * curr_ellipse.major_radius + 0.5);
                y2 = (int)(ratio * 2.0 * curr_ellipse.minor_radius + 0.5);
                // Set world transform of graphics object to translate.
                g.TranslateTransform(
                    -(float)(shiftx + ratio * curr_ellipse.centre.X - 0.5),
                    -(float)(shifty + ratio * curr_ellipse.centre.Y - 0.5));
                // Rotate, prepending rotation matrix.
                g.RotateTransform(-(float)curr_ellipse.angle_major_radius, MatrixOrder.Append);
                // Set world transform of graphics object to translate.
                g.TranslateTransform(
                    (float)(shiftx + ratio * curr_ellipse.centre.X - 0.5),
                    (float)(shifty + ratio * curr_ellipse.centre.Y - 0.5),
                    MatrixOrder.Append);
                g.DrawEllipse(boundingellipse_pen, x1, y1, x2, y2);
                boundingellipse_pen.Dispose();
            }
        }


        private void button_Save_Image_Click(object sender, EventArgs e)
        {
            if (pictureBox_Source_Image.Image == null)
                return;

            // get the currently visualized content
            Bitmap bmp = (Bitmap)pictureBox_Source_Image.Image.Clone();
            if (bmp == null)
                return;

            using (Graphics g = Graphics.FromImage(bmp))
            {
                PaintAddOnsOverInputImage(g, bmp.Width, bmp.Height); // paint extracted features as overlay
            }

            // choose name and format for saving
            // REM: all rices will be saved separately, and the serial number (ID) will be appended to the file name
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter =
                "bitmap images (*.bmp)|*.bmp|jpeg images (*.jpg,*.jpeg)|*.jpg;*.jpeg|png images (*.png)|*.png|All files (*.*)|*.*";

            // the default format is ".jpg"
            dlg.FilterIndex = 2;

            if (dlg.ShowDialog() != DialogResult.OK)
                return;             // cancelled by user

            ImageFormat format;
            string fileName = dlg.FileName;
            if (dlg.FilterIndex == 1)
            {
                format = ImageFormat.Bmp;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != ".bmp")
                    fileName = fileName + ".bmp";
            }
            else if (dlg.FilterIndex == 2)
            {
                format = ImageFormat.Jpeg;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != ".jpg")
                    fileName = fileName + ".jpg";
            }
            else
            {
                format = ImageFormat.Png;
                if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != ".png")
                    fileName = fileName + ".png";
            }

            ImageProcessing ip = new ImageProcessing();
            ip.SaveBitmap(bmp, fileName, format);
        }

        //----------------------------------------------------------------------------------------

        private void ClearGUIData()
        {
            listBox_Classes.Items.Clear();

            listView_Reps.Items.Clear();

            listBox_Images.Items.Clear();
            textBox_Current_Source_Image_File_Name.ResetText();
            pictureBox_Source_Image.Image = null;
            Input_Empty_Bitmap = null;
            Input_Segmented_Bitmap = null;
            Input_Mask_With_Stems = null;
            Input_Mask_No_Noise = null;
            Input_Mask_Bitmap = null;

            cached_class_name = null;
            cached_rep_name = null;
            cached_rep_masks.Clear();
            cached_rep_masks_no_noise.Clear();
            cached_rep_masks_with_stems.Clear();
            cached_rep_seg_imgs.Clear();
            cached_rep_empty_imgs.Clear();
            current_filename = null;
        }

        private void ClearFeatures()
        {
            rosette_contours.Clear();
            rosette_areas.Clear();
            rosette_perimeters.Clear();
            convex_hull_contours.Clear();
            convex_hull_areas.Clear();
            convex_hull_perimeters.Clear();
            convex_hull_equiv_circles.Clear();
            convex_hull_inside_equiv_circles.Clear();
            standard_diameters.Clear();
            bounding_boxes.Clear();
            bounding_circles.Clear();
            bounding_ellipses.Clear();
            ellipses_2nd_central_moment.Clear();

            rep_serialnumbers.Clear();
            rep_lengths.Clear();
            bounding_box_areas.Clear();
            std_diameters.Clear();
            area_growth_rates.Clear();
            convex_hull_growth_rates.Clear();
            circumferences.Clear();
            eccentricities.Clear();
            area_to_perimeter_ratios.Clear();
            compactnesses.Clear();
            extents.Clear();
            plant_roundnesses.Clear();
            surface_coverages.Clear();
            bounding_box_aspect_ratios.Clear();
            rotational_mass_asymmetries.Clear();
            convex_hull_roundnesses.Clear();
            convex_hull_elongations.Clear();
            bounding_ellipse_circularities.Clear();

            data_per_DAS.Clear();
        }

        private void LoadandShowCurrentImage()
        {
            if (textBox_Root_FolderName.Text == "")
                return;
            if (listBox_Classes.Items.Count == 0)
                return;

            if (listView_Reps.Items.Count == 0)
                return;

            if (textBox_Current_Source_Image_File_Name.Text == "")
                return;
            current_filename = textBox_Current_Source_Image_File_Name.Text;

            string filename = textBox_Root_FolderName.Text;
            string current_filename_seg_img = textBox_Root_FolderName.Text;
            ImageProcessing ip = new ImageProcessing();

            //Invoke in UI thread
            listBox_Classes.Invoke((MethodInvoker)delegate
            {
                filename += "\\" + listBox_Classes.SelectedItem;
                current_filename_seg_img += "\\" + listBox_Classes.SelectedItem;
            });
            string rep_item = "";

            listView_Reps.Invoke((MethodInvoker)delegate
            {
                ListView.SelectedIndexCollection selectedindices = listView_Reps.SelectedIndices;
                int selectedindex = selectedindices.Count == 0 ? 0 : listView_Reps.SelectedIndices[0];
                rep_item = listView_Reps.Items[selectedindex].Text;
            });

            filename += "\\" + rep_item + "\\masks";
            filename += "\\" + textBox_Current_Source_Image_File_Name.Text;

            current_filename_seg_img += "\\" + rep_item + "\\segmented_images";
            current_filename_seg_img += "\\" + textBox_Current_Source_Image_File_Name.Text.Replace("mask", "seg");
            if (cached_rep_masks.ContainsKey(current_filename) && cached_rep_seg_imgs.ContainsKey(current_filename_seg_img))
            {
                Input_Mask_Bitmap = cached_rep_masks[current_filename];
                Input_Mask_No_Noise = cached_rep_masks_no_noise[current_filename];
                Input_Mask_With_Stems = cached_rep_masks_with_stems[current_filename];
                Input_Segmented_Bitmap = cached_rep_seg_imgs[current_filename_seg_img];
                Input_Empty_Bitmap = cached_rep_empty_imgs[current_filename];
            }
            else
            {
                try
                {
                    Bitmap tmp_bitmap = new Bitmap(filename); // exception, if unsuccessful;
                    Input_Mask_Bitmap = new Bitmap(tmp_bitmap);
                    tmp_bitmap.Dispose();
                    Input_Empty_Bitmap = new Bitmap(Input_Mask_Bitmap.Width, Input_Mask_Bitmap.Height, Input_Mask_Bitmap.PixelFormat);
                    (Input_Mask_No_Noise, Input_Mask_With_Stems) = ip.GetFilteredMask(Input_Mask_Bitmap, 8, 9);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot read file \"" + filename + "\"!..." + ex.ToString());
                    return;
                }

                try
                {
                    Bitmap tmp_bitmap = new Bitmap(current_filename_seg_img); // exception, if unsuccessful;
                    Input_Segmented_Bitmap = new Bitmap(tmp_bitmap);
                    tmp_bitmap.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Cannot read file \"" + current_filename_seg_img + "\"! >>> " + ex.ToString());
                    Input_Mask_Bitmap = null;
                    Input_Mask_No_Noise = null;
                    Input_Mask_With_Stems = null;
                    Input_Empty_Bitmap = null;
                    return;
                }

                cached_rep_masks[current_filename] = Input_Mask_Bitmap;
                cached_rep_masks_no_noise[current_filename] = Input_Mask_No_Noise;
                cached_rep_masks_with_stems[current_filename] = Input_Mask_With_Stems;
                cached_rep_seg_imgs[current_filename_seg_img] = Input_Segmented_Bitmap;
                cached_rep_empty_imgs[current_filename] = Input_Empty_Bitmap;
            }

            if (radioButton_Show_Image_Mask.Checked)
            {
                if (Processing_Activated)
                {
                    //Invoke in UI thread
                    pictureBox_Source_Image.Invoke((MethodInvoker)delegate
                    {
                        pictureBox_Source_Image.Image = Input_Mask_Bitmap;
                    });
                }
                else
                    SetPictureBoxImage();
            }
            else if (radioButton_Show_Image_Segmented.Checked)
            {
                if (Processing_Activated)
                {
                    //Invoke in UI thread
                    pictureBox_Source_Image.Invoke((MethodInvoker)delegate
                    {
                        pictureBox_Source_Image.Image = Input_Segmented_Bitmap;
                    });
                }
                else
                    SetPictureBoxImage();
            }
            else if (radioButton_Show_Image_None.Checked)
            {
                if (Processing_Activated)
                {
                    //Invoke in UI thread
                    pictureBox_Source_Image.Invoke((MethodInvoker)delegate
                    {
                        pictureBox_Source_Image.Image = Input_Empty_Bitmap;
                    });
                }
                else
                    SetPictureBoxImage();
            }

            //Invoke in UI thread
            pictureBox_Source_Image.Invoke((MethodInvoker)delegate
            {
                pictureBox_Source_Image.Refresh();
            });

            // do not show pixel data
            textBox_Cursor_Position.Invoke((MethodInvoker)delegate
            {
                textBox_Cursor_Position.Text = "";
            });

            textBox_Pixel.Invoke((MethodInvoker)delegate
            {
                textBox_Pixel.Text = "";
            });

        }

        private Point WindowToImage(Point pos)
        {
            Point pt = new Point(0, 0);
            if ( pictureBox_Source_Image.Image == null)
                return pt;

            // transform the positions from windows's coordinate system to
            // the coordinate system of image
            double ratio_image = (double)Input_Mask_Bitmap.Width / (double)Input_Mask_Bitmap.Height;
            double ratio_PictureBox = (double)pictureBox_Source_Image.Width / (double)pictureBox_Source_Image.Height;
            double ratio;
            double shiftx, shifty;
            if (ratio_image > ratio_PictureBox)
            {
                // the pictureBox's height is bigger than image's one
                ratio = (double)pictureBox_Source_Image.Width / (double)Input_Mask_Bitmap.Width;
                shiftx = 0;
                shifty = (double)(pictureBox_Source_Image.Height - ratio * (double)Input_Mask_Bitmap.Height) / 2.0;
            }
            else
            {
                // the pictureBox's width is bigger than image's one
                ratio = (double)pictureBox_Source_Image.Height / (double)Input_Mask_Bitmap.Height;
                shiftx = (double)(pictureBox_Source_Image.Width - ratio * (double)Input_Mask_Bitmap.Width) / 2.0;
                shifty = 0;
            }
            pt.X = (int)((double)(pos.X - shiftx) / ratio + 0.5);
            pt.Y = (int)((double)(pos.Y - shifty) / ratio + 0.5);

            return pt;
        }

        private void ShowPixelData( int xpos, int ypos)
        {
            if (tabControl_Show.SelectedIndex != 0)
                xpos = -1;
            textBox_Cursor_Position.Text = xpos==-1 ? "" : "( " + Convert.ToString(xpos) + ", " + Convert.ToString(ypos) + " )";
            textBox_Cursor_Position.Refresh();

            if (xpos == -1)
                textBox_Pixel.Text = "";
            else
            {
                Color pixelvalue = new Color();
                pixelvalue = Input_Mask_Bitmap.GetPixel(xpos, ypos);
                if ( pixelvalue.R==0)
                    textBox_Pixel.Text = "---";
                else
                    textBox_Pixel.Text = "Plant";
            }
        }

        private void ClearFeatureLists()
        {
            DAS_s.Clear();
            date_str.Clear();
            time_str.Clear();
            rosette_contours.Clear();
            rosette_areas.Clear();
            rosette_perimeters.Clear();
            convex_hull_contours.Clear();
            convex_hull_areas.Clear();
            convex_hull_perimeters.Clear();
            convex_hull_equiv_circles.Clear();
            convex_hull_inside_equiv_circles.Clear();
            standard_diameters.Clear();
            bounding_boxes.Clear();
            bounding_circles.Clear();
            bounding_ellipses.Clear();
            ellipses_2nd_central_moment.Clear();

            rep_serialnumbers.Clear();
            rep_lengths.Clear();
            bounding_box_areas.Clear();
            std_diameters.Clear();
            area_growth_rates.Clear();
            convex_hull_growth_rates.Clear();
            circumferences.Clear();
            eccentricities.Clear();
            area_to_perimeter_ratios.Clear();
            compactnesses.Clear();
            extents.Clear();
            plant_roundnesses.Clear();
            surface_coverages.Clear();
            bounding_box_aspect_ratios.Clear();
            rotational_mass_asymmetries.Clear();
            convex_hull_roundnesses.Clear();
            convex_hull_elongations.Clear();
            bounding_ellipse_circularities.Clear();

            data_per_DAS.Clear();
        }

        //public string ProcessClass(string classname, Boolean[] selected_Reps)
        public List<List<string>> ProcessClass(string classname, Boolean[] selected_Reps)
        {
            //string class_data = "";
            List<List<string>> class_data = new List<List<string>>();

            // init the list of features, as new class is being processed
            ClearFeatureLists();

            // delete all previously created outout files
            DeletePreviousResultFiles( classname);

            // Re-allocate data structures per feature
            foreach (string feature_name in feature_names)
                data_per_DAS[feature_name] = new SortedList<string, SortedList<int, List<double>>>();

            // attach necessary tools
            ImageProcessing ip = new ImageProcessing();
            FeatureProcessing fp = new FeatureProcessing();

            int nbofreps = listView_Reps.Items.Count;
            double[] rep_DAS_min = new double[nbofreps];
            int[] rep_days_shift = new int[nbofreps];

            DateTime sowing_date = dateTimePicker_Sowing_Start.Value.Date;

            for ( int repindex = 0; repindex< nbofreps; repindex++)
            {
                if (selected_Reps[repindex] == false)
                {
                    rep_serialnumbers.Add(-1);
                    rep_lengths.Add(0);
                    rep_DAS_min[repindex] = 0;

                    continue;
                }

                //Invoke in UI thread
                string rep_item = "";
                listView_Reps.Invoke((MethodInvoker)delegate
                {
                    listView_Reps.Items[repindex].Selected = true;
                    listView_Reps.Items[repindex].EnsureVisible();
                    listView_Reps.Select();
                    listView_Reps.Refresh();
                    rep_item = listView_Reps.Items[repindex].Text;
                });

                // fill the list of Images
				//Invoke in UI thread
                listBox_Images.Invoke((MethodInvoker)delegate
                {
                    listBox_Images.Items.Clear();
                });
                
                rep_serialnumbers.Add(Convert.ToInt16(rep_item.Substring(4, 2)));
                string imagesfoldername = textBox_Root_FolderName.Text + "\\" + classname + "\\" + rep_item + "\\masks";
                DirectoryInfo d2info = new DirectoryInfo(imagesfoldername);
                if (d2info.Exists == false)
                    return class_data;
                FileInfo[] files = d2info.GetFiles();
                rep_lengths.Add(files.Count());
                if (files.Count() == 0)
                    return class_data;
                int counter = 0;
                Dictionary<int, int> int_DAS_per_file_index = new Dictionary<int, int>();

                foreach (FileInfo file in files)
                {
                    string filename = file.Name;
                    counter++;
                    string item = counter < 100 ? "0" : "";
                    item += counter < 10 ? "0" : "";
                    item += Convert.ToString(counter);
                    item += "        ";
                    item += filename;
					//Invoke in UI thread
                    listBox_Images.Invoke((MethodInvoker)delegate
                    {
                        listBox_Images.Items.Add(item);
                    });

                    // compute and store DAS, date and time
                    string str1 = filename.Remove(0, filename.IndexOf('_')+1);
                    string year = str1.Remove(str1.IndexOf('_'));
                    str1 = str1.Remove(0, str1.IndexOf('_')+1);
                    string month = str1.Remove(str1.IndexOf('_'));
                    str1 = str1.Remove(0, str1.IndexOf('_')+1);
                    string day = str1.Remove(str1.IndexOf('_'));
                    str1 = str1.Remove(0, str1.IndexOf('_')+1);
                    string hour = str1.Remove(str1.IndexOf('_'));
                    str1 = str1.Remove(0, str1.IndexOf('_')+1);
                    string minute = str1.Remove(str1.IndexOf('_'));
                    DateTime datetime = new DateTime(Convert.ToInt16(year), Convert.ToInt16(month),
                        Convert.ToInt16(day), Convert.ToInt16(hour), Convert.ToInt16(minute), 0);
                    var hours = (datetime - sowing_date).TotalHours;
                    double DAS = hours / 24.0;
                    DAS_s.Add(DAS);
                    string Date_str = day + "/" + month + "/" + year;
                    date_str.Add(Date_str);
                    string Time_str = hour + ':' + minute;
                    time_str.Add(Time_str);
                    int DAS_int = (int)DAS;
                    if (!data_per_DAS[feature_names[0]].ContainsKey(rep_item))
                    {
                        // Add rep name for all features
                        foreach(string feature_name in feature_names)
                            data_per_DAS[feature_name].Add(rep_item, new SortedList<int, List<double>>());
                    }    
                    if (!data_per_DAS[feature_names[0]][rep_item].ContainsKey(DAS_int))
                    {
                        // Add DAS for this rep for all features
                        foreach (string feature_name in feature_names)
                            data_per_DAS[feature_name][rep_item].Add(DAS_int, new List<double>());
                    }
                    int_DAS_per_file_index[counter - 1] = DAS_int;
                }

                rep_DAS_min[repindex] = DAS_s.GetRange(
                    DAS_s.Count - rep_lengths[rep_lengths.Count - 1],
                    rep_lengths[rep_lengths.Count - 1]).Min();

				//Invoke in UI thread
                listBox_Images.Invoke((MethodInvoker)delegate
                {
                    listBox_Images.Refresh();
                });

                //----------------------------------------------------------------------
                // process all images of current Rep and store the results in Lists
                // - extract basic features
                for ( int imageindex = 0; imageindex<counter; imageindex++)
                {
					//Invoke in UI thread
                    string filename = "";
                    listBox_Images.Invoke((MethodInvoker)delegate
                    {
                        listBox_Images.SelectedIndex = imageindex;
                        filename = (string)listBox_Images.SelectedItem;
                    });
                    
                    filename = filename.Remove(0, 11);
					//Invoke in UI thread
                    textBox_Current_Source_Image_File_Name.Invoke((MethodInvoker)delegate
                    {
                        textBox_Current_Source_Image_File_Name.Text = filename;
                        textBox_Current_Source_Image_File_Name.Refresh();
                    });
                    

                    // load and show current image
                    LoadandShowCurrentImage();

                    Bitmap Input_Mask_Bitmap_Clone = (Bitmap)Input_Mask_Bitmap.Clone();
                    Bitmap Input_Mask_No_Noise_Clone = (Bitmap)Input_Mask_No_Noise.Clone();
                    Bitmap Input_Mask_With_Stems_Clone = (Bitmap)Input_Mask_With_Stems.Clone();

                    // ### process currently loaded image ###
                    // 1. get the plant's contour
                    List<Point> rosette_contour = ip.GetPlantContour_OpenCV(Input_Mask_With_Stems_Clone);
                    rosette_contours.Add(rosette_contour);

                    // 2. get the area of plant ( in number of pixels - will be scaled into physical size in real scene)
                    double rosette_area = ip.GetAreaBasedOnMask(Input_Mask_No_Noise_Clone);
                    rosette_areas.Add(rosette_area);
                    data_per_DAS["rosette_areas"][rep_item][int_DAS_per_file_index[imageindex]].Add(rosette_area);

                    // 3. get the perimeter of plant's contour (in pixels)
                    double rosette_perimeter = fp.GetPerimeter(rosette_contour);
                    rosette_perimeters.Add(rosette_perimeter);
                    data_per_DAS["perimeters"][rep_item][int_DAS_per_file_index[imageindex]].Add(rosette_perimeter);
                    if (rosette_perimeter == 0.0)
                    {
                        data_per_DAS["area_to_perimeter_ratios"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);
                        data_per_DAS["plant_roundnesses"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);
                    }
                    else
                    {
                        data_per_DAS["area_to_perimeter_ratios"][rep_item][int_DAS_per_file_index[imageindex]].Add(rosette_area / rosette_perimeter);
                        data_per_DAS["plant_roundnesses"][rep_item][int_DAS_per_file_index[imageindex]].Add(
                            4.0 * System.Math.PI * rosette_area / System.Math.Pow(rosette_perimeter, 2.0));
                    }

                    // 4. get convex hull
                    List<Point> convex_hull_contour = fp.GetConvexHull(rosette_contour);
                    convex_hull_contours.Add(convex_hull_contour);

                    // 5. get the area of convex hull
                    double convexhull_area = fp.GetAreaBasedOnContour(convex_hull_contour);
                    convex_hull_areas.Add(convexhull_area);
                    data_per_DAS["convexhull_areas"][rep_item][int_DAS_per_file_index[imageindex]].Add(convexhull_area);
                    if (convexhull_area == 0.0)
                        data_per_DAS["compactnesses"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);
                    else
                        data_per_DAS["compactnesses"][rep_item][int_DAS_per_file_index[imageindex]].Add(rosette_area / convexhull_area);

                    // 6. get the perimeter of convex hull (in pixels)
                    double convexhull_perimeter = fp.GetPerimeter(convex_hull_contour);
                    convex_hull_perimeters.Add(convexhull_perimeter);
                    data_per_DAS["convexhull_perimeters"][rep_item][int_DAS_per_file_index[imageindex]].Add(convexhull_perimeter);
                    if (convexhull_perimeter == 0.0)
                    {
                        data_per_DAS["convex_hull_roundnesses"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);
                    }
                    else
                    {
                        data_per_DAS["convex_hull_roundnesses"][rep_item][int_DAS_per_file_index[imageindex]].Add(
                            4 * System.Math.PI * convexhull_area / System.Math.Pow(convexhull_perimeter, 2));
                    }

                    // 7. get the standard diameter of convex hull
                    Point[] standard_distance_endpoints = fp.GetStandardDiameter(convex_hull_contour);
                    Standard_Diameter standard_diameter = new Standard_Diameter();
                    standard_diameter.x1 = standard_distance_endpoints[0].X;
                    standard_diameter.y1 = standard_distance_endpoints[0].Y;
                    standard_diameter.x2 = standard_distance_endpoints[1].X;
                    standard_diameter.y2 = standard_distance_endpoints[1].Y;
                    standard_diameter.diameter = System.Math.Sqrt(System.Math.Pow((double)(standard_diameter.x2 - standard_diameter.x1), 2.0) +
                        System.Math.Pow((double)(standard_diameter.y2 - standard_diameter.y1), 2.0));
                    standard_diameters.Add(standard_diameter);
                    data_per_DAS["std_diameters"][rep_item][int_DAS_per_file_index[imageindex]].Add(standard_diameter.diameter);

                    // 8. get the bounding box (with area)
                    List<Point> corner_points = fp.GetBoundingBox(convex_hull_contour);
                    Bounding_Box bounding_box = new Bounding_Box();
                    bounding_box.p1 = corner_points[0];
                    bounding_box.p2 = corner_points[1];
                    bounding_box.p3 = corner_points[2];
                    bounding_box.p4 = corner_points[3];
                    bounding_box.area = fp.GetAreaBasedOnContour(corner_points);
                    bounding_boxes.Add(bounding_box);
                    data_per_DAS["bounding_box_areas"][rep_item][int_DAS_per_file_index[imageindex]].Add(bounding_box.area);
                    if (bounding_box.area == 0.0)
                        data_per_DAS["extents"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);
                    else
                        data_per_DAS["extents"][rep_item][int_DAS_per_file_index[imageindex]].Add(rosette_area / bounding_box.area);
                    double side1 = System.Math.Sqrt(System.Math.Pow(bounding_box.p2.X - bounding_box.p1.X, 2.0) +
                        System.Math.Pow(bounding_box.p2.Y - bounding_box.p1.Y, 2.0));
                    double side2 = System.Math.Sqrt(System.Math.Pow(bounding_box.p3.X - bounding_box.p2.X, 2.0) +
                        System.Math.Pow(bounding_box.p3.Y - bounding_box.p2.Y, 2.0));
                    double aspect_ratio = 0;
                    if (side1 > 0 && side2 > 0)
                        aspect_ratio = side1 < side2 ? (side1 / side2) : (side2 / side1);
                    if (aspect_ratio == 0.0)
                        data_per_DAS["BB_aspect_ratios"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);
                    else
                        data_per_DAS["BB_aspect_ratios"][rep_item][int_DAS_per_file_index[imageindex]].Add(aspect_ratio);

                    // 9. get the bounding circle (with area)
                    List<double> circle_data = fp.GetBoundingCircle(convex_hull_contour);
                    Circle_Int bounding_circle = new Circle_Int();
                    bounding_circle.centre.X = (int)(circle_data[0] + 0.5);
                    bounding_circle.centre.Y = (int)(circle_data[1] + 0.5);
                    bounding_circle.radius = (int)(circle_data[2] + 0.5);
                    bounding_circle.area = System.Math.PI * System.Math.Pow(circle_data[2], 2.0);
                    bounding_circles.Add(bounding_circle);
                    data_per_DAS["circumferences"][rep_item][int_DAS_per_file_index[imageindex]].Add(System.Math.PI * 2.0 * bounding_circle.radius);
                    if (bounding_circle.area == 0.0)
                        data_per_DAS["surface_coverages"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);
                    else
                        data_per_DAS["surface_coverages"][rep_item][int_DAS_per_file_index[imageindex]].Add(rosette_area / bounding_circle.area);

                    // 10. get the bounding ellipse (with area)
                    List<double> ellipse_data = fp.GetBoundingEllipse(convex_hull_contour);
                    Ellipse_Int bounding_ellipse = new Ellipse_Int();
                    bounding_ellipse.centre.X = (int)(ellipse_data[0] + 0.5);
                    bounding_ellipse.centre.Y = (int)(ellipse_data[1] + 0.5);
                    bounding_ellipse.major_radius = (int)(ellipse_data[2] + 0.5);
                    bounding_ellipse.minor_radius = (int)(ellipse_data[3] + 0.5);
                    bounding_ellipse.angle_major_radius = ellipse_data[4];
                    bounding_ellipse.area = System.Math.PI * bounding_ellipse.major_radius * bounding_ellipse.minor_radius;
                    bounding_ellipses.Add(bounding_ellipse);

                    //List<Point> input_mask_points = fp.MaskToListOfPoints(Input_Mask_Bitmap_Clone);
                    List<Point> input_mask_points = fp.MaskToListOfPoints(Input_Mask_No_Noise_Clone);
                    List<double> ellipse_data_2nd_central_moment = fp.GetEllipseBasedOn2ndCentralMoment(input_mask_points);
                    Ellipse_Double ellipse_2nd_central_moment = new Ellipse_Double();
                    ellipse_2nd_central_moment.centre_x = ellipse_data_2nd_central_moment[0] + 0.5;
                    ellipse_2nd_central_moment.centre_y = ellipse_data_2nd_central_moment[1] + 0.5;
                    ellipse_2nd_central_moment.major_radius = ellipse_data_2nd_central_moment[2] + 0.5;
                    ellipse_2nd_central_moment.minor_radius = ellipse_data_2nd_central_moment[3] + 0.5;
                    ellipse_2nd_central_moment.angle_major_radius = ellipse_data_2nd_central_moment[4];
                    ellipse_2nd_central_moment.area = System.Math.PI * ellipse_2nd_central_moment.major_radius * ellipse_2nd_central_moment.minor_radius;
                    ellipses_2nd_central_moment.Add(ellipse_2nd_central_moment);

                    double curr_eccentricity = 0.0;
                    double curr_bounding_ellipse_circularity = 0.0;
                    if (bounding_ellipse.major_radius > 0)
                    {
                        curr_eccentricity = System.Math.Sqrt(
                                1 - System.Math.Pow((double)bounding_ellipse.minor_radius / (double)bounding_ellipse.major_radius, 2));
                        double a = bounding_ellipse.major_radius;
                        double b = bounding_ellipse.minor_radius;
                        curr_bounding_ellipse_circularity = 
                            4 * System.Math.PI * bounding_ellipse.area 
                            / System.Math.Pow(System.Math.PI * (3 * (a + b) - System.Math.Sqrt(10 * a * b + 3 * (a * a + b * b))), 2);
                    }
                    data_per_DAS["eccentricities"][rep_item][int_DAS_per_file_index[imageindex]].Add(curr_eccentricity);
                    data_per_DAS["bounding_ellipse_circularities"][rep_item][int_DAS_per_file_index[imageindex]].Add(
                        curr_bounding_ellipse_circularity);
                    double curr_rotational_mass_asymmetry = 0.0;
                    if (ellipse_2nd_central_moment.major_radius > 0)
                    {
                        curr_rotational_mass_asymmetry = 2 * System.Math.Sqrt(
                            System.Math.Pow(ellipse_2nd_central_moment.major_radius / 2.0, 2)
                            - System.Math.Pow(ellipse_2nd_central_moment.minor_radius / 2.0, 2)) / (ellipse_2nd_central_moment.major_radius);
                    }
                    data_per_DAS["rotational_mass_asymmetries"][rep_item][int_DAS_per_file_index[imageindex]].Add(curr_rotational_mass_asymmetry);

                    // 11. Get equivalent circle of the plant and convex hull
                    List<double> plant_mask_equiv_circle_data = ip.GetEquivalentCircleFromBitmap(Input_Mask_No_Noise_Clone);

                    List<Point> convex_hull_mask_points = 
                        fp.FillContour(convex_hull_contour, Input_Mask_No_Noise_Clone.Size.Width, Input_Mask_No_Noise_Clone.Size.Height);
                    List<double> conv_hull_equiv_circle_data = fp.GetEquivalentCircleFromListOfPoints(convex_hull_mask_points);
                    Circle_Double conv_hull_equivalent_circle = new Circle_Double();
                    conv_hull_equivalent_circle.centre_x = plant_mask_equiv_circle_data[0];
                    conv_hull_equivalent_circle.centre_y = plant_mask_equiv_circle_data[1];
                    conv_hull_equivalent_circle.radius = conv_hull_equiv_circle_data[2];
                    conv_hull_equivalent_circle.area = (double)convex_hull_mask_points.Count;
                    convex_hull_equiv_circles.Add(conv_hull_equivalent_circle);

                    double area_inside_circle = fp.GetAreaInsideCircle(
                        convex_hull_mask_points,
                        conv_hull_equivalent_circle.centre_x,
                        conv_hull_equivalent_circle.centre_y,
                        conv_hull_equivalent_circle.radius);
                    convex_hull_inside_equiv_circles.Add(area_inside_circle);
                    double convex_hull_area_outside_intersection = (double)convex_hull_mask_points.Count - area_inside_circle;
                    double circle_area_outside_convexhull = convex_hull_area_outside_intersection;  // Because the circle has the same area as the convex hull
                    double curr_convex_hull_elongation = (convex_hull_area_outside_intersection + circle_area_outside_convexhull) / area_inside_circle;
                    if (conv_hull_equivalent_circle.area == 0)
                        data_per_DAS["convex_hull_elongations"][rep_item][int_DAS_per_file_index[imageindex]].Add(0.0);  // Just in case
                    else
                        data_per_DAS["convex_hull_elongations"][rep_item][int_DAS_per_file_index[imageindex]].Add(curr_convex_hull_elongation);
                }
            }

            //----------------------------------------------------------------------
            // (all features are extracted and stored into lists for all repetitions of current class)

            // - decide the vector of repetition lengths
            Plots plots = new Plots();
            string plotsfoldername = textBox_Out_FolderName.Text + "\\_Plots";
            string plotfoldername = plotsfoldername + "\\" + classname;

            // decide the used DAS interval (it is started with the earlier image, and ends with the user-given date)
            double DAS_min = 10000;
            for (int i = 0; i < DAS_s.Count; i++)
                DAS_min = System.Math.Min(DAS_min, DAS_s[i]);
            DAS_min = (double)(int)DAS_min; // starting date (day)
            if (DAS_min == 10000)
                return class_data;

            // adjust DAS_min (allowing only increasing)

            DateTime screening_date = dateTimePicker_Screening_Date.Value.Date;

            double DAS_min_user = (screening_date - sowing_date).Days;
            // Alek comment start: it was System.Math.Max but should be System.Math.min, because the days should be start from min value of those, not max
            DAS_min = System.Math.Min(DAS_min, DAS_min_user);
            // Alek comment end

            for (int rep_idx = 0; rep_idx < nbofreps; rep_idx++)
                rep_days_shift[rep_idx] = (int)System.Math.Max(0, rep_DAS_min[rep_idx] - DAS_min);

            DateTime ending_date = dateTimePicker_Capturing_End_Date.Value.Date;

            var totalhours = (ending_date - sowing_date).TotalHours;
            double DAS_max = (double)(int)(totalhours/24.0 +0.5);

            double scaling_factor = Convert.ToDouble(textBox_Calibration_Data.Text);
            int plotimagewidth = Convert.ToInt16(textBox_Plot_Image_Width.Text);
            int plotimageheight = Convert.ToInt16(textBox_Plot_Image_Height.Text);

            //-----------------------------------------------------------------------------------------------------------------------
            // ### create plots for fundamental features ###

            Boolean bSplineInterpolation = false;

            // create plot for 'rosette area'
            string plot_filename = plotfoldername + "\\" + classname + "_rosette_area_feature.png";
            string plot_title = "Rosette Area [mm²]";
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                rosette_areas, plot_colors, scaling_factor * scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'rosette perimeter'
            plot_filename = plotfoldername + "\\" + classname + "_perimeter_feature.png";
            plot_title = "Perimeter [mm]";
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                rosette_perimeters, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'convex hull area'
            plot_filename = plotfoldername + "\\" + classname + "_convex_hull_area_feature.png";
            plot_title = "Convex Hull Area [mm²]";
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_areas, plot_colors, scaling_factor * scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'convex hull perimeter'
            plot_filename = plotfoldername + "\\" + classname + "_convex_hull_perimeter_feature.png";
            plot_title = "Convex Hull Perimeter [mm]";
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_perimeters, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'bounding box area'
            plot_filename = plotfoldername + "\\" + classname + "_bounding_box_area_feature.png";
            plot_title = "Bounding Box Area [mm²]";
            for (int i = 0; i < bounding_boxes.Count; i++)
                bounding_box_areas.Add(bounding_boxes[i].area);
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                bounding_box_areas, plot_colors, scaling_factor * scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'width (diameter)'
            plot_filename = plotfoldername + "\\" + classname + "_width_aka_diameter_feature.png";
            plot_title = "Width (Diameter) [mm]";
            for (int i = 0; i < standard_diameters.Count; i++)
                std_diameters.Add(standard_diameters[i].diameter);
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                std_diameters, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            //-----------------------------------------------------------------------------------------------------------------------
            // ### create plots for derived plant traits ###

            // create plot for 'circumference'
            plot_filename = plotfoldername + "\\" + classname + "_circumference_feature.png";
            plot_title = "Circumference [mm]";
            for (int i = 0; i < bounding_circles.Count; i++)
                circumferences.Add(System.Math.PI * 2.0 * bounding_circles[i].radius);
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                circumferences, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'eccentricity'
            plot_filename = plotfoldername + "\\" + classname + "_eccentricity_feature.png";
            plot_title = "Eccentricity";
            for (int i = 0; i < bounding_ellipses.Count; i++)
            {
                double curr_eccentricity = 0.0;
                if (bounding_ellipses[i].major_radius > 0)
                {
                    curr_eccentricity = System.Math.Sqrt(
                            1 - System.Math.Pow((double)bounding_ellipses[i].minor_radius / (double)bounding_ellipses[i].major_radius, 2));
                }
                eccentricities.Add(curr_eccentricity);
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                eccentricities, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);


            // create plot for 'area_to_perimeter_ratio'
            plot_filename = plotfoldername + "\\" + classname + "_area_to_perimeter_ratio_feature.png";
            plot_title = "Area-to-Perimeter Ratio [mm]";
            for (int i = 0; i < rosette_areas.Count; i++)
            {
                if (rosette_perimeters[i]==0)
                    area_to_perimeter_ratios.Add((i == 0) ? 0 : area_to_perimeter_ratios[i - 1]);
                else
                    area_to_perimeter_ratios.Add(rosette_areas[i] / rosette_perimeters[i]);
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                area_to_perimeter_ratios, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'compactness'
            plot_filename = plotfoldername + "\\" + classname + "_compactness_feature.png";
            plot_title = "Compactness";
            for (int i = 0; i < rosette_areas.Count; i++)
            {
                if (convex_hull_areas[i]==0)
                    compactnesses.Add((i == 0) ? 0 : compactnesses[i - 1]);
                else
                    compactnesses.Add(rosette_areas[i] / convex_hull_areas[i]);
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                compactnesses, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'extent'
            plot_filename = plotfoldername + "\\" + classname + "_extent_feature.png";
            plot_title = "Extent";  // rectangularity
            for (int i = 0; i < rosette_areas.Count; i++)
            {
                if (bounding_boxes[i].area==0)
                    extents.Add((i == 0) ? 0 : extents[i - 1]);
                else
                    extents.Add(rosette_areas[i] / bounding_boxes[i].area);
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                extents, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'plant roundness'
            plot_filename = plotfoldername + "\\" + classname + "_plant_roundness_feature.png";
            plot_title = "Roundness";
            for (int i = 0; i < rosette_areas.Count; i++)
            {
                if (rosette_perimeters[i] == 0)
                    plant_roundnesses.Add((i == 0) ? 0 : plant_roundnesses[i - 1]);
                else
                    plant_roundnesses.Add(4.0 * System.Math.PI * rosette_areas[i] / System.Math.Pow(rosette_perimeters[i], 2.0));
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                plant_roundnesses, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'surface coverage'
            plot_filename = plotfoldername + "\\" + classname + "_surface_coverage_feature.png";
            plot_title = "Surface Coverage";
            for (int i = 0; i < rosette_areas.Count; i++)
            {
                if (bounding_circles[i].area==0)
                    surface_coverages.Add((i == 0) ? 0 : surface_coverages[i - 1]);
                else
                    surface_coverages.Add(rosette_areas[i] / bounding_circles[i].area);
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                surface_coverages, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'bounding box aspect ratio'
            for (int i = 0; i < bounding_boxes.Count; i++)
            {
                double side1 = System.Math.Sqrt(System.Math.Pow(bounding_boxes[i].p2.X - bounding_boxes[i].p1.X, 2.0) +
                    System.Math.Pow(bounding_boxes[i].p2.Y - bounding_boxes[i].p1.Y, 2.0));
                double side2 = System.Math.Sqrt(System.Math.Pow(bounding_boxes[i].p3.X - bounding_boxes[i].p2.X, 2.0) +
                    System.Math.Pow(bounding_boxes[i].p3.Y - bounding_boxes[i].p2.Y, 2.0));
                double aspect_ratio = 0;
                if (side1 > 0 && side2 > 0)
                    aspect_ratio = side1 < side2 ? (side1 / side2) : (side2 / side1);
                if (aspect_ratio == 0)
                    bounding_box_aspect_ratios.Add((i == 0) ? 0 : bounding_box_aspect_ratios[i - 1]);
                else
                    bounding_box_aspect_ratios.Add(aspect_ratio);
            }

            // create plot for 'rotational mass asymmetry'
            plot_filename = plotfoldername + "\\" + classname + "_rotational_mass_asymmetry_feature.png";
            plot_title = "Rotational Mass Asymmetry";
            for (int i = 0; i < ellipses_2nd_central_moment.Count; i++)
            {
                double curr_rotational_mass_asymmetry = 0.0;
                if (ellipses_2nd_central_moment[i].major_radius > 0)
                {
                    curr_rotational_mass_asymmetry = 2 * System.Math.Sqrt(
                        System.Math.Pow((ellipses_2nd_central_moment[i].major_radius) / 2.0, 2) 
                        - System.Math.Pow((ellipses_2nd_central_moment[i].minor_radius) / 2.0, 2)) / (ellipses_2nd_central_moment[i].major_radius);
                }
                rotational_mass_asymmetries.Add(curr_rotational_mass_asymmetry);
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                rotational_mass_asymmetries, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);


            // create plot for 'convex hull roundness'
            plot_filename = plotfoldername + "\\" + classname + "_convex_hull_roundness_feature.png";
            plot_title = "Convex Hull Roundness";
            for (int i = 0; i < convex_hull_areas.Count; i++)
            {
                if (convex_hull_perimeters[i] == 0)
                    convex_hull_roundnesses.Add((i == 0) ? 0 : convex_hull_roundnesses[i - 1]);
                else
                    convex_hull_roundnesses.Add(4 * System.Math.PI * convex_hull_areas[i] / System.Math.Pow(convex_hull_perimeters[i], 2));
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_roundnesses, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);


            // create plot for 'convex hull elongation'
            plot_filename = plotfoldername + "\\" + classname + "_convex_hull_elongation_feature.png";
            plot_title = "Convex Hull Elongation";
            for (int i = 0; i < convex_hull_equiv_circles.Count; i++)
            {
                if (convex_hull_equiv_circles[i].area == 0)
                    convex_hull_elongations.Add((i == 0) ? 0 : convex_hull_elongations[i - 1]);
                else
                    convex_hull_elongations.Add(2 * (convex_hull_equiv_circles[i].area - convex_hull_inside_equiv_circles[i]) / convex_hull_inside_equiv_circles[i]);
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(
                classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_elongations, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);


            // create plot for 'bounding ellipse circularity'
            plot_filename = plotfoldername + "\\" + classname + "_bounding_ellipse_circularity_feature.png";
            plot_title = "Bounding Ellipse Circularity";
            for (int i = 0; i < bounding_ellipses.Count; i++)
            {
                if (bounding_ellipses[i].area == 0)
                    bounding_ellipse_circularities.Add((i == 0) ? 0 : bounding_ellipse_circularities[i - 1]);
                else
                {
                    double a = bounding_ellipses[i].major_radius;
                    double b = bounding_ellipses[i].minor_radius;
                    bounding_ellipse_circularities.Add(
                        4 * System.Math.PI * bounding_ellipses[i].area
                        / System.Math.Pow(System.Math.PI * (3 * (a + b) - System.Math.Sqrt(10 * a * b + 3 * (a * a + b * b))), 2));
                }
            }
            plots.Create_Plot_Of_DAS_Avg__Oxyplot_2(classname, plot_filename, plot_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                bounding_ellipse_circularities, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            //----------------------------------------------------------------------
            // ### create HTML plots for fundamental features ###

            string plotshtmlfoldername = textBox_Out_FolderName.Text + "\\_PlotsHtml";
            string plothtmlfoldername = plotshtmlfoldername + "\\" + classname;

            // create HTML plot for 'rosette area'
            string plothtml_filename = plothtmlfoldername + "\\" + classname + "_rosette_area_feature";
            string plothtml_title = "Rosette Area [mm²]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                rosette_areas, plot_colors, scaling_factor * scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'rosette perimeter'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_perimeter_feature";
            plothtml_title = "Perimeter [mm]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                rosette_perimeters, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'convex hull area'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_convex_hull_area_feature";
            plothtml_title = "Convex Hull Area [mm²]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_areas, plot_colors, scaling_factor * scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'convex hull perimeter'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_convex_hull_perimeter_feature";
            plothtml_title = "Convex Hull Perimeter [mm]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_perimeters, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'bounding box area'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_bounding_box_area_feature";
            plothtml_title = "Bounding Box Area [mm²]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                bounding_box_areas, plot_colors, scaling_factor * scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'width (diameter)'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_width_aka_diameter_feature";
            plothtml_title = "Width (Diameter) [mm]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                std_diameters, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // ### create HTML plots for derived plant traits ###

            // create plot for 'circumference'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_circumference_feature";
            plothtml_title = "Circumference [mm]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                circumferences, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'eccentricity'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_eccentricity_feature";
            plothtml_title = "Eccentricity";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                eccentricities, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'area_to_perimeter_ratio'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_area_to_perimeter_ratio_feature";
            plothtml_title = "Area-to-Perimeter Ratio [mm]";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                area_to_perimeter_ratios, plot_colors, scaling_factor, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'compactness'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_compactness_feature";
            plothtml_title = "Compactness";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                compactnesses, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'extent'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_extent_feature";
            plothtml_title = "Extent";  // rectangularity
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                extents, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'plant roundness'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_plant_roundness_feature";
            plothtml_title = "Roundness";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                plant_roundnesses, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'surface coverage'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_surface_coverage_feature";
            plothtml_title = "Surface Coverage";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                surface_coverages, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'rotational mass asymmetry'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_rotational_mass_asymmetry_feature";
            plothtml_title = "Rotational Mass Asymmetry";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                rotational_mass_asymmetries, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'convex_hull_roundness'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_convex_hull_roundness_feature";
            plothtml_title = "Convex Hull Roundness";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_roundnesses, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'convex_hull_elongation'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_convex_hull_elongation_feature";
            plothtml_title = "Convex Hull Elongation";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                convex_hull_elongations, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);

            // create plot for 'bounding_ellipse_circularity'
            plothtml_filename = plothtmlfoldername + "\\" + classname + "_bounding_ellipse_circularity_feature";
            plothtml_title = "Bounding Ellipse Circularity";
            plots.Create_PlotHtml(classname, plothtml_filename, plothtml_title, rep_serialnumbers, rep_lengths, rep_days_shift, DAS_s,
                bounding_ellipse_circularities, plot_colors, 1.0, plotimagewidth, plotimageheight, bSplineInterpolation);


            //----------------------------------------------------------------------
            // ### create bars ###
            Bars bar = new PlantInspector.Bars();
            string bars_foldername = textBox_Out_FolderName.Text + "\\_Bars";
            string bar_class_folder = bars_foldername + "\\" + classname + "\\";
            string bar_filename = "";
            int barimagewidth = Convert.ToUInt16(textBox_Plot_Image_Width.Text);
            int barimageheight = Convert.ToUInt16(textBox_Plot_Image_Height.Text);

            // extract average values in all reps for all values
            int start_index = 0;
            int nb_of_days = (int)DAS_max - (int)DAS_min + 1;
            if (nb_of_days < 0)
                return class_data;
            int[] rep_data_nb = new int[nb_of_days];
            int[] class_data_nb = new int[nb_of_days];
            double[] plot_area_rep_avgs = new double[nb_of_days];
            double[] convex_hull_area_rep_avgs = new double[nb_of_days];

            double[] rosette_area_rep_growth_rates = new double[nb_of_days - 1];
            double[] convex_hull_area_rep_growth_rates = new double[nb_of_days - 1];
            double[] rosette_area_class_growth_rates = new double[nb_of_days - 1];
            double[] convex_hull_area_class_growth_rates = new double[nb_of_days - 1];
            double[] rosette_area_class_growth_rate_avgs = new double[nb_of_days - 1];
            double[] convex_hull_area_class_growth_rate_avgs = new double[nb_of_days - 1];
            int[] class_rosette_growth_nb = new int[nb_of_days - 1];
            int[] class_convex_hull_growth_nb = new int[nb_of_days - 1];

            List<string> date_str_shifted = new List<string>();

            for (int k = 0; k < nb_of_days; k++)
            {
                if (k < nb_of_days - 1)
                {
                    rosette_area_class_growth_rate_avgs[k] = 0.0;
                    convex_hull_area_class_growth_rate_avgs[k] = 0.0;
                    class_data_nb[k] = 0;
                    rosette_area_class_growth_rates[k] = 0.0;
                    convex_hull_area_class_growth_rates[k] = 0.0;
                    class_rosette_growth_nb[k] = 0;
                    class_convex_hull_growth_nb[k] = 0;
                }
            }

            for ( int i_rep=0; i_rep< nbofreps; i_rep++)
            {
                if (selected_Reps[i_rep] == false || rep_lengths[i_rep]<=0)
                    continue;

                int rep_length = rep_lengths[i_rep];
                int rep_DAS_max = (int)DAS_s[start_index + rep_length - 1];
                for (int k = 0; k < nb_of_days; k++)
                {
                    plot_area_rep_avgs[k] = 0.0;
                    convex_hull_area_rep_avgs[k] = 0.0;
                    rep_data_nb[k] = 0;
                    if (k < nb_of_days - 1)
                    {
                        rosette_area_rep_growth_rates[k] = 0.0;
                        convex_hull_area_rep_growth_rates[k] = 0.0;
                    }
                }

                for (int j_file = start_index; j_file < (start_index + rep_length); j_file++)
                {
                    int day_index = (int)DAS_s[j_file] - (int)rep_DAS_min[i_rep];
                    plot_area_rep_avgs[day_index] += rosette_areas[j_file];
                    convex_hull_area_rep_avgs[day_index] += convex_hull_areas[j_file];
                    rep_data_nb[day_index]++;

                    DateTime date_shifted = DateTime.ParseExact(
                        date_str[j_file],
                        "dd/MM/yyyy",
                        System.Globalization.CultureInfo.InvariantCulture).AddDays(-rep_days_shift[i_rep]);
                    date_str_shifted.Add(date_shifted.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture));
                }
                for ( int m=0; m< nb_of_days; m++)
                {
                    if (rep_data_nb[m] > 1)
                    {
                        plot_area_rep_avgs[m] /= rep_data_nb[m];
                        convex_hull_area_rep_avgs[m] /= rep_data_nb[m];
                    }
                    if (m > 0)
                    {
                        if ((plot_area_rep_avgs[m - 1] != 0) && (plot_area_rep_avgs[m] != 0))
                        {
                            rosette_area_rep_growth_rates[m - 1] = 
                                (plot_area_rep_avgs[m] - plot_area_rep_avgs[m - 1]) / plot_area_rep_avgs[m - 1];
                            rosette_area_class_growth_rates[m - 1] += rosette_area_rep_growth_rates[m - 1];
                            class_rosette_growth_nb[m - 1]++;
                        }
                        if ((convex_hull_area_rep_avgs[m-1] != 0) && (convex_hull_area_rep_avgs[m] != 0))
                        {
                            convex_hull_area_rep_growth_rates[m - 1] = 
                                (convex_hull_area_rep_avgs[m] - convex_hull_area_rep_avgs[m - 1]) / convex_hull_area_rep_avgs[m - 1];
                            convex_hull_area_class_growth_rates[m - 1] += convex_hull_area_rep_growth_rates[m - 1];
                            class_convex_hull_growth_nb[m - 1]++;
                        }
                    }
                }

                string rep_id = "";

                //Invoke in UI thread
                listView_Reps.Invoke((MethodInvoker)delegate
                {
                    rep_id = listView_Reps.Items[i_rep].Text;
                });

                // Create sub-folder for current replicate
                string bar_rep_folder = bar_class_folder + rep_id + "\\";
                if (!Directory.Exists(bar_rep_folder))
                    Directory.CreateDirectory(bar_rep_folder);

                // create 'rosette area growth' bar for current rep.
                bar_filename = bar_rep_folder + classname + '_' + rep_id + "_area_growth_rate";
                bar.Create_Growth_Rate_Bar__Oxyplot(classname + " - " + rep_id, bar_filename, "Days After Sowing", "Rosette Area Growth Rate",
                    (int)rep_DAS_min[i_rep] - rep_days_shift[i_rep] + 1, rep_DAS_max - rep_days_shift[i_rep],
                    rosette_area_rep_growth_rates, barimagewidth, barimageheight);

                //// create 'convex hull area growth' bar for current rep.
                bar_filename = bar_rep_folder + classname + '_' + rep_id + "_convex_hull_growth_rate";
                bar.Create_Growth_Rate_Bar__Oxyplot(classname + " - " + rep_id, bar_filename, "Days After Sowing", "Convex Hull Area Growth Rate",
                    (int)rep_DAS_min[i_rep] - rep_days_shift[i_rep] + 1, rep_DAS_max - rep_days_shift[i_rep],
                    convex_hull_area_rep_growth_rates, barimagewidth, barimageheight);

                // accumulate the current rep's data for class bar
                for (int k = 0; k < nb_of_days - 1; k++)
                {
                    if ((plot_area_rep_avgs[k] != 0) && (plot_area_rep_avgs[k + 1] != 0))
                    {
                        rosette_area_class_growth_rate_avgs[k] += rosette_area_rep_growth_rates[k];
                        convex_hull_area_class_growth_rate_avgs[k] += convex_hull_area_rep_growth_rates[k];
                        class_data_nb[k]++;
                    }
                }
                start_index += rep_length;
            }

            for (int m = 0; m < nb_of_days; m++)
            {
                if (class_data_nb[m] > 1)
                {
                    rosette_area_class_growth_rate_avgs[m] /= class_data_nb[m];
                    convex_hull_area_class_growth_rate_avgs[m] /= class_data_nb[m];
                }
            }


            // create 'rosette area growth' bar for current class
            bar_filename = bar_class_folder + classname + '_' + "area_growth_rate";
            bar.Create_Growth_Rate_Bar__Oxyplot(classname, bar_filename, "Days After Sowing", "Rosette Area Growth Rate",
                (int)DAS_min + 1, (int)DAS_max, rosette_area_class_growth_rate_avgs, barimagewidth, barimageheight);

            // create 'convex hull area growth' bar for current class
            bar_filename = bar_class_folder + classname + '_' + "convex_hull_growth_rate";
            bar.Create_Growth_Rate_Bar__Oxyplot(classname, bar_filename, "Days After Sowing", "Convex Hull Area Growth Rate",
                (int)DAS_min + 1, (int)DAS_max, convex_hull_area_class_growth_rate_avgs, barimagewidth, barimageheight);

            //----------------------------------------------------------------------
            // ### create Excel file for current class ###
            string excelsfoldername = textBox_Out_FolderName.Text + "\\_Excels";
            string excelfoldername = excelsfoldername + "\\" + classname;
            string excel_filename = excelfoldername + "\\" + classname + "_extracted_features.xlsx";

            //// add class data
            int n_digits = 3;
            string float_format = string.Format("F{0}", n_digits);
            List<string> class_name_per_row = new List<string>();
            List<string> rep_sn_per_row = new List<string>();
            for (int i = 0; i < rep_serialnumbers.Count; i++)
            {
                int rep_sn = rep_serialnumbers[i];
                int rep_length = rep_lengths[i];
                for (int j = start_index; j < (start_index + rep_length); j++)
                {
                    class_name_per_row.Add(classname);
                    rep_sn_per_row.Add(Convert.ToString(rep_sn));
                }
                start_index += rep_length;
            }

            class_data = Excels_EPP.Prepare_Class_Data_for_Export_to_Excel(
                scaling_factor, n_digits, float_format,
                class_name_per_row, rep_sn_per_row,
                date_str_shifted, time_str,
                rosette_areas, area_to_perimeter_ratios,
                bounding_box_areas, bounding_box_aspect_ratios, bounding_ellipse_circularities,
                circumferences, compactnesses,
                convex_hull_areas, convex_hull_elongations, convex_hull_roundnesses,
                std_diameters, eccentricities, extents,
                rosette_perimeters, plant_roundnesses,
                rotational_mass_asymmetries, surface_coverages);
            Excels_EPP.Write_to_Excel_File(excel_filename, class_data);

            //----------------------------------------------------------------------
            // ### save (serialize) the content of those lists of extracted features, that can be visualized as overlay ###
            SaveLists( classname );

            SaveGlobalParameters();

            return class_data;
        }

        private void SaveLists(string classname)
        {
            //string listsfoldername = textBox_Root_FolderName.Text + "\\_Saved_Lists";
            string listsfoldername = textBox_Out_FolderName.Text + "\\_Saved_Lists";
            string listfoldername = listsfoldername + "\\" + classname;
            string list_filename = listfoldername + "\\" + classname + "_lists.txt";

            int ser_size = 0;
            int rep_number = listView_Reps.Items.Count;
            ser_size += 2*rep_number+1;                     // rep_number, list of serial numbers, list of rep lengths
            int item_number = 0;
            for (int i = 0; i < rep_number; i++)
                item_number += System.Math.Max(0, rep_lengths[i]);
            for (int i = 0; i < rosette_contours.Count; i++)           // list of rosette_contours
                ser_size += 1 + 2 * rosette_contours[i].Count;
            for (int i = 0; i < convex_hull_contours.Count; i++)           // list of convex hull contours
                ser_size += 1 + 2 * convex_hull_contours[i].Count;
            ser_size += 5 * item_number;                    // list of standard_diameters
            ser_size += 9 * item_number;                    // list of bounding_boxes
            ser_size += 4 * item_number;                    // list of bounding_circles
            ser_size += 6 * item_number;                    // list of bounding_ellipses
            ser_size += 4 * item_number;                    // list of convex_hull_equiv_circles
            double[] listdata = new double[ser_size];

            int index = 0;
            listdata[index++] = rep_number;
            for (int i = 0; i < rep_number; i++)
            {
                listdata[index++] = rep_serialnumbers[i];
            }
            for (int i = 0; i < rep_number; i++)
            {
                listdata[index++] = rep_lengths[i];
            }

            for (int i = 0; i < item_number; i++)
            {
                listdata[index++] = rosette_contours[i].Count;
                for (int j = 0; j < rosette_contours[i].Count; j++)
                {
                    listdata[index++] = rosette_contours[i][j].X;
                    listdata[index++] = rosette_contours[i][j].Y;
                }
            }

            for (int i = 0; i < item_number; i++)
            {
                listdata[index++] = convex_hull_contours[i].Count;
                for (int j = 0; j < convex_hull_contours[i].Count; j++)
                {
                    listdata[index++] = convex_hull_contours[i][j].X;
                    listdata[index++] = convex_hull_contours[i][j].Y;
                }
            }

            for (int i = 0; i < item_number; i++)
            {
                listdata[index++] = standard_diameters[i].diameter;
                listdata[index++] = standard_diameters[i].x1;
                listdata[index++] = standard_diameters[i].y1;
                listdata[index++] = standard_diameters[i].x2;
                listdata[index++] = standard_diameters[i].y2;
            }

            for (int i = 0; i < item_number; i++)
            {
                listdata[index++] = bounding_boxes[i].area;
                listdata[index++] = bounding_boxes[i].p1.X;
                listdata[index++] = bounding_boxes[i].p1.Y;
                listdata[index++] = bounding_boxes[i].p2.X;
                listdata[index++] = bounding_boxes[i].p2.Y;
                listdata[index++] = bounding_boxes[i].p3.X;
                listdata[index++] = bounding_boxes[i].p3.Y;
                listdata[index++] = bounding_boxes[i].p4.X;
                listdata[index++] = bounding_boxes[i].p4.Y;
            }

            for (int i = 0; i < item_number; i++)
            {
                listdata[index++] = bounding_circles[i].area;
                listdata[index++] = bounding_circles[i].centre.X;
                listdata[index++] = bounding_circles[i].centre.Y;
                listdata[index++] = bounding_circles[i].radius;
            }

            for (int i = 0; i < item_number; i++)
            {
                listdata[index++] = bounding_ellipses[i].area;
                listdata[index++] = bounding_ellipses[i].centre.X;
                listdata[index++] = bounding_ellipses[i].centre.Y;
                listdata[index++] = bounding_ellipses[i].major_radius;
                listdata[index++] = bounding_ellipses[i].minor_radius;
                listdata[index++] = bounding_ellipses[i].angle_major_radius;
            }

            for (int i = 0; i < item_number; i++)
            {
                listdata[index++] = convex_hull_equiv_circles[i].area;
                listdata[index++] = convex_hull_equiv_circles[i].centre_x;
                listdata[index++] = convex_hull_equiv_circles[i].centre_y;
                listdata[index++] = convex_hull_equiv_circles[i].radius;
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(list_filename, FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, listdata);
            stream.Close();
            stream.Dispose();
        }

        private void ReloadLists(string classname)
        {
            if (textBox_Out_FolderName.Text == "")
                return;

            string listsfoldername = textBox_Out_FolderName.Text + "\\_Saved_Lists";
            string listfoldername = listsfoldername + "\\" + classname;
            string list_filename = listfoldername + "\\" + classname + "_lists.txt";
            if (!System.IO.File.Exists(list_filename))
                return;

            Stream stream = new FileStream(list_filename, FileMode.Open, FileAccess.Read);
            IFormatter formatter = new BinaryFormatter();
            double[] listdata = (double[])formatter.Deserialize(stream);
            stream.Close();
            stream.Dispose();
            int index = 0;
            int rep_number = (int)listdata[index++];

            // list of serial numbers
            for (int i = 0; i < rep_number; i++)
            {
                rep_serialnumbers.Add((int)listdata[index++]);
            }

            // list of rep lengths
            int item_number = 0;
            for (int i = 0; i < rep_number; i++)
            {
                rep_lengths.Add((int)listdata[index++]);
                item_number += System.Math.Max(0, rep_lengths[i]);
            }

            // list of plant contours
            for (int i = 0; i < item_number; i++)
            {
                rosette_contours.Add(new List<Point>());
                int nbofpoints = (int)listdata[index++];
                for (int j = 0; j < nbofpoints; j++)
                {
                    Point point = new Point();
                    point.X = (int)listdata[index++];
                    point.Y = (int)listdata[index++];
                    rosette_contours[i].Add(point);
                }
            }

            // list of convex hulls
            for (int i = 0; i < item_number; i++)
            {
                convex_hull_contours.Add(new List<Point>());
                int nbofpoints = (int)listdata[index++];
                for (int j = 0; j < nbofpoints; j++)
                {
                    Point point = new Point();
                    point.X = (int)listdata[index++];
                    point.Y = (int)listdata[index++];
                    convex_hull_contours[i].Add(point);
                }
            }

            // list of standard diameters
            for (int i = 0; i < item_number; i++)
            {
                Standard_Diameter std_diameter = new Standard_Diameter();
                std_diameter.diameter = listdata[index++];
                std_diameter.x1 = (int)listdata[index++];
                std_diameter.y1 = (int)listdata[index++];
                std_diameter.x2 = (int)listdata[index++];
                std_diameter.y2 = (int)listdata[index++];
                standard_diameters.Add(std_diameter);
            }

            // list of bounding boxes
            for (int i = 0; i < item_number; i++)
            {
                Bounding_Box bounding_box = new Bounding_Box();
                bounding_box.area = listdata[index++];
                bounding_box.p1.X = (int)listdata[index++];
                bounding_box.p1.Y = (int)listdata[index++];
                bounding_box.p2.X = (int)listdata[index++];
                bounding_box.p2.Y = (int)listdata[index++];
                bounding_box.p3.X = (int)listdata[index++];
                bounding_box.p3.Y = (int)listdata[index++];
                bounding_box.p4.X = (int)listdata[index++];
                bounding_box.p4.Y = (int)listdata[index++];
                bounding_boxes.Add(bounding_box);
            }

            // list of bounding circles
            for (int i = 0; i < item_number; i++)
            {
                Circle_Int bounding_circle = new Circle_Int();
                bounding_circle.area = listdata[index++];
                bounding_circle.centre.X = (int)listdata[index++];
                bounding_circle.centre.Y = (int)listdata[index++];
                bounding_circle.radius = (int)listdata[index++];
                bounding_circles.Add(bounding_circle);
            }

            // list of bounding ellipses
            for (int i = 0; i < item_number; i++)
            {
                Ellipse_Int bounding_ellipse = new Ellipse_Int();
                bounding_ellipse.area = listdata[index++];
                bounding_ellipse.centre.X = (int)listdata[index++];
                bounding_ellipse.centre.Y = (int)listdata[index++];
                bounding_ellipse.major_radius = (int)listdata[index++];
                bounding_ellipse.minor_radius = (int)listdata[index++];
                bounding_ellipse.angle_major_radius = listdata[index++];
                bounding_ellipses.Add(bounding_ellipse);
            }

            // list of convex hull equivalent circles
            for (int i = 0; i < item_number; i++)
            {
                Circle_Double convex_hull_equiv_circle = new Circle_Double();
                convex_hull_equiv_circle.area = listdata[index++];
                convex_hull_equiv_circle.centre_x = (int)listdata[index++];
                convex_hull_equiv_circle.centre_y = (int)listdata[index++];
                convex_hull_equiv_circle.radius = (int)listdata[index++];
                convex_hull_equiv_circles.Add(convex_hull_equiv_circle);
            }
        }


        private void button_Reset_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to reset all GUI parameters?",
                      "Reset Warning", MessageBoxButtons.YesNo);
            if (dr == DialogResult.No)
                return;

            textBox_Out_FolderName.ResetText();
            textBox_Root_FolderName.ResetText();

            // Uncheck checkboxes
            checkBox_Show_Rosette_Area.Checked = false;
            checkBox_Show_Convex_Hull.Checked = false;
            checkBox_Show_Standard_Diameter.Checked = false;
            checkBox_Show_Bounding_Box.Checked = false;
            checkBox_Show_Bounding_Circle.Checked = false;
            checkBox_Show_Bounding_Ellipse.Checked = false;
            checkBox_Show_CHull_Equiv_Circle.Checked = false;

            SetDataset();

            textBox_Plot_Image_Width.Text = Convert.ToString(DEFAULT_PLOT_WIDTH);
            textBox_Plot_Image_Height.Text = Convert.ToString(DEFAULT_PLOT_HEIGHT);
        }

        //----------------------------------------------------------------------------------

        private void CreateOutputDirectories( string subfoldername)
        {
            string foldername = textBox_Out_FolderName.Text + subfoldername;
            if (!Directory.Exists(foldername))
                Directory.CreateDirectory(foldername);
            int nbofclasses = listBox_Classes.Items.Count;
            for (int i = 0; i < nbofclasses; i++)
            {
                string classfoldername = foldername + "\\" + listBox_Classes.Items[i];
                if (!Directory.Exists(classfoldername))
                    Directory.CreateDirectory(classfoldername);
            }
        }

        private void GetPlotColors()
        {
            List<String> html_colors = new List<String>() {
                "#000000", "#00ff00", "#af02f5", "#f46b02", "#079cbe", "#00139f", "#4b7a22", "#a70136",
                "#6ffe7c", "#ae73b1", "#00ffff", "#fe258b", "#92ca0b", "#e9b45a", "#4051fc", "#6bc7ff",
                "#ff80ff", "#01d562", "#5f3764", "#0000ff", "#6ca974", "#ff0000", "#aa6f41", "#763901",
                "#063e35", "#5a01c9", "#047175", "#0cb301", "#afc7a9", "#e93ae8", "#26e8b6", "#58fc20",
                "#a0229d", "#fb6e67", "#8080ff", "#4d6fb1", "#9740fe", "#ff00ff", "#0d0152", "#cc3819",
                "#eeae01", "#0080ff", "#bf9ffb", "#13c2f8", "#049842" };

            plot_colors.Clear();
            foreach (String html_color in html_colors)
                plot_colors.Add(System.Drawing.ColorTranslator.FromHtml(html_color));
        }

        private void tabControl_Show_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( tabControl_Show.SelectedIndex==3)
            {
                pictureBox_Source_Image.Hide();
                listView_Show_Data.Show();
                listView_Show_Data.Visible = true;
            }
            else
            {
                listView_Show_Data.Hide();
                if (tabControl_Show.SelectedIndex == 0)
                    pictureBox_Source_Image.Width = groupBox_Show_Features.Left - 5 - pictureBox_Source_Image.Left;
                else
                    pictureBox_Source_Image.Width = listView_Show_Data.Width;
                pictureBox_Source_Image.Show();
            }
            SetPictureBoxImage();
        }

        private void comboBox_Show_Plot_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetPictureBoxImage();
        }


        private void button_Interactive_Plot_Click(object sender, EventArgs e)
        {
            if (Input_Mask_Bitmap == null)
                return;

            if (textBox_Root_FolderName.Text == "")
            {
                return;
            }

            if (textBox_Out_FolderName.Text == "")
            {
                return;
            }
            
            string classname = (string)listBox_Classes.SelectedItem;
            string plotshtml_foldername = textBox_Out_FolderName.Text + "\\_PlotsHtml" + "\\" + classname;
            if (!Directory.Exists(plotshtml_foldername))
            {
                System.Windows.Forms.MessageBox.Show($"Output folder \"{plotshtml_foldername}\" does not exist.");
                return;
            }

            var name_dict = new Dictionary<string, string>
            {
                { "Rosette Area", "rosette_area_feature" },
                { "Perimeter", "perimeter_feature" },
                { "Convex Hull Area", "convex_hull_area_feature" },
                { "Convex Hull Perimeter", "convex_hull_perimeter_feature" },
                { "Convex Hull Roundness", "convex_hull_roundness_feature" },
                { "Convex Hull Elongation", "convex_hull_elongation_feature" },
                { "Roundness", "plant_roundness_feature" },
                { "Circumference", "circumference_feature" },
                { "Eccentricity", "eccentricity_feature" },
                { "Compactness", "compactness_feature" },
                { "Bounding Box Area", "bounding_box_area_feature" },
                { "Width (Diameter)", "width_aka_diameter_feature" },
                { "Area-to-Perimeter Ratio", "area_to_perimeter_ratio_feature" },
                { "Extent", "extent_feature" },
                { "Surface Coverage", "surface_coverage_feature" },
                { "Rotational Mass Asymmetry", "rotational_mass_asymmetry_feature" },
                { "Bounding Ellipse Circularity", "bounding_ellipse_circularity_feature" },
            };
            string plotshtml_filename = classname + "_" + name_dict[(string)comboBox_Show_Plot.SelectedItem] + ".html";
            string plotshtml_full_path = plotshtml_foldername + "\\" + plotshtml_filename;
            if (!System.IO.File.Exists(plotshtml_full_path))
            {
                System.Windows.Forms.MessageBox.Show($"File \"{plotshtml_full_path}\" does not exist.");
                return;
            }

            var uri_address = new Uri(String.Format("file:///{0}", plotshtml_full_path));
            System.Diagnostics.Process.Start(uri_address.ToString());
        }

        private void radioButton_Bar_Growth_CheckedChanged(object sender, EventArgs e)
        {
            SetPictureBoxImage();
        }

        private void radioButton_Show_Bar_CheckedChanged(object sender, EventArgs e)
        {
            SetPictureBoxImage();
        }

        private void SetPictureBoxImage()
        {
            int selectedShowItem = tabControl_Show.SelectedIndex;
            string classname = (string)listBox_Classes.SelectedItem;
            string plotfilename = textBox_Out_FolderName.Text + "\\_Plots\\" + classname + "\\" + classname + '_';
            string barfilename = textBox_Out_FolderName.Text + "\\_Bars\\";
            string excelfilename = textBox_Out_FolderName.Text + "\\_Excels\\";

            switch (selectedShowItem)
            {
                case 0:
                default:
                    // 'Image' is selected
                    if (pictureBox_Source_Image.Controls.Count > 0)
                        pictureBox_Source_Image.Controls.RemoveAt(pictureBox_Source_Image.Controls.Count - 1);
                    if (radioButton_Show_Image_None.Checked)
                    {
                        pictureBox_Source_Image.Image = Input_Empty_Bitmap;
                    }
                    else if (radioButton_Show_Image_Segmented.Checked)
                        pictureBox_Source_Image.Image = Input_Segmented_Bitmap;
                    else
                        pictureBox_Source_Image.Image = Input_Mask_Bitmap;
                    break;
                case 1:
                    // 'Plot' is selected
                    if (textBox_Out_FolderName.Text == "")
                        pictureBox_Source_Image.Image = null;
                    else
                    {
                        switch (comboBox_Show_Plot.SelectedIndex)
                        {
                            case 0:
                                plotfilename += "rosette_area_feature";
                                break;
                            case 1:
                                plotfilename += "perimeter_feature";
                                break;
                            case 2:
                                plotfilename += "convex_hull_area_feature";
                                break;
                            case 3:
                                plotfilename += "convex_hull_perimeter_feature";
                                break;
                            case 4:
                                plotfilename += "convex_hull_roundness_feature";
                                break;
                            case 5:
                                plotfilename += "convex_hull_elongation_feature";
                                break;
                            case 6:
                                plotfilename += "plant_roundness_feature";
                                break;
                            case 7:
                                plotfilename += "circumference_feature";
                                break;
                            case 8:
                                plotfilename += "eccentricity_feature";
                                break;
                            case 9:
                                plotfilename += "compactness_feature";
                                break;
                            case 10:
                                plotfilename += "bounding_box_area_feature";
                                break;
                            case 11:
                                plotfilename += "width_aka_diameter_feature";
                                break;
                            case 12:
                                plotfilename += "area_to_perimeter_ratio_feature";
                                break;
                            case 13:
                                plotfilename += "extent_feature";
                                break;
                            case 14:
                                plotfilename += "surface_coverage_feature";
                                break;
                            case 15:
                                plotfilename += "rotational_mass_asymmetry_feature";
                                break;
                            case 16:
                                plotfilename += "bounding_ellipse_circularity_feature";
                                break;
                        }
                        plotfilename += ".png";
                        if (!System.IO.File.Exists(plotfilename))
                            pictureBox_Source_Image.Image = null;
                        else
                        {
                            Bitmap tmp_bitmap = new Bitmap(plotfilename);
                            Image image = new Bitmap(tmp_bitmap);
                            pictureBox_Source_Image.Image = image;
                            tmp_bitmap.Dispose();
                        }
                    }
                    break;
                case 2:
                    // 'Bar' is selected
                    if ((listView_Reps.Items.Count == 0) || (textBox_Out_FolderName.Text == ""))
                    {
                        pictureBox_Source_Image.Image = null;
                        return;
                    }
                    ListView.SelectedListViewItemCollection selected = listView_Reps.SelectedItems;
                    string rep_item = selected.Count>0 ? listView_Reps.SelectedItems[0].Text : "";
                    if (rep_item == "")
                        return;
                    if (pictureBox_Source_Image.Controls.Count > 0)
                        pictureBox_Source_Image.Controls.RemoveAt(pictureBox_Source_Image.Controls.Count - 1);
                    if (radioButton_Area_Growth_Rate.Checked)
                    {
                        if (radioButton_Show_Bar_Class.Checked)
                            barfilename += classname + "\\" + classname + "_area_growth_rate";
                        else
                            barfilename += classname + "\\" + rep_item + "\\" + classname + '_' + rep_item + "_area_growth_rate";
                    }
                    else
                    {
                        if (radioButton_Show_Bar_Class.Checked)
                            barfilename += classname + "\\" + classname + "_convex_hull_growth_rate";
                        else
                            barfilename += classname + "\\" + rep_item + "\\" + classname + '_' + rep_item + "_convex_hull_growth_rate";
                    }
                    barfilename += ".png";
                    if (!System.IO.File.Exists(barfilename))
                        pictureBox_Source_Image.Image = null;
                    else
                    {
                        Bitmap tmp_bitmap = new Bitmap(barfilename);
                        Image image = new Bitmap(tmp_bitmap);
                        pictureBox_Source_Image.Image = image;
                        tmp_bitmap.Dispose();
                    }
                    break;
                case 3:
                    // 'data'  is selected
                    listView_Show_Data.Items.Clear();
                    if ((listBox_Classes.Items.Count == 0) || (textBox_Out_FolderName.Text == ""))
                        return;
                    string class_item = (string)listBox_Classes.SelectedItem;
                    excelfilename += classname + "\\" + class_item + "_extracted_features.xlsx";
                    if (!System.IO.File.Exists(excelfilename))
                        return;
                    if (pictureBox_Source_Image.Controls.Count > 0)
                        pictureBox_Source_Image.Controls.RemoveAt(pictureBox_Source_Image.Controls.Count - 1);
                    List<List<string> > excel_data = Excels_EPP.Read_from_Excel_File(excelfilename);
                    for (int i_row = 1; i_row < excel_data.Count; i_row++)
                    {
                        ListViewItem items = new ListViewItem(excel_data[i_row].ToArray());
                        listView_Show_Data.Items.Add(items);
                    }
                    break;
            }
        }

        private void Set_ProcessRep_Checked_State(Boolean state)
        {
            m_bRepSelectionLoading = true;
            for (int i = 0; i < listView_Reps.Items.Count; i++) {
                listView_Reps.Items[i].Checked = state;
            }
            m_bRepSelectionLoading = false;

            SaveRepSelections();
        }


        private void listView_Reps_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (m_bRepSelectionLoading == true)
                return;

            SaveRepSelections();
        }

        private void DeletePreviousResultFiles(string classname)
        {
            // delete plots
            string plotsfoldername = textBox_Out_FolderName.Text + "\\_Plots" + "\\" + classname;
            System.IO.DirectoryInfo di = new DirectoryInfo(plotsfoldername);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            // delete html plots
            string plotshtmlfoldername = textBox_Out_FolderName.Text + "\\_PlotsHtml" + "\\" + classname;
            di = new DirectoryInfo(plotshtmlfoldername);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            // delete bars
            string barsfoldername = textBox_Out_FolderName.Text + "\\_Bars" + "\\" + classname;
            di = new DirectoryInfo(barsfoldername);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            // delete excel data files
            string excelsfoldername = textBox_Out_FolderName.Text + "\\_Excels" + "\\" + classname;
            di = new DirectoryInfo(excelsfoldername);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            // delete saved list dataa files
            string listfilename = textBox_Out_FolderName.Text + "\\_Saved_lists" + "\\" + classname + "\\" + classname + "_lists.txt";
            System.IO.File.Delete(listfilename);

        }
		
        private void Form1_Load(object sender, EventArgs e)
        {
            typeof(PictureBox).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, pictureBox_Source_Image, new object[] { true });

            //Register delete of closing Form
            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);

            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer,
                true);

            SetDataset();
        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

            // When closing form, abort the current processing thread
            if (singleThread != null)
            {
                singleThread.Abort();
                singleThread = null;
            }

            if (multiThread != null)
            {
                multiThread.Abort();
                multiThread = null;
            }
            
        }

        void SaveRepSelections()
        {
            string file_name = GetRepSelections_FileName();
            if (file_name == "")
                return;

            string foldername = file_name.Substring(0, file_name.IndexOf("\\selected_reps.txt"));
            if (!Directory.Exists(foldername))
                return;

            int number_of_reps = listView_Reps.Items.Count;
            int[] rep_selections = new int[number_of_reps];
            for (int i = 0; i < number_of_reps; i++)
                rep_selections[i] = listView_Reps.Items[i].Checked == true ? 1 : 0;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(file_name, FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, rep_selections);
            stream.Close();
            stream.Dispose();

        }

        void LoadOrInitializeRepSelections()
        {
            string file_name = GetRepSelections_FileName();
            if (file_name == "")
                return;

            // init.
            m_bRepSelectionLoading = true;
            int number_of_reps = listView_Reps.Items.Count;
            for (int i = 0; i < number_of_reps; i++)
                listView_Reps.Items[i].Checked = false;

            // load previously saved selectiosn, if any
            if (System.IO.File.Exists(file_name))
            {
                Stream stream = new FileStream(file_name, FileMode.Open, FileAccess.Read);
                IFormatter formatter = new BinaryFormatter();
                int[] rep_selections = (int[])formatter.Deserialize(stream);
                stream.Close();
                stream.Dispose();

                int nb_of_settings_to_be_loaded = System.Math.Min(number_of_reps, rep_selections.Count());
                for (int i = 0; i < nb_of_settings_to_be_loaded; i++)
                    listView_Reps.Items[i].Checked = rep_selections[i] == 0 ? false : true;
            }
            m_bRepSelectionLoading = false;
        }

        string GetRepSelections_FileName()
        {
            string file_name = "";

            if (textBox_Out_FolderName.Text == "")
                return file_name;

            string class_name = (string)listBox_Classes.SelectedItem;
            if (class_name==null || class_name == "")
                return file_name;

            string folder_name = textBox_Out_FolderName.Text + "\\_Saved_Lists\\" + class_name;
            file_name = folder_name + "\\selected_reps.txt";

            return file_name;
        }

        private void SaveGlobalParameters()
        {
            // Prepare data
            var coder = new JsonObject()
            {
                ["Dataset Type"] = (radioButton_Dataset_1.Checked == true) ? 1 : 2
            };

            //convert to JSON string
            var jsonOptions = new JsonSerializerOptions() { WriteIndented = true };
            var coderJson = coder.ToJsonString(jsonOptions);
            // Write to file
            System.IO.File.WriteAllText(textBox_Out_FolderName.Text + "\\_Saved_Lists\\global_params.json", coderJson);
        }

        private void ReadGlobalParameters()
        {
            if (!Directory.Exists(textBox_Out_FolderName.Text + "\\_Saved_Lists"))
                return;

            string filename = textBox_Out_FolderName.Text + "\\_Saved_Lists\\global_params.json";
            if (!System.IO.File.Exists(filename))
            {
                System.Windows.Forms.MessageBox.Show("ERROR: Output folder \"_Saved_Lists\" exists but file \"" + filename + "\" is not present.");
                return;
            }

            string json_string = System.IO.File.ReadAllText(filename);
            JsonElement jsonData = JsonSerializer.Deserialize<JsonElement>(json_string);
            try
            {
                int saved_dataset_type = jsonData.GetProperty("Dataset Type").Deserialize<int>();
                if (saved_dataset_type == 1)
                    if ((textBox_Root_FolderName.Text != "") && (radioButton_Dataset_2.Checked == true))
                    {
                        System.Windows.Forms.MessageBox.Show("WARNING: Output folder contains \"Dataset 2\" type results.\nChoose empty folder or folder with type \"Dataset 1\"");
                        textBox_Out_FolderName.Text = "";
                        return;
                    }
                    else
                        radioButton_Dataset_1.Checked = true;
                else if (saved_dataset_type == 2)
                    if ((textBox_Root_FolderName.Text != "") && (radioButton_Dataset_1.Checked == true))
                    {
                        System.Windows.Forms.MessageBox.Show("WARNING: Output folder contains \"Dataset 1\" type results.\nChoose empty folder or folder with type \"Dataset 2\"");
                        textBox_Out_FolderName.Text = "";
                        return;
                    }
                    else
                        radioButton_Dataset_2.Checked = true;
                else
                    System.Windows.Forms.MessageBox.Show(
                        "ERROR: Unknown \"Dataset Type\" value: " + Convert.ToString(jsonData.GetProperty("Dataset Type").Deserialize<int>()));
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.GetType().Name + ": " + ex.Message);
            }
        }
    }

}
