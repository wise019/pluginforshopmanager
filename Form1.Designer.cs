namespace ExcelToStore
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.barManager1 = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this.bar1 = new DevExpress.XtraBars.Bar();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this.barButtonItem1 = new DevExpress.XtraBars.BarButtonItem();
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this.gcdata = new DevExpress.XtraGrid.GridControl();
            this.gvdata = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.txtMissingTableNo = new System.Windows.Forms.TextBox();
            this.txtMissingDishName = new System.Windows.Forms.TextBox();
            this.baseDataSet = new ExcelToStore.BaseDataSet();
            this.dtExcelModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.colno = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colorderno = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colorderdate = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colordertime = new DevExpress.XtraGrid.Columns.GridColumn();
            this.coltableno = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colcustomcount = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colfooddetail = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colfoodprice = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colfoodnumber = new DevExpress.XtraGrid.Columns.GridColumn();
            this.colorderprice = new DevExpress.XtraGrid.Columns.GridColumn();
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcdata)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvdata)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.baseDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtExcelModelBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // barManager1
            // 
            this.barManager1.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1,
            this.bar3});
            this.barManager1.DockControls.Add(this.barDockControlTop);
            this.barManager1.DockControls.Add(this.barDockControlBottom);
            this.barManager1.DockControls.Add(this.barDockControlLeft);
            this.barManager1.DockControls.Add(this.barDockControlRight);
            this.barManager1.Form = this;
            this.barManager1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.barButtonItem1,
            this.barButtonItem2});
            this.barManager1.MaxItemId = 2;
            this.barManager1.StatusBar = this.bar3;
            // 
            // barDockControlTop
            // 
            this.barDockControlTop.CausesValidation = false;
            this.barDockControlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.barDockControlTop.Location = new System.Drawing.Point(0, 0);
            this.barDockControlTop.Manager = this.barManager1;
            this.barDockControlTop.Size = new System.Drawing.Size(1731, 31);
            // 
            // barDockControlBottom
            // 
            this.barDockControlBottom.CausesValidation = false;
            this.barDockControlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barDockControlBottom.Location = new System.Drawing.Point(0, 964);
            this.barDockControlBottom.Manager = this.barManager1;
            this.barDockControlBottom.Size = new System.Drawing.Size(1731, 20);
            // 
            // barDockControlLeft
            // 
            this.barDockControlLeft.CausesValidation = false;
            this.barDockControlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.barDockControlLeft.Location = new System.Drawing.Point(0, 31);
            this.barDockControlLeft.Manager = this.barManager1;
            this.barDockControlLeft.Size = new System.Drawing.Size(0, 933);
            // 
            // barDockControlRight
            // 
            this.barDockControlRight.CausesValidation = false;
            this.barDockControlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.barDockControlRight.Location = new System.Drawing.Point(1731, 31);
            this.barDockControlRight.Manager = this.barManager1;
            this.barDockControlRight.Size = new System.Drawing.Size(0, 933);
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this.barButtonItem1, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem2)});
            this.bar1.Text = "Tools";
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.bar3.OptionsBar.AllowQuickCustomization = false;
            this.bar3.OptionsBar.DrawDragBorder = false;
            this.bar3.OptionsBar.UseWholeRow = true;
            this.bar3.Text = "Status bar";
            // 
            // barButtonItem1
            // 
            this.barButtonItem1.Caption = "EXCEL导入";
            this.barButtonItem1.Id = 0;
            this.barButtonItem1.ImageOptions.LargeImage = ((System.Drawing.Image)(resources.GetObject("barButtonItem1.ImageOptions.LargeImage")));
            this.barButtonItem1.Name = "barButtonItem1";
            this.barButtonItem1.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph;
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Id = 1;
            this.barButtonItem2.Name = "barButtonItem2";
            // 
            // gcdata
            // 
            this.gcdata.DataSource = this.dtExcelModelBindingSource;
            this.gcdata.Dock = System.Windows.Forms.DockStyle.Left;
            this.gcdata.Location = new System.Drawing.Point(0, 31);
            this.gcdata.MainView = this.gvdata;
            this.gcdata.MenuManager = this.barManager1;
            this.gcdata.Name = "gcdata";
            this.gcdata.Size = new System.Drawing.Size(1026, 933);
            this.gcdata.TabIndex = 4;
            this.gcdata.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gvdata});
            // 
            // gvdata
            // 
            this.gvdata.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.colno,
            this.colorderno,
            this.colorderdate,
            this.colordertime,
            this.coltableno,
            this.colcustomcount,
            this.colfooddetail,
            this.colfoodprice,
            this.colfoodnumber,
            this.colorderprice});
            this.gvdata.GridControl = this.gcdata;
            this.gvdata.Name = "gvdata";
            this.gvdata.OptionsView.ShowGroupPanel = false;
            // 
            // panelControl1
            // 
            this.panelControl1.Controls.Add(this.groupControl2);
            this.panelControl1.Controls.Add(this.groupControl1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelControl1.Location = new System.Drawing.Point(1026, 31);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(705, 933);
            this.panelControl1.TabIndex = 5;
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.txtMissingTableNo);
            this.groupControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupControl1.Location = new System.Drawing.Point(2, 2);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(701, 448);
            this.groupControl1.TabIndex = 0;
            this.groupControl1.Text = "缺失桌名";
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.txtMissingDishName);
            this.groupControl2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupControl2.Location = new System.Drawing.Point(2, 450);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(701, 551);
            this.groupControl2.TabIndex = 1;
            this.groupControl2.Text = "缺失菜品";
            // 
            // txtMissingTableNo
            // 
            this.txtMissingTableNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMissingTableNo.Location = new System.Drawing.Point(2, 34);
            this.txtMissingTableNo.Multiline = true;
            this.txtMissingTableNo.Name = "txtMissingTableNo";
            this.txtMissingTableNo.Size = new System.Drawing.Size(697, 412);
            this.txtMissingTableNo.TabIndex = 0;
            // 
            // txtMissingDishName
            // 
            this.txtMissingDishName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMissingDishName.Location = new System.Drawing.Point(2, 34);
            this.txtMissingDishName.Multiline = true;
            this.txtMissingDishName.Name = "txtMissingDishName";
            this.txtMissingDishName.Size = new System.Drawing.Size(697, 515);
            this.txtMissingDishName.TabIndex = 1;
            // 
            // baseDataSet
            // 
            this.baseDataSet.DataSetName = "BaseDataSet";
            this.baseDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // dtExcelModelBindingSource
            // 
            this.dtExcelModelBindingSource.DataMember = "dtExcelModel";
            this.dtExcelModelBindingSource.DataSource = this.baseDataSet;
            // 
            // colno
            // 
            this.colno.FieldName = "no";
            this.colno.MinWidth = 30;
            this.colno.Name = "colno";
            this.colno.Visible = true;
            this.colno.VisibleIndex = 0;
            this.colno.Width = 112;
            // 
            // colorderno
            // 
            this.colorderno.FieldName = "orderno";
            this.colorderno.MinWidth = 30;
            this.colorderno.Name = "colorderno";
            this.colorderno.Visible = true;
            this.colorderno.VisibleIndex = 1;
            this.colorderno.Width = 112;
            // 
            // colorderdate
            // 
            this.colorderdate.FieldName = "orderdate";
            this.colorderdate.MinWidth = 30;
            this.colorderdate.Name = "colorderdate";
            this.colorderdate.Visible = true;
            this.colorderdate.VisibleIndex = 2;
            this.colorderdate.Width = 112;
            // 
            // colordertime
            // 
            this.colordertime.FieldName = "ordertime";
            this.colordertime.MinWidth = 30;
            this.colordertime.Name = "colordertime";
            this.colordertime.Visible = true;
            this.colordertime.VisibleIndex = 3;
            this.colordertime.Width = 112;
            // 
            // coltableno
            // 
            this.coltableno.FieldName = "tableno";
            this.coltableno.MinWidth = 30;
            this.coltableno.Name = "coltableno";
            this.coltableno.Visible = true;
            this.coltableno.VisibleIndex = 4;
            this.coltableno.Width = 112;
            // 
            // colcustomcount
            // 
            this.colcustomcount.FieldName = "customcount";
            this.colcustomcount.MinWidth = 30;
            this.colcustomcount.Name = "colcustomcount";
            this.colcustomcount.Visible = true;
            this.colcustomcount.VisibleIndex = 5;
            this.colcustomcount.Width = 112;
            // 
            // colfooddetail
            // 
            this.colfooddetail.FieldName = "fooddetail";
            this.colfooddetail.MinWidth = 30;
            this.colfooddetail.Name = "colfooddetail";
            this.colfooddetail.Visible = true;
            this.colfooddetail.VisibleIndex = 6;
            this.colfooddetail.Width = 112;
            // 
            // colfoodprice
            // 
            this.colfoodprice.FieldName = "foodprice";
            this.colfoodprice.MinWidth = 30;
            this.colfoodprice.Name = "colfoodprice";
            this.colfoodprice.Visible = true;
            this.colfoodprice.VisibleIndex = 7;
            this.colfoodprice.Width = 112;
            // 
            // colfoodnumber
            // 
            this.colfoodnumber.FieldName = "foodnumber";
            this.colfoodnumber.MinWidth = 30;
            this.colfoodnumber.Name = "colfoodnumber";
            this.colfoodnumber.Visible = true;
            this.colfoodnumber.VisibleIndex = 8;
            this.colfoodnumber.Width = 112;
            // 
            // colorderprice
            // 
            this.colorderprice.FieldName = "orderprice";
            this.colorderprice.MinWidth = 30;
            this.colorderprice.Name = "colorderprice";
            this.colorderprice.Visible = true;
            this.colorderprice.VisibleIndex = 9;
            this.colorderprice.Width = 112;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1731, 984);
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this.gcdata);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "Form1";
            this.Text = "订单导入软件";
            ((System.ComponentModel.ISupportInitialize)(this.barManager1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gcdata)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gvdata)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.groupControl1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.baseDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtExcelModelBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.BarManager barManager1;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraGrid.GridControl gcdata;
        private DevExpress.XtraGrid.Views.Grid.GridView gvdata;
        private DevExpress.XtraEditors.PanelControl panelControl1;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private System.Windows.Forms.TextBox txtMissingTableNo;
        private System.Windows.Forms.TextBox txtMissingDishName;
        private System.Windows.Forms.BindingSource dtExcelModelBindingSource;
        private BaseDataSet baseDataSet;
        private DevExpress.XtraGrid.Columns.GridColumn colno;
        private DevExpress.XtraGrid.Columns.GridColumn colorderno;
        private DevExpress.XtraGrid.Columns.GridColumn colorderdate;
        private DevExpress.XtraGrid.Columns.GridColumn colordertime;
        private DevExpress.XtraGrid.Columns.GridColumn coltableno;
        private DevExpress.XtraGrid.Columns.GridColumn colcustomcount;
        private DevExpress.XtraGrid.Columns.GridColumn colfooddetail;
        private DevExpress.XtraGrid.Columns.GridColumn colfoodprice;
        private DevExpress.XtraGrid.Columns.GridColumn colfoodnumber;
        private DevExpress.XtraGrid.Columns.GridColumn colorderprice;
    }
}

