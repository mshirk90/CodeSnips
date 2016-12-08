using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class secure_ORSV : System.Web.UI.Page
{
    ORSV o = new ORSV();
    Place p = new Place();
    ORSVPlace op = new ORSVPlace();
    AdminFee af = new AdminFee();

    string ItemName = "ORSV";

    protected void Page_Load(object sender, EventArgs e)
    {
        ctlORSVEvent.btnSave_Click += new EventSaveEventHandler(CalculateLetterCount);
        ctlORSVPlace.btnSave_Click += new PlaceSaveEventHandler(CalculateLetterCount);
        ctlORSVEvent.btnAdd_Click += new EventAddEventHandler(SaveForEvent);
        ctlDocument.btnBack_Click += new DocumentBackEventHandler(Document_btnBack);

        StorageTotal.Attributes.Add("readonly", "readonly");
        Total.Attributes.Add("readonly", "readonly");
        

        if (!(IsPostBack))
        {
            FillDropDowns();

            if ((!(Request.QueryString["ID"] == null)) || (!(Request.QueryString["QCID"] == null)))
            {
                if (Request.QueryString["ID"] == null)
                {
                    FillDetails(Request.QueryString["QCID"].ToString());
                }
                else
                {
                    FillDetails(Request.QueryString["ID"].ToString());
                }
                ctlORSVEvent.FillList();

                ORSVTypeID_SelectedIndexChanged(null, null);
                ctlORSVPlace.FillList();

                PostageFee p = new PostageFee();
                PerLetterFee.Text = p.TotalFormatted;

                CalculateLetterCount(null, null);
                CalculateStorageTotal(null, null);
                CalculateAdminFee(null, null);
                AdjStorageDate_TextChanged(null, null);
                //CalculateTotal(null, null);
                SwitchFormMode(Util.FormMode.Edit);

                VIN.Focus();
            }
            else
            {
                FillList();
                SwitchFormMode(Util.FormMode.List);
            }
        }
    }

    #region VARIABLES (UPDATE THESE)
    protected DataTable dtList() // datatable for the main list/search (pnlList)
    {
        o = new ORSV();
        o.ORSVID = fltrORSVID.Text;
        o.VIN = fltrVIN.Text;
        o.CustomerID = fltrCustomerID.SelectedValue;
        o.ConfirmationNum = fltrConfirmationNum.Text;
        o.ORSVTypeID = fltrORSVTypeID.SelectedValue;
        o.EventID = fltrEventID.SelectedValue;
        o.HideClosed = fltrHideClosed.Checked;
        o.HideDeleted = (fltrHideDeleted.Checked == false);
        return o.Search();
    }

    protected DataTable dtDetails(string ID) // datable for the main detail section (pnlDetails)
    {
        o = new ORSV(ID);
        return o.ORSVDetails;
    }

    protected bool SaveForEvent()
    {
        if (Validation())
        {
            string ID = SaveRecord();
            FillDetails(ID);
            SwitchFormMode(Util.FormMode.Edit);
            return true;
        }
        return false;
    }

    protected string SaveRecord() // procedure to Save a record (add or update)
    {
        o.ORSVID = ORSVID.Text;
        o.ORSVTypeID = ORSVTypeID.SelectedValue;
        o.CustomerID = CustomerID.SelectedValue;
        o.VIN = VIN.Text;
        
        if (ORSVTypeID.SelectedValue == "1")
        {
            o.Year = VehicleYear.Text;
            o.MakeID = VehicleMakeID.SelectedValue;
            o.Model = VehicleModel.Text;
            o.StateCode = StateCode.Text;
        }
        else if (ORSVTypeID.SelectedValue == "2")
        {
            o.Year = VesselYear.Text;
            o.MakeID = VesselMakeID.SelectedValue;
            o.Model = VesselModel.Text;
            o.StateCode = VesselStateCode.Text;
        }

        o.Plate = Plate.Text;
        o.PlateExp = PlateExp.Text;
        o.ConfirmationNum = ConfirmationNum.Text;
        o.ReceiveDate = ReceiveDate.Text;
        o.PermitDeadlineDate = PermitDeadlineDate.Text;
        o.OrigStorageDate = OrigStorageDate.Text;
        o.AdjStorageDate = AdjStorageDate.Text;
        o.AgencyID = AgencyID.SelectedValue;
        o.ConditionID = ConditionID.SelectedValue;
        o.PickupLocation = PickupLocation.Text;
        o.Remarks = Remarks.Text;
        o.Notes = Notes.Text;
        o.InvoiceNum = InvoiceNum.Text;
        o.StorageDailyFee = Util.StripCharsForDecimal(StorageDailyFee.Text);
        o.StorageDaysOverride = StorageDaysOverride.Checked;
        o.StorageDays = StorageDays.Text;
        o.LetterCountOverride = LetterCountOverride.Checked;
        o.LetterCount = LetterCount.Text;
        o.DecalNum = DecalNum.Text;
        o.VesselLength = VesselLength.Text;
        o.MotorSerialNum = MotorSerialNum.Text;
        o.PerLetterFee = Util.StripCharsForDecimal(PerLetterFee.Text);
        o.Registration = Registration.Text;
        o.ORSVID = o.Save();

        //Now Save the Fees
        o.ORSVFee = new DataTable();
        o.ORSVFee.Columns.Add("ORSVFeeID");
        o.ORSVFee.Columns.Add("ORSVFeeTypeID");
        o.ORSVFee.Columns.Add("Fee");

        foreach (RepeaterItem ri in rpFees.Items)
        {
            TextBox ID = (TextBox)ri.FindControl("ORSVFeeID");
            TextBox FeeType = (TextBox)ri.FindControl("ORSVFeeTypeID");
            TextBox txt = (TextBox)ri.FindControl("txt");

            if (txt.Text.Length > 0)
            {
                DataRow NewRow = o.ORSVFee.NewRow();

                NewRow["ORSVFeeID"] = ID.Text;
                NewRow["ORSVFeeTypeID"] = FeeType.Text;
                NewRow["Fee"] = Util.StripCharsForDecimal(txt.Text);

                o.ORSVFee.Rows.Add(NewRow);
            }
        }
        o.SaveFees();

        return o.ORSVID;
    }

    protected void DeleteRestore(string ORSVID, bool Deleted)
    {
        o.ORSVID = ORSVID;
        o.Deleted = !Deleted;
        o.Delete();
    }
    #endregion

    #region BUTTONS & LISTS
    protected void FillDropDowns()
    {
        Util.LoadPanelLists(upnlDetails);

        //state dropdowns
        Util.LoadList(StateCode, "usp_StateDDL_Sel", "StateCode", "StateCode");
        Util.LoadList(VesselStateCode, "usp_StateDDL_Sel", "StateCode", "StateCode");

        //Make dropdowns
        Util.LoadList(VehicleMakeID, "usp_MakeDDL_Sel", "MakeID", "MakeName");
        Util.LoadList(VesselMakeID, "usp_MakeDDL_Sel", "MakeID", "MakeName");
        //Search Filters
        Util.LoadList(fltrCustomerID, "usp_CustomerDDL_Sel", "CustomerID", "CustomerName");
        Util.LoadList(fltrORSVTypeID, "usp_ORSVTypeDDL_Sel", "ORSVTypeID", "ORSVTypeName");
        Util.LoadList(fltrEventID, "usp_EventDDL_Sel", "EventID", "EventName");

        ctlORSVEvent.FillDropDowns();
    }

    protected void Buttons(object sender, EventArgs e)
    {
        Control ctl = sender as Control;
        PostageFee p = new PostageFee();
        switch (ctl.ID)
        {
            case "btnSave":
                if (Validation())
                {
                    SaveRecord();
                    if (!(Request.QueryString["ID"] == null))
                    {
                        Response.Redirect("Letters.aspx");
                    }
                    else if (!(Request.QueryString["QCID"] == null))
                    {
                        Response.Redirect("QualityControl.aspx");
                    }
                    else
                    {
                        FillList();
                        SwitchFormMode(Util.FormMode.List);
                    }
                }
                break;

            case "btnSaveAndContinue":
                if (Validation())
                {
                    SaveRecord();
                    Clear();
                    SwitchFormMode(Util.FormMode.Add);
                    lblError.Text = "";
                    lblCustomerID.Text = "Customer";
                    PerLetterFee.Text = p.TotalFormatted;
                    StateCode.SelectedIndex = -1;
                    StateCode.Items.FindByText("LA").Selected = true;
                    CalculateAdminFee(null, null);

                    //fees
                    o.ORSVID = "-1";
                    rpFees.DataSource = o.ORSVFees();
                    rpFees.DataBind();
                    o.ORSVID = "";
                    VIN.Focus();
                }
                break;

            case "btnCancel":
                if (!(Request.QueryString["ID"] == null))
                {
                    Response.Redirect("Letters.aspx");
                }
                else if (!(Request.QueryString["QCID"] == null))
                {
                    Response.Redirect("QualityControl.aspx");
                }
                else
                {
                    SwitchFormMode(Util.FormMode.List);
                }
                break;

            case "btnAddNew":
                SwitchFormMode(Util.FormMode.Add);
                lblError.Text = "";
                lblCustomerID.Text = "Customer";
                PerLetterFee.Text = p.TotalFormatted;
                StateCode.SelectedIndex = -1;
                StateCode.Items.FindByText("LA").Selected = true;
                //StateCode.SelectedValue = "LA";
                CalculateAdminFee(null, null);

                //fees
                o.ORSVID = "-1";
                rpFees.DataSource = o.ORSVFees();
                rpFees.DataBind();
                o.ORSVID = "";
                VIN.Focus();
                break;

            case "btnDelete":
                DeleteRestore(ORSVID.Text, true);
                FillList();
                SwitchFormMode(Util.FormMode.List);
                break;

            case "btnAddress":
                if (ORSVID.Text.Length > 0)
                {
                    ctlORSVPlace.FillDropDowns();
                    ctlORSVPlace.FillList();
                    AddressPopup.Show();
                }
                else
                {
                    if (SaveForEvent() == true)
                    {
                        ctlORSVPlace.FillDropDowns();
                        ctlORSVPlace.FillList();
                        AddressPopup.Show();
                    }
                }
                break;
            case "btnDocument":
                if (ORSVID.Text.Length > 0)
                {
                    ctlDocument.FillDropDowns();
                    ctlDocument.FillList();
                    ctlDocument.SwitchFormMode(Util.FormMode.List);
                    SwitchFormMode(Util.FormMode.DocumentUpload);
                }
                else
                {
                    if (SaveForEvent() == true)
                    {
                        SwitchFormMode(Util.FormMode.DocumentUpload);
                    }
                }
                break;
            case "btnSearch":
                FillList();
                break;
            case "btnReset":
                Util.ResetPanelControls(pnlFilters);
                FillList();
                break;

            case "btnPrint":
                if (ORSVTypeID.SelectedValue == "1")
                {
                    PrintORSV();
                }
                else if (ORSVTypeID.SelectedValue == "2")
                {
                    PrintVessel();
                }

                break;
        }
    }
    #endregion

    #region STANDARD FORM FUNCTIONS
    protected void SwitchFormMode(Util.FormMode Mode)
    {
        pnlDetails.Visible = (Mode == Util.FormMode.Edit || Mode == Util.FormMode.Add);
        pnlList.Visible = (Mode == Util.FormMode.List);
        pnlDocuments.Visible = (Mode == Util.FormMode.DocumentUpload);

        btnAddNew.Visible = (Mode == Util.FormMode.List);
        btnSearch.Visible = (Mode == Util.FormMode.List);
        btnReset.Visible = (Mode == Util.FormMode.List);

        btnSave.Visible = (Mode == Util.FormMode.Edit || Mode == Util.FormMode.Add);
        btnSaveAndContinue.Visible = (Mode == Util.FormMode.Edit || Mode == Util.FormMode.Add);
        btnCancel.Visible = (Mode == Util.FormMode.Edit || Mode == Util.FormMode.Add);
        btnDelete.Visible = (Mode == Util.FormMode.Edit);

        btnAddress.Visible = (Mode == Util.FormMode.Edit || Mode == Util.FormMode.Add);
        btnDocument.Visible = (Mode == Util.FormMode.Edit || Mode == Util.FormMode.Add);
        btnPrint.Visible = (Mode == Util.FormMode.Edit);

        switch (Mode)
        {
            case Util.FormMode.List:
                Clear();
                FillList();
                litHeaderText.Text = ItemName + " List";
                break;
            case Util.FormMode.Add:
                litHeaderText.Text = "New " + ItemName;

                //how nice of us to help them out, also shows full columns when entering new record
                ORSVTypeID.SelectedIndex = 1;
                ORSVTypeID_SelectedIndexChanged(null, null);
                Status.BorderColor = System.Drawing.ColorTranslator.FromHtml("#d5d5d5");
                lblStatus.BorderColor = System.Drawing.ColorTranslator.FromHtml("#d5d5d5");
                lblStatus.Text = "Status";
                lblStatus.ForeColor = System.Drawing.ColorTranslator.FromHtml("#535353");
                break;
            case Util.FormMode.Edit:
                litHeaderText.Text = ItemName + " Details";
                break;
            case Util.FormMode.DocumentUpload:
                litHeaderText.Text = ItemName + " Documents";
                break;  
        }
    }

    protected void Clear()
    {
        Util.ResetPanelControls(pnlDetails, true);
        lblError.Text = "";
        DetailError.Text = "";
        rpFees.DataSource = null;
        rpFees.DataBind();

        ctlORSVEvent.ResetForm();
        ctlORSVPlace.ResetForm();
    }

    protected void Document_btnBack()
    {
        SwitchFormMode(Util.FormMode.Edit);
    }

    protected void FillList(GridViewSortEventArgs SortArgs = null, GridViewPageEventArgs PagerArgs = null)
    {
        Util.SetGridView(gvList, dtList(), SortArgs, PagerArgs);
    }

    protected void gvList_Sorting(Object sender, GridViewSortEventArgs e)
    {
        FillList(e);
    }

    protected void gvList_PageIndexChanging(Object sender, GridViewPageEventArgs e)
    {
        FillList(null, e);
    }

    protected void gvList_RowCommand(Object sender, GridViewCommandEventArgs e)
    {
        int index;
        string ID;

        switch (e.CommandName)
        {
            case "Select":
                index = Convert.ToInt32(e.CommandArgument);
                ID = gvList.DataKeys[index].Values["ORSVID"].ToString();

                FillDetails(ID);
                ctlORSVEvent.FillList();

                ORSVTypeID_SelectedIndexChanged(null, null);
                ctlORSVPlace.FillList();

                PostageFee p = new PostageFee();
                PerLetterFee.Text = p.TotalFormatted;

                CalculateLetterCount(null, null);
                CalculateStorageTotal(null, null);
                CalculateAdminFee(null, null);
                AdjStorageDate_TextChanged(null, null);
                //CalculateTotal(null, null);
                IsCustomerLetter();
                IsMember();
                SwitchFormMode(Util.FormMode.Edit);

                VIN.Focus();
                break;

            case "DeleteRestore":
                index = Convert.ToInt32(e.CommandArgument);
                ID = gvList.DataKeys[index].Values["ORSVID"].ToString();
                bool Deleted = Convert.ToBoolean( gvList.DataKeys[index].Values["Deleted"].ToString());

                DeleteRestore(ID, Deleted);
                FillList();
                lblError.Text = "";
                break;
        }
    }

    protected void FillDetails(string ID)
    {
        DataTable dt = dtDetails(ID);
        Util.PopulatePanelControls(pnlDetails, dt, true);
        Util.PopulatePanelControls(pnlFees, dt);
        StateCode.SelectedValue = dt.Rows[0]["StateCode"].ToString();
        VesselMakeID.SelectedValue = dt.Rows[0]["VesselMakeID"].ToString();
        StorageDays.Text = dt.Rows[0]["StorageDays"].ToString();
        StorageDaysOverride.Checked = (bool)dt.Rows[0]["StorageDaysOverride"];

        if (dt.Rows[0]["Closed"] == "1")
        {
            Status.BorderColor = System.Drawing.Color.Red;
            lblStatus.Text = "Status - Record Closed";
            lblStatus.ForeColor = System.Drawing.Color.Red;
        }
        else
        {
            Status.BorderColor = System.Drawing.ColorTranslator.FromHtml("#d5d5d5");
            lblStatus.Text = "Status";
            lblStatus.ForeColor = System.Drawing.ColorTranslator.FromHtml("#535353");
        }

        //fees
        rpFees.DataSource = o.ORSVFees();
        rpFees.DataBind();
    }

    protected void PrintORSV()
    {
        Response.Clear();
        Response.ContentType = "application/pdf";
        Response.AddHeader("Content-Disposition", "attachment; filename=ORSVPrintout.pdf");
        Response.BinaryWrite(Util.ReportToPDF("ORSVCard", ORSVID.Text));
        Response.Flush();
    }

    protected void PrintVessel()
    {
        Response.Clear();
        Response.ContentType = "application/pdf";
        Response.AddHeader("Content-Disposition", "attachment; filename=VesselPrintout.pdf");
        Response.BinaryWrite(Util.ReportToPDF("VesselCard", ORSVID.Text));
        Response.Flush();
    }
    #endregion

    #region VALIDATION
    protected bool Validation()
    {
        bool IsValid = true;
        string ErrMsg = "";

        if (ORSVTypeID.SelectedValue == "1")
        {
            ErrMsg += Util.FormValidation("VIN", VIN, Util.ValidationType.Length, true);
        }
        else if (ORSVTypeID.SelectedValue == "2")
        {
            ErrMsg += Util.FormValidation("HIN", VIN, Util.ValidationType.Length, true, 12, 12);
        }

        ErrMsg += Util.FormValidation("Record Type", lblORSVTypeID, Util.ValidationType.ItemSelected);
        ErrMsg += Util.FormValidation("Customer", CustomerID, Util.ValidationType.ItemSelected);
        ErrMsg += Util.FormValidation("Date Received", ReceiveDate, Util.ValidationType.Date, true);
        ErrMsg += Util.FormValidation("Permit Date", PermitDeadlineDate, Util.ValidationType.Date);
        ErrMsg += Util.FormValidation("Storage Date", OrigStorageDate, Util.ValidationType.Date, true);
        ErrMsg += Util.FormValidation("Adj. Storage Date", AdjStorageDate, Util.ValidationType.Date);
        ErrMsg += Util.FormValidation("Year", VehicleYear, Util.ValidationType.YearBetween);
        ErrMsg += Util.FormValidation("Year", VesselYear, Util.ValidationType.YearBetween);
        ErrMsg += Util.FormValidation("Pickup Location", PickupLocation, Util.ValidationType.Length, false, 0, 1000);
        ErrMsg += Util.FormValidation("Remarks", Remarks, Util.ValidationType.Length, false, 0, 1000);
        ErrMsg += Util.FormValidation("Notes", Notes, Util.ValidationType.Length, false, 0, 1000);
        ErrMsg += Util.FormValidation("Days", StorageDays, Util.ValidationType.Numeric);
        ErrMsg += Util.FormValidation("Storage (Daily)", StorageDailyFee, Util.ValidationType.Decimal);
        ErrMsg += Util.FormValidation("# Of Letters", LetterCount, Util.ValidationType.Numeric);
        ErrMsg += Util.FormValidation("Record Type", ORSVTypeID, Util.ValidationType.ItemSelected);

        if (ErrMsg.Length == 0)
        {
            //if (!(Convert.ToDateTime(OrigStorageDate.Text) >= Convert.ToDateTime(AdjStorageDate.Text)))
            //{
            //    OrigStorageDate.BackColor = Util.ErrorColor;
            //    AdjStorageDate.BackColor = Util.ErrorColor;
            //    ErrMsg += "Adj. Storage Date cannot be after Storage Date.<br/>";
            //}
            //else 
            if (BusinessDayCount(Convert.ToDateTime(AdjStorageDate.Text), Convert.ToDateTime(ReceiveDate.Text)) > 2) //Counting today as the first day
            {
                ReceiveDate.BackColor = Util.ErrorColor;
                AdjStorageDate.BackColor = Util.ErrorColor;
                ErrMsg += "Adj. Storage Date cannot be more than three business days from Receive Date.<br/>";
            }

        }

        if (ORSVTypeID.SelectedItem.Text == "Vehicle")
        {
            ErrMsg += Util.FormValidation("State", StateCode, Util.ValidationType.ItemSelected);
        }
        else if (ORSVTypeID.SelectedItem.Text == "Vessel")
        {
            ErrMsg += Util.FormValidation("State", VesselStateCode, Util.ValidationType.ItemSelected);
        }

        //Exp, we'll have to do this one manually, format is 00/0000
        PlateExp.BackColor = Util.DefaultColor;
        if (PlateExp.Text.Length > 0)
        {
            Int32 tmpInt;
            if (Int32.TryParse(Util.StripChars(PlateExp.Text), out tmpInt) == false)
            {
                ErrMsg += "- Exp must be a valid date.<br/>";
                PlateExp.BackColor = Util.ErrorColor;
            }
        }

        if (ORSVTypeID.SelectedValue == "1")
        {
            ErrMsg += Util.FormValidation("Make", VehicleMakeID, Util.ValidationType.ItemSelected);
        }

        //now do the fees repeater!
        foreach (RepeaterItem ri in rpFees.Items)
        {
            Label lbl = (Label)ri.FindControl("lbl");
            TextBox txt = (TextBox)ri.FindControl("txt");

            ErrMsg += Util.FormValidation(lbl.Text, txt, Util.ValidationType.Decimal);
        }

        if (ErrMsg.Length > 0)
            IsValid = false;

        DetailError.Text = ErrMsg;
        return IsValid;
    }
    #endregion

    #region "Postback Functions"
    protected void ORSVTypeID_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ORSVTypeID.SelectedIndex > 0)
        {
            if (ORSVTypeID.SelectedValue == "1")
            {
                //Vehicle
                lblVIN.Text = "VIN";
                pnlVehicle.Visible = true;
                pnlVessel.Visible = false;
            }
            else if (ORSVTypeID.SelectedValue == "2")
            {
                //Vessel
                lblVIN.Text = "HIN";
                pnlVehicle.Visible = false;
                pnlVessel.Visible = true;
            }
        }
        VIN.Focus();
    }

    protected void OrigStorageDate_TextChanged(object sender, EventArgs e)
    {
        //We need to validate as date first, if not a date do nothing
        if (Util.FormValidation("", OrigStorageDate, Util.ValidationType.Date, true).Length > 0)
        {
            //only reset the color in this case
            OrigStorageDate.BackColor = Util.DefaultColor;
        }
        else
        {
            //copy date to the Adj. Storage Date box
            AdjStorageDate.Text = OrigStorageDate.Text;
        }

        if (!(StorageDaysOverride.Checked))
        {
            AdjStorageDate_TextChanged(null, null);
        }

        AdjStorageDate.Focus();
    }

    protected void AdjStorageDate_TextChanged(object sender, EventArgs e)
    {
        //warn if > 3 buisness days from date received
        //need to validate this and OrigStorageDate as a date as well, no need to try and warn if its not...
        if (Util.FormValidation("", AdjStorageDate, Util.ValidationType.Date, true).Length > 0 || Util.FormValidation("", OrigStorageDate, Util.ValidationType.Date, true).Length > 0)
        {
            //failed, reset color/do nothing
            AdjStorageDate.BackColor = Util.DefaultColor;
            OrigStorageDate.BackColor = Util.DefaultColor;
            StorageTotal.Text = "";
            StorageDays.Text = "";
            PermitDeadlineDate.Text = "N/A";
        }
        else
        {
            int WorkingDays = BusinessDayCount(Convert.ToDateTime(AdjStorageDate.Text), Convert.ToDateTime(ReceiveDate.Text));

            if (WorkingDays > 2) //Counting today as the first day
            {
                AdjStorageDate.BackColor = Util.ErrorColor;
                DetailError.Text = "Adj. Storage Date cannot be more than three business days from Receive Date.";
            }
            else
            {
                DetailError.Text = "";
            }

            TimeSpan ts = (DateTime.Today - Convert.ToDateTime(AdjStorageDate.Text));
            WorkingDays = ts.Days + 1;

            if (!(StorageDaysOverride.Checked))
            {
                if (WorkingDays > 90)
                {
                    StorageDays.Text = "90";
                }
                else
                {
                    StorageDays.Text = WorkingDays.ToString();
                }
            }
            else
            {
                StorageDays.Text = WorkingDays.ToString();
            }
            PermitDeadlineDate.Text = Convert.ToDateTime(AdjStorageDate.Text).AddDays(90).ToShortDateString();
        }
        CalculateStorageTotal(null, null);

        if (ORSVTypeID.SelectedValue == "1")
        { VehicleYear.Focus(); }
        else if (ORSVTypeID.SelectedValue == "2")
        { DecalNum.Focus(); }
    }

    protected void StorageDaysOverride_CheckedChanged(object sender, EventArgs e)
    {
        if (StorageDaysOverride.Checked)
        {
            StorageDays.Focus();
            AdjStorageDate_TextChanged(null, null);
        }
        else
        {
            StorageDailyFee.Focus();
            AdjStorageDate_TextChanged(null, null);
        }
        StorageDaysOverride.Focus();
    }

    protected void CalculateStorageTotal(object sender, EventArgs e)
    {
        //validate days and storage amount
        if ((Util.FormValidation("", StorageDays, Util.ValidationType.Numeric, true).Length > 0) || (Util.FormValidation("", StorageDailyFee, Util.ValidationType.Decimal, true).Length > 0))
        {
            //there was an error, clear textbox and reset backcolor
            StorageTotal.Text = "";
            StorageDays.BackColor = Util.DefaultColor;
            StorageDailyFee.BackColor = Util.DefaultColor;
        }
        else
        {
            //calculate
            StorageTotal.Text = (Convert.ToInt32(StorageDays.Text) * Convert.ToDecimal(Util.StripCharsForDecimal(StorageDailyFee.Text))).ToString("C2");
        }
        CalculateTotal(null, null);
        StorageTotal.Focus();
    }

    protected void LetterCountOverride_CheckedChanged(object sender, EventArgs e)
    {
        if (LetterCountOverride.Checked)
        {
            LetterCount.ReadOnly = false;
            LetterCount.CssClass = "Override";
            LetterCount.Focus();
            CalculateLetterCount(null, null);
        }
        else
        {
            LetterCount.ReadOnly = true;
            LetterCount.CssClass = "Override ReadOnly";
            CalculateLetterCount(null, null);
        }
    }

    protected void CalculateAdminFee(object sender, EventArgs e)
    {
        if ((Util.FormValidation("", StateCode, Util.ValidationType.ItemSelected).Length > 0) && ORSVTypeID.SelectedValue == "1")
        {
            //errors, reset colors
            StateCode.BackColor = Util.DefaultColor;
        }
        else if ((Util.FormValidation("", VesselStateCode, Util.ValidationType.ItemSelected).Length > 0) && ORSVTypeID.SelectedValue == "2")
        {
            //errors, reset colors
            VesselStateCode.BackColor = Util.DefaultColor;
        }
        else
        {
            string StateFee = "0";
            if (ORSVTypeID.SelectedValue == "1")
            {
                af.AdminFeeID = "1";
                af.StateCode = StateCode.SelectedItem.Text;
                StateFee = af.Search();
            }
            else if (ORSVTypeID.SelectedValue == "2")
            {
                af.AdminFeeID = "1";
                af.StateCode = VesselStateCode.SelectedItem.Text;
                StateFee = af.Search();
            }
            AdminFee.Text = StateFee;

            CalculateTotal(null, null);

            if (ORSVTypeID.SelectedValue == "1")
            { PlateExp.Focus(); }
            else if (ORSVTypeID.SelectedValue == "2")
            { VesselYear.Focus(); }

        }
    }

    protected void CalculateLetterCount(object sender, EventArgs e)
    {
        //count of events(document link)

        GridView gvEvent = (GridView)ctlORSVEvent.FindControl("gvList");
        int iLetterCount = 0;
        foreach (GridViewRow row in gvEvent.Rows)
        {
            HyperLink hl = (HyperLink)row.FindControl("hlDocumentLink");
            if (hl.NavigateUrl.Length > 0)
            {
                iLetterCount += (int)gvEvent.DataKeys[row.RowIndex].Values["NumOfLetters"];
            }
        }
        

        //now validate and calculate, unless override
        if (!(LetterCountOverride.Checked))
        {
            if (iLetterCount > 0)
            {
                LetterCount.Text = iLetterCount.ToString();
            }
            else
            {
                LetterCount.Text = "0";
            }
        }
    }

    protected void CalculateTotal(object sender, EventArgs e)
    {
        double iTotal = 0;

        //Repeater items
        foreach (RepeaterItem ri in rpFees.Items)
        {
            TextBox txt = (TextBox)ri.FindControl("txt");

            if (Util.FormValidation("", txt, Util.ValidationType.Decimal, true).Length > 0)
            {
                txt.BackColor = Util.DefaultColor;
            }
            else
            {
                iTotal += Convert.ToDouble(Util.StripCharsForDecimal(txt.Text));
            }
        }

        //Storage total
        if (Util.FormValidation("", StorageTotal, Util.ValidationType.Decimal, true).Length > 0)
        {
            StorageTotal.BackColor = Util.DefaultColor;
        }
        else
        {
            iTotal += Convert.ToDouble(Util.StripCharsForDecimal(StorageTotal.Text));
        }

        //Admin Fee
        if (Util.FormValidation("", AdminFee, Util.ValidationType.Decimal, true).Length > 0)
        {
            AdminFee.BackColor = Util.DefaultColor;
        }
        else
        {
            iTotal += Convert.ToDouble(Util.StripCharsForDecimal(AdminFee.Text));
        }

        //letter total
        if ((Util.FormValidation("", PerLetterFee, Util.ValidationType.Decimal, true).Length > 0 || Util.FormValidation("", LetterCount, Util.ValidationType.Numeric, true).Length > 0))
        {
            PerLetterFee.BackColor = Util.DefaultColor;
            LetterCount.BackColor = Util.DefaultColor;
        }
        else
        {
            //calculate!
            LetterFeeTotal.Text = (Convert.ToDouble(Util.StripCharsForDecimal(PerLetterFee.Text)) * Convert.ToDouble(LetterCount.Text)).ToString("C2");
            iTotal += Convert.ToDouble(Util.StripCharsForDecimal(LetterFeeTotal.Text));
        }

        //Send to the textbox
        Total.Text = iTotal.ToString("C2");
    }

    protected void CustomerID_SelectedIndexChanged(object sender, EventArgs e)
    {
        DetailError.Text = "";
        Customer c = new Customer(CustomerID.SelectedValue); 
        IsCustomerLetter(c);
        CanUseCustomer(c);
        IsMember(c);

        ConfirmationNum.Focus();
    }

    protected void IsMember(Customer c = null)
    {
        if (c == null)
        { c = new Customer(CustomerID.SelectedValue); }

        if (c.IsMember == "0")
        { CustomerID.CssClass = "NonMember"; }
        else
        { CustomerID.CssClass = ""; }
    }

    protected void IsCustomerLetter(Customer c = null)
    {
        if (Util.FormValidation("", CustomerID, Util.ValidationType.ItemSelected).Length > 0)
        {
            CustomerID.BackColor = Util.DefaultColor;
            lblCustomerID.Text = "Customer";
        }
        else
        {
            if (c == null)
            { c = new Customer(CustomerID.SelectedValue); }

            //figure out if this is a letter customer (or non letter)      
            if (c.IsLetter == "1")
            {
                lblCustomerID.Text = "Customer - Letter";
            }
            else if (c.IsLetter == "")
            {
                lblCustomerID.Text = "Customer - Non-Letter";
            }
        }
    }

    protected void CanUseCustomer(Customer c)
    {
        if (Util.FormValidation("", CustomerID, Util.ValidationType.ItemSelected).Length > 0)
        {
            CustomerID.BackColor = Util.DefaultColor;
        }
        else
        {
            if (c.PastDue == true)
            {
                //warn of this
                DetailError.Text = CustomerID.SelectedItem.Text + " is marked Past Due!  Please select a different customer.";
                CustomerID.SelectedIndex = -1;
                lblCustomerID.Text = "Customer";
                CustomerID.Focus();
                return;
            }

            if (Convert.ToInt32(c.SLINYear) < Convert.ToInt32(DateTime.Now.ToString("yy")))
            {
                DetailError.Text = CustomerID.SelectedItem.Text + " has an SILN that is not current.";
                CustomerID.SelectedIndex = -1;
                lblCustomerID.Text = "Customer";
                CustomerID.Focus();
            }
        }
    }

    protected void VIN_TextChanged(object sender, EventArgs e)
    {
        if (ORSVTypeID.SelectedValue == "1")
        {
            //first we need to get our jobject, but no point if the vin validation fails
            DetailError.Text = Util.FormValidation("", VIN, Util.ValidationType.VIN, true);
            if (DetailError.Text.Length == 0)
            {
                //first, reset color
                VIN.BackColor = Util.DefaultColor;
                //now reset error message
                DetailError.Text = "";
                string VINapiReturn = Util.VinValidator(VIN.Text);

                //new get the jObject
                try
                {
                    JObject o = JObject.Parse(VINapiReturn);

                    if (!(o == null))
                    {
                        //alright, now we need to breakout the [node?], remember case sensitive
                        JArray styleHolder = (JArray)o["styleHolder"];
                        VehicleYear.Text = (string)styleHolder.First["year"];
                        VehicleModel.Text = (string)styleHolder.First["modelName"];

                        //add the Make to the dropdown if it's not there
                        DropDownList ddlMake = VehicleMakeID;
                        string sMake = (string)styleHolder.First["makeName"];
                        string sMakeID = "";

                        //not using "FindByText" because it is case-sensitive
                        bool isMissing = true;
                        foreach (ListItem item in ddlMake.Items)
                        {
                            if (sMake.ToLower() == item.Text.ToLower())
                            {
                                isMissing = false;
                                sMakeID = item.Value;
                                break;
                            }
                        }

                        //add the Make if it's missing
                        if (isMissing)
                        {
                            //create the new lookup table record
                            sMakeID = usp_zlk_Make_Ins(sMake);
                            //refresh the Make dropdown
                            Util.LoadList(ddlMake, "usp_MakeDDL_Sel", "MakeID", "MakeName");
                        }
                        //finally, select the make from the dropdown
                        ddlMake.SelectedValue = sMakeID;
                    }
                }
                catch (Exception ex)
                {
                    DetailError.Text = VINapiReturn;
                }
            }
        }
        else if (ORSVTypeID.SelectedValue == "2")
        {
            DetailError.Text = Util.FormValidation("HIN", VIN, Util.ValidationType.Length, true, 12, 12);
        }
        CustomerID.Focus();
    }

    public string usp_zlk_Make_Ins(string MakeName)
    {
        DataMaven dm = new DataMaven();
        dm.SetSP("usp_zlk_Make_Ins");
        dm.AddParameter("@MakeName", SqlDbType.VarChar, ParameterDirection.Input, MakeName);
        dm.AddParameter("@UserID", SqlDbType.Int, ParameterDirection.Input, Util.LoggedInUser());
        dm.AddParameter("@NewID", SqlDbType.Int, ParameterDirection.Output, null);

        SqlParameterCollection parms = dm.ExecNonQuery();
        return parms["@NewID"].Value.ToString();
    }
    #endregion

    protected int BusinessDayCount(DateTime StartDate, DateTime EndDate)
    {
        DataMaven dm = new DataMaven();
        dm.SetSP("usp_BusinessDayCount_Sel");

        dm.AddParameter("@StartDate", SqlDbType.DateTime, ParameterDirection.Input, StartDate);
        dm.AddParameter("@EndDate", SqlDbType.DateTime, ParameterDirection.Input, EndDate);
        dm.AddParameter("@DayCount", SqlDbType.Int, ParameterDirection.Output, null);

        SqlParameterCollection parms = dm.ExecNonQuery();
        return (int)parms["@DayCount"].Value;
    }
}