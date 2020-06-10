using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Security.Cryptography;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace HPSBYS_RenewOnlineEnrollReminder_WS
{
    public class SmsHpsbys
    {



        string strusername = "hpgovt-HIMCARE";
        string strPassword = "Priya@dev@9";
        string senderid = "hpgovt";
        string SecureKey = "54f32344-5767-482d-8410-5ba3ae7e0463";

        public SmsHpsbys()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected String encryptedPasswod(String password)
        {

            byte[] encPwd = Encoding.UTF8.GetBytes(password);
            //static byte[] pwd = new byte[encPwd.Length];
            HashAlgorithm sha1 = HashAlgorithm.Create("SHA1");
            byte[] pp = sha1.ComputeHash(encPwd);
            // static string result = System.Text.Encoding.UTF8.GetString(pp);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in pp)
            {

                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();

        }

        protected String hashGenerator(String Username, String sender_id, String message, String secure_key)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(Username).Append(sender_id).Append(message).Append(secure_key);
            byte[] genkey = Encoding.UTF8.GetBytes(sb.ToString());
            //static byte[] pwd = new byte[encPwd.Length];
            HashAlgorithm sha1 = HashAlgorithm.Create("SHA512");
            byte[] sec_key = sha1.ComputeHash(genkey);

            StringBuilder sb1 = new StringBuilder();
            for (int i = 0; i < sec_key.Length; i++)
            {
                sb1.Append(sec_key[i].ToString("x2"));
            }
            return sb1.ToString();
        }

        public void SingleSMS(string moblineno, string message, string uniqueNo)
        {
            Stream dataStream;
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //note comment by NSA -SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 not compatable with .NET 4.0
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";
            ServicePointManager.CertificatePolicy = new MyPolicy1();
            String encryptedPassword = encryptedPasswod(strPassword);
            String NewsecureKey = hashGenerator(strusername, senderid, message, SecureKey);
            String smsservicetype = "singlemsg"; //For single message.
            String query = "username=" + HttpUtility.UrlEncode(strusername) +
                "&password=" + HttpUtility.UrlEncode(encryptedPassword) +
                "&smsservicetype=" + HttpUtility.UrlEncode(smsservicetype) +
                "&content=" + HttpUtility.UrlEncode(message) +
                "&mobileno=" + HttpUtility.UrlEncode(moblineno) +
                "&senderid=" + HttpUtility.UrlEncode(senderid) +
              "&key=" + HttpUtility.UrlEncode(NewsecureKey);

            byte[] byteArray = Encoding.ASCII.GetBytes(query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            String Status = ((HttpWebResponse)response).StatusDescription;
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            String responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();

            AddOccupation(moblineno, message, uniqueNo);
        }

        public void BulkSMS(string moblinenos, string message, string uniqueNo)
        {
            Stream dataStream;
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";
            ServicePointManager.CertificatePolicy = new MyPolicy1();
            String encryptedPassword = encryptedPasswod(strPassword);
            String NewsecureKey = hashGenerator(strusername, senderid, message, SecureKey);
            Console.Write(NewsecureKey);
            Console.Write(encryptedPassword);

            String smsservicetype = "bulkmsg"; // for bulk msg
            String query = "username=" + HttpUtility.UrlEncode(strusername) +
             "&password=" + HttpUtility.UrlEncode(encryptedPassword) +
             "&smsservicetype=" + HttpUtility.UrlEncode(smsservicetype) +
             "&content=" + HttpUtility.UrlEncode(message) +
             "&bulkmobno=" + HttpUtility.UrlEncode(moblinenos) +
             "&senderid=" + HttpUtility.UrlEncode(senderid) +
            "&key=" + HttpUtility.UrlEncode(NewsecureKey);
            Console.Write(query);
            byte[] byteArray = Encoding.ASCII.GetBytes(query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            String Status = ((HttpWebResponse)response).StatusDescription;
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            String responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();

            AddOccupation(moblinenos, message, uniqueNo);
        }

        /// <summary>
        /// Method for sending OTP MSG.
        /// </summary>
        /// <param name="username"> Registered user name</param>
        /// <param name="password"> Valid login password</param>
        /// <param name="senderid">Sender ID </param>
        /// <param name="mobileNo"> valid single Mobile Number </param>
        /// <param name="message">Message Content </param>
        /// <param name="secureKey">Department generate key by login to services portal</param>
        // Method for sending OTP MSG.
        public void sendOTPMSG(string moblineno, string message)
        {
            Stream dataStream;
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";
            ServicePointManager.CertificatePolicy = new MyPolicy1();
            String encryptedPassword = encryptedPasswod(strPassword);
            String key = hashGenerator(strusername, senderid, moblineno, SecureKey);
            String smsservicetype = "otpmsg"; //For OTP message.
            String query = "username=" + HttpUtility.UrlEncode(strusername) +
                "&password=" + HttpUtility.UrlEncode(encryptedPassword) +
                "&smsservicetype=" + HttpUtility.UrlEncode(smsservicetype) +
                "&content=" + HttpUtility.UrlEncode(moblineno) +
                "&mobileno=" + HttpUtility.UrlEncode(message) +
                "&senderid=" + HttpUtility.UrlEncode(senderid) +
              "&key=" + HttpUtility.UrlEncode(key);
            byte[] byteArray = Encoding.ASCII.GetBytes(query);
            request.ContentType = "application/x-www-form-urlencoded";

            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            String Status = ((HttpWebResponse)response).StatusDescription;
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            String responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
        }

        /// <summary>
        /// method for Sending unicode..
        /// </summary>
        /// <param name="username"> Registered user name</param>
        /// <param name="password"> Valid login password</param>
        /// <param name="senderid">Sender ID </param>
        /// <param name="mobileNo"> valid Mobile Numbers </param>
        /// <param name="Unicodemessage">Unicodemessage Message Content</param>
        /// <param name="secureKey">Department generate key by login to services portal</param>
        //method for Sending unicode..
        public string sendUnicodeSMS(string moblineno, string Unicodemessage)
        {
            Stream dataStream;
            HttpWebRequest request =
           (HttpWebRequest)WebRequest.Create("http://msdgweb.mgov.gov.in/esms/sendsmsrequest");
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLimit = 1;
            //((HttpWebRequest)request).UserAgent = ".NET Framework Example Client";
            ((HttpWebRequest)request).UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows 98; DigExt)";
            request.Method = "POST";
            string U_Convertedmessage = "";

            foreach (char c in Unicodemessage)
            {
                int j = (int)c;
                string sss = "&#" + j + ";";
                U_Convertedmessage = U_Convertedmessage + sss;
            }
            string encryptedPassword = encryptedPasswod(strPassword);
            string NewsecureKey = hashGenerator(strusername, senderid, U_Convertedmessage, SecureKey);

            string smsservicetype = "unicodemsg"; // for unicode msg
            string query = "username=" + HttpUtility.UrlEncode(strusername) +
                            "&password=" + HttpUtility.UrlEncode(encryptedPassword) +
                            "&smsservicetype=" + HttpUtility.UrlEncode(smsservicetype) +
                            "&content=" + HttpUtility.UrlEncode(U_Convertedmessage) +
                            "&bulkmobno=" + HttpUtility.UrlEncode(moblineno) +
                            "&senderid=" + HttpUtility.UrlEncode(senderid) +
                            "&key=" + HttpUtility.UrlEncode(NewsecureKey);

            byte[] byteArray = Encoding.ASCII.GetBytes(query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            string Status = ((HttpWebResponse)response).StatusDescription;
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }

        #region Send Mail
        public void sendMail(string Subject, string MailBody, string strMailID)
        {
            try
            {
                // Gmail Address from where you send the mail
                var fromAddress = ConfigurationManager.AppSettings["UserName"]; //reading from web.config  
                                                                                // any address where the email will be sending
                var toAddress = strMailID;
                //Password of your gmail address
                string fromPassword = ConfigurationManager.AppSettings["Password"]; //reading from web.config  
                                                                                    // smtp settings
                var smtp = new System.Net.Mail.SmtpClient();
                {
                    smtp.Host = ConfigurationManager.AppSettings["Host"];
                    smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                    smtp.Timeout = 20000;
                }
                // Passing values to smtp object
                smtp.Send(fromAddress, toAddress, Subject, MailBody);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        public void MaintainServiceLog(string inputText)
        {
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            SqlCommand sqlCmd;
            try
            {


                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                sqlCmd = new SqlCommand();
                sqlCmd.Connection = connection;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "USP_OnlineEnrollReminderSMS_ServiceLog";
                sqlCmd.CommandTimeout = 0;
                sqlCmd.Parameters.Add("@vchMessage", SqlDbType.VarChar, 8000).Value = inputText;
                sqlCmd.Parameters.Add("@dtmLogOn", SqlDbType.DateTime).Value = DateTime.Now;
                sqlCmd.ExecuteNonQuery();
                sqlCmd.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connection != null)
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
        public void AddOccupation(string moblineno, string message, string uniqueNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = connection;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "USP_HIMCARESMS_LOG";
                    cmd.Parameters.Add("@CHA_ACTIONCODE", SqlDbType.VarChar).Value = "B";
                    cmd.Parameters.Add("@moblineno", SqlDbType.VarChar).Value = moblineno;
                    cmd.Parameters.Add("@message", SqlDbType.VarChar).Value = message;
                    cmd.Parameters.Add("@uniqueNo", SqlDbType.VarChar).Value = uniqueNo;

                    connection.Open();
                    cmd.ExecuteNonQuery();

                }

            }
            catch (Exception ex)
            {
                MaintainServiceLog("Service Error: " + ex.Message + ex.StackTrace);
            }

        }


    }

    public class MyPolicy1 : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            return true;
        }
    }
}
