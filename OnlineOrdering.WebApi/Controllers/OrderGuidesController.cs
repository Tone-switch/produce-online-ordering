using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OnlineOrdering.WebApi.Models;
using Online_Ordering;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace OnlineOrdering.WebApi.Controllers
{
    [Authorize]
    public class OrderGuidesController : ApiController
    {

        [ActionName("GetSession")]
        [HttpGet]
        public HttpResponseMessage GetSession()
        {

            try
            {
                var session = HttpContext.Current.Session;
                var sessionValue = session["UserName"];
                var isAuthenticated = User.Identity.IsAuthenticated;

                if (isAuthenticated == false)
                {
                    FormsAuthentication.SignOut();
                    sessionValue = null;
                }

                if (sessionValue == null)
                {
                    FormsAuthentication.SignOut();
                }

                var message = Request.CreateResponse(HttpStatusCode.OK, sessionValue);
                message.Headers.Location = new Uri(Request.RequestUri.ToString());
                return message;
            }
            catch (Exception ex)

            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

            
        }

        [ActionName("UpdateSorting")]
        [HttpPost]
        public HttpResponseMessage UpdateSorting([FromBody] List<OrderGuides.OrderGuide> GuideItems)
        {

            try
            {
                var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                var dataTable = new DataTable();

                dataTable.Columns.Add("prodCode", typeof(string));
                dataTable.Columns.Add("unit_id", typeof(string));
                dataTable.Columns.Add("group_id", typeof(int));
                dataTable.Columns.Add("qty", typeof(double));
                dataTable.Columns.Add("itemComment", typeof(string));
                dataTable.Columns.Add("sortOrder", typeof(int));

                using (connect)
                {
                    SqlCommand cmd = new SqlCommand("Delete From masterOrderGuide where group_id = @GroupID", connect);
                    cmd.Parameters.AddWithValue("@GroupID", GuideItems[0].groupID);

                    connect.Open();
                    cmd.ExecuteNonQuery();

                    foreach (var dG in GuideItems)
                    {
                        dataTable.Rows.Add(dG.Code, dG.Unit, dG.groupID, dG.Qty, dG.Comment, dG.sortOrder);

                    }

                    var bulkCopy = new SqlBulkCopy(connect, SqlBulkCopyOptions.KeepIdentity, null);
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                    bulkCopy.DestinationTableName = "masterOrderGuide";
                    bulkCopy.WriteToServer(dataTable);
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ UpdateSorting() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
  
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            return message;
        }


        [ActionName("GetOrderGuides")]
        public HttpResponseMessage GetOrderGuides()
        {
            Logger.WriteLine("Getting order Guides...");
            var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
            int rowID = login.GetLoggedInUser.m_userRowID;
            List<OrderGuides.OrderGuidesList> orderGuideList = new List<OrderGuides.OrderGuidesList>();

            try
            {
                using (connect)
                {
                    SqlCommand cmd = new SqlCommand("SELECT dbo.loginGroups.login_id, dbo.customerGroups.group_id, dbo.customerGroups.custCode, dbo.customerGroups.group_name , dbo.customerGroups.group_description FROM dbo.loginGroups INNER JOIN dbo.customerGroups ON dbo.loginGroups.group_id = dbo.customerGroups.group_id Where login_id = @userID", connect);
                    cmd.Parameters.AddWithValue("@userID", rowID);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            OrderGuides.OrderGuidesList ogl = new OrderGuides.OrderGuidesList();
                            ogl.name = row["group_description"].ToString();
                            ogl.groupID = Convert.ToInt16(row["group_id"]);
                            orderGuideList.Add(ogl);
                        }
                    }
                }

                var message = Request.CreateResponse(HttpStatusCode.OK, orderGuideList);
                message.Headers.Location = new Uri(Request.RequestUri.ToString());
                Logger.WriteLine("Fetching orderguides successful.");
                return message;
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ GetOrderGuides() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }


        [HttpDelete]
        [ActionName("DeleteItemFromGuide")]
        public HttpResponseMessage DeleteItemFromGuide([FromBody] OrderGuides.OrderGuide orderguide)
        {
            try
            {
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                using (connect)
                {
                    SqlCommand cmd = new SqlCommand("Delete From masterOrderGuide Where group_id = @groupID AND (prodCode = @prodCode AND unit_id = @unit_id)", connect);
                    cmd.Parameters.AddWithValue("@prodCode", orderguide.Code);
                    cmd.Parameters.AddWithValue("@unit_id", orderguide.Unit);
                    cmd.Parameters.AddWithValue("@groupID", orderguide.groupID);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ DeleteItemFromGuide() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Item Deleted from Guide. Item : " + orderguide.Code.ToString() + ", Unit : " + orderguide.Unit +" , Group ID : " + orderguide.groupID.ToString());
            return message;
        }



        [HttpPost]
        [ActionName("AddItemToGuide")]
        public HttpResponseMessage AddItemToGuide([FromBody] OrderGuides.OrderGuide orderguide)
        {
            try
            {
                Logger.WriteLine("Adding item to Order Guide..");
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                using (connect)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO masterOrderGuide (prodCode, unit_id, group_id, qty, itemComment, sortOrder) VALUES (@prodCode, @unit_id, @group_id, @qty, @itemComment, @sortOrder)", connect);
                    cmd.Parameters.AddWithValue("@prodCode", orderguide.Code);
                    cmd.Parameters.AddWithValue("@unit_id", orderguide.Unit);
                    cmd.Parameters.AddWithValue("@group_id", orderguide.groupID);
                    cmd.Parameters.AddWithValue("@qty", orderguide.Qty);
                    cmd.Parameters.AddWithValue("@itemComment", orderguide.Comment);
                    cmd.Parameters.AddWithValue("@sortOrder", orderguide.sortOrder);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ AddItemToGuide() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }

            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Adding Item to Guide Successful. Item : " + orderguide.Code.ToString() + ", Group ID : " + orderguide.groupID.ToString());
            return message;
        }


        [HttpPost,HttpGet]
        [ActionName("RecoverGuide")]
        public HttpResponseMessage RecoverGuide([FromUri] int groupID)
        {
            OrderGuides.DeletedGuide dGuide = new OrderGuides.DeletedGuide();
            try
            {
                Logger.WriteLine("Recovering order guide....");
                List<OrderGuides.DeletedGuide> deletedGuides = new List<OrderGuides.DeletedGuide>();
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                using (connect)
                {
                    SqlDataAdapter da = new SqlDataAdapter("Select * From DeletedGuides Where GroupID = @gID And LoginID = @loginID", connect);
                    DataTable dt = new DataTable();
                    da.SelectCommand.Parameters.AddWithValue("@loginID", login.GetLoggedInUser.m_userRowID);
                    da.SelectCommand.Parameters.AddWithValue("@gID", groupID);
                    da.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        OrderGuides.DeletedGuide dG = new OrderGuides.DeletedGuide();
                        dG.ProductCode = dr["ProductCode"].ToString();
                        dG.Unit = dr["Unit"].ToString();
                        dG.GroupID = Convert.ToInt32(dr["GroupID"]);
                        dG.Quantity = Convert.ToInt32(dr["Quantity"]);
                        dG.ItemComment = dr["ItemComment"].ToString();
                        dG.SortOrder = Convert.ToInt32(dr["SortOrder"]);
                        dG.CustCode = dr["CustCode"].ToString();
                        dG.GroupName = dr["GroupName"].ToString();
                        dG.GroupDescription = dr["GroupDescription"].ToString();
                        dG.LoginID = Convert.ToInt32(dr["LoginID"]);
                        deletedGuides.Add(dG);
                        dGuide = dG;
                    }
                    
                    SqlCommand cmd = new SqlCommand("Insert into loginGroups Values(@loginID, @groupID)", connect);
                    cmd.Parameters.AddWithValue("@loginID", login.GetLoggedInUser.m_userRowID);
                    cmd.Parameters.AddWithValue("@groupID", groupID);

                    SqlCommand cmd1 = new SqlCommand("SET IDENTITY_INSERT customerGroups ON " +
                                                    "Insert into customerGroups (group_id, custCode, group_name, group_description, sortOrder) Values(@grpID, @custCode, @groupName, @groupDesc, @sOrder) " +
                                                    "SET IDENTITY_INSERT customerGroups OFF", connect);
                    cmd1.Parameters.AddWithValue("@grpID", groupID);
                    cmd1.Parameters.AddWithValue("@custCode", "");
                    cmd1.Parameters.AddWithValue("@groupName", dGuide.GroupName);
                    cmd1.Parameters.AddWithValue("@groupDesc", dGuide.GroupDescription);
                    cmd1.Parameters.AddWithValue("@sOrder", 0);

                    connect.Open();

                    cmd.ExecuteNonQuery();
                    cmd1.ExecuteNonQuery();

                    ThreadStart threadStart = delegate ()
                    {
                        recoverGuide(deletedGuides, groupID);
                    };
                    Thread thread = new Thread(threadStart);
                    thread.Start();

                }

            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ RecoverGuide() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
            var message = Request.CreateResponse(HttpStatusCode.OK, (dGuide.GroupDescription).ToString());
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Recovering order Guide successful. groupID = " + groupID);
            return message;
        }


        public void recoverGuide(List<OrderGuides.DeletedGuide> deletedGuides, int groupID)
        {
           
            try
            {
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                var dataTable = new DataTable();
               
                dataTable.Columns.Add("prodCode",typeof(string));
                dataTable.Columns.Add("unit_id", typeof(string));
                dataTable.Columns.Add("group_id", typeof(int));
                dataTable.Columns.Add("qty", typeof(double));
                dataTable.Columns.Add("itemComment", typeof(string));
                dataTable.Columns.Add("sortOrder", typeof(int));

                using (connect)
                {
                    connect.Open();

                    SqlCommand cmd = new SqlCommand("Delete From DeletedGuides Where GroupID = @gID And LoginID = @loginID", connect);
                    cmd.Parameters.AddWithValue("@gID", groupID);
                    cmd.Parameters.AddWithValue("@loginID", login.GetLoggedInUser.m_userRowID);
                    cmd.ExecuteNonQuery();

                    foreach (var dG in deletedGuides)
                    {
                        dataTable.Rows.Add(dG.ProductCode,dG.Unit,dG.GroupID,dG.Quantity,dG.ItemComment,dG.SortOrder);

                    }

                    var bulkCopy = new SqlBulkCopy(connect, SqlBulkCopyOptions.KeepIdentity, null);
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                    bulkCopy.DestinationTableName = "masterOrderGuide";
                    bulkCopy.WriteToServer(dataTable);

                }
               
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ recoverGuide() : " + ex.Message);
            }
            
        }


        [HttpDelete]
        [ActionName("DeleteOrderGuide")]
        public HttpResponseMessage DeleteOrderGuide([FromUri] string groupID)
        {
           List<OrderGuides.DeletedGuide> dGuides = new List<OrderGuides.DeletedGuide>();
            try
            {

                Logger.WriteLine("Deleting order guide....");
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                using (connect)
                {
                    //getting products from masterorderguide 
                    dGuides = GetProdcutsFromMasterOrderGuide(Convert.ToInt16(groupID));

                    SqlCommand cmd = new SqlCommand("Delete from customerGroups where group_id = @groupID", connect);
                    cmd.Parameters.AddWithValue("@groupID", groupID);

                    SqlCommand cmd2 = new SqlCommand("Delete from masterOrderGuide where group_id = @grpID", connect);
                    cmd2.Parameters.AddWithValue("@grpID", groupID);

                    SqlCommand cmd1 = new SqlCommand("Delete from loginGroups where login_id = @loginID and group_id = @groupID", connect);
                    cmd1.Parameters.AddWithValue("@loginID", login.GetLoggedInUser.m_userRowID);
                    cmd1.Parameters.AddWithValue("@groupID", groupID);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                    cmd1.ExecuteNonQuery();
                    cmd2.ExecuteNonQuery();

                    ThreadStart threadStart = delegate () 
                    {
                        //Adding items to DeletedGuides table
                        AddToDeletedGuides(dGuides);
                    };
                    Thread thread = new Thread(threadStart);
                    thread.Start();
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ DeleteOrderGuide() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Deleting order Guide successful. groupID = " + groupID);
            return message;
        }


        public List<OrderGuides.DeletedGuide> GetProdcutsFromMasterOrderGuide(int groupID)
        {
            List<OrderGuides.DeletedGuide> deletedGuides = new  List<OrderGuides.DeletedGuide>();
            try
            {
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    SqlDataAdapter da = new SqlDataAdapter("Select * From masterOrderGuide where group_id = @grpID", connect);
                    da.SelectCommand.Parameters.AddWithValue("@grpID",groupID);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    SqlDataAdapter da1 = new SqlDataAdapter("Select * From customerGroups where group_id = @groupID", connect);
                    da1.SelectCommand.Parameters.AddWithValue("@groupID", groupID);
                    DataTable dt1 = new DataTable();
                    da1.Fill(dt1);

                    if (dt.Rows.Count < 1)
                    {
                        OrderGuides.DeletedGuide dG = new OrderGuides.DeletedGuide();

                        foreach (DataRow dr in dt1.Rows)
                        {
                            dG.GroupName = dr["group_name"].ToString();
                            dG.GroupDescription = dr["group_description"].ToString();
                            dG.CustCode = dr["custCode"].ToString();
                        }

                        dG.LoginID = login.GetLoggedInUser.m_userRowID;
                        dG.ProductCode = "";
                        dG.Unit = "";
                        dG.GroupID = groupID;
                        dG.Quantity = 0;
                        dG.ItemComment = "";
                        dG.SortOrder = 0;
                        deletedGuides.Add(dG);

                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            OrderGuides.DeletedGuide dG = new OrderGuides.DeletedGuide();
                            foreach (DataRow dr in dt1.Rows)
                            {
                                dG.GroupName = dr["group_name"].ToString();
                                dG.GroupDescription = dr["group_description"].ToString();
                                dG.CustCode = dr["custCode"].ToString();
                            }
                            dG.LoginID = login.GetLoggedInUser.m_userRowID;
                            dG.ProductCode = row["prodCode"].ToString().Trim();
                            dG.Unit = row["unit_id"].ToString();
                            dG.GroupID = Convert.ToInt16(row["group_id"]);
                            dG.Quantity = Convert.ToInt16(row["qty"]);
                            dG.ItemComment = row["itemComment"].ToString();
                            dG.SortOrder = Convert.ToInt16(row["qty"]);
                            deletedGuides.Add(dG);

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ GetProdcutsFromMasterOrderGuide() : " + ex.Message);
            }
            return deletedGuides;
        }

        [HttpGet]
        [ActionName("GetDeletedGuides")]
        public HttpResponseMessage GetDeletedGuides()
        {
            List<OrderGuides.OrderGuidesList> orderGuides = new List<OrderGuides.OrderGuidesList>();
            DataTable dt = new DataTable();
            try
            {
                
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    SqlDataAdapter da = new SqlDataAdapter("Select Distinct GroupID,GroupName,GroupDescription From DeletedGuides where loginID = @loginID", connect);
                    da.SelectCommand.Parameters.AddWithValue("@loginID", login.GetLoggedInUser.m_userRowID);
                    da.Fill(dt);
                }

                foreach (DataRow dr in dt.Rows)
                {
                    OrderGuides.OrderGuidesList og = new OrderGuides.OrderGuidesList();
                    og.groupID = Convert.ToInt32(dr["GroupID"]);
                    og.name = dr["GroupDescription"].ToString();
                    orderGuides.Add(og);
                }
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ GetDeletedGuides() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }

            var message = Request.CreateResponse(HttpStatusCode.OK, orderGuides);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Getting Deleted Guides successful.");
            return message;
        }


        public void AddToDeletedGuides(List<OrderGuides.DeletedGuide> DeletedGuide)
        {
            try
            {
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                var dt = new DataTable();

                dt.Columns.Add("ProductCode", typeof(string));
                dt.Columns.Add("Unit", typeof(string));
                dt.Columns.Add("GroupID", typeof(int));
                dt.Columns.Add("Quantity", typeof(double));
                dt.Columns.Add("ItemComment", typeof(string));
                dt.Columns.Add("SortOrder", typeof(int));
                dt.Columns.Add("CustCode", typeof(string));
                dt.Columns.Add("GroupName", typeof(string));
                dt.Columns.Add("GroupDescription", typeof(string));
                dt.Columns.Add("LoginID", typeof(int));
                dt.Columns.Add("DateCreated", typeof(DateTime));

                using (connect)
                {
                    DateTime dtime = DateTime.Now.Date;
                    foreach (OrderGuides.DeletedGuide dg in DeletedGuide)
                    {
                        dt.Rows.Add(dg.ProductCode, dg.Unit, dg.GroupID, dg.Quantity, dg.ItemComment, dg.SortOrder, dg.CustCode, dg.GroupName, dg.GroupDescription, dg.LoginID, dtime.ToString("MM/dd/yyyy"));
                
                    }

                    var bulkCopy = new SqlBulkCopy(connect, SqlBulkCopyOptions.KeepIdentity, null);
                    foreach (DataColumn col in dt.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                    bulkCopy.DestinationTableName = "DeletedGuides";
                    connect.Open();
                    bulkCopy.WriteToServer(dt);

                    Logger.WriteLine("Adding to DeletedGuides table successful. ");

                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ AddToDeletedGuides() : " + ex.Message);
            }
        }


        [HttpPost]
        [ActionName("CreateNewGuide")]
        public HttpResponseMessage CreateNewGuide([FromUri] string name)
        {
            var newGroupID = 0;
            try
            {
               
                Logger.WriteLine("Creating new order guide....");
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                using (connect)
                {
                    SqlCommand cmd = new SqlCommand("MOG_addUpdateGroup", connect);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@group_name", name);
                    cmd.Parameters.AddWithValue("@group_description", name);
                    cmd.Parameters.AddWithValue("@sortOrder", 0);
                    cmd.Parameters.AddWithValue("@group_id", 0);


                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    newGroupID = Convert.ToInt16(dt.Rows[0][0]);

                    AddGuideToUser(newGroupID);

                }

            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ CreateNewGuide() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
            var message = Request.CreateResponse(HttpStatusCode.OK, newGroupID);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Create new order Guide successful. Group id = " + newGroupID);
            return message;
        }


        public void AddGuideToUser(int groupID)
        {
            try
            {
                Logger.WriteLine("Adding Guide to user... ");
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    SqlCommand cmd = new SqlCommand("Insert Into loginGroups Values (@login_id,@group_id)", connect);
                    cmd.Parameters.AddWithValue("@login_id", login.GetLoggedInUser.m_userRowID);
                    cmd.Parameters.AddWithValue("@group_id",groupID);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                    Logger.WriteLine("Adding Guide to user successful. ");

                }

            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ AddGuideToUser() : " + ex.Message);
            }

        }


        [HttpGet]
        [ActionName("ShowMasterOrderGuide")]
        public HttpResponseMessage ShowMasterOrderGuide(int groupID)
        {
            Logger.WriteLine("Fetching master order guide content....");
            SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

            string userName = login.GetLoggedInUser.custCode;
            try
            {
                List<OrderGuides.OrderGuide> orderguide = new List<OrderGuides.OrderGuide>();
                using (connect)
                {

                    SqlCommand cmd = new SqlCommand("MOG_showGuide", connect);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@groupID", groupID);
                    cmd.Parameters.AddWithValue("@custCode", userName);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            OrderGuides.OrderGuide od = new OrderGuides.OrderGuide();
                            od.groupID = groupID;
                            od.delColumn = "<a data-toggle='tooltip' data-placement='right' title='Delete Item From Guide' class='DelFrmGuide' href='#Delete' id ='deleteGuideBtn'><span class='glyphicon glyphicon-minus-sign'></a>";
                            od.Code = row["prodCode"].ToString().Trim();
                            if (od.Code == string.Empty || od.Code == null)
                            {
                                var msg = Request.CreateResponse(HttpStatusCode.OK, "no content");
                                msg.Headers.Location = new Uri(Request.RequestUri.ToString());
                                return msg;
                            }
                            od.Description = row["Description"].ToString();
                            od.PackWeight = row["Pk_Wt_Sz"].ToString();
                            if (od.PackWeight == "" || od.PackWeight == string.Empty)
                            {
                                od.PackWeight = row["Pack"].ToString() + "/" + row["Weight"].ToString();
                            }
                            od.Qty = Convert.ToInt16(row["qty"].ToString());
                            od.Unit = row["unit_id"].ToString();
                            od.Comment = row["itemComment"].ToString();
                            od.sortOrder = Convert.ToInt32(row["sortOrder"]);
                            orderguide.Add(od);
                        }
                    }
                }

                var message = Request.CreateResponse(HttpStatusCode.OK, orderguide);
                message.Headers.Location = new Uri(Request.RequestUri.ToString());
                Logger.WriteLine("Fetching master order guide content successful. Group ID: " + groupID.ToString());
                return message;
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ ShowMasterOrderGuide() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }


        [HttpGet]
        [ActionName("GetSessionCart")]
        public HttpResponseMessage GetSessionCart()
        {
            Logger.WriteLine("Fetching Session Cart....");
            string custCode = login.GetLoggedInUser.m_sessionCustomer;
            string orderNumber = login.GetLoggedInUser.m_sessionOrderNum;
            int? priceLevel = login.GetLoggedInUser.m_priceLevel;

            List<OrderGuides.OrderGuide> OrderGuides = new List<OrderGuides.OrderGuide>();

            try
            {
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    SqlCommand cmd = new SqlCommand();

                    if (login.GetLoggedInUser.m_showPrices > 0)
                    {
                        cmd = new SqlCommand("SELECT SessionCart.Row_ID, SessionCart.orderNum, SessionCart.custCode, SessionCart.qtySold, SessionCart.prodCode," +
                        "SessionCart.itemComment, SessionCart.Unit_ID, Products.Description, Products.Pk_Wt_Sz, Products.Pack, Products.Weight," +
                        " Products.Case_ID, Products.Each_ID, Products.Pound_ID, Products.Default_ID, " +
                        "Prices.Sell_Price, Prices.SellID, Products.Product_Categ FROM SessionCart INNER JOIN Products ON SessionCart.ProdCode = " +
                        "Products.Product_Code INNER JOIN Prices ON SessionCart.ProdCode = Prices.Product_Code " +
                        "AND sessionCart.Unit_ID = Prices.SellID WHERE (SessionCart.orderNum = @orderNum" +
                        ") AND (Prices.Price_Level = @priceLevel AND Prices.Sell_Price > 0) ORDER BY SessionCart.Row_ID;", connect);
                        cmd.Parameters.AddWithValue("@orderNum", orderNumber);
                        cmd.Parameters.AddWithValue("@priceLevel", priceLevel);
                    }

                    else
                    {
                        cmd = new SqlCommand("SELECT SessionCart.Row_ID, SessionCart.orderNum, SessionCart.custCode, SessionCart.qtySold, SessionCart.prodCode," +
                        "SessionCart.itemComment, SessionCart.Unit_ID, Products.Description, Products.Pk_Wt_Sz, Products.Pack, Products.Weight ," +
                        " Products.Case_ID, Products.Each_ID, Products.Pound_ID, Products.Product_Categ, Products.Default_ID, Prices.SellID " +
                        " FROM SessionCart INNER JOIN Products ON SessionCart.ProdCode = Products.Product_Code INNER JOIN Prices ON SessionCart.ProdCode = Prices.Product_Code" +
                        " AND sessionCart.Unit_ID = Prices.SellID WHERE (SessionCart.orderNum = @orderNum) AND (Prices.Price_Level = @priceLevel AND Prices.Sell_Price > 0) ORDER BY SessionCart.Row_ID; ", connect);
                        cmd.Parameters.AddWithValue("@custCode", custCode);
                        cmd.Parameters.AddWithValue("@orderNum", orderNumber);
                        cmd.Parameters.AddWithValue("@priceLevel", priceLevel);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            OrderGuides.OrderGuide og = new OrderGuides.OrderGuide();

                            if (login.GetLoggedInUser.m_showPrices > 0)
                            {
                                og.showPrices = true;
                                og.sellPrice = dr["Sell_Price"].ToString();
                            }
                            else
                            {
                                og.showPrices = false;
                            }

                            og.RowID = Convert.ToInt64(dr["Row_ID"].ToString());
                            og.Code = dr["prodCode"].ToString().Trim();
                            if (dr["Product_Categ"].ToString().Contains("17-") || dr["Product_Categ"].ToString().Contains("19-"))
                            {
                                og.isManufactured = true;
                                og.Weight = Convert.ToDouble(dr["Weight"]);
                            }
                            else
                            {
                                og.isManufactured = false;
                            }
                            og.Comment = dr["itemComment"].ToString();
                            og.Qty = Convert.ToDouble(dr["qtySold"].ToString());
                            og.SellID = dr["SellID"].ToString();
                            og.Case_ID = dr["Case_ID"].ToString();
                            og.Each_ID = dr["Each_ID"].ToString();
                            og.Pound_ID = dr["Pound_ID"].ToString();

                            if (og.SellID == "C")
                            {
                                if (!string.IsNullOrEmpty(og.Case_ID))
                                {
                                    og.Unit = og.Case_ID;
                                }
                                else
                                {
                                    og.Unit = "Cs";
                                }
                            }

                            else if (og.SellID == "E")
                            {
                                if (!string.IsNullOrEmpty(og.Each_ID))
                                {
                                    og.Unit = og.Each_ID;
                                }
                                else
                                {
                                    og.Unit = "Ea";
                                }
                            }

                            else
                            {
                                if (!string.IsNullOrEmpty(og.Pound_ID))
                                {
                                    og.Unit = og.Pound_ID;
                                }
                                else
                                {
                                    og.Unit = "Lb";
                                }
                            }

                           
                            og.Description = dr["Description"].ToString();
                            og.PackWeight = dr["Pk_Wt_Sz"].ToString();
                            if (og.PackWeight == "" || og.PackWeight == string.Empty)
                            {
                                og.PackWeight = dr["Pack"].ToString() + "/" + dr["Weight"].ToString();
                            }
                            OrderGuides.Add(og);
                        }
                    }

                    else
                    {
                        Request.CreateResponse(HttpStatusCode.OK, "Current Order is Empty.");

                    }

                }


            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ GetSessionCart() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);
            }

            var message = Request.CreateResponse(HttpStatusCode.OK, OrderGuides);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Fetching SessionCart successful.");
            return message;
        }


        [HttpPost]
        [ActionName("MoveGuideToSession")]
        public HttpResponseMessage MoveGuideToSession(int groupID, int merge)
        {
            try
            {
                Logger.WriteLine("Moving Guide To Session...");
                string orderNumber = login.GetLoggedInUser.m_sessionOrderNum;
                string custCode = login.GetLoggedInUser.m_sessionCustomer;
                SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                using (connect)
                {
                    SqlCommand cmd = new SqlCommand("MOG_moveToSession", connect);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@group_id", groupID);
                    cmd.Parameters.AddWithValue("@orderNum", orderNumber);
                    cmd.Parameters.AddWithValue("@custCode", custCode);
                    cmd.Parameters.AddWithValue("@merge", merge);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                }

            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ MoveGuideToSession() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);
            }
            var message = Request.CreateResponse(HttpStatusCode.OK);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Moving Guide to session successful. groupID : " + groupID.ToString());
            return message;
        }

    }
}
