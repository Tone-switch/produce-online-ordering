using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Online_Ordering.Models;
using System.Data;
using System.Web.Security;
using System.Web;

namespace Online_Ordering.Controllers
{
    public class UsersController : ApiController
    {
        SqlConnection _connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

        public HttpResponseMessage GetUser(string loginUser, string loginPwd)
        {
            Users.User LoggedInUser = new Users.User();
            using (_connect)
            {
                SqlDataAdapter da = new SqlDataAdapter("select * from Logins where userName ='" + loginUser.Trim() + "' and passWord ='" + loginPwd + "'", _connect);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count < 1)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found.");
                }

                else
                {
                    //Logger.WriteLine("Verified validated login");
                    foreach (DataRow dr in dt.Rows)
                    {
                        LoggedInUser.userName = dr["userName"].ToString();
                        LoggedInUser.custCode = dr["custCode"].ToString();
                        LoggedInUser.m_emailOrders = dr["emailOrder"].ToString();
                        LoggedInUser.m_sessionCustomer = LoggedInUser.custCode;
                        LoggedInUser.m_MOGOnly = Convert.ToInt16(dr["MOG_Only"].ToString());
                        LoggedInUser.m_showEnhancedColumns = Convert.ToInt16(dr["OrderSession"].ToString());
                        LoggedInUser.m_showPrices = Convert.ToInt16(dr["noShowPrices"].ToString());
                        LoggedInUser.m_userRowID = Convert.ToInt16(dr["Row_ID"].ToString());
                    }

                    SqlDataAdapter da1 = new SqlDataAdapter("SELECT * FROM Customers WHERE Code = '" + LoggedInUser.custCode + "'", _connect);
                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);

                    if (dt1.Rows.Count > 1)
                    {
                        // Logger.WriteLine("Verified validatedCustomer");
                        foreach (DataRow dr in dt1.Rows)
                        {
                            LoggedInUser.m_priceLevel = Convert.ToInt16(dr["Pricing_Level"].ToString());
                            // use the customer master settings which will over ride login setting
                            LoggedInUser.m_showPrices = Convert.ToInt16(dr["Show_Web_Prices"].ToString());
                            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                            // ** Session variables should be put into js file or separate class

                            //Session["Confirmation"] = unixTimestamp.ToString();
                            //Session["Cart"] = unixTimestamp.ToString();
                            // Set order and confirmation numbers
                            //SessionVariables.m_submittedOrderNum = SessionVariables.m_sessionOrderNum = unixTimestamp.ToString();
                            unixTimestamp -= 10800; // Get a time that is 3 hours less than current time. The normal timeout is 45 minutes so any session cart older than that are abandoned carts
                            // Clear any old sessions
                            SqlCommand cmd = new SqlCommand("Delete from SessionCart where orderNum < " + unixTimestamp.ToString() );
                            cmd.ExecuteNonQuery();
                            //Logger.WriteLine("Clear old cart sessions");

                        }
                    }
                }

            }
            return Request.CreateResponse(HttpStatusCode.OK, LoggedInUser);
        }

        [HttpPost]
        [ActionName("LogOut")]
        public void LogOut()
        {
            FormsAuthentication.SignOut();

            if (HttpContext.Current.Session != null)
                HttpContext.Current.Session.Abandon();
            FormsAuthentication.RedirectToLoginPage();
        }

        
    }
}