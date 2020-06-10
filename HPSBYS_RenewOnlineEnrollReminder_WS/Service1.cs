using System.Threading;
using System.Configuration;
using System;
using System.ServiceProcess;
using System.Data;
using System.Data.SqlClient;


namespace HPSBYS_RenewOnlineEnrollReminder_WS
{
    public partial class Service1 : ServiceBase
    {
        private Timer Schedular;
        public Service1()
        {
            InitializeComponent();
        }

        private void WriteToFile(string text)
        {          
            MaintainServiceLog(text);       
        }

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

        protected override void OnStart(string[] args)
        {
            try
            {
                WriteToFile("Service is going to start");
                ScheduleService();
            }
            catch (Exception ex)
            {
                WriteToFile("Service Error: " + ex.Message + ex.StackTrace);

            }
        }

        private void SchedularCallback(object e)
        {
            this.WriteToFile("Service Started");
            //this.ScheduleService();
        }
        protected override void OnStop()
        {
            try
            {

                WriteToFile("Service Stopped");
                //ScheduleService();
            }
            catch (Exception ex)
            {
                WriteToFile("Service Error: " + ex.Message + ex.StackTrace);

            }
        }

        public void ScheduleService()
        {
            try
            {
                DateTime NextScheduledTime = DateTime.Now;
                Schedular = new Timer(new TimerCallback(SchedularCallback));

                if (String.Format("{0:HH:mm}", DateTime.Now) == String.Format("{0:HH:mm}", NextScheduledTime) || String.Format("{0:HH:mm}", DateTime.Now) == String.Format("{0:HH:mm}", NextScheduledTime.AddSeconds(-1)))
                {
                    //Debugger.Launch();
                    SendSMSReminder();

                    NextScheduledTime = DateTime.Now.AddMinutes(Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["NextScheduleHour"].ToString()));
                }

                WriteToFile("NextScheduledTime: " + NextScheduledTime);
                TimeSpan timeSpan = NextScheduledTime.Subtract(DateTime.Now);

                //Get the difference in Minutes between the Scheduled and Current Time.
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);
                //Change the Timer's Due Time.
                Schedular.Change(dueTime, Timeout.Infinite);
                WriteToFile("Service Completed");
            }
            catch (Exception ex)
            {
                WriteToFile("Service Error: " + ex.Message + ex.StackTrace);

                //Stop the Windows Service.
                using (System.ServiceProcess.ServiceController serviceController = new System.ServiceProcess.ServiceController("SimpleService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private void SendSMSReminder()
        {
            DataTable dsnew = new DataTable();
            SqlCommand sqlCom = new SqlCommand();
            SmsHpsbys smsObj = new SmsHpsbys();

            SqlDataAdapter sqlDA = new SqlDataAdapter();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
                {
                    sqlCom = new SqlCommand();
                    sqlCom.CommandText = "USP_ONLINE_ENROLLMENT_RENEW_WINDOWSERVICE";
                    sqlCom.CommandType = CommandType.StoredProcedure;
                    sqlCom.Connection = conn;
                    sqlCom.CommandTimeout = 100000;
                    sqlDA = new SqlDataAdapter(sqlCom);
                    sqlDA.Fill(dsnew);
                }

                if (dsnew.Rows.Count > 0)
                {
                    foreach (DataRow row in dsnew.Rows)
                    {
                        string mobileno = row["vchMobile"].ToString();
                        string daysleft = row["RemainingDays"].ToString();
                        string rationcardno = row["URN"].ToString();
                        string username = row["Ename"].ToString();
                        string policyEndDate = row["policyenddate"].ToString();

                        //Dear Tapas Kumar Samal, Your HIMCARE Policy No - 020220022002200 will expire on 24 - 01 - 2020.After 30 days of Expiry date of Policy, You wont able to Renew your Policy.Kindly visit "http://hpsbys.in/" to Renew your HIMCARE Policy.

                        string message = "Dear " + username + "," + Environment.NewLine + "Your HIMCARE Policy No - " + rationcardno + " will expire on " + policyEndDate + ".After 30 days of Expiry date of Policy, You wont able to Renew your Policy.Kindly visit https://hpsbys.in/ to Renew your HIMCARE Policy.";
                        smsObj.SingleSMS(mobileno, message, rationcardno);                   
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile("Service Error: " + ex.Message + ex.StackTrace);
            }
            finally
            {
                dsnew.Dispose();
            }

        }
    }
}
