using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace PlantInspector
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Drawing.Color form_color = System.Drawing.Color.SkyBlue;
            System.Drawing.Color group_box_color = System.Drawing.Color.FromArgb(209, 236, 240);
            System.Drawing.Color browse_button_color = System.Drawing.Color.Gold;
            System.Drawing.Color reset_button_color = System.Drawing.Color.LightSalmon;
            System.Drawing.Color process_button_color = System.Drawing.Color.Gold;
            System.Drawing.Color save_button_color = System.Drawing.Color.LightSalmon;
            System.Drawing.Color exit_button_color = System.Drawing.Color.Gold;

            this.button_Exit = new System.Windows.Forms.Button();
            this.groupBox_Root_Folder = new System.Windows.Forms.GroupBox();
            this.button_Browse_Root_Folder = new System.Windows.Forms.Button();
            this.textBox_Root_FolderName = new System.Windows.Forms.TextBox();
            this.label_Root_Folder = new System.Windows.Forms.Label();
            this.button_Browse_Out_Folder = new System.Windows.Forms.Button();
            this.textBox_Out_FolderName = new System.Windows.Forms.TextBox();
            this.label_Out_Folder = new System.Windows.Forms.Label();
            this.label_Current_Source_Image_File_Name = new System.Windows.Forms.Label();
            this.textBox_Current_Source_Image_File_Name = new System.Windows.Forms.TextBox();
            this.groupBox_Class_Rep_Name_Lists = new System.Windows.Forms.GroupBox();
            this.button_Remove_All_Reps = new System.Windows.Forms.Button();
            this.button_Add_All_Reps = new System.Windows.Forms.Button();
            this.listView_Reps = new System.Windows.Forms.ListView();
            this.listBox_Images = new System.Windows.Forms.ListBox();
            this.listBox_Classes = new System.Windows.Forms.ListBox();
            this.label_Image_List = new System.Windows.Forms.Label();
            this.label_Rep_List = new System.Windows.Forms.Label();
            this.label_Class_List = new System.Windows.Forms.Label();
            this.groupBox_Capturing_Data = new System.Windows.Forms.GroupBox();
            this.dateTimePicker_Screening_Date = new System.Windows.Forms.DateTimePicker();
            this.label_Screening_Date = new System.Windows.Forms.Label();
            this.dateTimePicker_Capturing_End_Date = new System.Windows.Forms.DateTimePicker();
            this.label_Capturing_End_Date = new System.Windows.Forms.Label();
            this.dateTimePicker_Sowing_Start = new System.Windows.Forms.DateTimePicker();
            this.label_Sowing_Start = new System.Windows.Forms.Label();
            this.label_Calibration_2 = new System.Windows.Forms.Label();
            this.textBox_Calibration_Data = new System.Windows.Forms.TextBox();
            this.label_Calibration_1 = new System.Windows.Forms.Label();
            this.radioButton_Show_Image_Mask = new System.Windows.Forms.RadioButton();
            this.radioButton_Show_Image_Segmented = new System.Windows.Forms.RadioButton();
            this.radioButton_Show_Image_None = new System.Windows.Forms.RadioButton();
            this.groupBox_Process = new System.Windows.Forms.GroupBox();
            this.groupBox_Selected_All = new System.Windows.Forms.GroupBox();
            this.radioButton_Process_Selected_Class = new System.Windows.Forms.RadioButton();
            this.radioButton_Process_All_Classes = new System.Windows.Forms.RadioButton();
            this.checkBox_Selected_Reps = new System.Windows.Forms.CheckBox();
            this.button_Reset = new System.Windows.Forms.Button();
            this.button_Process = new System.Windows.Forms.Button();
            this.pictureBox_Source_Image = new System.Windows.Forms.PictureBox();
            this.textBox_Cursor_Position = new System.Windows.Forms.TextBox();
            this.label_Cursor_Position = new System.Windows.Forms.Label();
            this.textBox_Pixel = new System.Windows.Forms.TextBox();
            this.groupBox_Show_Features = new System.Windows.Forms.GroupBox();
            this.button_CHull_Equiv_Circle_Color = new System.Windows.Forms.Button();
            this.checkBox_Show_CHull_Equiv_Circle = new System.Windows.Forms.CheckBox();
            this.button_Bounding_Ellipse_Color = new System.Windows.Forms.Button();
            this.checkBox_Show_Bounding_Ellipse = new System.Windows.Forms.CheckBox();
            this.button_Bounding_Circle_Color = new System.Windows.Forms.Button();
            this.checkBox_Show_Bounding_Circle = new System.Windows.Forms.CheckBox();
            this.button_Bounding_Box_Color = new System.Windows.Forms.Button();
            this.button_Standard_Diameter_Color = new System.Windows.Forms.Button();
            this.button_Convex_Hull_Color = new System.Windows.Forms.Button();
            this.button_Rosette_Area_Color = new System.Windows.Forms.Button();
            this.checkBox_Show_Bounding_Box = new System.Windows.Forms.CheckBox();
            this.checkBox_Show_Standard_Diameter = new System.Windows.Forms.CheckBox();
            this.checkBox_Show_Convex_Hull = new System.Windows.Forms.CheckBox();
            this.checkBox_Show_Rosette_Area = new System.Windows.Forms.CheckBox();
            this.button_Save_Image = new System.Windows.Forms.Button();
            this.groupBox_Plot_Bar_Image_Size = new System.Windows.Forms.GroupBox();
            this.textBox_Plot_Image_Height = new System.Windows.Forms.TextBox();
            this.textBox_Plot_Image_Width = new System.Windows.Forms.TextBox();
            this.label_Plot_Image_Height = new System.Windows.Forms.Label();
            this.label_Plot_Image_Width = new System.Windows.Forms.Label();
            this.groupBox_Rep_Colors = new System.Windows.Forms.GroupBox();
            this.label_Rep_10_Color = new System.Windows.Forms.Label();
            this.label_Rep_09_Color = new System.Windows.Forms.Label();
            this.label_Rep_08_Color = new System.Windows.Forms.Label();
            this.label_Rep_07_Color = new System.Windows.Forms.Label();
            this.label_Rep_06_Color = new System.Windows.Forms.Label();
            this.label_Rep_05_Color = new System.Windows.Forms.Label();
            this.label_Rep_04_Color = new System.Windows.Forms.Label();
            this.label_Rep_03_Color = new System.Windows.Forms.Label();
            this.label_Rep_02_Color = new System.Windows.Forms.Label();
            this.button_Rep_10_Color = new System.Windows.Forms.Button();
            this.button_Rep_09_Color = new System.Windows.Forms.Button();
            this.button_Rep_08_Color = new System.Windows.Forms.Button();
            this.button_Rep_07_Color = new System.Windows.Forms.Button();
            this.button_Rep_05_Color = new System.Windows.Forms.Button();
            this.button_Rep_04_Color = new System.Windows.Forms.Button();
            this.button_Rep_03_Color = new System.Windows.Forms.Button();
            this.button_Rep_02_Color = new System.Windows.Forms.Button();
            this.button_Rep_01_Color = new System.Windows.Forms.Button();
            this.button_Rep_06_Color = new System.Windows.Forms.Button();
            this.label_Rep_01_Color = new System.Windows.Forms.Label();
            this.tabControl_Show = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.comboBox_Show_Plot = new System.Windows.Forms.ComboBox();
            this.button_Interactive_Plot = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox_Bar_Selection = new System.Windows.Forms.GroupBox();
            this.groupBox_Bar_Growth = new System.Windows.Forms.GroupBox();
            this.radioButton_Convex_Hull_Growth_Rate = new System.Windows.Forms.RadioButton();
            this.radioButton_Area_Growth_Rate = new System.Windows.Forms.RadioButton();
            this.radioButton_Show_Bar_Rep = new System.Windows.Forms.RadioButton();
            this.radioButton_Show_Bar_Class = new System.Windows.Forms.RadioButton();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_Show_Data = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader18 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader21 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox_Dataset = new System.Windows.Forms.GroupBox();
            this.radioButton_Dataset_2 = new System.Windows.Forms.RadioButton();
            this.radioButton_Dataset_1 = new System.Windows.Forms.RadioButton();
            this.groupBox_Root_Folder.SuspendLayout();
            this.groupBox_Class_Rep_Name_Lists.SuspendLayout();
            this.groupBox_Capturing_Data.SuspendLayout();
            this.groupBox_Process.SuspendLayout();
            this.groupBox_Selected_All.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Source_Image)).BeginInit();
            this.groupBox_Show_Features.SuspendLayout();
            this.groupBox_Plot_Bar_Image_Size.SuspendLayout();
            this.groupBox_Rep_Colors.SuspendLayout();
            this.tabControl_Show.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox_Bar_Growth.SuspendLayout();
            this.groupBox_Bar_Selection.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox_Dataset.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Exit
            // 
            this.button_Exit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Exit.BackColor = exit_button_color;
            this.button_Exit.Location = new System.Drawing.Point(1038, 655);
            this.button_Exit.Name = "button_Exit";
            this.button_Exit.Size = new System.Drawing.Size(83, 28);
            this.button_Exit.TabIndex = 5;
            this.button_Exit.Text = "Exit";
            this.button_Exit.UseVisualStyleBackColor = false;
            this.button_Exit.Click += new System.EventHandler(this.button_Exit_Click);
            // 
            // groupBox_Root_Folder
            // 
            this.groupBox_Root_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Root_Folder.BackColor = group_box_color;
            this.groupBox_Root_Folder.Controls.Add(this.button_Browse_Root_Folder);
            this.groupBox_Root_Folder.Controls.Add(this.textBox_Root_FolderName);
            this.groupBox_Root_Folder.Controls.Add(this.label_Root_Folder);
            this.groupBox_Root_Folder.Controls.Add(this.button_Browse_Out_Folder);
            this.groupBox_Root_Folder.Controls.Add(this.textBox_Out_FolderName);
            this.groupBox_Root_Folder.Controls.Add(this.label_Out_Folder);
            this.groupBox_Root_Folder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox_Root_Folder.Location = new System.Drawing.Point(6, 12);
            this.groupBox_Root_Folder.Name = "groupBox_Root_Folder";
            this.groupBox_Root_Folder.Size = new System.Drawing.Size(999, 91);
            this.groupBox_Root_Folder.TabIndex = 6;
            this.groupBox_Root_Folder.TabStop = false;
            this.groupBox_Root_Folder.Text = "Root Folder of Source Images and Outputs";
            // 
            // button_Browse_Root_Folder
            // 
            this.button_Browse_Root_Folder.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.button_Browse_Root_Folder.BackColor = browse_button_color;
            this.button_Browse_Root_Folder.Location = new System.Drawing.Point(916, 24);
            this.button_Browse_Root_Folder.Name = "button_Browse_Root_Folder";
            this.button_Browse_Root_Folder.Size = new System.Drawing.Size(76, 27);
            this.button_Browse_Root_Folder.TabIndex = 50;
            this.button_Browse_Root_Folder.Text = "Browse";
            this.button_Browse_Root_Folder.UseVisualStyleBackColor = false;
            this.button_Browse_Root_Folder.Click += new System.EventHandler(this.button_Browse_Root_Folder_Click);
            // 
            // textBox_Root_FolderName
            // 
            this.textBox_Root_FolderName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Root_FolderName.Location = new System.Drawing.Point(155, 28);
            this.textBox_Root_FolderName.Name = "textBox_Root_FolderName";
            this.textBox_Root_FolderName.Size = new System.Drawing.Size(753, 20);
            this.textBox_Root_FolderName.TabIndex = 46;
            this.textBox_Root_FolderName.TextChanged += new System.EventHandler(this.textBox_Root_FolderName_TextChanged);
            // 
            // label_Root_Folder
            // 
            this.label_Root_Folder.AutoSize = true;
            this.label_Root_Folder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Root_Folder.Location = new System.Drawing.Point(7, 30);
            this.label_Root_Folder.Name = "label_Root_Folder";
            this.label_Root_Folder.Size = new System.Drawing.Size(128, 13);
            this.label_Root_Folder.TabIndex = 0;
            this.label_Root_Folder.Text = "Name of Root Folder:";
            // 
            // button_Browse_Out_Folder
            // 
            this.button_Browse_Out_Folder.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.button_Browse_Out_Folder.BackColor = browse_button_color;
            this.button_Browse_Out_Folder.Location = new System.Drawing.Point(916, 54);
            this.button_Browse_Out_Folder.Name = "button_Browse_Out_Folder";
            this.button_Browse_Out_Folder.Size = new System.Drawing.Size(76, 27);
            this.button_Browse_Out_Folder.TabIndex = 80;
            this.button_Browse_Out_Folder.Text = "Browse";
            this.button_Browse_Out_Folder.UseVisualStyleBackColor = false;
            this.button_Browse_Out_Folder.Click += new System.EventHandler(this.button_Browse_Out_Folder_Click);
            // 
            // textBox_Out_FolderName
            // 
            this.textBox_Out_FolderName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Out_FolderName.Location = new System.Drawing.Point(155, 58);
            this.textBox_Out_FolderName.Name = "textBox_Out_FolderName";
            this.textBox_Out_FolderName.Size = new System.Drawing.Size(753, 50);
            this.textBox_Out_FolderName.TabIndex = 81;
            this.textBox_Out_FolderName.TextChanged += new System.EventHandler(this.textBox_Out_FolderName_TextChanged);
            // 
            // label_Out_Folder
            // 
            this.label_Out_Folder.AutoSize = true;
            this.label_Out_Folder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_Out_Folder.Location = new System.Drawing.Point(7, 60);
            this.label_Out_Folder.Name = "label_Out_Folder";
            this.label_Out_Folder.Size = new System.Drawing.Size(128, 63);
            this.label_Out_Folder.TabIndex = 82;
            this.label_Out_Folder.Text = "Name of Output Folder:";
            // 
            // label_Current_Source_Image_File_Name
            // 
            this.label_Current_Source_Image_File_Name.AutoSize = true;
            this.label_Current_Source_Image_File_Name.Location = new System.Drawing.Point(13, 114);
            this.label_Current_Source_Image_File_Name.Name = "label_Current_Source_Image_File_Name";
            this.label_Current_Source_Image_File_Name.Size = new System.Drawing.Size(138, 13);
            this.label_Current_Source_Image_File_Name.TabIndex = 15;
            this.label_Current_Source_Image_File_Name.Text = "Current Source Image::";
            // 
            // textBox_Current_Source_Image_File_Name
            // 
            this.textBox_Current_Source_Image_File_Name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Current_Source_Image_File_Name.Enabled = false;
            this.textBox_Current_Source_Image_File_Name.Location = new System.Drawing.Point(161, 111);
            this.textBox_Current_Source_Image_File_Name.Name = "textBox_Current_Source_Image_File_Name";
            this.textBox_Current_Source_Image_File_Name.Size = new System.Drawing.Size(960, 20);
            this.textBox_Current_Source_Image_File_Name.TabIndex = 47;
            this.textBox_Current_Source_Image_File_Name.TextChanged += new System.EventHandler(this.textBox_Current_Source_Image_File_Name_TextChanged);
            // 
            // groupBox_Class_Rep_Name_Lists
            // 
            this.groupBox_Class_Rep_Name_Lists.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox_Class_Rep_Name_Lists.BackColor = group_box_color;
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.button_Remove_All_Reps);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.button_Add_All_Reps);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.listView_Reps);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.listBox_Images);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.listBox_Classes);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.label_Image_List);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.label_Rep_List);
            this.groupBox_Class_Rep_Name_Lists.Controls.Add(this.label_Class_List);
            this.groupBox_Class_Rep_Name_Lists.Location = new System.Drawing.Point(6, 137);
            this.groupBox_Class_Rep_Name_Lists.Name = "groupBox_Class_Rep_Name_Lists";
            this.groupBox_Class_Rep_Name_Lists.Size = new System.Drawing.Size(346, 207);
            this.groupBox_Class_Rep_Name_Lists.TabIndex = 48;
            this.groupBox_Class_Rep_Name_Lists.TabStop = false;
            // 
            // button_Remove_All_Reps
            // 
            this.button_Remove_All_Reps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Remove_All_Reps.Location = new System.Drawing.Point(227, 172);
            this.button_Remove_All_Reps.Name = "button_Remove_All_Reps";
            this.button_Remove_All_Reps.Size = new System.Drawing.Size(43, 23);
            this.button_Remove_All_Reps.TabIndex = 28;
            this.button_Remove_All_Reps.Text = "- All";
            this.button_Remove_All_Reps.UseVisualStyleBackColor = true;
            this.button_Remove_All_Reps.Click += new System.EventHandler(this.button_Remove_All_Reps_Click);
            // 
            // button_Add_All_Reps
            // 
            this.button_Add_All_Reps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Add_All_Reps.Location = new System.Drawing.Point(187, 172);
            this.button_Add_All_Reps.Name = "button_Add_All_Reps";
            this.button_Add_All_Reps.Size = new System.Drawing.Size(43, 23);
            this.button_Add_All_Reps.TabIndex = 27;
            this.button_Add_All_Reps.Text = "+ All";
            this.button_Add_All_Reps.UseVisualStyleBackColor = true;
            this.button_Add_All_Reps.Click += new System.EventHandler(this.button_Add_All_Reps_Click);
            // 
            // listView_Reps
            // 
            this.listView_Reps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listView_Reps.AutoArrange = false;
            this.listView_Reps.BackColor = System.Drawing.SystemColors.Window;
            this.listView_Reps.CheckBoxes = true;
            this.listView_Reps.GridLines = true;
            this.listView_Reps.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView_Reps.HideSelection = false;
            this.listView_Reps.Location = new System.Drawing.Point(187, 35);
            this.listView_Reps.MultiSelect = false;
            this.listView_Reps.Name = "listView_Reps";
            this.listView_Reps.ShowGroups = false;
            this.listView_Reps.Size = new System.Drawing.Size(82, 154);
            this.listView_Reps.TabIndex = 29;
            this.listView_Reps.UseCompatibleStateImageBehavior = false;
            this.listView_Reps.View = System.Windows.Forms.View.SmallIcon;
            this.listView_Reps.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_Reps_ItemChecked);
            this.listView_Reps.SelectedIndexChanged += new System.EventHandler(this.listView_Reps_SelectedIndexChanged);
            // 
            // listBox_Images
            // 
            this.listBox_Images.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_Images.FormattingEnabled = true;
            this.listBox_Images.Location = new System.Drawing.Point(285, 35);
            this.listBox_Images.Name = "listBox_Images";
            this.listBox_Images.Size = new System.Drawing.Size(47, 160);
            this.listBox_Images.TabIndex = 16;
            this.listBox_Images.SelectedIndexChanged += new System.EventHandler(this.listBox_Images_SelectedIndexChanged);
            // 
            // listBox_Classes
            // 
            this.listBox_Classes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_Classes.FormattingEnabled = true;
            this.listBox_Classes.Location = new System.Drawing.Point(14, 35);
            this.listBox_Classes.Name = "listBox_Classes";
            this.listBox_Classes.Size = new System.Drawing.Size(159, 160);
            this.listBox_Classes.TabIndex = 14;
            this.listBox_Classes.SelectedIndexChanged += new System.EventHandler(this.listBox_Classes_SelectedIndexChanged);
            // 
            // label_Image_List
            // 
            this.label_Image_List.AutoSize = true;
            this.label_Image_List.Location = new System.Drawing.Point(285, 15);
            this.label_Image_List.Name = "label_Image_List";
            this.label_Image_List.Size = new System.Drawing.Size(38, 13);
            this.label_Image_List.TabIndex = 2;
            this.label_Image_List.Text = "Data:";
            // 
            // label_Rep_List
            // 
            this.label_Rep_List.AutoSize = true;
            this.label_Rep_List.Location = new System.Drawing.Point(184, 15);
            this.label_Rep_List.Name = "label_Rep_List";
            this.label_Rep_List.Size = new System.Drawing.Size(40, 13);
            this.label_Rep_List.TabIndex = 1;
            this.label_Rep_List.Text = "Reps:";
            // 
            // label_Class_List
            // 
            this.label_Class_List.AutoSize = true;
            this.label_Class_List.Location = new System.Drawing.Point(12, 15);
            this.label_Class_List.Name = "label_Class_List";
            this.label_Class_List.Size = new System.Drawing.Size(69, 13);
            this.label_Class_List.TabIndex = 0;
            this.label_Class_List.Text = "Ecotype:";
            // 
            // groupBox_Capturing_Data
            // 
            this.groupBox_Capturing_Data.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox_Capturing_Data.BackColor = group_box_color;
            this.groupBox_Capturing_Data.Controls.Add(this.dateTimePicker_Screening_Date);
            this.groupBox_Capturing_Data.Controls.Add(this.label_Screening_Date);
            this.groupBox_Capturing_Data.Controls.Add(this.dateTimePicker_Capturing_End_Date);
            this.groupBox_Capturing_Data.Controls.Add(this.label_Capturing_End_Date);
            this.groupBox_Capturing_Data.Controls.Add(this.dateTimePicker_Sowing_Start);
            this.groupBox_Capturing_Data.Controls.Add(this.label_Sowing_Start);
            this.groupBox_Capturing_Data.Controls.Add(this.label_Calibration_2);
            this.groupBox_Capturing_Data.Controls.Add(this.textBox_Calibration_Data);
            this.groupBox_Capturing_Data.Controls.Add(this.label_Calibration_1);
            this.groupBox_Capturing_Data.Location = new System.Drawing.Point(6, 348);
            this.groupBox_Capturing_Data.Name = "groupBox_Capturing_Data";
            this.groupBox_Capturing_Data.Size = new System.Drawing.Size(346, 139);
            this.groupBox_Capturing_Data.TabIndex = 49;
            this.groupBox_Capturing_Data.TabStop = false;
            this.groupBox_Capturing_Data.Text = "Capturing Data";
            // 
            // dateTimePicker_Screening_Date
            // 
            this.dateTimePicker_Screening_Date.Location = new System.Drawing.Point(125, 73);
            this.dateTimePicker_Screening_Date.Name = "dateTimePicker_Screening_Date";
            this.dateTimePicker_Screening_Date.Size = new System.Drawing.Size(213, 20);
            this.dateTimePicker_Screening_Date.TabIndex = 60;
            // 
            // label_Screening_Date
            // 
            this.label_Screening_Date.AutoSize = true;
            this.label_Screening_Date.Location = new System.Drawing.Point(23, 79);
            this.label_Screening_Date.Name = "label_Screening_Date";
            this.label_Screening_Date.Size = new System.Drawing.Size(99, 13);
            this.label_Screening_Date.TabIndex = 59;
            this.label_Screening_Date.Text = "Screening Date:";
            // 
            // dateTimePicker_Capturing_End_Date
            // 
            this.dateTimePicker_Capturing_End_Date.Location = new System.Drawing.Point(125, 100);
            this.dateTimePicker_Capturing_End_Date.Name = "dateTimePicker_Capturing_End_Date";
            this.dateTimePicker_Capturing_End_Date.Size = new System.Drawing.Size(213, 20);
            this.dateTimePicker_Capturing_End_Date.TabIndex = 58;
            // 
            // label_Capturing_End_Date
            // 
            this.label_Capturing_End_Date.AutoSize = true;
            this.label_Capturing_End_Date.Location = new System.Drawing.Point(4, 105);
            this.label_Capturing_End_Date.Name = "label_Capturing_End_Date";
            this.label_Capturing_End_Date.Size = new System.Drawing.Size(114, 13);
            this.label_Capturing_End_Date.TabIndex = 57;
            this.label_Capturing_End_Date.Text = "End of Experiment:";
            // 
            // dateTimePicker_Snowing_Start
            // 
            this.dateTimePicker_Sowing_Start.Location = new System.Drawing.Point(125, 47);
            this.dateTimePicker_Sowing_Start.Name = "dateTimePicker_Snowing_Start";
            this.dateTimePicker_Sowing_Start.Size = new System.Drawing.Size(213, 20);
            this.dateTimePicker_Sowing_Start.TabIndex = 56;
            // 
            // label_Sowing_Start
            // 
            this.label_Sowing_Start.AutoSize = true;
            this.label_Sowing_Start.Location = new System.Drawing.Point(39, 53);
            this.label_Sowing_Start.Name = "label_Sowing_Start";
            this.label_Sowing_Start.Size = new System.Drawing.Size(83, 13);
            this.label_Sowing_Start.TabIndex = 50;
            this.label_Sowing_Start.Text = "Sowing Start:";
            // 
            // label_Calibration_2
            // 
            this.label_Calibration_2.AutoSize = true;
            this.label_Calibration_2.Location = new System.Drawing.Point(234, 21);
            this.label_Calibration_2.Name = "label_Calibration_2";
            this.label_Calibration_2.Size = new System.Drawing.Size(107, 13);
            this.label_Calibration_2.TabIndex = 49;
            this.label_Calibration_2.Text = "mm at plant level.";
            // 
            // textBox_Calibration_Data
            // 
            this.textBox_Calibration_Data.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Calibration_Data.Location = new System.Drawing.Point(125, 18);
            this.textBox_Calibration_Data.Name = "textBox_Calibration_Data";
            this.textBox_Calibration_Data.Size = new System.Drawing.Size(104, 20);
            this.textBox_Calibration_Data.TabIndex = 48;
            this.textBox_Calibration_Data.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_Calibration_1
            // 
            this.label_Calibration_1.AutoSize = true;
            this.label_Calibration_1.Location = new System.Drawing.Point(10, 21);
            this.label_Calibration_1.Name = "label_Calibration_1";
            this.label_Calibration_1.Size = new System.Drawing.Size(107, 13);
            this.label_Calibration_1.TabIndex = 17;
            this.label_Calibration_1.Text = "1 pixel represents";
            // 
            // radioButton_Show_Image_Mask
            // 
            this.radioButton_Show_Image_Mask.AutoSize = true;
            this.radioButton_Show_Image_Mask.Checked = true;
            this.radioButton_Show_Image_Mask.Location = new System.Drawing.Point(256, 25);
            this.radioButton_Show_Image_Mask.Name = "radioButton_Show_Image_Mask";
            this.radioButton_Show_Image_Mask.Size = new System.Drawing.Size(55, 17);
            this.radioButton_Show_Image_Mask.TabIndex = 2;
            this.radioButton_Show_Image_Mask.TabStop = true;
            this.radioButton_Show_Image_Mask.Text = "Mask";
            this.radioButton_Show_Image_Mask.UseVisualStyleBackColor = true;
            this.radioButton_Show_Image_Mask.CheckedChanged += new System.EventHandler(this.ShowImage_Changed);
            // 
            // radioButton_Show_Image_Segmented
            // 
            this.radioButton_Show_Image_Segmented.AutoSize = true;
            this.radioButton_Show_Image_Segmented.Location = new System.Drawing.Point(121, 25);
            this.radioButton_Show_Image_Segmented.Name = "radioButton_Show_Image_Segmented";
            this.radioButton_Show_Image_Segmented.Size = new System.Drawing.Size(88, 17);
            this.radioButton_Show_Image_Segmented.TabIndex = 1;
            this.radioButton_Show_Image_Segmented.Text = "Segmented";
            this.radioButton_Show_Image_Segmented.UseVisualStyleBackColor = true;
            this.radioButton_Show_Image_Segmented.CheckedChanged += new System.EventHandler(this.ShowImage_Changed);
            // 
            // radioButton_Show_Image_None
            // 
            this.radioButton_Show_Image_None.AutoSize = true;
            this.radioButton_Show_Image_None.Location = new System.Drawing.Point(22, 25);
            this.radioButton_Show_Image_None.Name = "radioButton_Show_Image_None";
            this.radioButton_Show_Image_None.Size = new System.Drawing.Size(55, 17);
            this.radioButton_Show_Image_None.TabIndex = 0;
            this.radioButton_Show_Image_None.Text = "None";
            this.radioButton_Show_Image_None.UseVisualStyleBackColor = true;
            this.radioButton_Show_Image_None.CheckedChanged += new System.EventHandler(this.ShowImage_Changed);
            // 
            // groupBox_Process
            // 
            this.groupBox_Process.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox_Process.BackColor = group_box_color;
            this.groupBox_Process.Controls.Add(this.groupBox_Selected_All);
            this.groupBox_Process.Controls.Add(this.checkBox_Selected_Reps);
            this.groupBox_Process.Controls.Add(this.button_Reset);
            this.groupBox_Process.Controls.Add(this.button_Process);
            this.groupBox_Process.Location = new System.Drawing.Point(6, 593);
            this.groupBox_Process.Name = "groupBox_Process";
            this.groupBox_Process.Size = new System.Drawing.Size(345, 89);
            this.groupBox_Process.TabIndex = 51;
            this.groupBox_Process.TabStop = false;
            this.groupBox_Process.Text = "Extract Features and Generate Outputs";
            // 
            // groupBox_Selected_All
            // 
            this.groupBox_Selected_All.Controls.Add(this.radioButton_Process_Selected_Class);
            this.groupBox_Selected_All.Controls.Add(this.radioButton_Process_All_Classes);
            this.groupBox_Selected_All.Location = new System.Drawing.Point(7, 15);
            this.groupBox_Selected_All.Name = "groupBox_Selected_All";
            this.groupBox_Selected_All.Size = new System.Drawing.Size(125, 62);
            this.groupBox_Selected_All.TabIndex = 12;
            this.groupBox_Selected_All.TabStop = false;
            // 
            // radioButton_Process_Selected_Class
            // 
            this.radioButton_Process_Selected_Class.AutoSize = true;
            this.radioButton_Process_Selected_Class.Location = new System.Drawing.Point(12, 13);
            this.radioButton_Process_Selected_Class.Name = "radioButton_Process_Selected_Class";
            this.radioButton_Process_Selected_Class.Size = new System.Drawing.Size(109, 17);
            this.radioButton_Process_Selected_Class.TabIndex = 0;
            this.radioButton_Process_Selected_Class.Text = "Selected Class";
            this.radioButton_Process_Selected_Class.UseVisualStyleBackColor = true;
            // 
            // radioButton_Process_All_Classes
            // 
            this.radioButton_Process_All_Classes.AutoSize = true;
            this.radioButton_Process_All_Classes.Checked = true;
            this.radioButton_Process_All_Classes.Location = new System.Drawing.Point(12, 39);
            this.radioButton_Process_All_Classes.Name = "radioButton_Process_All_Classes";
            this.radioButton_Process_All_Classes.Size = new System.Drawing.Size(86, 17);
            this.radioButton_Process_All_Classes.TabIndex = 2;
            this.radioButton_Process_All_Classes.TabStop = true;
            this.radioButton_Process_All_Classes.Text = "All Classes";
            this.radioButton_Process_All_Classes.UseVisualStyleBackColor = true;
            // 
            // checkBox_Selected_Reps
            // 
            this.checkBox_Selected_Reps.AutoSize = true;
            this.checkBox_Selected_Reps.Location = new System.Drawing.Point(148, 28);
            this.checkBox_Selected_Reps.Name = "checkBox_Selected_Reps";
            this.checkBox_Selected_Reps.Size = new System.Drawing.Size(109, 17);
            this.checkBox_Selected_Reps.TabIndex = 9;
            this.checkBox_Selected_Reps.Text = "Selected Reps";
            this.checkBox_Selected_Reps.UseVisualStyleBackColor = true;
            // 
            // button_Reset
            // 
            this.button_Reset.BackColor = reset_button_color;
            this.button_Reset.Location = new System.Drawing.Point(208, 54);
            this.button_Reset.Name = "button_Reset";
            this.button_Reset.Size = new System.Drawing.Size(61, 28);
            this.button_Reset.TabIndex = 8;
            this.button_Reset.Text = "Reset";
            this.button_Reset.UseVisualStyleBackColor = false;
            this.button_Reset.Click += new System.EventHandler(this.button_Reset_Click);
            // 
            // button_Process
            // 
            this.button_Process.BackColor = process_button_color;
            this.button_Process.Location = new System.Drawing.Point(273, 54);
            this.button_Process.Name = "button_Process";
            this.button_Process.Size = new System.Drawing.Size(65, 28);
            this.button_Process.TabIndex = 6;
            this.button_Process.Text = "Process";
            this.button_Process.UseVisualStyleBackColor = false;
            this.button_Process.Click += new System.EventHandler(this.button_Process_Click);
            // 
            // pictureBox_Source_Image
            // 
            this.pictureBox_Source_Image.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_Source_Image.BackColor = System.Drawing.Color.Black;
            this.pictureBox_Source_Image.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox_Source_Image.Location = new System.Drawing.Point(358, 137);
            this.pictureBox_Source_Image.Name = "pictureBox_Source_Image";
            this.pictureBox_Source_Image.Size = new System.Drawing.Size(512, 510);
            this.pictureBox_Source_Image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Source_Image.TabIndex = 52;
            this.pictureBox_Source_Image.TabStop = false;
            this.pictureBox_Source_Image.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox_Source_Image_Paint);
            this.pictureBox_Source_Image.MouseLeave += new System.EventHandler(this.pictureBox_Source_Image_MouseLeave);
            this.pictureBox_Source_Image.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_Source_Image_MouseMove);
            // 
            // textBox_Cursor_Position
            // 
            this.textBox_Cursor_Position.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_Cursor_Position.Enabled = false;
            this.textBox_Cursor_Position.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Cursor_Position.Location = new System.Drawing.Point(452, 661);
            this.textBox_Cursor_Position.Name = "textBox_Cursor_Position";
            this.textBox_Cursor_Position.Size = new System.Drawing.Size(101, 20);
            this.textBox_Cursor_Position.TabIndex = 53;
            this.textBox_Cursor_Position.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_Cursor_Position
            // 
            this.label_Cursor_Position.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_Cursor_Position.AutoSize = true;
            this.label_Cursor_Position.Location = new System.Drawing.Point(357, 664);
            this.label_Cursor_Position.Name = "label_Cursor_Position";
            this.label_Cursor_Position.Size = new System.Drawing.Size(96, 13);
            this.label_Cursor_Position.TabIndex = 54;
            this.label_Cursor_Position.Text = "Cursor Position:";
            // 
            // textBox_Pixel
            // 
            this.textBox_Pixel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_Pixel.Enabled = false;
            this.textBox_Pixel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Pixel.Location = new System.Drawing.Point(559, 660);
            this.textBox_Pixel.Name = "textBox_Pixel";
            this.textBox_Pixel.Size = new System.Drawing.Size(59, 20);
            this.textBox_Pixel.TabIndex = 55;
            this.textBox_Pixel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox_Show_Features
            // 
            this.groupBox_Show_Features.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Show_Features.BackColor = group_box_color;
            this.groupBox_Show_Features.Controls.Add(this.button_CHull_Equiv_Circle_Color);
            this.groupBox_Show_Features.Controls.Add(this.checkBox_Show_CHull_Equiv_Circle);
            this.groupBox_Show_Features.Controls.Add(this.button_Bounding_Ellipse_Color);
            this.groupBox_Show_Features.Controls.Add(this.checkBox_Show_Bounding_Ellipse);
            this.groupBox_Show_Features.Controls.Add(this.button_Bounding_Circle_Color);
            this.groupBox_Show_Features.Controls.Add(this.checkBox_Show_Bounding_Circle);
            this.groupBox_Show_Features.Controls.Add(this.button_Bounding_Box_Color);
            this.groupBox_Show_Features.Controls.Add(this.button_Standard_Diameter_Color);
            this.groupBox_Show_Features.Controls.Add(this.button_Convex_Hull_Color);
            this.groupBox_Show_Features.Controls.Add(this.button_Rosette_Area_Color);
            this.groupBox_Show_Features.Controls.Add(this.checkBox_Show_Bounding_Box);
            this.groupBox_Show_Features.Controls.Add(this.checkBox_Show_Standard_Diameter);
            this.groupBox_Show_Features.Controls.Add(this.checkBox_Show_Convex_Hull);
            this.groupBox_Show_Features.Controls.Add(this.checkBox_Show_Rosette_Area);
            this.groupBox_Show_Features.Location = new System.Drawing.Point(885, 137);
            this.groupBox_Show_Features.Name = "groupBox_Show_Features";
            this.groupBox_Show_Features.Size = new System.Drawing.Size(236, 249);
            this.groupBox_Show_Features.TabIndex = 56;
            this.groupBox_Show_Features.TabStop = false;
            this.groupBox_Show_Features.Text = "Display Traits";
            // 
            // button_CHull_Equiv_Circle_Color
            // 
            this.button_CHull_Equiv_Circle_Color.BackColor = System.Drawing.Color.Blue;
            this.button_CHull_Equiv_Circle_Color.Location = new System.Drawing.Point(153, 214);
            this.button_CHull_Equiv_Circle_Color.Name = "button_CHull_Equiv_Circle_Color";
            this.button_CHull_Equiv_Circle_Color.Size = new System.Drawing.Size(68, 28);
            this.button_CHull_Equiv_Circle_Color.TabIndex = 86;
            this.button_CHull_Equiv_Circle_Color.UseVisualStyleBackColor = false;
            this.button_CHull_Equiv_Circle_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // checkBox_Show_CHull_Equiv_Circle
            // 
            this.checkBox_Show_CHull_Equiv_Circle.AutoSize = true;
            this.checkBox_Show_CHull_Equiv_Circle.Location = new System.Drawing.Point(9, 221);
            this.checkBox_Show_CHull_Equiv_Circle.Name = "checkBox_Show_CHull_Equiv_Circle";
            this.checkBox_Show_CHull_Equiv_Circle.Size = new System.Drawing.Size(139, 17);
            this.checkBox_Show_CHull_Equiv_Circle.TabIndex = 85;
            this.checkBox_Show_CHull_Equiv_Circle.Text = "CvxHull Eqv. Circle";
            this.checkBox_Show_CHull_Equiv_Circle.UseVisualStyleBackColor = true;
            this.checkBox_Show_CHull_Equiv_Circle.CheckedChanged += new System.EventHandler(this.ShowFeatures_CheckedChanged);
            // 
            // button_Bounding_Ellipse_Color
            // 
            this.button_Bounding_Ellipse_Color.BackColor = System.Drawing.Color.LightPink;
            this.button_Bounding_Ellipse_Color.Location = new System.Drawing.Point(153, 182);
            this.button_Bounding_Ellipse_Color.Name = "button_Bounding_Ellipse_Color";
            this.button_Bounding_Ellipse_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Bounding_Ellipse_Color.TabIndex = 14;
            this.button_Bounding_Ellipse_Color.UseVisualStyleBackColor = false;
            this.button_Bounding_Ellipse_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // checkBox_Show_Bounding_Ellipse
            // 
            this.checkBox_Show_Bounding_Ellipse.AutoSize = true;
            this.checkBox_Show_Bounding_Ellipse.Location = new System.Drawing.Point(9, 189);
            this.checkBox_Show_Bounding_Ellipse.Name = "checkBox_Show_Bounding_Ellipse";
            this.checkBox_Show_Bounding_Ellipse.Size = new System.Drawing.Size(155, 17);
            this.checkBox_Show_Bounding_Ellipse.TabIndex = 13;
            this.checkBox_Show_Bounding_Ellipse.Text = "Bounding Ellipse";
            this.checkBox_Show_Bounding_Ellipse.UseVisualStyleBackColor = true;
            this.checkBox_Show_Bounding_Ellipse.CheckedChanged += new System.EventHandler(this.ShowFeatures_CheckedChanged);
            // 
            // button_Bounding_Circle_Color
            // 
            this.button_Bounding_Circle_Color.BackColor = System.Drawing.Color.SkyBlue;
            this.button_Bounding_Circle_Color.Location = new System.Drawing.Point(153, 150);
            this.button_Bounding_Circle_Color.Name = "button_Bounding_Circle_Color";
            this.button_Bounding_Circle_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Bounding_Circle_Color.TabIndex = 12;
            this.button_Bounding_Circle_Color.UseVisualStyleBackColor = false;
            this.button_Bounding_Circle_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // checkBox_Show_Bounding_Circle
            // 
            this.checkBox_Show_Bounding_Circle.AutoSize = true;
            this.checkBox_Show_Bounding_Circle.Location = new System.Drawing.Point(9, 157);
            this.checkBox_Show_Bounding_Circle.Name = "checkBox_Show_Bounding_Circle";
            this.checkBox_Show_Bounding_Circle.Size = new System.Drawing.Size(139, 17);
            this.checkBox_Show_Bounding_Circle.TabIndex = 11;
            this.checkBox_Show_Bounding_Circle.Text = "Bounding Circle";
            this.checkBox_Show_Bounding_Circle.UseVisualStyleBackColor = true;
            this.checkBox_Show_Bounding_Circle.CheckedChanged += new System.EventHandler(this.ShowFeatures_CheckedChanged);
            // 
            // button_Bounding_Box_Color
            // 
            this.button_Bounding_Box_Color.BackColor = System.Drawing.Color.Gold;
            this.button_Bounding_Box_Color.Location = new System.Drawing.Point(153, 118);
            this.button_Bounding_Box_Color.Name = "button_Bounding_Box_Color";
            this.button_Bounding_Box_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Bounding_Box_Color.TabIndex = 10;
            this.button_Bounding_Box_Color.UseVisualStyleBackColor = false;
            this.button_Bounding_Box_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Standard_Diameter_Color
            // 
            this.button_Standard_Diameter_Color.BackColor = System.Drawing.Color.DeepPink;
            this.button_Standard_Diameter_Color.Location = new System.Drawing.Point(153, 86);
            this.button_Standard_Diameter_Color.Name = "button_Standard_Diameter_Color";
            this.button_Standard_Diameter_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Standard_Diameter_Color.TabIndex = 9;
            this.button_Standard_Diameter_Color.UseVisualStyleBackColor = false;
            this.button_Standard_Diameter_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Convex_Hull_Color
            // 
            this.button_Convex_Hull_Color.BackColor = System.Drawing.Color.OrangeRed;
            this.button_Convex_Hull_Color.Location = new System.Drawing.Point(153, 54);
            this.button_Convex_Hull_Color.Name = "button_Convex_Hull_Color";
            this.button_Convex_Hull_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Convex_Hull_Color.TabIndex = 8;
            this.button_Convex_Hull_Color.UseVisualStyleBackColor = false;
            this.button_Convex_Hull_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rosette_Area_Color
            // 
            this.button_Rosette_Area_Color.BackColor = System.Drawing.Color.LimeGreen;
            this.button_Rosette_Area_Color.Location = new System.Drawing.Point(153, 20);
            this.button_Rosette_Area_Color.Name = "button_Rosette_Area_Color";
            this.button_Rosette_Area_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rosette_Area_Color.TabIndex = 7;
            this.button_Rosette_Area_Color.UseVisualStyleBackColor = false;
            this.button_Rosette_Area_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // checkBox_Show_Bounding_Box
            // 
            this.checkBox_Show_Bounding_Box.AutoSize = true;
            this.checkBox_Show_Bounding_Box.Location = new System.Drawing.Point(9, 125);
            this.checkBox_Show_Bounding_Box.Name = "checkBox_Show_Bounding_Box";
            this.checkBox_Show_Bounding_Box.Size = new System.Drawing.Size(124, 17);
            this.checkBox_Show_Bounding_Box.TabIndex = 3;
            this.checkBox_Show_Bounding_Box.Text = "Bounding Box";
            this.checkBox_Show_Bounding_Box.UseVisualStyleBackColor = true;
            this.checkBox_Show_Bounding_Box.CheckedChanged += new System.EventHandler(this.ShowFeatures_CheckedChanged);
            // 
            // checkBox_Show_Standard_Diameter
            // 
            this.checkBox_Show_Standard_Diameter.AutoSize = true;
            this.checkBox_Show_Standard_Diameter.Location = new System.Drawing.Point(9, 93);
            this.checkBox_Show_Standard_Diameter.Name = "checkBox_Show_Standard_Diameter";
            this.checkBox_Show_Standard_Diameter.Size = new System.Drawing.Size(109, 17);
            this.checkBox_Show_Standard_Diameter.TabIndex = 2;
            this.checkBox_Show_Standard_Diameter.Text = "Width (Diameter)";
            this.checkBox_Show_Standard_Diameter.UseVisualStyleBackColor = true;
            this.checkBox_Show_Standard_Diameter.CheckedChanged += new System.EventHandler(this.ShowFeatures_CheckedChanged);
            // 
            // checkBox_Show_Convex_Hull
            // 
            this.checkBox_Show_Convex_Hull.AutoSize = true;
            this.checkBox_Show_Convex_Hull.Location = new System.Drawing.Point(9, 61);
            this.checkBox_Show_Convex_Hull.Name = "checkBox_Show_Convex_Hull";
            this.checkBox_Show_Convex_Hull.Size = new System.Drawing.Size(94, 17);
            this.checkBox_Show_Convex_Hull.TabIndex = 1;
            this.checkBox_Show_Convex_Hull.Text = "Convex Hull";
            this.checkBox_Show_Convex_Hull.UseVisualStyleBackColor = true;
            this.checkBox_Show_Convex_Hull.CheckedChanged += new System.EventHandler(this.ShowFeatures_CheckedChanged);
            // 
            // checkBox_Show_Rosette_Area
            // 
            this.checkBox_Show_Rosette_Area.AutoSize = true;
            this.checkBox_Show_Rosette_Area.Location = new System.Drawing.Point(9, 29);
            this.checkBox_Show_Rosette_Area.Name = "checkBox_Show_Rosette_Area";
            this.checkBox_Show_Rosette_Area.Size = new System.Drawing.Size(79, 17);
            this.checkBox_Show_Rosette_Area.TabIndex = 0;
            this.checkBox_Show_Rosette_Area.Text = "Perimeter";
            this.checkBox_Show_Rosette_Area.UseVisualStyleBackColor = true;
            this.checkBox_Show_Rosette_Area.CheckedChanged += new System.EventHandler(this.ShowFeatures_CheckedChanged);
            // 
            // button_Save_Image
            // 
            this.button_Save_Image.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Save_Image.BackColor = save_button_color;
            this.button_Save_Image.Location = new System.Drawing.Point(923, 655);
            this.button_Save_Image.Name = "button_Save_Image";
            this.button_Save_Image.Size = new System.Drawing.Size(107, 28);
            this.button_Save_Image.TabIndex = 57;
            this.button_Save_Image.Text = "Save Image";
            this.button_Save_Image.UseVisualStyleBackColor = false;
            this.button_Save_Image.Click += new System.EventHandler(this.button_Save_Image_Click);
            // 
            // groupBox_Plot_Bar_Image_Size
            // 
            this.groupBox_Plot_Bar_Image_Size.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Plot_Bar_Image_Size.BackColor = group_box_color;
            this.groupBox_Plot_Bar_Image_Size.Controls.Add(this.textBox_Plot_Image_Height);
            this.groupBox_Plot_Bar_Image_Size.Controls.Add(this.textBox_Plot_Image_Width);
            this.groupBox_Plot_Bar_Image_Size.Controls.Add(this.label_Plot_Image_Height);
            this.groupBox_Plot_Bar_Image_Size.Controls.Add(this.label_Plot_Image_Width);
            this.groupBox_Plot_Bar_Image_Size.Location = new System.Drawing.Point(885, 397);
            this.groupBox_Plot_Bar_Image_Size.Name = "groupBox_Plot_Bar_Image_Size";
            this.groupBox_Plot_Bar_Image_Size.Size = new System.Drawing.Size(236, 53);
            this.groupBox_Plot_Bar_Image_Size.TabIndex = 58;
            this.groupBox_Plot_Bar_Image_Size.TabStop = false;
            this.groupBox_Plot_Bar_Image_Size.Text = "Size of Plot and Bar Images";
            // 
            // textBox_Plot_Image_Height
            // 
            this.textBox_Plot_Image_Height.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Plot_Image_Height.Location = new System.Drawing.Point(167, 25);
            this.textBox_Plot_Image_Height.Name = "textBox_Plot_Image_Height";
            this.textBox_Plot_Image_Height.Size = new System.Drawing.Size(54, 20);
            this.textBox_Plot_Image_Height.TabIndex = 54;
            this.textBox_Plot_Image_Height.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_Plot_Image_Width
            // 
            this.textBox_Plot_Image_Width.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Plot_Image_Width.Location = new System.Drawing.Point(53, 25);
            this.textBox_Plot_Image_Width.Name = "textBox_Plot_Image_Width";
            this.textBox_Plot_Image_Width.Size = new System.Drawing.Size(54, 20);
            this.textBox_Plot_Image_Width.TabIndex = 53;
            this.textBox_Plot_Image_Width.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_Plot_Image_Height
            // 
            this.label_Plot_Image_Height.AutoSize = true;
            this.label_Plot_Image_Height.Location = new System.Drawing.Point(113, 28);
            this.label_Plot_Image_Height.Name = "label_Plot_Image_Height";
            this.label_Plot_Image_Height.Size = new System.Drawing.Size(48, 13);
            this.label_Plot_Image_Height.TabIndex = 52;
            this.label_Plot_Image_Height.Text = "Height:";
            // 
            // label_Plot_Image_Width
            // 
            this.label_Plot_Image_Width.AutoSize = true;
            this.label_Plot_Image_Width.Location = new System.Drawing.Point(8, 28);
            this.label_Plot_Image_Width.Name = "label_Plot_Image_Width";
            this.label_Plot_Image_Width.Size = new System.Drawing.Size(44, 13);
            this.label_Plot_Image_Width.TabIndex = 51;
            this.label_Plot_Image_Width.Text = "Width:";
            // 
            // groupBox_Rep_Colors
            // 
            this.groupBox_Rep_Colors.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Rep_Colors.BackColor = group_box_color;
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_10_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_09_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_08_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_07_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_06_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_05_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_04_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_03_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_02_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_10_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_09_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_08_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_07_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_05_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_04_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_03_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_02_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_01_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.button_Rep_06_Color);
            this.groupBox_Rep_Colors.Controls.Add(this.label_Rep_01_Color);
            this.groupBox_Rep_Colors.Location = new System.Drawing.Point(885, 461);
            this.groupBox_Rep_Colors.Name = "groupBox_Rep_Colors";
            this.groupBox_Rep_Colors.Size = new System.Drawing.Size(236, 186);
            this.groupBox_Rep_Colors.TabIndex = 59;
            this.groupBox_Rep_Colors.TabStop = false;
            this.groupBox_Rep_Colors.Text = "Reps Colours on Plots";
            // 
            // label_Rep_10_Color
            // 
            this.label_Rep_10_Color.AutoSize = true;
            this.label_Rep_10_Color.Location = new System.Drawing.Point(123, 158);
            this.label_Rep_10_Color.Name = "label_Rep_10_Color";
            this.label_Rep_10_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_10_Color.TabIndex = 70;
            this.label_Rep_10_Color.Text = "10:";
            // 
            // label_Rep_09_Color
            // 
            this.label_Rep_09_Color.AutoSize = true;
            this.label_Rep_09_Color.Location = new System.Drawing.Point(123, 126);
            this.label_Rep_09_Color.Name = "label_Rep_09_Color";
            this.label_Rep_09_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_09_Color.TabIndex = 69;
            this.label_Rep_09_Color.Text = "09:";
            // 
            // label_Rep_08_Color
            // 
            this.label_Rep_08_Color.AutoSize = true;
            this.label_Rep_08_Color.Location = new System.Drawing.Point(123, 94);
            this.label_Rep_08_Color.Name = "label_Rep_08_Color";
            this.label_Rep_08_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_08_Color.TabIndex = 68;
            this.label_Rep_08_Color.Text = "08:";
            // 
            // label_Rep_07_Color
            // 
            this.label_Rep_07_Color.AutoSize = true;
            this.label_Rep_07_Color.Location = new System.Drawing.Point(123, 62);
            this.label_Rep_07_Color.Name = "label_Rep_07_Color";
            this.label_Rep_07_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_07_Color.TabIndex = 67;
            this.label_Rep_07_Color.Text = "07:";
            // 
            // label_Rep_06_Color
            // 
            this.label_Rep_06_Color.AutoSize = true;
            this.label_Rep_06_Color.Location = new System.Drawing.Point(123, 30);
            this.label_Rep_06_Color.Name = "label_Rep_06_Color";
            this.label_Rep_06_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_06_Color.TabIndex = 66;
            this.label_Rep_06_Color.Text = "06:";
            // 
            // label_Rep_05_Color
            // 
            this.label_Rep_05_Color.AutoSize = true;
            this.label_Rep_05_Color.Location = new System.Drawing.Point(9, 158);
            this.label_Rep_05_Color.Name = "label_Rep_05_Color";
            this.label_Rep_05_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_05_Color.TabIndex = 65;
            this.label_Rep_05_Color.Text = "05:";
            // 
            // label_Rep_04_Color
            // 
            this.label_Rep_04_Color.AutoSize = true;
            this.label_Rep_04_Color.Location = new System.Drawing.Point(9, 126);
            this.label_Rep_04_Color.Name = "label_Rep_04_Color";
            this.label_Rep_04_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_04_Color.TabIndex = 64;
            this.label_Rep_04_Color.Text = "04:";
            // 
            // label_Rep_03_Color
            // 
            this.label_Rep_03_Color.AutoSize = true;
            this.label_Rep_03_Color.Location = new System.Drawing.Point(9, 94);
            this.label_Rep_03_Color.Name = "label_Rep_03_Color";
            this.label_Rep_03_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_03_Color.TabIndex = 63;
            this.label_Rep_03_Color.Text = "03:";
            // 
            // label_Rep_02_Color
            // 
            this.label_Rep_02_Color.AutoSize = true;
            this.label_Rep_02_Color.Location = new System.Drawing.Point(9, 62);
            this.label_Rep_02_Color.Name = "label_Rep_02_Color";
            this.label_Rep_02_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_02_Color.TabIndex = 62;
            this.label_Rep_02_Color.Text = "02:";
            // 
            // button_Rep_10_Color
            // 
            this.button_Rep_10_Color.BackColor = System.Drawing.Color.Chocolate;
            this.button_Rep_10_Color.Location = new System.Drawing.Point(153, 150);
            this.button_Rep_10_Color.Name = "button_Rep_10_Color";
            this.button_Rep_10_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_10_Color.TabIndex = 61;
            this.button_Rep_10_Color.UseVisualStyleBackColor = false;
            this.button_Rep_10_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_09_Color
            // 
            this.button_Rep_09_Color.BackColor = System.Drawing.Color.Lime;
            this.button_Rep_09_Color.Location = new System.Drawing.Point(153, 118);
            this.button_Rep_09_Color.Name = "button_Rep_09_Color";
            this.button_Rep_09_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_09_Color.TabIndex = 60;
            this.button_Rep_09_Color.UseVisualStyleBackColor = false;
            this.button_Rep_09_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_08_Color
            // 
            this.button_Rep_08_Color.BackColor = System.Drawing.Color.Purple;
            this.button_Rep_08_Color.Location = new System.Drawing.Point(153, 86);
            this.button_Rep_08_Color.Name = "button_Rep_08_Color";
            this.button_Rep_08_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_08_Color.TabIndex = 59;
            this.button_Rep_08_Color.UseVisualStyleBackColor = false;
            this.button_Rep_08_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_07_Color
            // 
            this.button_Rep_07_Color.BackColor = System.Drawing.Color.Orange;
            this.button_Rep_07_Color.Location = new System.Drawing.Point(153, 54);
            this.button_Rep_07_Color.Name = "button_Rep_07_Color";
            this.button_Rep_07_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_07_Color.TabIndex = 58;
            this.button_Rep_07_Color.UseVisualStyleBackColor = false;
            this.button_Rep_07_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_05_Color
            // 
            this.button_Rep_05_Color.BackColor = System.Drawing.Color.Magenta;
            this.button_Rep_05_Color.Location = new System.Drawing.Point(40, 150);
            this.button_Rep_05_Color.Name = "button_Rep_05_Color";
            this.button_Rep_05_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_05_Color.TabIndex = 57;
            this.button_Rep_05_Color.UseVisualStyleBackColor = false;
            this.button_Rep_05_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_04_Color
            // 
            this.button_Rep_04_Color.BackColor = System.Drawing.Color.Violet;
            this.button_Rep_04_Color.Location = new System.Drawing.Point(40, 118);
            this.button_Rep_04_Color.Name = "button_Rep_04_Color";
            this.button_Rep_04_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_04_Color.TabIndex = 56;
            this.button_Rep_04_Color.UseVisualStyleBackColor = false;
            this.button_Rep_04_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_03_Color
            // 
            this.button_Rep_03_Color.BackColor = System.Drawing.Color.Blue;
            this.button_Rep_03_Color.Location = new System.Drawing.Point(40, 86);
            this.button_Rep_03_Color.Name = "button_Rep_03_Color";
            this.button_Rep_03_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_03_Color.TabIndex = 55;
            this.button_Rep_03_Color.UseVisualStyleBackColor = false;
            this.button_Rep_03_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_02_Color
            // 
            this.button_Rep_02_Color.BackColor = System.Drawing.Color.Green;
            this.button_Rep_02_Color.Location = new System.Drawing.Point(40, 54);
            this.button_Rep_02_Color.Name = "button_Rep_02_Color";
            this.button_Rep_02_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_02_Color.TabIndex = 54;
            this.button_Rep_02_Color.UseVisualStyleBackColor = false;
            this.button_Rep_02_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_01_Color
            // 
            this.button_Rep_01_Color.BackColor = System.Drawing.Color.Red;
            this.button_Rep_01_Color.Location = new System.Drawing.Point(40, 22);
            this.button_Rep_01_Color.Name = "button_Rep_01_Color";
            this.button_Rep_01_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_01_Color.TabIndex = 53;
            this.button_Rep_01_Color.UseVisualStyleBackColor = false;
            this.button_Rep_01_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // button_Rep_06_Color
            // 
            this.button_Rep_06_Color.BackColor = System.Drawing.Color.Cyan;
            this.button_Rep_06_Color.Location = new System.Drawing.Point(153, 22);
            this.button_Rep_06_Color.Name = "button_Rep_06_Color";
            this.button_Rep_06_Color.Size = new System.Drawing.Size(68, 28);
            this.button_Rep_06_Color.TabIndex = 52;
            this.button_Rep_06_Color.UseVisualStyleBackColor = false;
            this.button_Rep_06_Color.Click += new System.EventHandler(this.button_Color_Click);
            // 
            // label_Rep_01_Color
            // 
            this.label_Rep_01_Color.AutoSize = true;
            this.label_Rep_01_Color.Location = new System.Drawing.Point(8, 30);
            this.label_Rep_01_Color.Name = "label_Rep_01_Color";
            this.label_Rep_01_Color.Size = new System.Drawing.Size(25, 13);
            this.label_Rep_01_Color.TabIndex = 51;
            this.label_Rep_01_Color.Text = "01:";
            // 
            // tabControl_Show
            // 
            this.tabControl_Show.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tabControl_Show.Controls.Add(this.tabPage1);
            this.tabControl_Show.Controls.Add(this.tabPage2);
            this.tabControl_Show.Controls.Add(this.tabPage3);
            this.tabControl_Show.Controls.Add(this.tabPage4);
            this.tabControl_Show.Location = new System.Drawing.Point(6, 493);
            this.tabControl_Show.Name = "tabControl_Show";
            this.tabControl_Show.SelectedIndex = 0;
            this.tabControl_Show.Size = new System.Drawing.Size(345, 94);
            this.tabControl_Show.TabIndex = 60;
            this.tabControl_Show.SelectedIndexChanged += new System.EventHandler(this.tabControl_Show_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = group_box_color;
            this.tabPage1.Controls.Add(this.radioButton_Show_Image_Mask);
            this.tabPage1.Controls.Add(this.radioButton_Show_Image_None);
            this.tabPage1.Controls.Add(this.radioButton_Show_Image_Segmented);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(337, 68);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Image Type";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = group_box_color;
            this.tabPage2.Controls.Add(this.comboBox_Show_Plot);
            this.tabPage2.Controls.Add(this.button_Interactive_Plot);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(337, 68);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Plots";
            // 
            // comboBox_Show_Plot
            // 
            this.comboBox_Show_Plot.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Show_Plot.FormattingEnabled = true;
            this.comboBox_Show_Plot.Items.AddRange(new object[]
            {
                "Rosette Area",
                "Perimeter",
                "Convex Hull Area",
                "Convex Hull Perimeter",
                "Convex Hull Roundness",
                "Convex Hull Elongation",
                "Roundness",
                "Circumference",
                "Eccentricity",
                "Compactness",
                "Bounding Box Area",
                "Width (Diameter)",
                "Area-to-Perimeter Ratio",
                "Extent",
                "Surface Coverage",
                "Rotational Mass Asymmetry",
                "Bounding Ellipse Circularity",
            });
            this.comboBox_Show_Plot.Location = new System.Drawing.Point(63, 8);
            this.comboBox_Show_Plot.Name = "comboBox_Show_Plot";
            this.comboBox_Show_Plot.Size = new System.Drawing.Size(224, 21);
            this.comboBox_Show_Plot.TabIndex = 0;
            this.comboBox_Show_Plot.SelectedIndexChanged += new System.EventHandler(this.comboBox_Show_Plot_SelectedIndexChanged);
            // 
            // button_Interactive_Plot
            // 
            this.button_Interactive_Plot.BackColor = reset_button_color;
            this.button_Interactive_Plot.Location = new System.Drawing.Point(115, 35);
            this.button_Interactive_Plot.Name = "button_Interactive_Plot";
            this.button_Interactive_Plot.Size = new System.Drawing.Size(121, 28);
            this.button_Interactive_Plot.TabIndex = 88;
            this.button_Interactive_Plot.Text = "Interactive Plot";
            this.button_Interactive_Plot.UseVisualStyleBackColor = false;
            this.button_Interactive_Plot.Click += new System.EventHandler(this.button_Interactive_Plot_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = group_box_color;
            this.tabPage3.Controls.Add(this.groupBox_Bar_Selection);
            this.tabPage3.Controls.Add(this.groupBox_Bar_Growth);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(337, 68);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Bar charts";
            // 
            // groupBox_Bar_Selection
            // 
            this.groupBox_Bar_Selection.Controls.Add(this.radioButton_Show_Bar_Class);
            this.groupBox_Bar_Selection.Controls.Add(this.radioButton_Show_Bar_Rep);
            this.groupBox_Bar_Selection.Location = new System.Drawing.Point(11, 1);
            this.groupBox_Bar_Selection.Name = "groupBox_Bar_Selection";
            this.groupBox_Bar_Selection.Size = new System.Drawing.Size(155, 65);
            this.groupBox_Bar_Selection.TabIndex = 3;
            this.groupBox_Bar_Selection.TabStop = false;
            // 
            // groupBox_Bar_Growth
            // 
            this.groupBox_Bar_Growth.BackColor = System.Drawing.Color.FromArgb(168, 219, 240);
            this.groupBox_Bar_Growth.Controls.Add(this.radioButton_Convex_Hull_Growth_Rate);
            this.groupBox_Bar_Growth.Controls.Add(this.radioButton_Area_Growth_Rate);
            this.groupBox_Bar_Growth.Location = new System.Drawing.Point(174, 1);
            this.groupBox_Bar_Growth.Name = "groupBox_Bar_Growth";
            this.groupBox_Bar_Growth.Size = new System.Drawing.Size(155, 65);
            this.groupBox_Bar_Growth.TabIndex = 83;
            this.groupBox_Bar_Growth.TabStop = false;
            // 
            // radioButton_Area_Growth_Rate
            // 
            this.radioButton_Area_Growth_Rate.AutoSize = true;
            this.radioButton_Area_Growth_Rate.Checked = true;
            this.radioButton_Area_Growth_Rate.Location = new System.Drawing.Point(12, 10);
            this.radioButton_Area_Growth_Rate.Name = "radioButton_Area_Growth_Rate";
            this.radioButton_Area_Growth_Rate.Size = new System.Drawing.Size(126, 17);
            this.radioButton_Area_Growth_Rate.TabIndex = 85;
            this.radioButton_Area_Growth_Rate.TabStop = true;
            this.radioButton_Area_Growth_Rate.Text = "Area Growth Rate";
            this.radioButton_Area_Growth_Rate.UseVisualStyleBackColor = true;
            this.radioButton_Area_Growth_Rate.CheckedChanged += new System.EventHandler(this.radioButton_Bar_Growth_CheckedChanged);
            // 
            // radioButton_Convex_Hull_Growth_Rate
            // 
            this.radioButton_Convex_Hull_Growth_Rate.AutoSize = true;
            this.radioButton_Convex_Hull_Growth_Rate.Location = new System.Drawing.Point(12, 35);
            this.radioButton_Convex_Hull_Growth_Rate.Name = "radioButton_Convex_Hull_Growth_Rate";
            this.radioButton_Convex_Hull_Growth_Rate.Size = new System.Drawing.Size(126, 17);
            this.radioButton_Convex_Hull_Growth_Rate.TabIndex = 86;
            this.radioButton_Convex_Hull_Growth_Rate.Text = "CvxsHull Growth Rate";
            this.radioButton_Convex_Hull_Growth_Rate.UseVisualStyleBackColor = true;
            this.radioButton_Convex_Hull_Growth_Rate.CheckedChanged += new System.EventHandler(this.radioButton_Bar_Growth_CheckedChanged);
            // 
            // radioButton_Show_Bar_Class
            // 
            this.radioButton_Show_Bar_Class.AutoSize = true;
            this.radioButton_Show_Bar_Class.Checked = true;
            this.radioButton_Show_Bar_Class.Location = new System.Drawing.Point(12, 10);
            this.radioButton_Show_Bar_Class.Name = "radioButton_Show_Bar_Class";
            this.radioButton_Show_Bar_Class.Size = new System.Drawing.Size(126, 17);
            this.radioButton_Show_Bar_Class.TabIndex = 1;  // 2;
            this.radioButton_Show_Bar_Class.TabStop = true;
            this.radioButton_Show_Bar_Class.Text = "Selected Class";
            this.radioButton_Show_Bar_Class.UseVisualStyleBackColor = true;
            this.radioButton_Show_Bar_Class.CheckedChanged += new System.EventHandler(this.radioButton_Show_Bar_CheckedChanged);
            // 
            // radioButton_Show_Bar_Rep
            // 
            this.radioButton_Show_Bar_Rep.AutoSize = true;
            this.radioButton_Show_Bar_Rep.Checked = true;
            this.radioButton_Show_Bar_Rep.Location = new System.Drawing.Point(12, 35);
            this.radioButton_Show_Bar_Rep.Name = "radioButton_Show_Bar_Rep";
            this.radioButton_Show_Bar_Rep.Size = new System.Drawing.Size(126, 17);
            this.radioButton_Show_Bar_Rep.TabIndex = 2;
            this.radioButton_Show_Bar_Rep.TabStop = true;
            this.radioButton_Show_Bar_Rep.Text = "Selected Replicate";
            this.radioButton_Show_Bar_Rep.UseVisualStyleBackColor = true;
            this.radioButton_Show_Bar_Rep.CheckedChanged += new System.EventHandler(this.radioButton_Show_Bar_CheckedChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = group_box_color;
            this.tabPage4.Controls.Add(this.label2);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(337, 68);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Raw Data";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(325, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Show data extracted for selected class.";
            // 
            // listView_Show_Data
            // 
            this.listView_Show_Data.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listView_Show_Data.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_Show_Data.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader17,
            this.columnHeader18,
            this.columnHeader19,
            this.columnHeader20,
            this.columnHeader21});
            this.listView_Show_Data.GridLines = true;
            this.listView_Show_Data.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView_Show_Data.HideSelection = false;
            this.listView_Show_Data.HoverSelection = false;
            this.listView_Show_Data.Location = new System.Drawing.Point(358, 137);
            this.listView_Show_Data.MultiSelect = false;
            this.listView_Show_Data.Name = "listView_Show_Data";
            this.listView_Show_Data.Size = new System.Drawing.Size(763, 510);
            this.listView_Show_Data.TabIndex = 61;
            this.listView_Show_Data.UseCompatibleStateImageBehavior = false;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Date";
            this.columnHeader1.Width = 100;
            this.columnHeader1.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Time";
            this.columnHeader2.Width = 50;
            this.columnHeader2.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = " p_area ";
            this.columnHeader3.Width = -2;
            this.columnHeader3.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "p_area_to_perimeter_ratio";
            this.columnHeader4.Width = -2;
            this.columnHeader4.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "p_bounding_box_area";
            this.columnHeader5.Width = -2;
            this.columnHeader5.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "p_bounding_box_aspect_ratio";
            this.columnHeader6.Width = -2;
            this.columnHeader6.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "p_bounding_ellipse_circularity";
            this.columnHeader7.Width = -2;
            this.columnHeader7.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "p_circumference";
            this.columnHeader8.Width = -2;
            this.columnHeader8.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "p_compactness";
            this.columnHeader9.Width = -2;
            this.columnHeader9.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "p_con_hull_area";
            this.columnHeader10.Width = -2;
            this.columnHeader10.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "p_con_hull_elongation";
            this.columnHeader11.Width = -2;
            this.columnHeader11.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "p_con_hull_roundness";
            this.columnHeader12.Width = -2;
            this.columnHeader12.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "p_diameter";
            this.columnHeader13.Width = -2;
            this.columnHeader13.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "p_eccentricity";
            this.columnHeader14.Width = -2;
            this.columnHeader14.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "p_extent";
            this.columnHeader15.Width = -2;
            this.columnHeader15.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "p_perimeter";
            this.columnHeader16.Width = -2;
            this.columnHeader16.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "p_plant_roundness";
            this.columnHeader17.Width = -2;
            this.columnHeader17.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader18
            // 
            this.columnHeader18.Text = "p_rot_mass_asymmetry";
            this.columnHeader18.Width = -2;
            this.columnHeader18.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader19
            // 
            this.columnHeader19.Text = "p_surface_coverage";
            this.columnHeader19.Width = -2;
            this.columnHeader19.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader20
            // 
            this.columnHeader20.Text = "rep_num";
            this.columnHeader20.Width = -2;
            this.columnHeader20.TextAlign = HorizontalAlignment.Right;
            // 
            // columnHeader21
            // 
            this.columnHeader21.Text = "class";
            this.columnHeader21.Width = 80;
            this.columnHeader21.TextAlign = HorizontalAlignment.Left;
            // 
            // groupBox_Dataset
            // 
            this.groupBox_Dataset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_Dataset.BackColor = group_box_color;
            this.groupBox_Dataset.Controls.Add(this.radioButton_Dataset_2);
            this.groupBox_Dataset.Controls.Add(this.radioButton_Dataset_1);
            this.groupBox_Dataset.Location = new System.Drawing.Point(1011, 12);
            this.groupBox_Dataset.Name = "groupBox_Dataset";
            this.groupBox_Dataset.Size = new System.Drawing.Size(110, 91);
            this.groupBox_Dataset.TabIndex = 62;
            this.groupBox_Dataset.TabStop = false;
            this.groupBox_Dataset.Text = "Dataset";
            // 
            // radioButton_Dataset_2
            // 
            this.radioButton_Dataset_2.AutoSize = true;
            this.radioButton_Dataset_2.Location = new System.Drawing.Point(63, 43);
            this.radioButton_Dataset_2.Name = "radioButton_Dataset_2";
            this.radioButton_Dataset_2.Size = new System.Drawing.Size(32, 17);
            this.radioButton_Dataset_2.TabIndex = 1;
            this.radioButton_Dataset_2.TabStop = true;
            this.radioButton_Dataset_2.Text = "2";
            this.radioButton_Dataset_2.UseVisualStyleBackColor = true;
            this.radioButton_Dataset_2.CheckedChanged += new System.EventHandler(this.radioButton_Dataset_CheckedChanged);
            // 
            // radioButton_Dataset_1
            // 
            this.radioButton_Dataset_1.AutoSize = true;
            this.radioButton_Dataset_1.Checked = true;
            this.radioButton_Dataset_1.Location = new System.Drawing.Point(15, 43);
            this.radioButton_Dataset_1.Name = "radioButton_Dataset_1";
            this.radioButton_Dataset_1.Size = new System.Drawing.Size(32, 17);
            this.radioButton_Dataset_1.TabIndex = 0;
            this.radioButton_Dataset_1.Text = "1";
            this.radioButton_Dataset_1.UseVisualStyleBackColor = true;
            this.radioButton_Dataset_1.CheckedChanged += new System.EventHandler(this.radioButton_Dataset_CheckedChanged);
            // 
            // Form1
            // 
            this.Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = form_color;
            this.ClientSize = new System.Drawing.Size(1135, 695);
            this.Controls.Add(this.groupBox_Dataset);
            this.Controls.Add(this.listView_Show_Data);
            this.Controls.Add(this.tabControl_Show);
            this.Controls.Add(this.groupBox_Rep_Colors);
            this.Controls.Add(this.groupBox_Plot_Bar_Image_Size);
            this.Controls.Add(this.button_Save_Image);
            this.Controls.Add(this.groupBox_Show_Features);
            this.Controls.Add(this.textBox_Pixel);
            this.Controls.Add(this.label_Cursor_Position);
            this.Controls.Add(this.textBox_Cursor_Position);
            this.Controls.Add(this.pictureBox_Source_Image);
            this.Controls.Add(this.groupBox_Process);
            this.Controls.Add(this.groupBox_Capturing_Data);
            this.Controls.Add(this.groupBox_Class_Rep_Name_Lists);
            this.Controls.Add(this.textBox_Current_Source_Image_File_Name);
            this.Controls.Add(this.label_Current_Source_Image_File_Name);
            this.Controls.Add(this.groupBox_Root_Folder);
            this.Controls.Add(this.button_Exit);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(1151, 735);
            this.Name = "Form1";
            this.Text = "Plant Inspector";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox_Root_Folder.ResumeLayout(false);
            this.groupBox_Root_Folder.PerformLayout();
            this.groupBox_Class_Rep_Name_Lists.ResumeLayout(false);
            this.groupBox_Class_Rep_Name_Lists.PerformLayout();
            this.groupBox_Capturing_Data.ResumeLayout(false);
            this.groupBox_Capturing_Data.PerformLayout();
            this.groupBox_Process.ResumeLayout(false);
            this.groupBox_Process.PerformLayout();
            this.groupBox_Selected_All.ResumeLayout(false);
            this.groupBox_Selected_All.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Source_Image)).EndInit();
            this.groupBox_Show_Features.ResumeLayout(false);
            this.groupBox_Show_Features.PerformLayout();
            this.groupBox_Plot_Bar_Image_Size.ResumeLayout(false);
            this.groupBox_Plot_Bar_Image_Size.PerformLayout();
            this.groupBox_Rep_Colors.ResumeLayout(false);
            this.groupBox_Rep_Colors.PerformLayout();
            this.tabControl_Show.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox_Bar_Selection.ResumeLayout(false);
            this.groupBox_Bar_Selection.PerformLayout();
            this.groupBox_Bar_Growth.ResumeLayout(false);
            this.groupBox_Bar_Growth.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.groupBox_Dataset.ResumeLayout(false);
            this.groupBox_Dataset.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Exit;
        private System.Windows.Forms.GroupBox groupBox_Root_Folder;
        private System.Windows.Forms.Button button_Browse_Root_Folder;
        private System.Windows.Forms.TextBox textBox_Root_FolderName;
        private System.Windows.Forms.Label label_Root_Folder;
        private System.Windows.Forms.Button button_Browse_Out_Folder;
        private System.Windows.Forms.TextBox textBox_Out_FolderName;
        private System.Windows.Forms.Label label_Out_Folder;
        private System.Windows.Forms.Label label_Current_Source_Image_File_Name;
        private System.Windows.Forms.TextBox textBox_Current_Source_Image_File_Name;
        private System.Windows.Forms.GroupBox groupBox_Class_Rep_Name_Lists;
        private System.Windows.Forms.ListBox listBox_Classes;
        private System.Windows.Forms.Label label_Image_List;
        private System.Windows.Forms.Label label_Rep_List;
        private System.Windows.Forms.Label label_Class_List;
        private System.Windows.Forms.ListBox listBox_Images;
        private System.Windows.Forms.GroupBox groupBox_Capturing_Data;
        private System.Windows.Forms.Label label_Calibration_2;
        private System.Windows.Forms.TextBox textBox_Calibration_Data;
        private System.Windows.Forms.Label label_Calibration_1;
        private System.Windows.Forms.RadioButton radioButton_Show_Image_Mask;
        private System.Windows.Forms.RadioButton radioButton_Show_Image_Segmented;
        private System.Windows.Forms.RadioButton radioButton_Show_Image_None;
        private System.Windows.Forms.GroupBox groupBox_Process;
        private System.Windows.Forms.Button button_Process;
        private System.Windows.Forms.RadioButton radioButton_Process_All_Classes;
        private System.Windows.Forms.RadioButton radioButton_Process_Selected_Class;
        private System.Windows.Forms.PictureBox pictureBox_Source_Image;
        private System.Windows.Forms.TextBox textBox_Cursor_Position;
        private System.Windows.Forms.Label label_Cursor_Position;
        private System.Windows.Forms.TextBox textBox_Pixel;
        private System.Windows.Forms.Label label_Sowing_Start;
        private System.Windows.Forms.DateTimePicker dateTimePicker_Sowing_Start;
        private System.Windows.Forms.GroupBox groupBox_Show_Features;
        private System.Windows.Forms.Button button_Bounding_Box_Color;
        private System.Windows.Forms.Button button_Standard_Diameter_Color;
        private System.Windows.Forms.Button button_Convex_Hull_Color;
        private System.Windows.Forms.Button button_Rosette_Area_Color;
        private System.Windows.Forms.CheckBox checkBox_Show_Bounding_Box;
        private System.Windows.Forms.CheckBox checkBox_Show_Standard_Diameter;
        private System.Windows.Forms.CheckBox checkBox_Show_Convex_Hull;
        private System.Windows.Forms.CheckBox checkBox_Show_Rosette_Area;
        private System.Windows.Forms.Button button_Save_Image;
        private System.Windows.Forms.GroupBox groupBox_Plot_Bar_Image_Size;
        private System.Windows.Forms.TextBox textBox_Plot_Image_Height;
        private System.Windows.Forms.TextBox textBox_Plot_Image_Width;
        private System.Windows.Forms.Label label_Plot_Image_Height;
        private System.Windows.Forms.Label label_Plot_Image_Width;
        private System.Windows.Forms.DateTimePicker dateTimePicker_Capturing_End_Date;
        private System.Windows.Forms.Label label_Capturing_End_Date;
        private System.Windows.Forms.GroupBox groupBox_Rep_Colors;
        private System.Windows.Forms.Button button_Bounding_Circle_Color;
        private System.Windows.Forms.CheckBox checkBox_Show_Bounding_Circle;
        private System.Windows.Forms.Button button_Bounding_Ellipse_Color;
        private System.Windows.Forms.CheckBox checkBox_Show_Bounding_Ellipse;
        private System.Windows.Forms.Button button_CHull_Equiv_Circle_Color;
        private System.Windows.Forms.CheckBox checkBox_Show_CHull_Equiv_Circle;
        private System.Windows.Forms.Label label_Rep_10_Color;
        private System.Windows.Forms.Label label_Rep_09_Color;
        private System.Windows.Forms.Label label_Rep_08_Color;
        private System.Windows.Forms.Label label_Rep_07_Color;
        private System.Windows.Forms.Label label_Rep_06_Color;
        private System.Windows.Forms.Label label_Rep_05_Color;
        private System.Windows.Forms.Label label_Rep_04_Color;
        private System.Windows.Forms.Label label_Rep_03_Color;
        private System.Windows.Forms.Label label_Rep_02_Color;
        private System.Windows.Forms.Button button_Rep_10_Color;
        private System.Windows.Forms.Button button_Rep_09_Color;
        private System.Windows.Forms.Button button_Rep_08_Color;
        private System.Windows.Forms.Button button_Rep_07_Color;
        private System.Windows.Forms.Button button_Rep_05_Color;
        private System.Windows.Forms.Button button_Rep_04_Color;
        private System.Windows.Forms.Button button_Rep_03_Color;
        private System.Windows.Forms.Button button_Rep_02_Color;
        private System.Windows.Forms.Button button_Rep_01_Color;
        private System.Windows.Forms.Button button_Rep_06_Color;
        private System.Windows.Forms.Label label_Rep_01_Color;
        private System.Windows.Forms.DateTimePicker dateTimePicker_Screening_Date;
        private System.Windows.Forms.Label label_Screening_Date;
        private System.Windows.Forms.Button button_Reset;
        private System.Windows.Forms.CheckBox checkBox_Selected_Reps;
        private System.Windows.Forms.TabControl tabControl_Show;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button button_Interactive_Plot;
        private System.Windows.Forms.ComboBox comboBox_Show_Plot;
        private System.Windows.Forms.GroupBox groupBox_Bar_Selection;
        private System.Windows.Forms.GroupBox groupBox_Bar_Growth;
        private System.Windows.Forms.RadioButton radioButton_Convex_Hull_Growth_Rate;
        private System.Windows.Forms.RadioButton radioButton_Area_Growth_Rate;
        private System.Windows.Forms.RadioButton radioButton_Show_Bar_Rep;
        private System.Windows.Forms.RadioButton radioButton_Show_Bar_Class;
        private System.Windows.Forms.ListView listView_Show_Data;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ColumnHeader columnHeader18;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.GroupBox groupBox_Selected_All;
        private System.Windows.Forms.Button button_Remove_All_Reps;
        private System.Windows.Forms.Button button_Add_All_Reps;
        private System.Windows.Forms.GroupBox groupBox_Dataset;
        private System.Windows.Forms.RadioButton radioButton_Dataset_2;
        private System.Windows.Forms.RadioButton radioButton_Dataset_1;
        private System.Windows.Forms.ListView listView_Reps;

        private int groupBox_Show_Features_Left_Location = -1;
    }
}

