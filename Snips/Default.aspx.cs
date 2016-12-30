using System;
using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Data;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class _Default : System.Web.UI.Page
{
    Mongo m = new Mongo();
    JsonTranslator Translator = new JsonTranslator();
    DeviceMagicAPI DVAPI = new DeviceMagicAPI();

    //MongoExtension ME = new MongoExtension();

    public void Page_Load(object sender, EventArgs e)
    {
        IMongoCollection<BsonDocument> Collection = Connect();
       
        Collection.Database.DropCollection("DeviceMagic");
        Collection.Database.CreateCollection("DeviceMagic");        
    }

  
    protected void btnForm_Click(object sender, EventArgs e)
    {
        DVAPI = new DeviceMagicAPI();
        //List<JsonTranslator.FormData> Forms = DVAPI.GetForm(txtQuery.Text);

        //dmGridView.DataSource = Forms;
                       dmGridView.DataBind();

    }



    


    protected void btnTEST_Click1(object sender, EventArgs e)
    {

        m = new Mongo();
        //Grab all form data
        DVAPI = new DeviceMagicAPI();
        List<JsonTranslator.RootObject> RootO = DVAPI.GetForm(txtQuery.Text);




        Translator = new JsonTranslator();
        DataTable dtRoot = Translator.dtRoot();

        foreach (JsonTranslator.RootObject Rooto in RootO)
        {


            if (Rooto.submissions != null)
            {
                DataRow dr = dtRoot.NewRow();
                dr["per_page"] = Rooto.per_page;
                dr["current_page"] = Rooto.current_page;
                dr["total_pages"] = Rooto.total_pages;
                dr["current_count"] = Rooto.current_count;
                dr["total_count"] = Rooto.total_count;
                dr["submissions"] = Rooto.submissions;


                dtRoot.Rows.Add(dr);
            }
        }

        foreach (DataRow row in dtRoot.Rows)
        {

            m.InsertSingleRecord(row.ToBsonDocument());
        }

        dmGridView.DataSource = RootO;
        dmGridView.DataBind();



        //**********************************************************************************************************************************************

        //string BASE_URL = "https://www.devicemagic.com/api/forms/";
        //string USER_NAME = "3vbYNVjphf1_5wJeFsHt";
        //string PASSWORD = "x";
        //m = new Mongo();

        //HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + txtQuery.Text + "/device_magic_database.json");
        //string authInfo = USER_NAME + ":" + PASSWORD;
        //authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
        //req.Headers["Authorization"] = "Basic " + authInfo;

        //HttpWebResponse response = (HttpWebResponse)req.GetResponse();
        //Stream s = response.GetResponseStream();

        //string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        //JObject jsonO = JObject.Parse(json);
        //List<JsonTranslator.Submissions> bob =  JsonConvert.DeserializeObject<List<JsonTranslator.Submissions>>(json);
        ////lblDeviceMagic.Text = json.ToString();

        //dmGridView.DataSource = bob.ToString();
        //dmGridView.DataBind();
        //m.InsertSingleRecord(bob.ToBsonDocument());

    }

    protected void btnTEST2_Click(object sender, EventArgs e)
    {
        m = new Mongo();
        //Grab all form data
        DVAPI = new DeviceMagicAPI();
        List<JsonTranslator.Forms> Forms = DVAPI.GetList();
         
        


        Translator = new JsonTranslator();
        DataTable dtForms = Translator.dtForms();

        // remove all documents in the "stickynotes" collection

        //Loop through form data.  Put 'Exit Ticket' data into a datatable
        foreach (JsonTranslator.Forms Form in Forms)
        {


            if (Form.Description.Contains("Exit Ticket"))
            {
                DataRow dr = dtForms.NewRow();
                dr["id"] = Form.ID;
                dr["name"] = Form.Name;
                dr["namespace"] = Form.Namespace;
                dr["version"] = Form.Version;
                dr["description"] = Form.Description;
                dr["group"] = Form.Group;

                dtForms.Rows.Add(dr);
            }
        }
        //Loop through form datatable, pull the submittion data and put into datatable
        //foreach (DataRow row in dtForms.Rows)
        //{
        //    DVAPI.GetForm(row["id"].ToString());
        //}

        foreach (DataRow row in dtForms.Rows)
        {

            m.InsertSingleRecord(row.ToBsonDocument());
        }

        dmGridView.DataSource = Forms;
        dmGridView.DataBind();
    }

    private IMongoCollection<BsonDocument> Connect()
    {
        MongoClient client = new MongoClient("mongodb://localhost:27017"); //Host
        IMongoDatabase database = client.GetDatabase("MasteryPrep"); //Database
        return database.GetCollection<BsonDocument>("DeviceMagic"); //Table
    }


}
