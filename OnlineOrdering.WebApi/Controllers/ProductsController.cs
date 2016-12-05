
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using OnlineOrdering.WebApi.Models;
using System.Data;
using Online_Ordering;

namespace OnlineOrdering.WebApi.Controllers
{
    [Authorize]
    public class ProductsController : ApiController
    {

        [ActionName("GetAllProducts")]
        public HttpResponseMessage GetAllProducts()
        {
            Logger.WriteLine("Getting All Products ...");
            SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
            List<Products.ProductList> AllProducts = new List<Products.ProductList>();
            try
            {
                if (login.GetLoggedInUser.m_priceLevel == 0 && login.GetLoggedInUser.m_showPrices > 0)
                {
                    login.GetLoggedInUser.m_showPrices = 0;
                }

                using (connect)
                {
                    SqlDataAdapter adapter = new SqlDataAdapter("Select Products.RowID, Products.Pack, Products.Weight, Products.Product_Code, Products.Case_ID, Products.Each_ID, Products.Pound_ID, Products.Description, Products.Pk_Wt_Sz, Prices.Sell_Price, Prices.SellID from Products INNER JOIN Prices ON Products.Product_Code = Prices.Product_Code WHERE Prices.Price_Level = " + login.GetLoggedInUser.m_priceLevel + " Order By Products.RowID", connect);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            Products.ProductList p = new Products.ProductList();
                            p.Product_Code = row["Product_Code"].ToString().Trim();
                            p.Description = row["Description"].ToString();
                            p.Case_ID = row["Case_ID"].ToString();
                            p.Each_ID = row["Each_ID"].ToString();
                            p.Pound_ID = row["Pound_ID"].ToString();
                            p.Pk_Wt_Sz = row["Pk_Wt_Sz"].ToString();
                            p.Pack = Convert.ToDouble(row["Pack"].ToString());
                            p.Weight = double.Parse(row["Weight"].ToString());
                            p.Prices_Sell_Price = double.Parse(row["Sell_Price"].ToString());
                            if (p.Pk_Wt_Sz == "" || p.Pk_Wt_Sz == string.Empty)
                            {
                                p.Pk_Wt_Sz = p.Pack + "/" + p.Weight;
                            }
                            p.Prices_SellID = row["SellID"].ToString();


                            if (p.Prices_SellID == "C")
                            {
                                if (p.Case_ID == null || p.Case_ID == "")
                                {
                                    p.Case_ID = "Cs.";
                                }

                                else
                                {
                                    p.Case_ID = p.Case_ID.ToLower();
                                    p.Case_ID = UppercaseFirst(p.Case_ID);
                                }
                                u_cs = p.Case_ID ;
                                if (login.GetLoggedInUser.m_showPrices > 0)
                                {
                                    cs = String.Format("{0:C2}", p.Prices_Sell_Price);
                                }
                                else
                                {
                                    cs = "$ - - - ";
                                }

                                p.CaseString = "<div class='btn-group' style='display:flex' role='group' aria-label='...'><button data-unit='C' data-toggle='tooltip' data-placement='left' title='' data-original-title='Add Item To Guide' data-value='AddToGuide' type ='button' class='btn btn-primary'><span class='glyphicon glyphicon-list' aria-hidden='true'></span></button>" +
                                                "<button type='button' class='btn btn-default priceUnit'>" + cs + " / " + u_cs + "</button><button type = 'button' data-value ='C' data-toggle='tooltip' data-placement='right' title='' data-original-title='Add Item To Order' class='btn btn-success'><i class='fa fa-shopping-cart cartIcon' aria-hidden='true'></i></button></div>";
                            }


                            if (p.Prices_SellID == "E")
                            {
                                if (p.Each_ID == null || p.Each_ID == "")
                                {
                                    p.Each_ID = "Ea.";
                                }
                                else
                                {
                                    p.Each_ID = p.Each_ID.ToLower();
                                    p.Each_ID = UppercaseFirst(p.Each_ID);
                                }

                                u_ea = p.Each_ID ;


                                if (login.GetLoggedInUser.m_showPrices > 0)
                                {
                                    ea = String.Format("{0:C2}", p.Prices_Sell_Price);
                                }
                                else
                                {
                                    ea = "$ - - - ";
                                }
                                p.EachString = "<div class='btn-group' style='display:flex' role='group' aria-label='...'><button data-unit='E' data-toggle='tooltip' data-placement='left' title='' data-original-title='Add Item To Guide' data-value='AddToGuide' type ='button' class='btn btn-primary'><span class='glyphicon glyphicon-list' aria-hidden='true'></span></button>" +
                                                "<button type='button' class='btn btn-default priceUnit'>" + ea + " / " + u_ea + "</button><button type = 'button' data-value ='E' data-toggle='tooltip' data-placement='right' data-original-title='Add Item To Order' class='btn btn-success'><i class='fa fa-shopping-cart cartIcon' aria-hidden='true'></i></button></div>";

                            }

                            if (p.Prices_SellID == "P")
                            {
                                if (p.Pound_ID == null || p.Pound_ID == "")
                                {
                                    p.Pound_ID = "Lb.";
                                }
                                else
                                {
                                    p.Pound_ID = p.Pound_ID.ToLower();
                                    p.Pound_ID = UppercaseFirst(p.Pound_ID);
                                }

                                u_lb = p.Pound_ID ;

                                if (login.GetLoggedInUser.m_showPrices > 0)
                                {
                                    lb = String.Format("{0:C2}", p.Prices_Sell_Price);
                                }
                                else
                                {
                                    lb = "$ - - - ";
                                }

                                p.PoundString = "<div class='btn-group' role='group' style='display:flex' aria-label='...'><button data-unit='P' data-toggle='tooltip' data-placement='left' data-original-title='Add Item To Guide' data-value='AddToGuide' type ='button' class='btn btn-primary'><span class='glyphicon glyphicon-list' aria-hidden='true'></span></button>" +
                                              "<button type='button' class='btn btn-default priceUnit'>" + lb + " / " + u_lb + "</button><button type = 'button' data-value ='P' data-toggle='tooltip' data-placement='right' data-original-title='Add Item To Order' class='btn btn-success'><i class='fa fa-shopping-cart cartIcon' aria-hidden='true'></i></button></div>";
                            }


                            var match = AllProducts.FirstOrDefault(product => product.Product_Code == p.Product_Code);
                            if (match != null)
                            {
                                int index = AllProducts.IndexOf(match);
                                if (p.CaseString != null)
                                {
                                    AllProducts[index].CaseString = p.CaseString;
                                }
                                else if (p.EachString != null)
                                {
                                    AllProducts[index].EachString = p.EachString;
                                }
                                else if (p.PoundString != null)
                                {
                                    AllProducts[index].PoundString = p.PoundString;
                                }

                            }
                            else
                            {
                                AllProducts.Add(p);
                            }

                        }
                    }
                }

                var message = Request.CreateResponse(HttpStatusCode.OK, AllProducts);
                message.Headers.Location = new Uri(Request.RequestUri.ToString());
                Logger.WriteLine("Getting all products successful.");
                return message;
            }

            catch (Exception ex)

            {
                Logger.WriteLine("Exception @ GetAllProducts() : " + ex.Message.ToString());
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
                
            }
        }


        [ActionName("GetAllProductCateGories")]
        public HttpResponseMessage GetAllProductCateGories()
        {
            Logger.WriteLine("Getting All Product Categories...");
            SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
            List<Products.Product> allProducts = new List<Products.Product>();
            try
            {
                using (connect)
                {
                    SqlDataAdapter adapter = new SqlDataAdapter("Select Category_Code, Category_Desc from Categories ORDER BY Category_Desc", connect);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            Products.Product product = new Products.Product();
                            product.CategoryName = row["Category_Desc"].ToString();
                            product.CategoryCode = row["Category_Code"].ToString();
                            allProducts.Add(product);


                        }
                    }
                }

                var message = Request.CreateResponse(HttpStatusCode.OK, allProducts);
                message.Headers.Location = new Uri(Request.RequestUri.ToString());
                Logger.Write("Getting All Product Categories successful.");
                return message;
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ GetAllProductCateGories() : " + ex.Message.ToString());
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex);
            }  
        }

        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);

        }

        public string cs = "", ea = "", lb = "";
        public string u_cs = "", u_ea = "", u_lb = "";
        [ActionName("GetProductList")]
        public HttpResponseMessage GetProductList([FromUri]string category)
        {
            Logger.WriteLine("Getting product list...");
            SqlConnection connect = new SqlConnection(ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString);
            if (login.GetLoggedInUser.m_priceLevel == 0 && login.GetLoggedInUser.m_showPrices > 0)
            {
                login.GetLoggedInUser.m_showPrices = 0;
            }

            List<Products.ProductList> ProductList = new List<Products.ProductList>();
            try
            {
                using (connect)
                {
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT Products.RowID, Products.Product_Code, Products.Description, Products.Price_List_Status, Products.Pk_Wt_Sz,"
                        + "Products.Case_ID, Products.Each_ID, Products.Pound_ID, Products.Default_Unit_ID, Products.Default_Price_ID," +
                        "Products.Pack, Products.Weight, Products.Web_Link, Prices.Product_Code AS ProdCode," +
                        "Prices.SellID, Prices.Price_Level, Prices.Sell_Price, Products.Product_Categ FROM Products " +
                        "INNER JOIN Prices ON Products.Product_Code = Prices.Product_Code WHERE Prices.Price_Level = "+ login.GetLoggedInUser.m_priceLevel +
                        " AND Prices.Sell_Price > 0 And Product_Categ = '" + category + "'" +
                        " ORDER BY Products.Product_Categ, Products.Description", connect);

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            Products.ProductList p = new Products.ProductList();
                            p.RowID = Convert.ToInt16(row["RowID"].ToString());
                            p.Product_Code = row["Product_Code"].ToString().Trim();
                            p.Description = row["Description"].ToString();
                            p.Price_List_Status = row["Price_List_Status"].ToString();
                            p.Description = row["Description"].ToString();
                            p.Pk_Wt_Sz = row["Pk_Wt_Sz"].ToString();
                            p.Case_ID = row["Case_ID"].ToString();
                            p.Each_ID = row["Each_ID"].ToString();
                            p.Pound_ID = row["Pound_ID"].ToString();
                            p.Default_Unit_ID = row["Default_Unit_ID"].ToString();
                            p.Default_Price_ID = row["Default_Unit_ID"].ToString();
                            p.Pack = Convert.ToDouble(row["Pack"].ToString());
                            p.Weight = double.Parse(row["Weight"].ToString());
                            if (p.Pk_Wt_Sz == "" || p.Pk_Wt_Sz == string.Empty)
                            {
                                p.Pk_Wt_Sz = p.Pack + "/" + p.Weight;
                            }
                            p.Web_Link = row["Web_Link"].ToString();
                            p.ProdCode = row["ProdCode"].ToString();
                            p.Prices_SellID = row["SellID"].ToString();
                            p.Prices_Price_Level = row["Price_Level"].ToString();
                            p.Prices_Sell_Price = double.Parse(row["Sell_Price"].ToString());
                            p.Prices_Product_Categ = row["Product_Categ"].ToString();

                            if (p.Prices_SellID == "C")
                            {
                                if (p.Case_ID == null || p.Case_ID == "")
                                {
                                    p.Case_ID = "Cs.";
                                }

                                else
                                {
                                    p.Case_ID = p.Case_ID.ToLower();
                                    p.Case_ID = UppercaseFirst(p.Case_ID);
                                }

                                u_cs = p.Case_ID ;

                                if (login.GetLoggedInUser.m_showPrices > 0)
                                {
                                    cs = String.Format("{0:C2}", p.Prices_Sell_Price);
                                }
                                else
                                {
                                    cs = "$ - - - ";
                                }

                                p.CaseString = "<div class='btn-group' style='display:flex' role='group' aria-label='...'><button data-unit='C' data-toggle='tooltip' data-placement='left' title='' data-original-title='Add Item To Guide' data-value='AddToGuide' type ='button' class='btn btn-primary'><span class='glyphicon glyphicon-list' aria-hidden='true'></span></button>" +
                                                 "<button type='button' class='btn btn-default priceUnit'>" + cs + " / " + u_cs + "</button><button type = 'button' data-value ='C' data-toggle='tooltip' data-placement='right' title='' data-original-title='Add Item To Order' class='btn btn-success'><i class='fa fa-shopping-cart cartIcon' aria-hidden='true'></i></button></div>";


                            }


                            if (p.Prices_SellID == "E")
                            {
                                if (p.Each_ID == null || p.Each_ID == "")
                                {
                                    p.Each_ID = "Ea.";
                                }
                                else
                                {
                                    p.Each_ID = p.Each_ID.ToLower();
                                    p.Each_ID = UppercaseFirst(p.Each_ID);
                                }
                                   
                                u_ea = p.Each_ID ;


                                if (login.GetLoggedInUser.m_showPrices > 0)
                                {
                                    ea = String.Format("{0:C2}", p.Prices_Sell_Price);
                                }
                                else
                                {
                                    ea = "$ - - - ";
                                }

                                p.EachString = "<div class='btn-group' style='display:flex' role='group' aria-label='...'><button data-unit='E' data-toggle='tooltip' data-placement='left' title='' data-original-title='Add Item To Guide' data-value='AddToGuide' type ='button' class='btn btn-primary'><span class='glyphicon glyphicon-list' aria-hidden='true'></span></button>" +
                                                "<button type='button' class='btn btn-default priceUnit'>" + ea + " / " + u_ea + "</button><button type = 'button' data-value ='E' data-toggle='tooltip' data-placement='right' data-original-title='Add Item To Order' class='btn btn-success'><i class='fa fa-shopping-cart cartIcon' aria-hidden='true'></i></button></div>";
                            }
                          


                            if (p.Prices_SellID == "P")
                            {
                                if (p.Pound_ID == null || p.Pound_ID == "")
                                {
                                    p.Pound_ID = "Lb.";
                                }
                                else
                                {
                                    p.Pound_ID = p.Pound_ID.ToLower();
                                    p.Pound_ID = UppercaseFirst(p.Pound_ID);
                                }

                                u_lb = p.Pound_ID ;

                                if (login.GetLoggedInUser.m_showPrices > 0)
                                {
                                    lb = String.Format("{0:C2}", p.Prices_Sell_Price);
                                }
                                else
                                {
                                    lb = "$ - - - ";
                                }

                                p.PoundString = "<div class='btn-group' role='group' style='display:flex' aria-label='...'><button data-unit='P' data-toggle='tooltip' data-placement='left' data-original-title='Add Item To Guide' data-value='AddToGuide' type ='button' class='btn btn-primary'><span class='glyphicon glyphicon-list' aria-hidden='true'></span></button>" +
                                              "<button type='button' class='btn btn-default priceUnit'>" + lb + " / " + u_lb + "</button><button type = 'button' data-value ='P' data-toggle='tooltip' data-placement='right' data-original-title='Add Item To Order' class='btn btn-success'><i class='fa fa-shopping-cart cartIcon' aria-hidden='true'></i></button></div>";
                            }
                         

                            var match = ProductList.FirstOrDefault(product => product.Product_Code == p.Product_Code );
                            if (match != null)
                            {
                                int index = ProductList.IndexOf(match);
                                if (p.CaseString != null)
                                {
                                    ProductList[index].CaseString = p.CaseString;
                                }
                                else if (p.EachString != null)
                                {
                                    ProductList[index].EachString = p.EachString;
                                }
                                else if (p.PoundString != null)
                                {
                                    ProductList[index].PoundString = p.PoundString;
                                }
                               
                            }
                            else
                            {
                                ProductList.Add(p);
                            }

                        }
                    }

                }

                var message = Request.CreateResponse(HttpStatusCode.OK, ProductList);
                message.Headers.Location = new Uri(Request.RequestUri.ToString());
                Logger.WriteLine("Getting product list successful. Category : " + category);
                return message;
            }

            catch (Exception ex)
            {
                Logger.WriteLine("Exception @ GetAllProducts() : " + ex.Message.ToString());
                return Request.CreateResponse(HttpStatusCode.NotFound, ex);
            }
        }
    }
}
