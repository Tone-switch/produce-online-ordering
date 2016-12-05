using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OnlineOrdering.WebApi.Models;

namespace OnlineOrdering.WebApi.Models
{
    public class Cart
    {
        public string rowid { get; set; }
        public string quantity { get; set; }
        public string comment { get; set; }

        public partial class LineItem
        {
            public string orderNum { get; set; }
            public string quantity { get; set; }
            public string prodCode { get; set; }
            public string custCode { get; set; }
            public string itemComment { get; set; }
            public string unit { get; set; }
        }

        public partial class OrderDetails
        {
            public string deliveryDate { get; set; }
            public string pOnum { get; set; }
            public string orderComment { get; set; }
        }

        public partial class DeliveryAddress
        {
            public string deliveryName { get; set; }
            public string deliveryAddress { get; set; }
            public string deliveryOtherAddress { get; set; }
            public string deliveryCity { get; set; }
            public string deliveryZipCode { get; set; }
        }
    }
}