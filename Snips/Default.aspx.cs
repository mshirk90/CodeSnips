using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Net;
using System.IO;
using DeviceMagicAPI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace DeviceMagicAPI
{
    public partial class _Default : Page
    {

        static string BASE_URL = "https://www.devicemagic.com/organizations/10";
        static string USER_NAME = "3vbYNVjphf1_5wJeFsHt";
        static string PASSWORD = "x";
        

        public void Page_Load(object sender, EventArgs e)
        {

        }



        public void Main(string[] args)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/devices.xml");
            string authInfo = USER_NAME + ":" + PASSWORD;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream s = response.GetResponseStream();

            StreamReader reader = new StreamReader(s, Encoding.UTF8);
            string content = reader.ReadToEnd();

            Console.WriteLine(content);
            Console.ReadLine();
        }

        public void btnText_Click(object sender, EventArgs e)
        {

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/devices.json");
            string authInfo = USER_NAME + ":" + PASSWORD;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream s = response.GetResponseStream();

            StreamReader reader = new StreamReader(s, Encoding.UTF8);
            string jsonData = reader.ReadToEnd();
            dynamic o = JsonConvert.DeserializeObject(jsonData);
            

            JObject json = JObject.Parse(o);

            txtData.Text = json.ToString();


        }

        public void btnLabel_Click(object sender, EventArgs e)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/devices.xml");
            string authInfo = USER_NAME + ":" + PASSWORD;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream s = response.GetResponseStream();

            StreamReader reader = new StreamReader(s, Encoding.UTF8);
            string content = reader.ReadToEnd();

            Console.WriteLine(content);
            Console.ReadLine();


            lblData.Text = content;
        }
    }
}







using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MagicDeviceApiTest;

namespace MagicDeviceApiTest
{
    public partial class _Default : Page
    {
        static string BASE_URL = "https://www.devicemagic.com/organizations/10";
        static string USER_NAME = "3vbYNVjphf1_5wJeFsHt";
        static string PASSWORD = "x";

        protected void Page_Load(object sender, EventArgs e, string[] args)
        {

        }



        public void btnDeviceMagic_Click(object sender, EventArgs e)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/forms.json");
                string authInfo = USER_NAME + ":" + PASSWORD;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                req.Headers["Authorization"] = "Basic " + authInfo;

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                Stream sResponse = response.GetResponseStream();
                string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                lblDeviceMagic.Text = json.ToString();

            }
            catch (WebException wex)
            {
                //failed
                Stream stream = wex.Response.GetResponseStream();
                var resp = new System.IO.StreamReader(stream).ReadToEnd();

                try
                {
                    dynamic obj = JsonConvert.DeserializeObject(resp);
                    var messageFromServer = obj.error.message;
                    lblDeviceMagic.Text = messageFromServer.ToString();
                }
                catch (Exception ex)
                {

                }
            }
        }





        public static void Main(string[] args)
        {
            TextBox ta = new TextBox();

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/devices.xml");
            string authInfo = USER_NAME + ":" + PASSWORD;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream s = response.GetResponseStream();

            StreamReader reader = new StreamReader(s, Encoding.UTF8);
            string content = reader.ReadToEnd();

            Console.WriteLine(content);           
            Console.ReadLine();


        }

    }
}





using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MagicDeviceApiTest;
using System.ComponentModel;

namespace MagicDeviceApiTest
{
    public partial class _Default : Page
    {
        static string BASE_URL = "https://www.devicemagic.com/organizations/104196";
        static string USER_NAME = "3vbYNVjphf1_5wJeFsHt";
        static string PASSWORD = "x";

        public void Page_Load(object sender, EventArgs e)
        {
            try
            {


                //lblDeviceMagic.Text = json.ToString();

            }
            catch (WebException wex)
            {
                //failed
                Stream stream = wex.Response.GetResponseStream();
                var resp = new System.IO.StreamReader(stream).ReadToEnd();

                try
                {
                    dynamic obj = JsonConvert.DeserializeObject(resp);
                    var messageFromServer = obj.error.message;
                    lblDeviceMagic.Text = messageFromServer.ToString();
                }
                catch (Exception ex)
                {
                    lblDeviceMagic.Text = ex.Message;
                }
            }
        }

        public void btnFormList_Click(object sender, EventArgs e)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/forms.json");
            string authInfo = USER_NAME + ":" + PASSWORD;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream sResponse = response.GetResponseStream();
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JObject jsonO = JObject.Parse(json);
            FormList forms = JsonConvert.DeserializeObject<FormList>(json);

            dmGridView.DataSource = forms.forms;
            dmGridView.DataBind();
        }


        //public void btnSelectID_Click(object sender, EventArgs e)
        //{
        //    string SelectedRow = string.Empty;
        //    if (dmGridView.SelectedIndex != null)
        //    {
        //        dmGridView.SelectedIndex[1] = add
        //    }

        //    var index = Convert.ToInt32(dmGridView.SelectedIndex[0].Value);


        //    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/forms/" + btnSelectID_Click.dmGridView. + ".json");
        //    string authInfo = USER_NAME + ":" + PASSWORD;
        //    authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
        //    req.Headers["Authorization"] = "Basic " + authInfo;

        //    HttpWebResponse response = (HttpWebResponse)req.GetResponse();
        //    Stream sResponse = response.GetResponseStream();
        //    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        //    JObject jsonO = JObject.Parse(json);
        //    FormDataList forms = JsonConvert.DeserializeObject<FormDataList>(json);

        //    dmGridView.DataSource = forms.children;
        //    dmGridView.DataBind();
        //}

        protected void btnForm_Click(object sender, EventArgs e)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(BASE_URL + "/forms/" + txtQuery.Text + ".json");
            string authInfo = USER_NAME + ":" + PASSWORD;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            req.Headers["Authorization"] = "Basic " + authInfo;

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream sResponse = response.GetResponseStream();
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JObject jsonO = JObject.Parse(json);
            FormDataList forms = JsonConvert.DeserializeObject<FormDataList>(json);

            dmGridView.DataSource = forms.children;
            dmGridView.DataBind();
        }

        public class Forms
        {
            [JsonProperty("id")]
            public int ID { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("namespace")]
            public string Namespace { get; set; }
            [JsonProperty("version")]
            public decimal Version { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
            [JsonProperty("group")]
            public string Group { get; set; }
        }
        public class FormList
        {
            [JsonProperty("forms")]
            public List<Forms> forms { get; set; }
        }


        public class FormData
        {

            [JsonProperty("identifier")]
            public string Identifier { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("autoidentifier")]
            public bool AutoIdentifier { get; set; }

            [JsonProperty("options")]
            public List<DropDownList> Options { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("hint")]
            public string Hint { get; set; }

            [JsonProperty("required_rule")]
            public string R_Rule { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("visible_rule")]
            public string V_Rule { get; set; }

            [JsonProperty("visible_expr")]
            public string Visible_Expression { get; set; }

            [JsonProperty("customOptionIdentifier")]
            public bool OptionIdentifier { get; set; }

            [JsonProperty("auto_resolve")]
            public bool Auto_Resolve { get; set; }

            [JsonProperty("multiple")]
            public bool Multiple { get; set; }

           

        }



        public class FormDataList
        {
            [JsonProperty("children")]
            public List<FormData> children { get; set; }
        }
    }
}



