using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineOrdering.WebApi.Models
{
    public class Products
    {
        public partial class Product
        {
            public string CategoryCode { get; set; }
            public string CategoryName { get; set; }
        }

        public partial class ProductList
        {
            public int? RowID { get; set; }
            public string Product_Code { get; set; }
            public string Description { get; set; }
            public string Price_List_Status { get; set; }
            public string Pk_Wt_Sz { get; set; }
            public string Case_ID { get; set; }
            public string Each_ID { get; set; }
            public string Pound_ID { get; set; }
            public string Default_Unit_ID { get; set; }
            public string Default_Price_ID { get; set; }
            public double? Pack { get; set; }
            public double? Weight { get; set; }
            public string Web_Link { get; set; }
            public string ProdCode { get; set; }
            public string Prices_SellID { get; set; }
            public string Prices_Price_Level { get; set; }
            public double? Prices_Sell_Price { get; set; }
            public string Prices_Product_Categ { get; set; }
            public string CaseString { get; set; }
            public string EachString { get; set; }
            public string PoundString { get; set; }



        }

    }
}