using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;

namespace OnlineOrdering.WebApi
{
    public class Global : System.Web.HttpApplication
    {
        
        protected void Application_Start(object sender, EventArgs e)
        {
            // Web API routes
            RouteTable.Routes.MapHttpRoute(
               name: "OrderGuidesApi",
               routeTemplate: "api/{controller}/{action}/{groupID}/{merge}",
               defaults: new { action = RouteParameter.Optional, groupID = RouteParameter.Optional, merge = RouteParameter.Optional }
           );

            RouteTable.Routes.MapHttpRoute(
              name: "CartItemDeleteApi",
             routeTemplate: "api/{controller}/{action}/{rowId}",
             defaults: new { rowId = RouteParameter.Optional }
             );

            RouteTable.Routes.MapHttpRoute(
            name: "CartUpdateApi",
            routeTemplate: "api/{controller}/{action}/{rid}/{quantity}/{comment}",
            defaults: new { comment = RouteParameter.Optional }
            );
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            if (Session["UserName"] == null)
            {

            }

            else
            {

            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
           

        }


        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Application_PostAuthorizeRequest()
        {
            HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        }

        protected void Session_End(object sender, EventArgs e)

        {
            Session["UserName"] = null;
            Session.Remove("UserName");
            Session.Clear();
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}