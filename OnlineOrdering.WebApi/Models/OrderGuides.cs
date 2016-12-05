using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineOrdering.WebApi.Models
{
    public class OrderGuides
    {
        public partial class OrderGuidesList
        {
            public string name { get; set; }
            public int groupID { get; set; }
        }

        public partial class DeletedGuide
        {
            //Order guide model for holding deleted guides
            public string ProductCode { get; set; }
            public string Unit { get; set; }
            public int GroupID { get; set; }
            public int Quantity { get; set; }
            public string ItemComment { get; set; }
            public int? SortOrder { get; set; }
            public string CustCode { get; set; }
            public string GroupName { get; set; }
            public string GroupDescription { get; set; }
            public int LoginID { get; set; }
        }

        public partial class OrderGuide
        {
            public long RowID { get; set; }
            public string Code { get; set; }
            public string Description { get; set; }
            public string PackWeight { get; set; }
            public double? Weight { get; set; }
            public double Qty { get; set; }
            public string Unit { get; set; }
            public string Case_ID { get; set; }
            public string Each_ID { get; set; }
            public string Pound_ID { get; set; }
            public string SellID { get; set; }
            public string sellPrice { get; set; }
            public string Comment { get; set; }
            public bool isManufactured { get; set; }
            public bool showPrices { get; set; }

            //for posting orderguides
            public int? groupID { get; set; }
            public int? sortOrder { get; set; }

            // delete column
            public string delColumn { get; set; }
        }
    }
}