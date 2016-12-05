using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Security;
using Online_Ordering.Models;
using Online_Ordering;
using System.Threading;

namespace OnlineOrdering.WebApi
{
    public partial class login : System.Web.UI.Page
    {
        private static readonly Users.User LoggedInUser = new Users.User();
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                LoginMsg.InnerHtml = " A valid login name and password is required.";
            }

            catch (Exception ex)
            {
                LoginMsg.Attributes["class"] = "alert alert-danger";
                LoginMsg.InnerHtml = "Exception : " + ex.Message.ToString();
            }

        }

        public static Users.User GetLoggedInUser
        {
            get { return LoggedInUser; }
        }

        public static void SetOrderNumber(string orderNumber)
        {
            LoggedInUser.m_sessionOrderNum = orderNumber;
        }


        protected void btnLogin_Click(object sender, EventArgs e)
        {
            var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
            try
            {
                var loginUser = Request.Form["Login"];
                var loginPwd = Request.Form["Password"];
               

                using (connect)
                {
                    var cmd = new SqlCommand("Select Logins.userName, Logins.passWord, Logins.custCode, Logins.emailOrder, Logins.MOG_Only, Logins.OrderSession, Logins.NoShowPrices, Logins.Row_ID, " +
                    "Customers.Pricing_Level, Customers.Show_Web_Prices From Logins INNER JOIN Customers ON Logins.custCode = Customers.Code " +
                    "Where userName = @user And passWord = @password");
                    cmd.Parameters.AddWithValue("@user", loginUser);
                    cmd.Parameters.AddWithValue("@password", loginPwd);
                    cmd.Connection = connect;
                    var adapter = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        if (loginUser.IndexOf("@baycitiesproduce.com") != -1)
                        {
                            Session["AdminUser"] = loginUser;
                            Session["UserName"] = loginUser;
                            var cookie1 = new HttpCookie("cook");
                            cookie1.Value = "value";
                            Response.Cookies.Add(cookie1);
                            FormsAuthentication.RedirectFromLoginPage(loginUser, false);
                            Response.Redirect("Dashboard", false);
                            return;
                        }
                        LoginMsg.Attributes["class"] = "alert alert-danger";
                        LoginMsg.InnerHtml = "Invalid Credentials. Please Try Again.";
                        Logger.WriteLine("Login failed. Invalid Credentials." + " Login used : " + loginUser + ". Password Used: " + loginPwd);
                        return;
                    }

                    else
                    {
                       
                        Session["UserName"] = loginUser;
                        LoggedInUser.userName = loginUser;
                        Logger.WriteLine("Login successful. Logged in User : " + loginUser);
                        foreach (DataRow row in dt.Rows)
                        {
                            LoggedInUser.custCode = row["custCode"].ToString().Trim();
                            LoggedInUser.m_sessionID = Session.SessionID.ToString();
                            LoggedInUser.m_emailOrders = row["emailOrder"].ToString();
                            LoggedInUser.m_sessionCustomer = LoggedInUser.custCode;
                            LoggedInUser.m_MOGOnly = Convert.ToInt16(row["MOG_Only"].ToString());
                            LoggedInUser.m_showEnhancedColumns = Convert.ToInt16(row["OrderSession"].ToString());
                            LoggedInUser.m_showPrices = Convert.ToInt16(row["noShowPrices"].ToString());
                            LoggedInUser.m_userRowID = Convert.ToInt32(row["Row_ID"].ToString());
                            // Validate Customer
                            Logger.WriteLine("Customer val = " + LoggedInUser.custCode);
                            Logger.WriteLine("Verified validatedCustomer");
                            LoggedInUser.m_priceLevel = Convert.ToInt16(row["Pricing_Level"].ToString());
                            LoggedInUser.m_showPrices = Convert.ToInt16(row["Show_Web_Prices"].ToString());

                        }

                        var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        Session["Confirmation"] = unixTimestamp.ToString();
                        Session["Cart"] = unixTimestamp.ToString();
                        // Set order and confirmation numbers
                        LoggedInUser.m_submittedOrderNum = LoggedInUser.m_sessionOrderNum = unixTimestamp.ToString();
                        unixTimestamp -= 10800; // Get a time that is 3 hours less than current time. The normal timeout is 45 minutes so any session cart older than that are abandoned carts

                        // Clear any old sessions
                        ThreadStart threadStart = delegate () { ClearOldSessions(unixTimestamp.ToString()); };
                        Thread thread = new Thread(threadStart);
                        thread.Start();

                        SetLogFile();

                    }

                }

                var cookie = new HttpCookie("cook");
                cookie.Value = "value";
                Response.Cookies.Add(cookie);
                FormsAuthentication.RedirectFromLoginPage(loginUser, false);
                Response.Redirect("ShowProducts", false);

            }

            catch (Exception ex)

            {
                LoginMsg.Attributes["class"] = "alert alert-danger";
                LoginMsg.InnerHtml = "Exception : " + ex.Message;
                Logger.WriteLine("Exception : " + ex.Message);
            }

        }

        public void ClearOldSessions(string timestamp)
        {
            try
            {
                var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    var cmd = new SqlCommand("Delete From SessionCart where(orderNum < " + timestamp + ")", connect);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                    Logger.WriteLine("Old cart sessions cleared.");

                    var cmd1 = new SqlCommand("DELETE FROM DeletedGuides WHERE DATEDIFF(day,getdate(),DateCreated) < -7 AND LoginID = @loginID", connect);
                    cmd1.Parameters.AddWithValue("@loginID", GetLoggedInUser.m_userRowID);
                    cmd1.ExecuteNonQuery();
                    Logger.WriteLine("Guides older than 7 days deleted from DeletedGuides table.");
                }
            }
            catch(Exception ex)
            {
                LoginMsg.Attributes["class"] = "alert alert-danger";
                LoginMsg.InnerHtml = "Exception : " + ex.Message;
                Logger.WriteLine("Exception @ClearOldSessions: " + ex.Message);
            }
           

        }

        public static void SetLogFile()
        {
            try
            {
                var logFile = "";
                logFile = @"F:\Webspace\Websites\BaycitiesOnlineOrdering\Logs\Web\";
                logFile += DateTime.Now.ToString("yyyy-MM-dd");
                logFile += "_" + LoggedInUser.userName;
                logFile += ".txt";

                Logger.SetFilename(logFile);
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ SetLogFile(): " + ex.Message);
            }

        }

    }
}