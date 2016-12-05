using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Online_Ordering.Models
{
    public class Users
    {
        public partial class User
        {
            public string userName{ get; set; }
            public string custCode { get; set; }
            public  string m_sessionftpLogPath { get; set; }
            public  string m_sessionID { get; set; }
            public  string m_sessionOrderNum { get; set; }
            public  string m_sessionCustomer { get; set; }
            public  string m_submittedOrderNum { get; set; }
            public  string m_sessionftpPath { get; set; }
            public  int? m_isAnonLogin { get; set; }
            public  int? m_showPrices { get; set; }
            public  int? m_priceLevel { get; set; }
            public  int? m_showEnhancedColumns { get; set; }
            public  string m_sessionOrderDate { get; set; }
            public  string m_sessionPONumber { get; set; }
            public  string m_sessionOrderComment { get; set; }
            public  int? m_MOGOnly { get; set; }
            public  List<int> m_groupID { get; set; }
            public  int? m_isAdminLogin { get; set; }
            public  string m_emailOrders { get; set; }
            public  int? m_ftpOrders { get; set; }
            public  string m_sessionEmailPath { get; set; }
            public  int m_userRowID { get; set; }
        }
    }
}