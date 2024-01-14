using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;


namespace FamilyParameterEditor
{
	public class FamilyParamEditorForm : System.Windows.Forms.Form
	{
		private readonly Document _doc;

		private readonly List<string> AllSharedParametrsNames;

		private readonly List<Definition> AllSharedParametrs;

		private readonly IContainer components = null;

		public Button btnOk;
		private Button btnCancel;

		private Label label1;
		public TextBox tbx1;
		public Button btnSelect;

		public DataGridView dgv1;
        private DataGridViewTextBoxColumn TypeBlackWall;
        private DataGridViewComboBoxColumn Fop;
        private DataGridViewCheckBoxColumn Parameter_Family;
        public Label lb_Status;
        readonly Config config;

		public FamilyParamEditorForm(Document doc)
		{
			InitializeComponent();
			base.KeyDown += Form1_KeyDown;
			config = new Config();

			string text = config.Read("X", "100");
			string text2 = config.Read("Y", "100");
			base.Location = Position(text, text2);
			_doc = doc;
			AllSharedParametrsNames = new List<string>();
			AllSharedParametrs = new List<Definition>();
			List<Definition> list = new List<Definition>();
			DefinitionFile val = _doc.Application.OpenSharedParameterFile();
			if (val == null)
			{
				MessageBox.Show("Не подключен файл общих параметров. Подключите его к проекту.");
				return;
			}
			foreach (DefinitionGroup group in val.Groups)
			{
				foreach (Definition definition in group.Definitions)
				{
					AllSharedParametrsNames.Add(definition.Name);
					list.Add(definition);
				}
			}
			AllSharedParametrsNames.Sort();
			foreach (string allSharedParametrsName in AllSharedParametrsNames)
			{
				foreach (Definition item in list)
				{
					if (allSharedParametrsName == item.Name)
					{
						AllSharedParametrs.Add(item);
					}
				}
			}
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Escape)
			{
				Close();
			}
		}

		public static System.Drawing.Point Position(string x, string y)
		{
			int num = Convert.ToInt32(x);
			int num2 = Convert.ToInt32(y);
			Screen[] allScreens = Screen.AllScreens;
			bool flag = false;
			for (int i = 0; i < allScreens.Length; i++)
			{
				if ((num >= allScreens[i].Bounds.Left) & (num < allScreens[i].Bounds.Right - 20) & (num2 >= allScreens[i].Bounds.Top) & (num2 < allScreens[i].Bounds.Bottom - 20))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				num = Screen.PrimaryScreen.Bounds.Left + 50;
				num2 = Screen.PrimaryScreen.Bounds.Top + 50;
			}
			return new System.Drawing.Point(num, num2);
		}

		private void DeleteSharedParameterFrm_Load(object sender, EventArgs e)
		{
		}

		private void DeleteSharedParameterFrm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Config config = new Config();
			config.Write("X", base.Location.X.ToString());
			config.Write("Y", base.Location.Y.ToString());
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void BtnSelect_Click(object sender, EventArgs e)
		{
			dgv1.Rows.Clear();
			string lastPath = config.Read("last_path", "");

			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (Directory.Exists(lastPath))
            {
			    folderBrowserDialog.SelectedPath = lastPath;

            }

			if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				tbx1.Text = folderBrowserDialog.SelectedPath;
				config.Write("last_path", folderBrowserDialog.SelectedPath);


                List<string> listFamilysFile = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.rfa", SearchOption.AllDirectories).ToList();
                List<string> listExistSharedParamInFamilys = new List<string>();
                List<ParameterType> listExistSharedParamTypeInFamilys = new List<ParameterType>();
                lb_Status.Text = string.Format("Обнаруженно {0} семейств", listFamilysFile.Count);
                int num = 0;
                foreach (string FamilysFile in listFamilysFile)
                {
                    Document familyDoc = _doc.Application.OpenDocumentFile(FamilysFile);
                    SetParameters(familyDoc, listExistSharedParamInFamilys, listExistSharedParamTypeInFamilys);
                    num++;
                    lb_Status.Text = string.Format("Обработано {0} семейств. Всего {1}\n" +
                        "Текущий файл:{2}", num, listFamilysFile.Count,Path.GetFileName( FamilysFile));

                }
                for (int i = 0; i < listExistSharedParamInFamilys.Count; i++)
                {
                    dgv1.Rows.Add();
                }
                for (int j = 0; j < listExistSharedParamInFamilys.Count; j++)
                {
                    dgv1.Rows[j].Cells[0].Value = listExistSharedParamInFamilys[j];
                    dgv1.Columns[2].DefaultCellStyle.NullValue = false;
                    ParameterType val = listExistSharedParamTypeInFamilys[j];

                    var allowParamFromSPF = from v in AllSharedParametrs
                                            where v.ParameterType == val
                                            select v.Name;

                    var ReplacedParamName = config.Read(listExistSharedParamInFamilys[j], "");

                    if (!allowParamFromSPF.Contains(ReplacedParamName))
                    {
                        ReplacedParamName = "";
                    }

                    //foreach (Definition allSharedParametr in AllSharedParametrs)
                    //{
                    //	if (allSharedParametr.ParameterType == val)
                    //	{
                    //		allowParamFromSPF.Add(allSharedParametr.Name);
                    //	}
                    //}
                    CreateCustomComboBoxDataSouce(j, allowParamFromSPF.ToList(), ReplacedParamName);
                    lb_Status.Text = "";

                }
			
			}
		}


		private void CreateCustomComboBoxDataSouce(int row, List<string> data,string Name)
		{
			DataGridViewComboBoxCell dataGridViewComboBoxCell = dgv1[1, row] as DataGridViewComboBoxCell;
			dataGridViewComboBoxCell.DataSource = new BindingSource(data, null);
            if (Name!="")
            {
				dataGridViewComboBoxCell.Value = Name;

            }
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
		}

		private void SetParameters(Document FamilyDoc, List<string> OldParametersNames, List<ParameterType> OldParameters)
		{
			FilteredElementCollector collector = new FilteredElementCollector(FamilyDoc);
			IList<Element> sharedParameters = collector.OfClass(typeof(SharedParameterElement)).ToElements();
			foreach (ParameterElement sharedParam in sharedParameters.Cast<ParameterElement>())
			{
				
				ParameterElement val2 = sharedParam;
				if (!OldParametersNames.Contains(((Definition)val2.GetDefinition()).Name))
				{
					OldParametersNames.Add(((Definition)val2.GetDefinition()).Name);
					OldParameters.Add(((Definition)val2.GetDefinition()).ParameterType);
				}
			}
			FilteredElementCollector collector2 = new FilteredElementCollector(FamilyDoc);
			IList<Element> subFamilys = collector2.OfClass(typeof(Family)).ToElements();
			foreach (Family subFamily in subFamilys.Cast<Family>())
			{
				if (subFamily.IsEditable)
				{
					Document familyDoc = _doc.EditFamily(subFamily);
					SetParameters(familyDoc, OldParametersNames, OldParameters);
				}
			}
			FamilyDoc.Close(false);
		}

		private void dgv1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			int columnIndex = e.ColumnIndex;
            if(e.ColumnIndex == 2 && dgv1.SelectedRows.Count>0 ) 
            { 
                foreach (DataGridViewRow row in dgv1.SelectedRows)
                {
                    if (row.Cells[columnIndex].Value != null) 
                    { 
                        row.Cells[2].Value = !(bool)row.Cells[2].Value;
                    }
                    else
                    {
                        row.Cells[2].Value = true;
                    }
                }
            }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbx1 = new System.Windows.Forms.TextBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.dgv1 = new System.Windows.Forms.DataGridView();
            this.TypeBlackWall = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Fop = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Parameter_Family = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.lb_Status = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgv1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnOk.Location = new System.Drawing.Point(511, 760);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(90, 30);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Готово";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnCancel.Location = new System.Drawing.Point(607, 760);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Выберите папку с семействами";
            // 
            // tbx1
            // 
            this.tbx1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbx1.Location = new System.Drawing.Point(12, 45);
            this.tbx1.Name = "tbx1";
            this.tbx1.Size = new System.Drawing.Size(589, 20);
            this.tbx1.TabIndex = 9;
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Location = new System.Drawing.Point(607, 39);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(90, 30);
            this.btnSelect.TabIndex = 10;
            this.btnSelect.Text = "Обзор";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.BtnSelect_Click);
            // 
            // dgv1
            // 
            this.dgv1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv1.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dgv1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TypeBlackWall,
            this.Fop,
            this.Parameter_Family});
            this.dgv1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgv1.Location = new System.Drawing.Point(12, 86);
            this.dgv1.Name = "dgv1";
            this.dgv1.Size = new System.Drawing.Size(685, 659);
            this.dgv1.TabIndex = 12;
            this.dgv1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv1_CellContentClick);
            this.dgv1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv1_CellDoubleClick);
            // 
            // TypeBlackWall
            // 
            this.TypeBlackWall.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.TypeBlackWall.HeaderText = "Параметр семейства";
            this.TypeBlackWall.MinimumWidth = 10;
            this.TypeBlackWall.Name = "TypeBlackWall";
            this.TypeBlackWall.Width = 129;
            // 
            // Fop
            // 
            this.Fop.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Fop.HeaderText = "Параметр из ФОПа";
            this.Fop.Name = "Fop";
            // 
            // Parameter_Family
            // 
            this.Parameter_Family.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.Parameter_Family.HeaderText = "Параметр семейства";
            this.Parameter_Family.Name = "Parameter_Family";
            // 
            // label2
            // 
            this.lb_Status.AutoSize = true;
            this.lb_Status.Location = new System.Drawing.Point(17, 760);
            this.lb_Status.Name = "lb_status";
            this.lb_Status.Size = new System.Drawing.Size(35, 13);
            this.lb_Status.TabIndex = 13;
            this.lb_Status.Text = "";
            // 
            // FamilyParamEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(708, 802);
            this.Controls.Add(this.lb_Status);
            this.Controls.Add(this.dgv1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbx1);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.KeyPreview = true;
            this.Name = "FamilyParamEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Замена параметров";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeleteSharedParameterFrm_FormClosing);
            this.Load += new System.EventHandler(this.DeleteSharedParameterFrm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgv1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        private void dgv1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
			if (dgv1.SelectedCells.Count > 1)
			{
				foreach (DataGridViewCell cell in dgv1.SelectedCells)
				{
					var curCell = dgv1.Rows[cell.RowIndex].Cells[2];

                    if (curCell.Value==null)
                    {
						curCell.Value = true;
                    } 
					else
                    {
						curCell.Value = false;
                    }
				}
			}

		}

    }

}
