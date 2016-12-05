using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using OnlineOrdering.WebApi.Models;
using System.Net.Mail;
using System.Web.Http;
using Online_Ordering;
using System.Threading;

namespace OnlineOrdering.WebApi.Controllers
{
    [Authorize]
    public class CartController : ApiController
    {
        [HttpGet]
        [ActionName("GetDeliveryAddress")]
        public HttpResponseMessage GetDeliveryAddress()
        {
            Logger.WriteLine("Getting Delivery address..");
            var deliveryAddress = new Cart.DeliveryAddress();
            try
            {

                var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    var cmd = new SqlCommand("Select * From Customers Where LTRIM(RTRIM(Code)) = @code", connect);
                    cmd.Parameters.AddWithValue("@code", login.GetLoggedInUser.custCode);
                    var dt = new DataTable();
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    foreach(DataRow dr in dt.Rows)
                    {
                        deliveryAddress.deliveryName = dr["Delivery_Name"].ToString();
                        deliveryAddress.deliveryAddress = dr["Delivery_Address"].ToString();
                        deliveryAddress.deliveryOtherAddress = dr["Delivery_Other_Addr"].ToString();
                        deliveryAddress.deliveryCity = dr["Delivery_City"].ToString();
                        deliveryAddress.deliveryZipCode = dr["Delivery_Zip_Code"].ToString();
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ GetDeliveryAddress() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);

            }
            var message = Request.CreateResponse(HttpStatusCode.OK, deliveryAddress);
            message.Headers.Location = new Uri(Request.RequestUri.ToString());
            Logger.WriteLine("Getting Delivery address successful.");
            return message;
            
        }


        [HttpPost]
        [ActionName("AddItem")]
        public HttpResponseMessage AddItem([FromBody]Cart.LineItem lineItem)
        {
            try
            {
                var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    var cmd = new SqlCommand("Insert Into SessionCart(orderNum, qtySold, prodCode, custCode, itemComment, Unit_ID) "+
                                                    "VALUES(@ordernum,@quantity,@prodcode,@custcode,@comment,@unit)", connect);

                    cmd.Parameters.AddWithValue("@ordernum",login.GetLoggedInUser.m_sessionOrderNum);
                    cmd.Parameters.AddWithValue("@quantity", lineItem.quantity);
                    cmd.Parameters.AddWithValue("@prodcode", lineItem.prodCode);
                    cmd.Parameters.AddWithValue("@custcode", login.GetLoggedInUser.custCode);
                    cmd.Parameters.AddWithValue("@comment", "");
                    cmd.Parameters.AddWithValue("@unit", lineItem.unit);
                    connect.Open();
                    cmd.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ AddItem() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);
            }
            Logger.WriteLine("Item Added to session cart : " + lineItem.prodCode);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [ActionName("DeleteItemById")]
        public HttpResponseMessage DeleteItemById(int rowId)
        {
            try
            {
                var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                using (connect)
                {
                    connect.Open();
                    var cmd = new SqlCommand("Delete from SessionCart Where Row_ID = @rowID", connect);
                    cmd.Parameters.AddWithValue("@rowID", rowId);
                    cmd.ExecuteNonQuery();
                }

            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ DeleteItemById() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);
            }
            Logger.WriteLine("Item Deleted from session cart. rowID :" + rowId.ToString());
            return Request.CreateResponse(HttpStatusCode.OK, rowId);

        }

        [HttpPut, HttpDelete]
        [ActionName("UpdateCart")]
        public HttpResponseMessage UpdateCart([FromBody]List<Cart> SessionCart)
        {
            var dt = new DataTable();
            try
            {
                var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);

                using (connect)
                {
                    var cmd = new SqlCommand("Select * From SessionCart Where orderNum = @orderNum and custCode = @custCode", connect);
                    cmd.Parameters.AddWithValue("@orderNum", login.GetLoggedInUser.m_sessionOrderNum);
                    cmd.Parameters.AddWithValue("@custCode", login.GetLoggedInUser.custCode);
                    var adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            foreach (var lineitem in SessionCart)
                            {
                                if (dr["Row_ID"].ToString() == lineitem.rowid)
                                {
                                    if (string.IsNullOrEmpty(lineitem.quantity))
                                    {
                                        dr["qtySold"] = 0;
                                    }
                                    else
                                    {
                                        dr["qtySold"] = lineitem.quantity;
                                    }
                                    dr["itemComment"] = lineitem.comment;
                                }
                            }

                            if (Convert.ToDouble(dr["qtySold"]) <= 0)
                            {
                                dr.Delete();
                            }

                        }

                    }

                    cmd.CommandText = "Delete From SessionCart Where orderNum = @orderNum1 and custCode = @custCode1";
                    cmd.Parameters.AddWithValue("@orderNum1", login.GetLoggedInUser.m_sessionOrderNum);
                    cmd.Parameters.AddWithValue("@custCode1", login.GetLoggedInUser.custCode);
                    connect.Open();
                    cmd.ExecuteNonQuery();

                    var bulkCopy = new SqlBulkCopy(connect,SqlBulkCopyOptions.KeepIdentity,null);
                    foreach (DataColumn col in dt.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                    }
                    bulkCopy.DestinationTableName = "SessionCart";
                    bulkCopy.WriteToServer(dt);

                }
                
           

            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ UpdateCart() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }

            Logger.WriteLine("Session Cart Updated. Order number: " + login.GetLoggedInUser.m_sessionOrderNum);
            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpPost]
        [ActionName("SubmitOrder")]
        public HttpResponseMessage SubmitOrder([FromBody] Cart.OrderDetails orderDetails)
        {
            var logfile = @"F:\Webspace\Websites\BaycitiesOnlineOrdering\Logs\Orders\";
            //var logfile = @"C:\Users\Abhi\Desktop\logs\orders\";
            logfile += login.GetLoggedInUser.userName + "_";
            logfile += login.GetLoggedInUser.m_sessionOrderNum;
            logfile += ".txt";
            Logger.SetFilename(logfile);
            Logger.WriteLine("Submitting Order...");

            var dt = new DataTable();
            var ordernumber = "";
            try
            {
                var connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
                var unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                using (connect)
                {
                    var cmd = new SqlCommand("INSERT INTO WebOrders (orderNum, custCode, qtySold, prodCode, itemComment, Unit_ID) " +
                        "SELECT orderNum, custCode, qtySold, prodCode, itemComment, Unit_ID FROM SessionCart Where orderNum = @orderNum AND custCode = @custCode", connect);
                    cmd.Parameters.AddWithValue("@orderNum", login.GetLoggedInUser.m_sessionOrderNum);
                    cmd.Parameters.AddWithValue("@custCode", login.GetLoggedInUser.custCode);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                    
                    cmd.CommandText = "UPDATE WebOrders SET orderDate = @orderdate, last_Update = @lastupdate, orderComment = @orderComment, orderPONum = @ponum" +
                        " WHERE custCode = @custCode1 AND orderNum = @ordernum1";
                    cmd.Parameters.AddWithValue("@orderdate", orderDetails.deliveryDate);
                    cmd.Parameters.AddWithValue("@lastupdate", unixTimestamp.ToString());
                    cmd.Parameters.AddWithValue("@orderComment", orderDetails.orderComment);
                    cmd.Parameters.AddWithValue("@ponum", orderDetails.pOnum);
                    cmd.Parameters.AddWithValue("@custCode1", login.GetLoggedInUser.custCode);
                    cmd.Parameters.AddWithValue("@ordernum1", login.GetLoggedInUser.m_sessionOrderNum);
                    ordernumber = login.GetLoggedInUser.m_sessionOrderNum;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "Select *, Products.Description FROM WebOrders INNER JOIN Products ON WebOrders.prodCode = Products.Product_Code Where orderNum = @orderNum2 AND custCode = @custCode2";
                    cmd.Parameters.AddWithValue("@orderNum2", login.GetLoggedInUser.m_sessionOrderNum);
                    cmd.Parameters.AddWithValue("@custCode2", login.GetLoggedInUser.custCode);
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);

                    Logger.WriteLine("<---------------------------------------------Order Details-------------------------------------------------------------->");
                    Logger.Write("Cust Code: " + login.GetLoggedInUser.custCode);
                    Logger.Write("Date Order Placed: " + DateTime.Now.ToString());
                    Logger.Write("Delivery Date: " + orderDetails.deliveryDate);
                    Logger.Write("PO# : " + orderDetails.pOnum);
                    Logger.Write("Order Comment : " + orderDetails.orderComment);
                    Logger.Write("****************************************");
                    foreach (DataRow dr in dt.Rows)
                    {   
                        Logger.Write("ProdCode : " + dr["prodCode"].ToString());
                        Logger.Write("Unit : " + dr["Unit_ID"].ToString());
                        Logger.Write("Quantity : " + dr["qtySold"].ToString());
                        Logger.Write("Item Comment : " + dr["itemComment"].ToString());
                        Logger.Write("Last Update : " + dr["last_Update"].ToString());
                        Logger.Write("-------------end of row-----------------");

                    }
                    Logger.Write("<-------------------------------------------------------End of Order------------------------------------------------------------>");
                    Logger.WriteLine("Order Submission successful: Order Number : " + login.GetLoggedInUser.m_sessionOrderNum);

                    cmd.CommandText = "Delete From SessionCart Where orderNum = @orderNum3 and custCode = @custCode3";
                    cmd.Parameters.AddWithValue("@orderNum3", login.GetLoggedInUser.m_sessionOrderNum);
                    cmd.Parameters.AddWithValue("@custCode3", login.GetLoggedInUser.custCode);
                    cmd.ExecuteNonQuery();
                }

                ThreadStart threadStart = delegate () { SendEmail(ordernumber, dt, orderDetails); };
                Thread thread = new Thread(threadStart);
                thread.Start();

                login.SetLogFile();
                Logger.WriteLine("Order Submission successful: Order Number : " + login.GetLoggedInUser.m_sessionOrderNum);
                login.SetOrderNumber(unixTimestamp.ToString());
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ SubmitOrder() : " + ex.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);
            }
            return Request.CreateResponse(HttpStatusCode.OK, ordernumber);
        }


        public void SendEmail(string orderNumber, DataTable orderTable, Cart.OrderDetails orderDetails) 
        {

            try
            {
                var fromAddress = new MailAddress("abhi@vintrex.com", "Confirmations");

                var _subject = login.GetLoggedInUser.m_userRowID.ToString() + " - Web Order Confirmation: " + orderNumber;


                var _body = "Web Order #: " + orderNumber + "<br>"+
                               "Customer : " + login.GetLoggedInUser.m_userRowID.ToString() + "<br>" +
                               "Date : " + orderDetails.deliveryDate + "<br>" +
                               "PO #: " + orderDetails.pOnum + "<br>" +
                               "Order Comment : " + orderDetails.orderComment + "<br><br>" +
                               "<table style = 'width:60% ; border:1px solid black; border-collapse:collapse ; text-align:center'>" +
                               "<thead><tr><th style ='text-align:center; border:1px black solid'>Quantity</th><th style ='text-align:center; border:1px black solid'>Unit</th><th style = 'text-align:center; border:1px black solid'>ProdCode</th><th style = 'text-align:center; border:1px black solid'>Description</th><th style = 'text-align:center; border:1px black solid'>Comment</th></tr></thead>";

                 _body += "<tbody>";
                foreach (DataRow dr in orderTable.Rows)
                {
                    _body += "<tr><td style='border: 1px black solid'>" + dr["qtySold"].ToString() + "</td>" +
                            "<td style='border: 1px black solid'>" + dr["Unit_ID"].ToString() + "</td>" +
                            "<td style='border: 1px black solid'>" + dr["prodCode"].ToString() + "</td>" +
                             "<td style='border: 1px black solid'>" + dr["Description"].ToString() + "</td>" +
                             "<td style='border: 1px black solid'>" + dr["itemComment"].ToString() + "</td></tr>";
                }


                _body += "</tbody></table>";

                var smtp = new SmtpClient
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("abhi@vintrex.com", "VIN!!a4i"),
                    Host = "smtp.office365.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

                using (var message = new MailMessage()
                {
                    Subject = _subject,
                    Body = _body,
                    IsBodyHtml = true,
                    BodyEncoding = System.Text.Encoding.UTF8,

                })

                {
                    message.From = fromAddress;
                    message.To.Add("abhhishek.paudel@gmail.com");
                    smtp.Send(message);
                    Logger.WriteLine("Confirmation Emails Sent.");
                }
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ SendEmail() : " + ex.Message);
                Request.CreateResponse(HttpStatusCode.OK, ex.Message);
            }

        }
    }
}
