
var _GlobVar = {
    firstCategory : $(),
    mousePosn: { x: -1, y: -1 },
    firstLogin: true,
    GuideItem: $(),
    CartItem: $(),
    CurrentGuideId: function () {
        var id = $('#GroupID').val();
        return id;
    }
};


var res = {
    loader: $('<div />', { class: 'loader' }),
    container: $('.container')
};


$(document).mousemove(function (event) {
    _GlobVar.mousePosn.x = event.pageX;
    _GlobVar.mousePosn.y = event.pageY;
});

function AddItemToOrderGuide(unit, code, groupID, guideName) {

    var orderGuideItem = {
        Code: code,
        Unit: unit,
        groupID: groupID,
        Qty: 0,
        Comment: '',
        sortOrder: 0
    }

    $.ajax({
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(orderGuideItem),
        url: '/api/OrderGuides/AddItemToGuide/',
        beforeSend: function () {
            res.container.append(res.loader);
        },
        success: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowItemMsg('Item "' + orderGuideItem.Code + '" has been added to the following order guide : <br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;' + guideName );

        },
        error: function (jqXHR) {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');

            if ((jqXHR.responseText).indexOf('Cannot insert duplicate key') > -1)
            {
                ShowErrorMsg('Guide already contains item : ' + orderGuideItem.Code + ' with unit : ' + orderGuideItem.Unit);
            }
            else {
                ShowErrorMsg('Unable to add item to Order Guide. Please Try Again.');
            }   
        }
    })
}


    $('#AddToGuide').click(function () {
        $('#AddToGuideModal').modal('show');
    })

    $('#AddNewGuideModal').on('shown.bs.modal', function () {
        $('#inputForGuideName').focus();
    })

    $('#AddToGuideModal').on('shown.bs.modal', function (e) {

        $('#modalContent a').click(function () {
            $('#modalContent a').removeClass('active');
            $(this).addClass('active');
        });
    });

    $('#RecoverGuideModal').on('shown.bs.modal', function (e) {

        $('#RGModalBody a').click(function () {
            $('#RGModalBody a').removeClass('active');
            $(this).addClass('active');
        });
    });

    $('#btnRecover').click(function () {
        $('#RGModalBody a').each(function () {
            if ($(this).attr('class') === 'list-group-item active')
            {
                var id = $(this).data('value');
                RecoverGuide(id);
            }
        });
    })

function RecoverGuide(id) {
    $.ajax({
        type: 'GET',
        url: '/api/OrderGuides/RecoverGuide?groupID=' + id,
        beforeSend: function () {
            res.container.append(res.loader);
        },

        success: function (data, textStatus, jqXHR) {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            GetOrderGuides();
            ShowItemMsg('Order Guide : ' + jqXHR.responseText + ' has been recovered.');
        },
        error: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowErrorMsg('Unable to Recover selected Deleted Guides. Please Try Again.');
        }
    });
};

function HighlightGuide(id) {

    $('#OrderGuides a').each(function () {
        $(this).removeClass('active');
        if ($(this).data('value') === id) {
            $(this).addClass('list-group-item active');
        }
    });

};

$(document).ajaxStart(function () {
   
    $.ajax({
        type: 'GET',
        url: '/api/OrderGuides/GetSession/',
        success: function (data) {
            if (data === null)
            {
                window.location.hash = '#inactive';
                window.location.replace("login.aspx");
                location.reload();
            }
        },
        error: function () {
            //ShowErrorMsg('Unable to Fetch Deleted Guides. Please Try Again.');
        }
    });
   
});


$(document).ready(function () {
   
    $.getScript('scripts/jquery.dataTables.min.js', function () {
    });

    $.getScript('scripts/jquery-ui.min.js', function () {
    });
    
    $.getScript('scripts/browser.js', function () {
    });

    GetFirstCategory();
    GetOrderGuides();
    AddItemToCart();

    $('#ChooseGuide').click(function () {

        $('#modalContent a').each(function () {
            var attr = $(this).attr('class');

            if (attr === 'list-group-item active') {
                var id = $(this).data('value');
                if (id == 286){
                    ShowErrorMsg('This Guide Cannot Be Edited.');
                    $(this).removeClass('active');
                    return;
                }
                HighlightGuide(id);
                $(this).removeClass('active');
            }
        });
    });

    $('#SaveGuideName').click(function () {
        var name = $('#inputForGuideName').val();
        CreateNewOrderGuide(name);
    });

    $('#DeleteOK').click(function () {
        var groupid = _GlobVar.CurrentGuideId();
        DeleteOrderGuide(groupid);
        GetProductList(_GlobVar.firstCategory);
    });
    
});


function GetSelection() {

    $('#ChangeSelection').click(function (e) {
        $('#AddToGuideModal').modal('show');
        e.preventDefault();
    });

    $('#AddNewGuides').click(function (e) {
        $('#AddNewGuideModal').modal('show');
        $('#inputForGuideName').val('');
        e.preventDefault();
    });

    $('#RecoverGuide').click(function (e) {
       
        $.ajax({
            type: 'GET',
            url: '/api/OrderGuides/GetDeletedGuides/',
            beforeSend: function () {
                res.container.append(res.loader);
            },
            success: function (data) {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                $('#RGModalBody').empty();

                if (data.length < 1)
                {
                    $('#RecoverGuideModal').modal('show');
                    $('#RGModalBodyMain').text('No Order Guides Found.');
                }

                else {
                    $.each(data, function (i, item) {
                        $('#RGModalBody').append('<a href="#/" data-value="' + item.groupID + '" class="list-group-item" >' + item.name + '</a>');
                    });
                    $('#RecoverGuideModal').modal('show');
                }
            },
            error: function () {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Unable to Fetch Deleted Guides. Please Try Again.');
            }
        });
        e.preventDefault();
    })

}


function DeleteOrderGuide(groupID) {
    $.ajax({
        type: 'Delete',
        url: '/api/OrderGuides/DeleteOrderGuide?groupID=' + groupID,
        beforeSend: function () {
            res.container.append(res.loader);
        },
        success: function (data) {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowItemMsg('Guide has been deleted.');
            GetOrderGuides();
        },
        error: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowErrorMsg('Unable to delete guide. Please Try Again.');
        }
    })
}

function CreateNewOrderGuide(name) {

    $.ajax({
        type: 'POST',
        url: '/api/OrderGuides/CreateNewGuide?name=' + name,
        beforeSend: function () {
            res.container.append(res.loader);
        },
        success: function (data) {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowItemMsg('Guide : "' + name + '" has been created.');
            GetOrderGuides();
            $('.options').trigger('click');
            
        },
        error: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowErrorMsg('Unable to create new guide. Please Try Again.');
        }
    });

}

function GetFirstCategory() {
    $.ajax({
        type: 'GET',
        url: '/api/Products/GetAllProductCateGories/',
        success: function (data) {
           _GlobVar.firstCategory = data[0].CategoryCode;
            $.each(data,function (i, listitem) {
                var categorycode = listitem.CategoryCode;
                $('#Categories').append('<a class="list-group-item" data-value ="' + categorycode + '" href="javascript:GetProductList(' + '\'' + categorycode + '\'' + ');"><li>' + listitem.CategoryName + '</li></a>');
            });
        },
        error: function () {
            ShowErrorMsg('Unable to fetch product categories. Please Try Again.');
        }
    }).pipe(function () {
        GetProductList(_GlobVar.firstCategory);
    });

}

function AddItemToCart() {

    $('#ProductsTable tbody').on('click', 'tr td button', function () {
       
        var code = $(this).closest('tr').find('td:first').text();

        var attr = $(this).data('value');

        if (attr === 'C') {
            AddItem('C', code);
        }
        if (attr === 'E') {
            AddItem('E', code);
        }
        if (attr === 'P') {
            AddItem('P', code);
        }
        if (attr === 'AddToGuide')
        {
            var unit = $(this).data('unit');
            var selectionMade = false;
            var guideName;
            var groupID;

            $('#OrderGuides a').each(function () {
                
                var attribute = $(this).attr('class');
            
                if (attribute === 'list-group-item list-group-item-action text-center og active') {
                    selectionMade = true;
                    guideName = $(this).text();
                    groupID = $(this).data('value');
                    return false;
                }
                else {
                    selectionMade = false;
                    guideName = $(this).text();
                }
            })

            if (selectionMade === true) {

                AddItemToOrderGuide(unit, code, groupID, guideName);
            }
            else {
                $('#AddToGuideModal').modal('show');;
            }
        }
    });

};

function ShowErrorMsg(msg) {
    var notify = $.notify('<span class = "glyphicon glyphicon-alert"></span>&nbsp;&nbsp;&nbsp;<strong>' + msg + '</strong>', {
        type: 'danger',
        allow_dismiss: true,
        showProgressbar: false,
        placement: {
            from: "top",
            align: "left"
        },
    });
};

function ShowItemMsg(msg) {

    var notify = $.notify('<strong><span class="glyphicon glyphicon-info-sign"></span>&nbsp;&nbsp;' + msg + '</strong>', {
        type: 'success',
        allow_dismiss: true,
        showProgressbar: false,
        animate: {
            enter: 'animated fadeInDown',
            exit: 'animated fadeOutUp'
        },
    });

};



function ShowOrderControls() {
    $('#OrderElements').attr({ style: 'display:block' });
    $('#orderControls').attr({ style: 'display:block ; margin-top:15px' });
};

function HideOrderControls() {
    $('#OrderElements').attr({ style: 'display:none' });
    $('#orderControls').attr({ style: 'display:none' });

};

function ShowOrderConfrimation() {
    $('#OrderConfirmation').attr({ style: 'display:block' });
};

function HideOrderConfrimation() {
    $('#OrderConfirmation').attr({ style: 'display:none' });
};

function ShowLeftNavbar() {
    $('#leftNav').show();
};

function HideLeftNavbar() {
    $('#leftNav').hide();
};

function ShowCartControls() {
    $('#Cart').show();
    ShowSessionCartControls();
    $('#CartMsg').show();
    $('#hideCartMsg').click(function () {
        $('#CartMsg').fadeOut(400, function () {
            $('#CartMsg').attr({ style: 'display:none', });
        });
    });
}

function ShowCheckOutControls() {
    $('#checkoutControls').attr({ style: 'display : block ; margin-top : 15px' });
}

function HideCheckOutControls() {
    $('#checkoutControls').attr({ style: 'display : none' });
}

function ShowSessionCartControls() {
    $('#sessionCart').attr({ style: 'display : block ; margin-top : 15px; margin-bottom:200px' });
}

function HideSessionCartControls() {
    $('#sessionCart').attr({ style: 'display : none' });
}

function HideCartControls() {
    $('#Cart').hide();
    $('#sessionCart').hide();
    $('#CartMsg').hide();
}

function ShowOrderGuideControls() {
    $('#OrderGuideBtns').show();
    $('#OGuideTable').show();
    $('#pLabel').show();
}

function HideOrderGuideControls() {
    $('#OrderGuideBtns').hide();
    $('#OGuideTable').hide();
}

function ShowPtableControls() {
    $('#Ptable').show();
    $('#ptableMsg').show();
    $('#pLabel').show();
}

function HidePtableControls() {
    $('#Ptable').hide();
    $('#ptableMsg').hide();
}

function HideEmptyOrderGuideMsg() {
    $('#notFound').attr('style', 'display:none');
}

function ShowEmptyOrderGuideMsg()
{
    $('#notFound').attr('style', 'display:block');
    $('#OGuideTable').attr('style', 'display:none');
    $('#Ptable').attr('style', 'display:none');
    $('#ptableMsg').attr('style', 'display:none');
    $('#OrderGuideBtns').show();
    HideCartControls();
    HideCheckOutControls();
    HideOrderControls();
    HideEmptyCartMsg();
}

function ShowMasterOrderGuide(groupid) {

    $('#GroupID').val(groupid); //set groupID value
    $.ajax({
        type: 'GET',
        url: '/api/OrderGuides/ShowMasterOrderGuide/' + groupid,
        beforeSend: function() {
            res.container.append(res.loader);
        },
        success: function (data) {
        
            $('#Categories a').each(function () {
                var ID = $(this).data('value');
                if (groupid == ID) {
                    $('#pLabel').text('Order Guide : ' + $(this).text());
                    $('#pLabel').attr('style', 'display:block');
                }
            });

            if (data === 'no content') {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowEmptyOrderGuideMsg();
                return false;
            }

            HideCheckOutControls();
            HidePtableControls();
            HideCartControls();
            HideOrderControls();
            ShowOrderGuideControls();
            HideOrderConfrimation();
            HideEmptyCartMsg();
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            $('#OrderGuideTable')
                .DataTable({
                    "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, " All"]],
                    data: data,
                    "stateSave": true,
                    "ordering": false,
                    rowId: 'groupID',
                    "bDestroy": true,
                    "pagingType": "full_numbers",
                    columns: [
                        { 'data': 'Code' },
                        { 'data': 'Description' },
                        { 'data': 'PackWeight', 'bSortable': false },
                        { 'data': 'Qty' },
                        { 'data': 'Unit' },
                        { 'data': 'Comment' },
                        { 'data': 'delColumn', 'bSortable': false },
                    ]
                });
            $('.DelFrmGuide').tooltip({ container: 'body' });
            HideEmptyOrderGuideMsg();
            
        },
        error: function() {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowErrorMsg('Unable to fetch current Order Guide. Please Try Again.');
        }
    });
}


function SortGuide() {
    var fixHelperModified = function(e, tr) {
        var $originals = tr.children();
        var $helper = tr.clone();
        $helper.children().each(function(index)
        {
            $(this).width($originals.eq(index).width())
        });
        return $helper;
    };

    $('#SortTable tbody').sortable({
        helper: fixHelperModified,
        start: function(e, ui){
            ui.placeholder.height(ui.item.height());
        },
        stop: function (event, ui) { renumber_table('#SortTable') }
    });

}


//Renumber table rows
function renumber_table(tableID) {
    $(tableID + " tr").each(function() {
        count = $(this).parent().children().index($(this)) + 1;
        $(this).find('.Sort').html('<input type="number" class="text-center" tabindex="" id="" value="'+count+'" disabled>');
    });
}

$('#SortGuide').click(function () {

    var groupid = _GlobVar.CurrentGuideId()

    if (groupid == 286) {
        ShowErrorMsg('This Guide Cannot Be Sorted.');
        return;
    };

    var table = $('#OrderGuideTable').DataTable();

    var data = table
        .rows()
        .data();

    $('#SortBody').text('');
    var sortNumbers = 0;

    $.each(data, function () {
        sortNumbers += 1;
        var item = $(this)[0];
        $('#SortBody').append('<tr></td><td class="Sort"><input type="number" class="text-center" tabindex="" id="" value="' + sortNumbers/* item.sortOrder */ + '" disabled></td><td class="col-lg-4 text-center">' + item.Code + '</td><td class="col-lg-4 text-center">' + item.Description
                            + '</td><td class="col-lg-3 text-center">' + item.PackWeight + '</td><td class="col-lg-3 text-center">' + item.Unit + '</td></tr>');
    });

    $('#SortModal').modal('show');
    SortGuide();
});

$('#SaveSort').click(function () {
    var SortTable = [];
    var sortRow = $();
    var guideID = _GlobVar.CurrentGuideId();

    $('#SortTable tbody tr').each(function () {
        sortRow = {
            Code: $(this).closest('tr').find('td').eq(1).text(),
            Description: $(this).closest('tr').find('td').eq(2).text(),
            Unit: $(this).closest('tr').find('td').eq(4).text(),
            groupID: _GlobVar.CurrentGuideId(),
            Qty: 0,
            Comment: '',
            sortOrder: $(this).closest('tr').find('td').eq(0).find(':input[type="number"]').val(),
        }

        SortTable.push(sortRow);
    });

    $.ajax({
        type: 'POST',
        contentType:"application/json",
        url: '/api/OrderGuides/UpdateSorting/',
        data: JSON.stringify(SortTable),
        beforesend: function () {
            res.container.append(res.loader);
        },
        success: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowMasterOrderGuide(guideID);
            ShowItemMsg('Order Guide Has Been Sorted.');
        },
        error: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowErrorMsg('Error Sorting Order Guide. Please try again.');
        }
    });

})



$('#DeleteGuide').click(function () {
   
    var groupid = _GlobVar.CurrentGuideId();

    if (groupid == 286) {
        ShowErrorMsg('This Guide Cannot Be Deleted.');
        return;
    };
    $('#DelGuideConfirmation').modal('show');

});

$('#ProductsTable').on('draw.dt', function () {
    EnableToolTip();
});


$('#OrderGuideTable').on('draw.dt', function () {
   
    var groupID = _GlobVar.CurrentGuideId();

    $('.DelFrmGuide').on('click', function (e) {

        if (groupID == 286) {
            e.preventDefault();
            ShowErrorMsg('Items in this Guide Cannot be Deleted.');
            return;
        }

        _GlobVar.GuideItem = {
            Code: $(this).closest('tr').find('td:first').text(),
            Unit: $(this).closest('tr').find('td').eq(4).text(),
            groupID: $(this).closest('tr').attr('id')
        }

        $('#delGuideItemConf').modal('show');
        $('#delGuideItemConfContent').text('Delete Item : ' + _GlobVar.GuideItem.Code + ' ?');
    });

});


$('#okDelete').click(_GlobVar.GuideItem, function () {
   
    $.ajax({
        type: 'DELETE',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(_GlobVar.GuideItem),
        url: '/api/OrderGuides/DeleteItemFromGuide/',
        beforesend: function () {
            res.container.append(res.loader);
        },
        success: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            $(this).closest('tr').hide();
            ShowMasterOrderGuide(_GlobVar.GuideItem.groupID);
            ShowItemMsg('Item : "' + _GlobVar.GuideItem.Code + '" has been deleted from Order Guide.');
            _GlobVar.GuideItem = null;

        },
        error: function () {
            res.container.find(res.loader).remove();
            $('.loader').attr('style', 'display:none');
            ShowErrorMsg('Item : "' + _GlobVar.GuideItem.Code + '"could not be Deleted. please try again.');
            _GlobVar.GuideItem = null;
        }
    });
});


function Collapse() {
    $('.panel-heading.options').click(function () {
        $('.collapse').collapse('toggle');
        $('#arrow').toggleClass('glyphicon-chevron-down glyphicon-chevron-up');
    });
}

function GetOrderGuides() {

    $.ajax({
        type: 'GET',
        url: '/api/OrderGuides/GetOrderGuides/',
        success: function (data) {
            $('#OrderGuides a').remove();
            $('#OrderGuides li').remove();
     
            if (data.length < 1) {
                $('#Settings').remove();
                $('#OrderGuides').append('<div id="Settings" class="panel panel-primary"><div class="panel-heading options"><h4 class="panel-title "> ' +
                               'Options<span id="arrow" class="glyphicon glyphicon-chevron-down pull-right"></span></a></h4></div><div id="collapseOne" class="panel-collapse collapse">' +
                               '<a id="AddNewGuides" class="text-center list-group-item" href ="#AddNewGuide"><li>Create New Guide<span id="AddNewGuide" class="glyphicon glyphicon-plus pull-right"></span></li></a> '
                               + ' <a id="ChangeSelection" href="#GuideSelection" class="text-center list-group-item"><li>Select Guide to Edit<span id="GuideSelection" class="glyphicon glyphicon-tasks pull-right"></li></a>' +
                                '<a id="RecoverGuide" class="text-center list-group-item" href ="#DeletedGuides"><li> Recover Deleted Guides<span id="DeletedGuides" class="glyphicon glyphicon-floppy-remove pull-right"></span></li></a>' +
                               '</div></div>');

                $('#OrderGuides').append('<li class="text-center list-group-item">Order Guides Not Available</li>');
                Collapse();
                GetSelection();
            } else {
                if ($.isArray(data)) {
                    $('#Settings').remove();
                    $('#OrderGuides').append('<div id="Settings" class="panel panel-primary"><div class="panel-heading options"><h4 class="panel-title "> ' +
                            'Options<span id="arrow" class="glyphicon glyphicon-chevron-down pull-right"></span></a></h4></div><div id="collapseOne" class="panel-collapse collapse">' +
                            '<a id="AddNewGuides" class="text-center list-group-item" href ="#AddNewGuide"><li>Create New Guide<span id="AddNewGuide" class="glyphicon glyphicon-plus pull-right"></span></li></a> '
                            + ' <a id="ChangeSelection" href="#GuideSelection" class="text-center list-group-item"><li>Select Guide to Edit<span id="GuideSelection" class="glyphicon glyphicon-tasks pull-right"></li></a>' +
                             '<a id="RecoverGuide" class="text-center list-group-item" href ="#DeletedGuides"><li>Recover Deleted Guides<span id="DeletedGuides" class="glyphicon glyphicon-floppy-remove pull-right"></span></li></a>' +
                            '</div></div>');

                    $('#modalContent').empty();

                    $.each(data, function (i, orderguide) {
                       
                        $('#OrderGuides').append('<div class="input-group"><span class="input-group-addon"><i class="fa fa-list" aria-hidden="true"></i></span>' +
                                                '<a class="list-group-item list-group-item-action text-center og" data-value="' + orderguide.groupID + '" href = "javascript:ShowMasterOrderGuide(' + 
                                                '\'' + orderguide.groupID + '\'' + ');">' + orderguide.name + '</div>');

                        $('#modalContent').append('<a data-value=' + orderguide.groupID + ' href="#SelectGuide" class="list-group-item">' + orderguide.name + '</a>');
                        
                    });
                    GetSelection();
                    Collapse();
                } else {
                    $('#Settings').remove();

                    $('#OrderGuides').append('<div id="Settings" class="panel panel-primary"><div class="panel-heading options"><h4 class="panel-title "> ' +

                            'Options<span id="arrow" class="glyphicon glyphicon-chevron-down pull-right"></span></a></h4></div><div id="collapseOne" class="panel-collapse collapse">' +
                            '<a id="AddNewGuides" class="text-center list-group-item" href ="#AddNewGuide"><li>Create New Guide<span id="AddNewGuide" class="glyphicon glyphicon-plus pull-right"></span></li></a> '
                            + ' <a id="ChangeSelection" href="#GuideSelection" class="text-center list-group-item"><li>Select Guide to Edit<span id="GuideSelection" class="glyphicon glyphicon-tasks pull-right"></li></a>' +
                             '<a id="RecoverGuide" class="text-center list-group-item" href ="#DeletedGuides"><li>Recover Deleted Guides<span id="DeletedGuides" class="glyphicon glyphicon-floppy-remove pull-right"></span></li></a>' +
                            '</div></div>');
                    $('#modalContent').empty();

                    $('#OrderGuides').append('<a id="AddNewGuides" class="text-center" href ="#AddNewGuide"><li>Create New Guide<span id="AddNewGuide" class="glyphicon glyphicon-plus pull-right"></span></li></a> '
                    + ' <a id="ChangeSelection" href="#GuideSelection" class="text-center"><li>Select Guide to Edit<span id="GuideSelection" class="glyphicon glyphicon-tasks pull-right"></li></a>' +
                    '<a id="RecoverGuide" class="text-center" href ="#DeletedGuides"><li>Recover Deleted Guides<span id="DeletedGuides" class="glyphicon glyphicon-floppy-remove pull-right"></span></li></a>');


                    $('#OrderGuides').append('<a class="list-group-item list-group-item-action text-center" data-value="' + orderguide.groupID + '" href = "javascript:ShowMasterOrderGuide(' + '\'' + orderguide.groupID + '\'' + ');">' + orderguide.name + '</a>');


                    $('#modalContent').append(' <a data-value=' + orderguide.groupID + ' href="#SelectGuide" class="list-group-item">' + orderguide.name + '<i class="fa fa-book pull-left" aria-hidden="true" style="font-size: 23px;color:black;"></i></a>');
                   
                    GetSelection();
                    Collapse();
                }
            }

        },
        error: function() {
            ShowErrorMsg('Unable to fetch Order Guides. Please Try Again.');
        }
    });
}



    function GetAllProducts() {

        $.ajax({
            type: 'GET',
            url: '/api/Products/GetAllProducts/',
            beforeSend: function() {
                res.container.append(res.loader);
            },
            success: function(data) {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                HideCartControls();
                HideOrderGuideControls();
                HideOrderControls();
                ShowPtableControls();
                HideOrderConfrimation();
                HideEmptyOrderGuideMsg();
                HideEmptyCartMsg();
                $('#ProductsTable')
                    .DataTable({
                        data: data,
                        "lengthMenu": [[10, 25, 50, 100, -1], [10, 25, 50, 100, " All"]],
                        "bDestroy": true,
                        "pagingType": "full_numbers",
                        'columnDefs': [
                                      { className: 'tdNoStyle', 'targets': [3, 4, 5] }
                                      ],
                        columns: [
                            { 'data': 'Product_Code' },
                            { 'data': 'Description' },
                            { 'data': 'Pk_Wt_Sz', 'bSortable': false },
                            { 'data': 'CaseString', 'bSortable': false },
                            { 'data': 'EachString', 'bSortable': false },
                            { 'data': 'PoundString', 'bSortable': false },
                        ]
                    });
                $('#pLabel').text('All Products (Complete List)');
                EnableToolTip();
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Unable to fetch all products categories. Please Try Again.');
            }
        });
    }

    function EnableToolTip() {
        $('[data-toggle="tooltip"]').tooltip({ container: 'body' });
    };

    $('#LogOut').on('click', function() {
        $.ajax({
            type: 'POST',
            url: '/api/Users/LogOut/',
            success: function() {
                window.location.replace("login.aspx");
                window.location.hash = '#';
            }
        });
    });

    function CurrentOrderbadgeDecrement(number) {
        var count = Number($('#CurrentOrder').text());
        count = count - number;
        $('#CurrentOrder').text(count);
    };

    function CurrentOrderbadgeIncrement(number) {
        var count = Number($('#CurrentOrder').text());
        count = count + number;
        $('#CurrentOrder').text(count);
    };



    function AddItem(unit, code) {

        if (unit === 'C')
        {
            var lineitem = {
                orderNum: '',
                quantity: 1,
                prodCode: $.trim(code),
                custCode: '',
                itemComment: '',
                unit: 'C'
            }
            $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(lineitem),
                url: '/api/Cart/AddItem/',
                beforeSend: function() {
                    res.container.append(res.loader);
                },
                success: function(data) {
                    res.container.find(res.loader).remove();
                    $('.loader').attr('style', 'display:none');
                    CurrentOrderbadgeIncrement(1);
                    ShowItemMsg('Item : "' + lineitem.prodCode + '" Has Been Added To Your Order.' );
               
                },
                error: function() {
                    res.container.find(res.loader).remove();
                    $('.loader').attr('style', 'display:none');
                    ShowErrorMsg('Product Could Not Be Added. Please Try Again.')
                }
            });
        }
 
        if (unit === 'E') {
            var lineitem = {
                orderNum: '',
                quantity: 1,
                prodCode: $.trim(code),
                custCode: '',
                itemComment: '',
                unit: 'E'
            }
            $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(lineitem),
                url: '/api/Cart/AddItem/',
                beforeSend: function() {
                    res.container.append(res.loader);
                },
                success: function(data) {
                    res.container.find(res.loader).remove();
                    $('.loader').attr('style', 'display:none');
                    CurrentOrderbadgeIncrement(1);

                    ShowItemMsg('Item : "' + lineitem.prodCode + '" Has Been Added To Your Order.');
                },
                error: function() {
                    res.container.find(res.loader).remove();
                    $('.loader').attr('style', 'display:none');
                    ShowErrorMsg('Product Could Not Be Added. Please Try Again.')
                }
            });
        }


        if (unit === 'P')
        {
            var lineitem = {
                orderNum: '',
                quantity: 1,
                prodCode: $.trim(code),
                custCode: '',
                itemComment: '',
                unit: 'P'
            }
            $.ajax({
                type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(lineitem),
                url: '/api/Cart/AddItem/',
                beforeSend: function() {
                    res.container.append(res.loader);
                },
                success: function(data) {
                    res.container.find(res.loader).remove();
                    $('.loader').attr('style', 'display:none');
                    CurrentOrderbadgeIncrement(1);

                    ShowItemMsg('Item : "' + lineitem.prodCode + '" Has Been Added To Your Order.');

                },
                error: function() {
                    res.container.find(res.loader).remove();
                    $('.loader').attr('style', 'display:none');
                    ShowErrorMsg('Product Could Not Be Added. Please Try Again.');
                }
            });

        }

    }

    function GetDeliveryAddress() {
        $.ajax({
            type: 'GET',
            url: 'api/Cart/GetDeliveryAddress/',
            beforeSend: function() {
                res.container.append(res.loader);
            },
            success: function(data) {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                $('#AddressDetails')
                    .html(data.deliveryName +
                        '<br>' +
                        data.deliveryAddress +
                        '<br>' +
                        data.deliveryCity +
                        '<br>' +
                        data.deliveryZipCode);
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                $('#AddressDetails').html('Network Error.<br> Address Could Not Be Loaded <br>. Please Try Again.');
            }
        });
    }

    function ClearOrderDetails(){
        $('#DateValue').val('');
        $('#inputforPO').val('');
        $('#OrderCmt').val('');
    }

    function OrderSubmission() {

        var OrderDetails = {
            deliveryDate: $('#DateValue').val(),
            pOnum: $('#inputforPO').val(),
            orderComment: $('#OrderCmt').val(),
        }

        $.ajax({
            type: 'POST',
            url: '/api/Cart/SubmitOrder/',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(OrderDetails),
            beforeSend: function() {
                res.container.append(res.loader);
            },
            success: function(data) {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                $('#CurrentOrder').text(0);
                ClearOrderDetails();
                HideOrderControls();
                HideCartControls();
                ShowOrderConfrimation();
                $('#Oconfirmation').html('Thank you. <br> Your order has been placed. <br> Your order confirmation number is: ' + data);
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                $('#confirmation').attr({ class: 'panel panel-danger col-lg-9 col-md-9 col-sm-9' });
                $('#oc').attr({ class: 'glyphicon glyphicon-exclamation-sign' })
                $('#Oconfirmation').html('Network Error. Order not submitted. Please try again.');
            }
        });
    }

    $('#placeOrder').click(function() {
        $('html, body').animate({ scrollTop: 0 }, 'fast');
        OrderSubmission();
    });

    function GetProductList(category) {

        $('#Categories a').each(function () {
            var categCode = $(this).data('value');
            if (categCode === category)
            {
                $('#pLabel').text($(this).text());
                $('#pLabel').attr('style', 'block');
            }
        })

        if (_GlobVar.firstLogin === true){
            $('html, body').animate({ scrollTop: 0 }, 'fast');
            _GlobVar.firstLogin = false;
        }
        else {
            $('html, body').animate({ scrollTop: 200 }, 'fast');
            _GlobVar.firstLogin = false;
        }

        $.ajax({
            type: 'GET',
            url: '/api/Products/GetProductList?category=' + category,
            beforeSend: function() {
                res.container.append(res.loader);
            },
          
            success: function (data) {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                HideCartControls();
                HideOrderGuideControls();
                ShowPtableControls();
                HideCheckOutControls();
                HideOrderControls();
                HideOrderConfrimation();
                HideEmptyOrderGuideMsg();
                HideEmptyCartMsg();
                $('#ProductsTable').DataTable({
                    'lengthMenu': [[10, 25, 50, 100, -1], [10, 25, 50, 100, " All"]],
                    'columnDefs': [
                        { className:'tdNoStyle', 'targets': [3,4,5]}
                    ],
                    data: data,
                    'bDestroy': true,
                    'pagingType': 'full_numbers',
                    columns: [
                        { 'data': 'ProdCode' },
                        { 'data': 'Description' },
                        { 'data': 'Pk_Wt_Sz' , 'bSortable': false },
                        { 'data': 'CaseString', 'bSortable': false },
                        { 'data': 'EachString', 'bSortable': false },
                        { 'data': 'PoundString', 'bSortable': false },
                    ]
                });

                AddItem();
                EnableToolTip();
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Unable to fetch product list by current category. Please Try Again.');
            }
        });
    }


    $('#MoveToSession').on('click', function(groupID) {
        groupID = _GlobVar.CurrentGuideId();
        var merge = 0;
        $.ajax({
            type: 'POST',
            url: '/api/OrderGuides/MoveGuideToSession/' + groupID + '/' + merge,
            success: function(data) {
                GetSessionCart();
                HideOrderGuideControls();
                $('#CartMsg')
                    .html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp; Your Order Guide Has Replaced Your Existing Cart.');
                $('#CartMsg')
                    .attr({
                        style: 'display:block',
                        class: 'alert alert-success text-center'
                    });
                $('#CartMsg')
                    .append('<button id="hideCartMsg" type="button" class="close" aria-hidden="true"> × </button>');
                ShowCartControls();
                HideOrderGuideControls();
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Unable to use this guide now. Please Try Again.');
            }
        });
    });


    $('#MergeToSession').on('click',function() {
        var groupID = _GlobVar.CurrentGuideId();
        var merge = 1;
        $.ajax({
            type: 'POST',
            url: '/api/OrderGuides/MoveGuideToSession/' + groupID + '/' + merge,

            success: function(data) {
                GetSessionCart();
                HideOrderGuideControls();
                $('#CartMsg')
                    .html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp; Your Order Guide Has Been Merged With Your Existing Cart.');
                $('#CartMsg')
                    .attr({
                        style: 'display:block',
                        class: 'alert alert-success text-center'
                    });
                $('#CartMsg')
                    .append('<button id="hideCartMsg" type="button" class="close" aria-hidden="true"> × </button>')
                ShowCartControls();
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Unable to use this guide now. Please Try Again.');
            }
        });
    });


    $('#ViewCurrentOrder').on('click', function () {
        var priceStatus = $('#Cart').data('value');

        if (priceStatus === 'withPrice') {
            $('#extendedHeader').replaceWith('<th id ="delColumn">Remove</th>');
            $('#extd').remove();
        }

        HideOrderGuideControls();
        HidePtableControls();
        ShowCartControls();
        $('#CartMsg').hide();
        GetSessionCart();
        HideCheckOutControls();
        HideOrderControls();
        HideOrderConfrimation();
    });

    function UpdateWeight() {
        $('#Cart tbody tr #quantity').on('focusout', function () {
            var quantity = $(this).val();
            var weight = $(this).closest('tr').find('#weight').data('value');
            if (weight != null) {
                $(this).closest('tr').find('#weight').text((weight * quantity).toFixed(2));
            }
        
        });
    }

    function ShowEmptyCartMsg() {
        $('#emptyCart').attr('style', 'display:block');
        $('#Cart').hide();
        $('#CartMsg').hide();
    }

    function HideEmptyCartMsg() {
        $('#emptyCart').attr('style', 'display:none');
    }

    function GetSessionCart() {

        HideEmptyOrderGuideMsg();
        $('#pLabel').text('Current Order');
        $('#pLabel').show();

        var priceStatus = $('#Cart').data('value');

        if (priceStatus === 'withPrice') {
            $('#Footer > tr').replaceWith('<tr><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>');
            $('#extendedHeader').remove();
            $('#extd').remove();
        };

        $.ajax({
            type: 'GET',
            url: '/api/OrderGuides/GetSessionCart/',
            beforeSend: function() {
                res.container.append(res.loader);
            },
            success: function(OrderGuides) {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');

                if (OrderGuides.length > 0) {
                    $('#Cart tbody > tr').remove();
                    if (priceStatus === 'withPrice') {
                        $.each(OrderGuides, function (i, OrderGuide) {
                            var weight;
                            if (OrderGuide.isManufactured === true) {
                                weight = OrderGuide.Weight * OrderGuide.Qty;
                            } else {
                                weight = "";
                            }
                            var tabindex = i + 1;

                            $(this).find('#extended').remove();

                            $('#Cart').append('<tr id="' + OrderGuide.RowID +
                                '"><td id="Code" class="col-lg-1">' + OrderGuide.Code + '</td><td id="Description" class="col-lg-3">' +
                                OrderGuide.Description + '</td><td class="col-lg-1 text-center">' + OrderGuide.PackWeight +
                                '</td><td class="col-lg-1 text-center"><input type="number" class="form-control" tabindex=' + tabindex + ' id="quantity" value="' +
                                OrderGuide.Qty + '"></td><td class="col-lg-1 text-center">' + OrderGuide.Unit +
                                '</td><td id="weight" data-value="' + OrderGuide.Weight + '" class="text-center col-lg-1">' + weight +
                                '</td><td class="text-center col-lg-3"><input type="text" maxlength="15" class="form-control" id="inputforcomment" value="' +
                                $.trim(OrderGuide.Comment) + '"></td><td id="Price" class="text-right col-lg-1">' + OrderGuide.sellPrice + '</td>' +
                                '<td id="deleteBtn" class="col-lg-1 text-center"><button type="button" class="btn btn-danger CartItem"><span class="glyphicon glyphicon-remove"></span></button></td></tr>'
                            );
                        });
                       
                    }

                    else {
                        $.each(OrderGuides, function (i, OrderGuide) {
                            var weight;
                            if (OrderGuide.isManufactured === true) {
                                weight = OrderGuide.Weight * OrderGuide.Qty;
                            } else {
                                weight = "";
                            }
                            var tabindex = i + 1;
                            $('#Cart').append('<tr id="' + OrderGuide.RowID +
                                '"><td id="Code" class="col-lg-1">' + OrderGuide.Code + '</td><td id="Description" class="col-lg-3">' +
                                OrderGuide.Description + '</td><td class="col-lg-1 text-center">' + OrderGuide.PackWeight +
                                '</td><td class="col-lg-1 text-center"><input type="number" class="form-control" tabindex=' + tabindex + ' id="quantity" value="' +
                                OrderGuide.Qty + '"></td><td class="col-lg-1 text-center">' + OrderGuide.Unit +
                                '</td><td id="weight" data-value="' + OrderGuide.Weight + '" class="text-center col-lg-1">' + weight +
                                '</td><td class="text-center col-lg-3"><input type="text" maxlength="15" class="form-control" id="inputforcomment" value="' +
                                $.trim(OrderGuide.Comment) + '">' +
                                '</td><td id="deleteBtn" class="col-lg-1 text-center"><button type="button" class="btn btn-danger CartItem"><span class="glyphicon glyphicon-remove"></span></button></td></tr>'
                            );
                        });
                    }

                    $('#CurrentOrder').text(OrderGuides.length);
                    DeleteItem();
                    UpdateWeight();

                } else {
                    $('#Cart tbody > tr').remove();
                    $('#CurrentOrder').text(0);
                    $('#CartMsg').html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp; Warning : Your Cart is Empty !');
                    $('#CartMsg')
                        .attr({
                            style: 'display:block',
                            class: 'alert alert-danger text-center',
                        });
                    $('#CartMsg').append('<button id="hideCartMsg" type="button" class="close" aria-hidden="true"> × </button>');
                    ShowCartControls();
                    ShowEmptyCartMsg();

                }

            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Unable to fetch current order. Please Try Again.');
            }
        });
    }


    $('#Update').on('click', function () {
        $('html, body').animate({ scrollTop: 0 }, 'fast');
        UpdateCart();
    });

    $('#checkout').click(function () {
        var priceStatus = $('#Cart').data('value');

        $('html, body').animate({ scrollTop: 0 }, 'fast');
        var CurrentOrder = [];

        $('#Cart tbody tr').each(function() {
            var lineItem = {
                rowid: $(this).closest('tr').attr('id'),
                quantity: $(this).find('td').eq(3).find('#quantity').val(),
                comment: $(this).find('td').eq(6).find('#inputforcomment').val()
            }
            CurrentOrder.push(lineItem);
        });

        $.ajax({
            type: 'PUT',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(CurrentOrder),
            url: '/api/Cart/UpdateCart/',
            beforeSend: function() {
                res.container.append(res.loader);
            },
            success: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');

                var rowCount = $('#Cart tbody tr').length;
                if (rowCount < 1) {
                    $('#CartMsg').html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp; <strong>Warning! Your Cart is Empty.</strong>');
                    $('#CartMsg').attr({
                        style: 'display:block',
                        class: 'alert alert-danger text-center'
                    });
                    return false;
                }

                $('#Cart tbody tr') .each(function() {
                    var quantity = Number($(this).find('#quantity').val());
                    if (quantity <= 0) {
                        $(this).remove();
                    }
                    else {

                        if (priceStatus === 'withPrice') {

                            var quantity = Number($(this).find('#quantity').val());
                            var Price = Number($(this).find('#Price').text());
                            var Extended = (quantity * Price).toFixed(2);

                            $(this).append('<td id="extended" class="text-right">' + Extended + '</td>');

                            $('#quantity').replaceWith(quantity);
                            $('#inputforcomment').replaceWith($('#inputforcomment').val());
                            var weight = Number($(this).find('#weight').attr('data-value'));
                            var weightValue = weight * quantity;

                            if ($.isNumeric(weight)) {
                                $(this).find('#weight').text(weightValue);
                            }
                            else {
                                $(this).find('#weight').text('');
                            }

                            $('#deleteBtn').remove();

                            $('#CartMsg').html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp; <strong>Please Review Your Order...Prices (if shown) Subject To Change.</strong>');
                            $('#CartMsg')
                                .attr({
                                    style: 'display:block',
                                    class: 'alert alert-info text-center'
                                });
                            HideSessionCartControls();
                            ShowCheckOutControls();

                        }

                        else {

                            $('#quantity').replaceWith($('#quantity').val());
                            $('#inputforcomment').replaceWith($('#inputforcomment').val());
                            var weight = Number($(this).find('#weight').attr('data-value'));
                            var weightValue = weight * quantity;
                            if ($.isNumeric(weight)) {
                                $(this).find('#weight').text(weightValue);
                            } else {
                                $(this).find('#weight').text('');
                            }

                            $('#deleteBtn').replaceWith('<td></td>');

                            $('#CartMsg')
                                .html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp; <strong>Please Review Your Order...Prices (if shown) Subject To Change.</strong>');
                            $('#CartMsg')
                                .attr({
                                    style: 'display:block',
                                    class: 'alert alert-info text-center'
                                });
                            HideSessionCartControls();
                            ShowCheckOutControls();

                        }
                    }
                });

                if (priceStatus === 'withPrice') {
                    $('#delColumn').remove();
                    $('#CartHeader tr').append('<th id="extendedHeader">Extended</th>');
                    $('#Footer > tr').remove();
                    $('#Footer').append('<tr id ="priceFooter"><td></td><td colspan="3" class="text-center">( Confirmation only, not an invoice )</td>' +
                    '<td></td><td></td><td class ="text-right">Est Total :</td><td class="text-center" colspan="2" id="Total"></td></tr>');
                    var grandTotal = 0;
                    $('#Cart tbody tr').each(function () {
                        var eachTotal = Number($(this).find('#extended').text());
                        grandTotal += eachTotal;
                    });
                    $('#Total').text('$ ' + grandTotal.toFixed(2));
                }

                var rCount = $('#Cart tbody tr').length;
                $('#CurrentOrder').text(rCount);
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Network Error. Please Try again.');
            }
        });
    });

    $('#back').click(function () {
        $('html, body').animate({ scrollTop: 0 }, 'fast');
        $('#ViewCurrentOrder').trigger('click');
    })

    $('#backToCurrentOrder').click(function () {

        var priceStatus = $('#Cart').data('value');

        if (priceStatus === 'withPrice') {
            $('#extendedHeader').replaceWith('<th id ="delColumn">Remove</th>');
            $('#extd').remove();
        }

        $('html, body').animate({ scrollTop: 0 }, 'fast');
        GetSessionCart();
        HideCheckOutControls();
        HideOrderControls();
        ShowSessionCartControls();
        $('#CartMsg').hide();
    });

    $('#continueCheckout').click(function() {
        $('html, body').animate({ scrollTop: 0 }, 'fast');
        $('#checkoutControls').hide();
        ShowOrderControls();
        $('#CartMsg').hide();
        GetDeliveryAddress();
        $('#pLabel').hide();
    });

    function UpdateCart() {
        $('html, body').animate({ scrollTop: 0 }, 'fast');

        var rowCount = $('#Cart tbody tr').length;
        if (rowCount < 1) {
            $('#CartMsg').html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp; <strong>Warning! Your Cart is Empty.</strong>');
            $('#CartMsg').attr({
                style: 'display:block',
                class: 'alert alert-danger text-center'
            });
            return false;
        }

        var CurrentOrder = [];
        $('#Cart tbody tr')
            .each(function() {

                var lineItem = {
                    rowid: $(this).closest('tr').attr('id'),
                    quantity: $(this).find('td').eq(3).find('#quantity').val(),
                    comment: $(this).find('td').eq(5).find('#inputforcomment').val(),
                }

                CurrentOrder.push(lineItem);
            });

        $.ajax({
            type: 'PUT',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(CurrentOrder),
            url: '/api/Cart/UpdateCart/',
            beforeSend: function() {
                res.container.append(res.loader);
            },
            success: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                $('#CartMsg').attr({
                    style: 'display:block',
                    class: 'alert alert-success text-center',
                });
                GetSessionCart();
                $('#CartMsg')
                    .html('<span class="glyphicon glyphicon-info-sign"></span>&nbsp;&nbsp;Your Cart Has Been Updated.');
            },
            error: function() {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Current Order could not be updated. Please Try again.')
            }
        });

    }

    

    $('#deleteCartItem').click(_GlobVar.CartItem, function () {
       
        var item = _GlobVar.CartItem;

        $.ajax({
            type: 'Delete',
            url: '/api/Cart/DeleteItemById?rowId=' + _GlobVar.CartItem.RowID,
            contentType: 'application/json; charset=utf-8',

            beforeSend: function () {
                res.container.append(res.loader);
            },

            success: function () {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowItemMsg('Item : "' + item.Name + '" has been deleted from your Order.');
                CurrentOrderbadgeDecrement(1);
            },

            error: function () {
                res.container.find(res.loader).remove();
                $('.loader').attr('style', 'display:none');
                ShowErrorMsg('Current row could not be deleted. Please Try again.');
            }
        });

        $('#Cart tbody tr').each(function () {
            var id = $(this).attr('id');
            if (id == _GlobVar.CartItem.RowID) {

                $('#' + id).fadeOut(150, function () {
                    $('#' + id).remove();
                });
            };
        });

        _GlobVar.CartItem = null;
        
    });
    
    function DeleteItem() {
        $('.btn.btn-danger.CartItem').on('click', function () {

            _GlobVar.CartItem = {
                RowID : $(this).closest('tr').attr('id'),
                Name : $(this).closest('tr').find('td').eq(0).text()
            }

            $('#delItemConf').modal('show');
            $('#delItemConfContent').text('Delete Item : ' + _GlobVar.CartItem.Name + ' ?');
        });
    }


