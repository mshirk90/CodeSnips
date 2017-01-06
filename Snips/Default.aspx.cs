using System;
using System.Collections.Generic;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Linq;
using System.Threading.Tasks;
using System.Data;

public partial class _Default : System.Web.UI.Page
{
    Mongo m = new Mongo();
    JsonTranslator Translator = new JsonTranslator();
    DeviceMagicAPI DVAPI = new DeviceMagicAPI();


    public void Page_Load(object sender, EventArgs e)
    {
       
    }

    private void LoadFormList()
    {
        m = new Mongo();
        DVAPI = new DeviceMagicAPI();

        IMongoCollection<BsonDocument> Collection = Connect(); //Connect to Mongo
        string jsonList = DVAPI.GetFormList(""); //Pull form data from Device Magic


        try //Insert current data into Mongo
        {
            
            var jsonReader2 = new MongoDB.Bson.IO.JsonReader(jsonList);
            
                var context2 = BsonDeserializationContext.CreateRoot(jsonReader2);
                var documentList = Collection.DocumentSerializer.Deserialize(context2);

            m.InsertSingleRecord(documentList); // Insert doc into mongo

            lblDeviceMagic.Text = "Data Successfully Added"; //User validation
        }
        catch (Exception ex) 
        {
            lblDeviceMagic.Text = "Error Importing Data  ->" + ex;
        }        

        List<JsonTranslator.Forms> Forms = DVAPI.GetList();
        //dmGridView.DataSource = Forms;
        //dmGridView.DataBind();
    }

    private void LoadForms(string formID)
    {
        m = new Mongo();
        DVAPI = new DeviceMagicAPI();

        IMongoCollection<BsonDocument> Collection = Connect(); //Connect to Mongo
        string jsonForm = DVAPI.GetJson(formID.ToString()); //Pull form data from Device Magic
        
   
        try //Insert current data into Mongo
        {
            var jsonReader = new MongoDB.Bson.IO.JsonReader(jsonForm);

            var context = BsonDeserializationContext.CreateRoot(jsonReader);
            var document = Collection.DocumentSerializer.Deserialize(context);
            
                m.InsertSingleRecord(document); // Insert forms into mongo
           
            
            lblDeviceMagic.Text = "Data Successfully Added"; //User validation
        }
        catch (Exception ex)
        {
            lblDeviceMagic.Text = "Error Importing Data  ->" + ex;
        }
    }   

    protected void btnRefreshDatabase_Click(object sender, EventArgs e)
    {
        IMongoCollection<BsonDocument> Collection = Connect();

        Collection.Database.DropCollection("DeviceMagic");
        Collection.Database.CreateCollection("DeviceMagic");

        List<JsonTranslator.Forms> Forms = DVAPI.GetList();
        //dmGridView.DataSource = Forms;
        //dmGridView.DataBind();

        foreach (JsonTranslator.Forms form in Forms)
        {
            if (form != null)
            {

                var formID = form.ID;
                LoadForms(formID.ToString());
            }           
        }
        LoadFormList();
    }

    private IMongoCollection<BsonDocument> Connect()
    {
        MongoClient client = new MongoClient("mongodb://localhost:27017"); //Host
        IMongoDatabase database = client.GetDatabase("MasteryPrep"); //Database
        return database.GetCollection<BsonDocument>("DeviceMagic"); //Table
    }

}

