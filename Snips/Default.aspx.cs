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
