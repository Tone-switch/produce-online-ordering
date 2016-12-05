

$(document).ready(function () {

    if (location.hash != 0 && location.hash === '#inactive') {
        $('#inactive').modal('show');
    };
    location.hash = '#';
});


function LoginValidation() {
    var valid = true;
    var user = document.getElementById("Login").value;
    var password = document.getElementById("Password").value;

    if (user == '' || password == '')
    {
        if (user == '') {
            document.getElementById("LoginMsg").className = "alert alert-danger";
            document.getElementById("LoginMsg").innerHTML = "Login Name cannot be empty.";
            valid = false;
        }

        else
        {
            document.getElementById("LoginMsg").className = "alert alert-danger";
            document.getElementById("LoginMsg").innerHTML = "Password cannot be empty.";
            valid = false;
        }
    }

    return valid;
}


//$('#btnLogin').one('click', function (){
    
//    $.ajax({
//        type : 'GET',
//        url: 'api/Users/',
//        data: {
//            format : 'json',
//            datatype: 'json',
//            contentType: 'application/json ; charset = utf-8',
//            loginUser : $('#Login').val(),
//            loginPwd : $('#Password').val(),
//        },
//        success : function () {
//            alert('success');
//            window.location.replace("ShowProducts.aspx");
//        },
//        error : function () {
//            alert('login failed.');
//        },
        
//    })
//})