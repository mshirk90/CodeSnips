using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Reporting.WebForms;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// <summary>
// Provides util functions
// </summary>
public class Util
{
    public static readonly System.Drawing.Color ErrorColor = System.Drawing.ColorTranslator.FromHtml("#F7F383");
    public static readonly System.Drawing.Color DefaultColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF");

    public enum FormMode : int
    {
        View,
        Edit,
        Add,
        List,
        DocumentUpload
    }

    public enum ValidationType
    {
        Date,
        Numeric,
        Decimal,
        Length,
        ItemSelected,
        Email,
        Phone,
        ZIP,
        YearBetween,
        VIN
    }
    
    public static object ReturnEmptyStringIfDbNull(Object sqlVar, string ReplaceWith = null)
    {
        object ret = null;
        if (sqlVar is DBNull)
        {
            if (string.IsNullOrEmpty(ReplaceWith))
                ret = string.Empty;
            else
                ret = ReplaceWith;
        }
        else
        {
            ret = sqlVar;
        }
        return ret;
    }

    public static string ReturnEmptyStringIfNull(string var, string ReplaceWith = null)
    {
        string ret = null;
        if (var == null)
        {
            if (string.IsNullOrEmpty(ReplaceWith))
                ret = string.Empty;
            else
                ret = ReplaceWith;
        }
        else
        {
            ret = var;
        }
        return ret;
    }
    public static object ReturnEmptyStringIfNull(object var, object ReplaceWith = null)
    {
        object ret = null;
        if (var == null)
        {
            if (ReplaceWith == null)
                ret = null;
            else
                ret = ReplaceWith;
        }
        else
        {
            ret = var;
        }
        return ret;
    }

    public static void jsAlert(string msg)
    {
        //replace characters to prevent javascript problems
        msg = msg.Replace("'", "\"");
        msg = msg.Replace(((char)13).ToString() + ((char)10).ToString(), "\\n");

        //declare and register the script
        string jScript = "<script>alert('" + msg + "');</script>";
        System.Web.UI.Page P = (System.Web.UI.Page)System.Web.HttpContext.Current.Handler;
        P.ClientScript.RegisterStartupScript(P.GetType(), "", jScript);
    }

    public static void jsErrorAlert(string msg)
    {
        //set up the error message
        msg = "An error occured.\\n\\nPlease contact your administrator if the problem persists." + "\\n\\n_____________________________________________________\\n\\n" + "Error Details:\\n\\n" + msg + "\\n_____________________________________________________";

        jsAlert(msg);
    }
    
    public static void AddEmptyRowToDataTable(DataTable dt)
    {
        //insert a blank row
        DataRow dr = dt.NewRow();
        dt.Rows.InsertAt(dr, 0);
    }    

    public static string StripChars(string s)
    {
        s = s.Replace(")", "");
        s = s.Replace("(", "");
        s = s.Replace("-", "");
        s = s.Replace(".", "");
        s = s.Replace("@", "");
        s = s.Replace("/", "");
        s = s.Replace("\\", "");
        s = s.Replace(" ", "");
        s = s.Replace("_", "");

        return s;
    }

    public static string StripCharsForDecimal(string s)
    {
        s = s.Replace(")", "");
        s = s.Replace("(", "");
        s = s.Replace("-", "");
        s = s.Replace("%", "");
        s = s.Replace("@", "");
        s = s.Replace("/", "");
        s = s.Replace("\\", "");
        s = s.Replace(" ", "");
        s = s.Replace("_", "");
        s = s.Replace("$", "");

        return s;
    }

    public static void SetControlProps(Panel pnl, bool Locked)
    {
	    string css = "";
	    string RO = (Locked ? " ReadOnly" : "");

	    foreach (Control ctl in pnl.Controls) {
		    if (ctl is TextBox) {
			    TextBox txt = (TextBox)ctl;
			    css = txt.CssClass.Replace( " ReadOnly", "") + RO;
			    txt.ReadOnly = Locked | txt.ValidationGroup == "AlwaysLocked";
			    txt.CssClass = (txt.ValidationGroup == "AlwaysLocked" ? txt.CssClass : css);
		    } else if (ctl is DropDownList) {
			    DropDownList ddl = (DropDownList)ctl;
			    css = ddl.CssClass.Replace( " ReadOnly", "") + RO;
			    ddl.Enabled = !Locked;
			    ddl.CssClass = css;
		    } else if (ctl is CheckBoxList) {
			    CheckBoxList chk = (CheckBoxList)ctl;
			    chk.Enabled = !Locked;
		    } else if (ctl is CheckBox) {
			    CheckBox chk = (CheckBox)ctl;
			    chk.Enabled = !Locked;
		    } else if (ctl is RadioButtonList) {
			    RadioButtonList rdo = (RadioButtonList)ctl;
			    rdo.Enabled = !Locked;
		    } else if (ctl is Button) {
			    Button btn = (Button)ctl;
			    if (btn.CssClass == "Calendar") {
				    ctl.Visible = !Locked;
			    }
		    } 
	    }
    }

    public static void PopulatePanelControls(Panel pnl, DataTable dt, Boolean PopulateSubPanels = false)
    {
        int i = 0;
        try
        {
            //TextBox
            foreach (TextBox txt in pnl.Controls.OfType<TextBox>())
            {
                if (txt.ValidationGroup != "NoLoad")
                { txt.Text = dt.Rows[0][txt.ID.ToString()].ToString(); }
            }

            //DropDownList
            foreach (DropDownList ddl in pnl.Controls.OfType<DropDownList>())
            {
                if (ddl.ValidationGroup != "NoLoad" && !string.IsNullOrEmpty(dt.Rows[0][ddl.ID.ToString()].ToString()))
                {
                    ddl.SelectedValue = dt.Rows[0][ddl.ID.ToString()].ToString();
                }
            }

            //Image
            foreach (Image img in pnl.Controls.OfType<Image>())
            {
                img.ImageUrl = dt.Rows[0][img.ID.ToString()].ToString();
            }

            //CheckBox
            foreach (CheckBox chk in pnl.Controls.OfType<CheckBox>())
            {
                if (dt.Rows[0][chk.ID.ToString()].ToString() != "")
                    chk.Checked = Convert.ToBoolean(dt.Rows[0][chk.ID.ToString()]);
                else
                    chk.Checked = false;
            }

            //RadioButtonList
            foreach (RadioButtonList rdo in pnl.Controls.OfType<RadioButtonList>())
            {
                i = 0;
                for (i = 0; i <= rdo.Items.Count - 1; i++)
                {
                    rdo.Items[i].Selected = (rdo.Items[i].Value == dt.Rows[0][rdo.ID.ToString()].ToString());
                }
            }

            //////CheckBoxList
            ////foreach (CheckBoxList chk in pnl.Controls.OfType<CheckBoxList>())
            ////{
            ////    if (chk.ValidationGroup != "NoLoad")
            ////    {
            ////        i = 0;
            ////        for (i = 0; i <= chk.Items.Count - 1; i++)
            ////        {
            ////            chk.Items[i].Selected = (chk.Items[i].Value == dt.Rows[0][chk.ID.ToString()].ToString());
            ////        }
            ////    }
            ////}

            //HiddenField
            foreach (HiddenField hdn in pnl.Controls.OfType<HiddenField>())
            {
                hdn.Value = dt.Rows[0][hdn.ID.ToString()].ToString();
            }

            //Panel (Sub-Panels)
            foreach (Panel spnl in pnl.Controls.OfType<Panel>())
            {
                if (PopulateSubPanels)
                {
                    PopulatePanelControls(spnl, dt);
                }
            }

            //UpdatePanel
            foreach (UpdatePanel upnl in pnl.Controls.OfType<UpdatePanel>())
            {
                PopulatePanelControls(upnl, dt, PopulateSubPanels);
            }
        }
        catch
        {
            //no errors when [textboxname] not found in dt...
        }
    }

    public static void PopulatePanelControls(UpdatePanel pnl, DataTable dt, Boolean PopulateSubPanels = false)
    {
        int i = 0;
        try
        {
            //TextBox
            foreach (TextBox txt in pnl.ContentTemplateContainer.Controls.OfType<TextBox>())
            {
                if (txt.ValidationGroup != "NoLoad")
                { txt.Text = dt.Rows[0][txt.ID.ToString()].ToString(); }
            }

            //DropDownList
            foreach (DropDownList ddl in pnl.ContentTemplateContainer.Controls.OfType<DropDownList>())
            {
                if (ddl.ValidationGroup != "NoLoad" && !string.IsNullOrEmpty(dt.Rows[0][ddl.ID.ToString()].ToString()))
                {
                    ddl.SelectedValue = dt.Rows[0][ddl.ID.ToString()].ToString();
                }
            }

            //Image
            foreach (Image img in pnl.ContentTemplateContainer.Controls.OfType<Image>())
            {
                img.ImageUrl = dt.Rows[0][img.ID.ToString()].ToString();
            }

            //CheckBox
            foreach (CheckBox chk in pnl.ContentTemplateContainer.Controls.OfType<CheckBox>())
            {
                if (dt.Rows[0][chk.ID.ToString()].ToString() != "")
                    chk.Checked = Convert.ToBoolean(dt.Rows[0][chk.ID.ToString()]);
                else
                    chk.Checked = false;
            }

            //RadioButtonList
            foreach (RadioButtonList rdo in pnl.ContentTemplateContainer.Controls.OfType<RadioButtonList>())
            {
                i = 0;
                for (i = 0; i <= rdo.Items.Count - 1; i++)
                {
                    rdo.Items[i].Selected = (rdo.Items[i].Value == dt.Rows[0][rdo.ID.ToString()].ToString());
                }
            }

            //CheckBoxList
            foreach (CheckBoxList chk in pnl.ContentTemplateContainer.Controls.OfType<CheckBoxList>())
            {
                i = 0;
                for (i = 0; i <= chk.Items.Count - 1; i++)
                {
                    chk.Items[i].Selected = (chk.Items[i].Value == dt.Rows[0][chk.ID.ToString()].ToString());
                }
            }

            //HiddenField
            foreach (HiddenField hdn in pnl.ContentTemplateContainer.Controls.OfType<HiddenField>())
            {
                hdn.Value = dt.Rows[0][hdn.ID.ToString()].ToString();
            }

            //Panel (Sub-Panels)
            foreach (Panel spnl in pnl.ContentTemplateContainer.Controls.OfType<Panel>())
            {
                if (PopulateSubPanels)
                {
                    PopulatePanelControls(spnl, dt, PopulateSubPanels);
                }
            }

            //UpdatePanel
            foreach (UpdatePanel upnl in pnl.ContentTemplateContainer.Controls.OfType<UpdatePanel>())
            {
                PopulatePanelControls(upnl, dt, PopulateSubPanels);
            }
        }
        catch
        {
            //no errors when [textboxname] not found in dt...
        }
    }

    public static void ResetPanelControls(Panel pnl, Boolean IncludeSubPanels = false)
    {
        int i = 0;
        TextBox txt = null;
        DropDownList ddl = null;
        RadioButtonList rdo = null;
        CheckBoxList cbl = null;
        HiddenField hdn = null;
        Panel SubPnl = null;
        Literal lit = null;
        //FreeTextBoxControls.FreeTextBox ftb = null;
        CheckBox ckb = null;

        //Image
        foreach (Image img in pnl.Controls.OfType<Image>())
        {
            img.ImageUrl = "";
        }


        foreach (Control ctl in pnl.Controls)
        {
            if (ctl is TextBox)
            {
                txt = (TextBox)ctl;
                txt.Text = "";
                txt.BackColor = DefaultColor;
            }
            else if (ctl is HiddenField)
            {
                hdn = (HiddenField)ctl;
                hdn.Value = "";
            }
            else if (ctl is DropDownList)
            {
                ddl = (DropDownList)ctl;
                ddl.SelectedIndex = -1;
                ddl.BackColor = DefaultColor;
            }
            else if (ctl is RadioButtonList)
            {
                rdo = (RadioButtonList)ctl;
                i = 0;
                for (i = 0; i <= rdo.Items.Count - 1; i++)
                {
                    rdo.Items[i].Selected = false;
                }
                rdo.BackColor = DefaultColor;
            }
            else if (ctl is CheckBoxList)
            {
                cbl = (CheckBoxList)ctl;
                i = 0;
                for (i = 0; i <= cbl.Items.Count - 1; i++)
                {
                    cbl.Items[i].Selected = false;
                }
                cbl.BackColor = DefaultColor;
            }
            else if (ctl is Panel & IncludeSubPanels)
            {
                SubPnl = (Panel)ctl;
                ResetPanelControls(SubPnl);
            }
            else if (ctl is Literal)
            {
                lit = (Literal)ctl;
                lit.Text = "";
            }
            else if (ctl is CheckBox)
            {
                ckb = (CheckBox)ctl;
                ckb.Checked = false;
            }
            else if (ctl is UpdatePanel & IncludeSubPanels)
            {
                ResetPanelControls((UpdatePanel)ctl, IncludeSubPanels);
            }
        }
    }

    public static void ResetPanelControls(UpdatePanel pnl, Boolean IncludeSubPanels = false)
    {
        int i = 0;
        TextBox txt = null;
        DropDownList ddl = null;
        RadioButtonList rdo = null;
        CheckBoxList cbl = null;
        HiddenField hdn = null;
        Panel SubPnl = null;
        Literal lit = null;
        //FreeTextBoxControls.FreeTextBox ftb = null;
        CheckBox ckb = null;

        //Image
        foreach (Image img in pnl.ContentTemplateContainer.Controls.OfType<Image>())
        {
            img.ImageUrl = "";
        }


        foreach (Control ctl in pnl.ContentTemplateContainer.Controls)
        {
            if (ctl is TextBox)
            {
                txt = (TextBox)ctl;
                txt.Text = "";
                txt.BackColor = DefaultColor;
            }
            else if (ctl is HiddenField)
            {
                hdn = (HiddenField)ctl;
                hdn.Value = "";
            }
            else if (ctl is DropDownList)
            {
                ddl = (DropDownList)ctl;
                ddl.SelectedIndex = -1;
                ddl.BackColor = DefaultColor;
            }
            else if (ctl is RadioButtonList)
            {
                rdo = (RadioButtonList)ctl;
                i = 0;
                for (i = 0; i <= rdo.Items.Count - 1; i++)
                {
                    rdo.Items[i].Selected = false;
                }
                rdo.BackColor = DefaultColor;
            }
            else if (ctl is CheckBoxList)
            {
                cbl = (CheckBoxList)ctl;
                i = 0;
                for (i = 0; i <= cbl.Items.Count - 1; i++)
                {
                    cbl.Items[i].Selected = false;
                }
                cbl.BackColor = DefaultColor;
            }
            else if (ctl is Panel & IncludeSubPanels)
            {
                SubPnl = (Panel)ctl;
                ResetPanelControls(SubPnl);
            }
            else if (ctl is Literal)
            {
                lit = (Literal)ctl;
                lit.Text = "";
            }
            else if (ctl is CheckBox)
            {
                ckb = (CheckBox)ctl;
                ckb.Checked = false;
            }
        }
    }

    public static void SetGridView(GridView gv, DataTable dt, GridViewSortEventArgs SortArgs, GridViewPageEventArgs PagerArgs)
    {
        string SortExp = "";
        try
        {
            // make sure the table has records
            if (dt.Rows.Count == 0)
            {
                //If there are records for this search let them know
                //Util.jsAlert("The search did not return any matches.")
                gv.DataSource = null;
                gv.DataBind();
            }
            else
            {
                //set the sorting if necessary
                if ((SortArgs != null))
                    SortExp = SortArgs.SortExpression;
                //Try/Catch here because if a page contains more than 1 gridview then setting the sorting
                try
                {
                    // could generate a "column not found" error. This occurs if the session variable for 
                    // SortExpression applies to a different gridview.
                    // If this happens we can just ignore it, gridview will revert to default sorting.
                    dt.DefaultView.Sort = SetSort(SortExp);
                }
                catch
                {
                }
                //set the Pager index if necessary
                if ((PagerArgs != null))
                    gv.PageIndex = PagerArgs.NewPageIndex;
                //set data source and bind
                gv.DataSource = dt;
                gv.DataBind();
            }
            //gv.DataSource = Nothing
            //gv.DataBind()
        }
        catch (Exception ex)
        {
            jsErrorAlert(ex.Message);
        }
    }

    public static string SetSort(string SortExp)
    {
        string returnString = "";
        string newSortExp = SortExp;
        //store the NEW sort expression, empty string if not included
        string newSortDir = "ASC";
        //store the NEW sort direction, assume ascending
        string currSortExp = System.Web.HttpContext.Current.Session["SortExpression"].ToString();
        //store the CURRENT sort expression
        string currSortDir = System.Web.HttpContext.Current.Session["SortDirection"].ToString();
        //store the CURRENT sort direction

        //if new sort expression is blank then just use the current sorting
        //  we do this because this function is called when the gridview paging occurs
        if (string.IsNullOrEmpty(newSortExp))
        {
            newSortExp = currSortExp;
            newSortDir = currSortDir;
            //if the new sort expression is the same as the current one then switch the sorting direction
        }
        else if (newSortExp == currSortExp & newSortDir == currSortDir)
        {
            newSortDir = "DESC";
        }

        //store the new settings in session variables for next time this function is called
        System.Web.HttpContext.Current.Session["SortExpression"] = newSortExp;
        System.Web.HttpContext.Current.Session["SortDirection"] = newSortDir;

        //only build the return string if we are sorting by something
        if (!string.IsNullOrEmpty(newSortExp))
        {
            returnString = newSortExp + " " + newSortDir;
        }

        return returnString;
    }

    public static void ShowErrorMessage(string ErrMsg)
    {
        ErrMsg = "The information could not be submitted.\\n\\nPlease correct the following errors:\\n" + ErrMsg;

        string jScript = "<script>alert('" + ErrMsg + "');</script>";

        //ClientScript.RegisterClientScriptBlock(Me.GetType, "jScriptClose", jScript)
        jsAlert(ErrMsg);
    }

    public static void LoadPanelLists(Panel pnl)
    {
        foreach (Control ctl in pnl.Controls)
        {
            if (ctl is DropDownList)
            {
                DropDownList ddl = (DropDownList)ctl; 
                if (ddl.ValidationGroup != "NoLoad")
                {
                    string sp = ddl.ID.Replace("ID", "");
                    sp = sp.Replace("Code", "");
                    sp = "usp_" + sp + "DDL_Sel";
                
                    Util.LoadList(ddl, sp);
                }
            }
            else if (ctl is CheckBoxList)
            {
                CheckBoxList chxlst = (CheckBoxList)ctl;
                if (chxlst.ValidationGroup != "NoLoad")
                {
                    string sp = chxlst.ID.Replace("ID", "");
                    sp = sp.Replace("Code", "");
                    sp = "usp_" + sp + "DDL_Sel";

                    Util.LoadList(chxlst, sp);
                }
            }
        }
    }

    public static void LoadPanelLists(UpdatePanel pnl)
    {
        foreach (Control ctl in pnl.ContentTemplateContainer.Controls)
        {
            if (ctl is DropDownList)
            {
                DropDownList ddl = (DropDownList)ctl;
                if (ddl.ValidationGroup != "NoLoad")
                {
                    string sp = ddl.ID.Replace("ID", "");
                    sp = sp.Replace("Code", "");
                    sp = "usp_" + sp + "DDL_Sel";

                    Util.LoadList(ddl, sp);
                }
            }
            else if (ctl is CheckBoxList)
            {
                CheckBoxList chxlst = (CheckBoxList)ctl;
                if (chxlst.ValidationGroup != "NoLoad")
                {
                    string sp = chxlst.ID.Replace("ID", "");
                    sp = sp.Replace("Code", "");
                    sp = "usp_" + sp + "DDL_Sel";

                    Util.LoadList(chxlst, sp);
                }
            }
        }
    }
    
    public static void LoadList(RadioButtonList rdo, string sp, string TextField, string ValueField, string[,] Parms = null)
    {
        try
        {
            DataMaven dm = new DataMaven();
            System.Data.SqlDbType dType = default(System.Data.SqlDbType);
            DataTable dt = null;
            int i = 0;

            dm.SetSP(sp);

            if ((Parms != null))
            {
                for (i = 0; i <= Parms.GetUpperBound(1); i++)
                {
                    if ((Parms != null))
                    {
                        dType = (SqlDbType)Parms.GetValue(i, 0);
                        dm.AddParameter(Parms.GetValue(i, 1).ToString(), dType, ParameterDirection.Input, Parms.GetValue(i, 2));
                    }
                }
            }

            dt = dm.ExecToDataTable();
            
            // Bind data to RadioButtonList
            rdo.DataSource = dt;
            rdo.DataTextField = TextField;
            rdo.DataValueField = ValueField;
            rdo.DataBind();
        }
        catch (Exception ex)
        {
            // Display error message
            Util.jsErrorAlert(ex.Message);
        }
    }

    public static void LoadList(CheckBoxList chk, string sp = null, string TextField = null, string ValueField = null, string Parms = null, string SelectedColumn = null)
    {
        try
        {
            if (sp == null)
            {
                sp = chk.ID.Replace("ID", "");
                sp = sp.Replace("Code", "");
                sp = "usp_" + sp + "DDL_Sel";
            }

            DataMaven dm = new DataMaven(sp, Parms);

            //set the value and Text fields
            if (ValueField == null)
            {
                string s = chk.ID.Replace("ID", "");
                s = s.Replace("Code", "");
                ValueField = s + "ID";
                TextField = s + "Name";
            }

            chk.DataValueField = ValueField;
            chk.DataTextField = TextField;

            DataTable dt = dm.ExecToDataTable();

            // Bind data to CheckBoxList
            chk.DataSource = dt;
            chk.DataTextField = TextField;
            chk.DataValueField = ValueField;
            chk.DataBind();

            // Set selected items, only if the DataTable contains a "Selected" column
            if (dt.Columns.Contains("Selected"))
            {
                for (int i = 0; i <= chk.Items.Count - 1; i++)
                {
                    DataRow[] result = dt.Select(SelectedColumn + " = " + chk.Items[i].Value);
                    chk.Items[i].Selected = Convert.ToBoolean(result[0]["Selected"]);
                }
            }
        }
        catch (Exception ex)
        {
            // Display error message
            Util.jsErrorAlert(ex.Message);
        }
    }

    public static void LoadList(DropDownList ddl, string SQL, string ValueField = null, string TextField = null)
    {
        try
        {
            DataMaven dm = new DataMaven(SQL);
            DataTable dt = dm.ExecToDataTable();

            // Add empty row
            DataRow dr = dt.NewRow();
            dt.Rows.InsertAt(dr, 0);

            //set the value and Text fields
            if (ValueField == null)
            {
                string s = ddl.ID.Replace("ID", "");
                s = s.Replace("Code", "");
                ValueField = s +"ID";
                TextField = s + "Name";
            }
            ddl.DataValueField = ValueField;
            ddl.DataTextField = TextField;

            // Bind data to DropDownList
            ddl.DataSource = dt;
            ddl.DataBind();
        }
        catch (Exception ex)
        {
            // Display error message
            Util.jsErrorAlert(ex.Message);

        }
    }

    public static string GetGridViewRowValue(GridViewRow row, string sControlName)
    {
        string sFieldValue = string.Empty;
        try
        {
            WebControl ctl = row.FindControl(sControlName) as WebControl;

            TextBox txt = null;
            CheckBox chk = null;
            DropDownList ddl = null;

            switch (ctl.GetType().ToString())
            { 
                case "System.Web.UI.WebControls.TextBox":
                    txt =  (TextBox)ctl;
                    sFieldValue = txt.Text;
                    break;
                case "System.Web.UI.WebControls.CheckBox":
                    chk = (CheckBox)ctl;
                    sFieldValue = chk.Checked.ToString();
                    break;
                case "System.Web.UI.WebControls.DropDownList":
                    ddl = (DropDownList)ctl;
                    sFieldValue = ddl.SelectedValue;
                    break;
            };

            return sFieldValue;
        }
        catch (Exception ex)
        {
            jsErrorAlert(ex.Message);
            return string.Empty;
        }
    }

    public static string LoggedInUser()
    {
        return HttpContext.Current.User.Identity.Name;
    }

    public static bool IsUserAuthenticated()
    {
        return HttpContext.Current.User.Identity.IsAuthenticated;
    }

    private static string DomainMapper(Match match)
    {
        Boolean invalid;
        // IdnMapping class with default property values.
        IdnMapping idn = new IdnMapping();

        string domainName = match.Groups[2].Value;
        try {
            domainName = idn.GetAscii(domainName);
        }
        catch (ArgumentException) {
            invalid = true;      
        }      
        return match.Groups[1].Value + domainName;
    }

    public static Boolean IsMobile()
    {
        bool m = false;
        if (HttpContext.Current.Request.Headers["User-Agent"] != "" 
            && (HttpContext.Current.Request.Browser["IsMobileDevice"] == "true" 
                || HttpContext.Current.Request.Browser["BlackBerry"] == "true" 
                || HttpContext.Current.Request.UserAgent.ToUpper().Contains("MIDP") 
                || HttpContext.Current.Request.UserAgent.ToUpper().Contains("CLDC") 
                || HttpContext.Current.Request.UserAgent.ToLower().Contains("iphone") 
                || HttpContext.Current.Request.UserAgent.ToLower().Contains("ipad") 
                || HttpContext.Current.Request.UserAgent.ToLower().Contains("android")))
        {
            m = true;
        }
        return m;
    }

    public static void HideTableColumns(GridView gv)
    {
        //hide all but first and last column
        for (int i = 1; i < gv.Columns.Count - 2; i++)
        {
            gv.Columns[i].Visible = false;
        }
    }

    public static void LoadSQLReport(string ReportName, ReportViewer rv)
    {
        rv.ServerReport.ReportServerUrl = new Uri(System.Configuration.ConfigurationManager.AppSettings["SQLReportServer"]);
        rv.ServerReport.ReportPath = "/" + System.Configuration.ConfigurationManager.AppSettings["SQLReportFolder"] + "/" + ReportName;

        rv.DataBind();
    }

    public static byte[] ReportToPDF(string ReportName, string param, string format = "PDF")
    {
        //valid formats are PDF, Excel, or Image
        ReportViewer rview = new ReportViewer();

        rview.ServerReport.ReportServerUrl = new Uri(System.Configuration.ConfigurationManager.AppSettings["SQLReportServer"]);
        rview.ServerReport.ReportPath = "/" + System.Configuration.ConfigurationManager.AppSettings["SQLReportFolder"] + "/" + ReportName;

        ReportParameterInfoCollection plist = rview.ServerReport.GetParameters();

        if (plist.Count > 0)
        {
            List<ReportParameter> pl = new List<ReportParameter>();
            switch (ReportName)
            {
                case "CertificateOfMailing":
                    pl.Add(new ReportParameter("ORSVEventIDs", param));
                    break;
                case "OOS_NR":
                case "PermitToSell":
                    pl.Add(new ReportParameter("ORSVID", param));
                    pl.Add(new ReportParameter("UserID", Util.LoggedInUser()));
                    break;
                default:
                    pl.Add(new ReportParameter("ORSVID", param));
                    break;
            }

            rview.ServerReport.SetParameters(pl);
        }

        string mimeType, encoding, extension, deviceInfo;
        string[] streamids;
        Warning[] warnings;

        deviceInfo = "<DeviceInfo><SimplePageHeaders>True</SimplePageHeaders></DeviceInfo>";

        byte[] bytes = rview.ServerReport.Render(format, deviceInfo, out mimeType, out encoding, out extension, out streamids, out warnings);

        //now return our bytes
        //dont eat them all
        return bytes;
    }

    public static string GetLinkList(string sDirectory, string sExtension, Boolean KeepPath = false)
    {
        try
        {
            string AppPath = HttpContext.Current.Request.PhysicalApplicationPath;
            string SearchDir = HttpContext.Current.Request.PhysicalApplicationPath + sDirectory;
            string[] filePaths = Directory.GetFiles(SearchDir, "*." + sExtension);

            string LinkList = "";
            string Link = "";
            string LinkText = "";
            string Dir = sDirectory;

            for (int i = 0; i <= filePaths.GetUpperBound(0); i++)
            {
                if (KeepPath)
                {
                    LinkText = filePaths[i].Replace(SearchDir, "");
                    Dir = sDirectory;
                    Dir = Dir.Replace(@"\", "/");
                    Dir = Dir.Substring(1, Dir.Length - 1);
                    Link = "/" + Dir + LinkText;
                }
                else
                {
                    Link = filePaths[i].Replace(SearchDir, "");
                    LinkText = Link;
                }
                LinkList += "<p><a target='_blank' href='" + Link + "'>" + LinkText + "<a/></p>";
            }

            return LinkList;
        }
        catch
        {
            return null;
        }
    }


    #region "Form Validation
    public static string FormValidation(String LabelName, TextBox ctl, ValidationType VType, Boolean Required = false, int MinLength = 0, int? MaxLength = null)
    {
        String Err = "";
        ctl.BackColor = DefaultColor;

        //First check if it can be blank
        if ((ctl.Text.Length == 0) && (Required == true))
        {
            //Cannot be empty
            ctl.BackColor = ErrorColor;
            Err += "- " + LabelName + " cannot be blank.<br/>";
            return Err;
        }
        else if ((ctl.Text.Length == 0) && (Required == false))
        {
            //Empty and not Required
            //Nothing to do
            return "";
        }

        Int64 tmpInt64;
        switch (VType)
        {
            case ValidationType.Length:
                if (ctl.Text.Length < MinLength)
                {
                    //Field isnt long enough
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " must be at least " + MinLength.ToString() + " characters long.<br/>";
                }
                else if ((!(MaxLength == null)) && (ctl.Text.Length > MaxLength))
                {
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " cannot be longer than " + MaxLength.ToString() + " characters long.<br/>";
                }
                break;

            case ValidationType.Date:
                DateTime tmpDate;

                if (DateTime.TryParse(ctl.Text, out tmpDate) == false)
                {
                    //not a valid date
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " must be a valid date.<br/>";
                }
                break;

            case ValidationType.Numeric:
                tmpInt64 = new Int64();

                if (Int64.TryParse(ctl.Text, out tmpInt64) == false)
                {
                    //not a valid number
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " must be numeric.<br/>";
                }
                break;

            case ValidationType.Decimal:
                Decimal tmpDecimal;

                if (Decimal.TryParse(StripCharsForDecimal(ctl.Text), out tmpDecimal) == false)
                {
                    //not a valid number
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " must be a valid decimal.<br/>";
                }
                break;

            case ValidationType.Email:
                try
                {
                    MailAddress Address = new MailAddress(ctl.Text);
                }
                catch (FormatException)
                {
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " must be a valid Email address.<br/>";
                }
                break;

            case ValidationType.Phone:
                Int64 tmpPhoneInt;

                if (StripChars(ctl.Text).Length > 0)
                {
                    if ((Int64.TryParse(StripChars(ctl.Text), out tmpPhoneInt) == false) || (!(StripChars(ctl.Text).Length >= 7)))
                    {
                        //not a valid number
                        ctl.BackColor = ErrorColor;
                        Err += "- " + LabelName + " must be a valid Phone number.<br/>";
                    }
                }
                else if (Required == true)
                {
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " cannot be blank.<br/>";
                }
                break;

            case ValidationType.ZIP:
                Int64 tmpZIP;

                if (Int64.TryParse(StripChars(ctl.Text), out tmpZIP) == false || !(StripChars(ctl.Text).Length >= 5))
                {
                    //not a valid zip
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " must be a valid ZIP Code.<br/>";
                }
                break;

            case ValidationType.YearBetween:
                tmpInt64 = new Int64();

                if (Int64.TryParse(ctl.Text, out tmpInt64) == false)
                {
                    //not a valid number
                    ctl.BackColor = ErrorColor;
                    Err += "- " + LabelName + " must be numeric.<br/>";
                }
                else
                {
                    int StartYear = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ValidationStartYear"].ToString());
                    int EndYear = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ValidationEndYear"].ToString());
                    int Year = Convert.ToInt32(ctl.Text);

                    if (!(Year >= StartYear && Year <= EndYear))
                    {
                        ctl.BackColor = ErrorColor;
                        Err += "- " + LabelName + " must be betweeen " + StartYear.ToString() + " and " + EndYear.ToString();
                    }
                }
                break;

            case ValidationType.VIN:
                if (VinValidator(ctl.Text) == null)
                {
                    Err += "- " + LabelName + "VIN cannot be decoded.";
                }
                break;
        }
        return Err;
    }

    public static string FormValidation(String LabelName, WebControl ctl, ValidationType VType)
    {
        String Err = "";
        ctl.BackColor = DefaultColor;

        if (ctl is DropDownList)
        {
            DropDownList DDL = (DropDownList)ctl;

            if (DDL.Text == "")
            {
                DDL.BackColor = ErrorColor;
                Err += "- " + LabelName + " must have an item selected.<br/>";
            }
        }

        return Err;
    }
    
    public static string VinValidator(string VIN)
    {
        if (VIN.Length != 17)
        {
            return "Invalid VIN: VIN must be 17 characters";
        }

        if (VIN.ToLower().IndexOf("o") != -1 || VIN.ToLower().IndexOf("i") != -1)
        {
            return @"Invalid VIN: VIN cannot contain the letters ""I"" or ""O""";
        }

        try
        {
            string url = "http://api.edmunds.com/v1/api/toolsrepository/vindecoder?vin=" + VIN + "&api_key=5u9qz3jeafthbcfsdk4zypgg&fmt=json";

            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();

            string json = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();

            //passed
            return json;
        }
        catch (WebException wex)
        {
            //failed
            System.IO.Stream stream = wex.Response.GetResponseStream();
            var resp = new System.IO.StreamReader(stream).ReadToEnd();

            try
            {
                dynamic obj = JsonConvert.DeserializeObject(resp);
                var messageFromServer = obj.error.message;
                return messageFromServer;
            }
            catch (Exception ex)
            {
                return "Invalid VIN";
            }
        }
    }

    #endregion
}
//Util