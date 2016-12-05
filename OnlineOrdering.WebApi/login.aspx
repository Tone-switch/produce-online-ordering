<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="OnlineOrdering.WebApi.login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta name="robots" content ="none"/>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <link href="~/Content/bootstrap-theme.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css?family=Amaranth|Merienda+One|Trirong:800" rel="stylesheet"/>
    <link href="Content/login.css" rel="stylesheet" />
    <title>Login</title>
</head>
<body>
    <table style =" border-spacing :0px ; padding : 0 ; width:300px ; border : 0 " align ="center">
      <tr>
        <td>
          <p style = "text-align:center"> <img src="../images/bcplogo.png" width ="300" height ="180"/></p>
        </td>
      </tr>
    </table>

    <form name="loginForm" action="login.aspx" method="post" runat="server">
        
<div class="container">

      <div class="modal fade" id="inactive" role="dialog">
                <div class="modal-dialog modal-sm">
                    <div class="modal-content">
                        <div class="modal-header" style="background-color:cadetblue">
                            <button type="button" class="close" data-dismiss="modal">&times;</button>
                            <h4 class="modal-title">Session Expired</h4>
                        </div>
                        <div class="modal-body" style="background-color:antiquewhite">
                            <p>You Have Been Logged Out Due To Inactivity.</p>
                            <p> Please Log In To Continue.</p>
                        </div>
                        <div class="modal-footer" style="background-color:cadetblue">
                            <button type="button" id="ChooseGuide" class="btn btn-default" data-dismiss="modal">Login</button>
                        </div>
                    </div>
                </div>
            </div>

    <div class="row vertical-offset-100">
    	<div class="col-md-4 col-md-offset-4">
    		<div class="panel panel-primary">
			  	<div class="panel-heading">
			    	<h3 class="panel-title" style="font-weight:bold; text-align:center" >Order Desk</h3>
			 	</div>
			  	<div class="panel-body">
                    <fieldset>
			    	  	<div class="form-group">
			    		    <input class="form-control" placeholder="Login Name" id="Login" name="Login" type="text"/>
			    		</div>
			    		<div class="form-group">
			    			<input class="form-control" placeholder="Password" id="Password" name="Password" type="password" value=""/>
			    		</div>
                        <asp:button class="btn btn-lg btn-success btn-block" runat="server" type="Submit" Text="Submit" ID="btnLogin" OnClick="btnLogin_Click" OnClientClick="return LoginValidation()"/>
			    	</fieldset>
			    </div>
			</div>
		</div>
	</div>
</div>
         
        <div class="alert alert-success" id="LoginMsg" runat="server" style="text-align:center; width:22%; font-size:16px; margin:0 auto;">
        </div>

    </form>

    <table style="border-spacing:1px; padding:1px; width:244px; height:37px; border:0px;" align="center">
      <tr>
        <td></td>
      </tr>
      <tr>
        <td>
          <p style="text-align:center;font-family:Arial;font-size:10px"><a style="font-size:12px" href="Help.htm">Help and Frequently Asked Questions</a></p>
        </td>
      </tr> 
    </table>
    <script src="scripts/jquery-3.1.0.min.js"></script>
    <script src="scripts/bootstrap.min.js"></script>
    <script src="scripts/Login.js"></script>
</body>
</html>
