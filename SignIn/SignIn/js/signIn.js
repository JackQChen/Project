
//文档加载
$(document).ready(function() {
//    $.ajax({
//        type: "POST",
//        url: "Ajax.ashx",
//        dataType: "json",
//        data: {
//            invoke: "SignIn.handle,SignIn|GetOpenIdByCode",
//            invokeParam: $.getUrlParam('code')
//        },
//        beforeSend: function() {
//        },
//        success: function(data) {
//            if (data.errmsg == null ) 
//            {
//                 $('#hfOpenId').val(data.openId);
//                 if(data.openId=='-1')
//                 {
//                bootbox.alert({
//                    title: "提示",
//                    message: "请在微信客户端进行浏览"
//                });
//                    $('#btnSign').attr('disabled', true);
//                    $('#btnSign').attr('class', 'del');
//                 }
//                else
//                {
//                init();
//                }
//            } 
//            else 
//            {
//                bootbox.alert({
//                    title: "提示",
//                    message: data.errmsg
//                });
//            }
//        },
//        complete: function(data) {
//        },
//        error: function(XMLHttpRequest, textStatus, thrownError) {
//            if (thrownError != '')
//                bootbox.alert(thrownError);
//        }
//    });
});

function init()
{
    $.ajax({
        type: "POST",
        url: "Ajax.ashx",
        dataType: "json",
        data: {
            invoke: "SignIn.handle,SignIn|GetInfo",
            invokeParam:  $('#hfOpenId').val()
        },
        beforeSend: function() {
            $('#btnSign').attr('disabled', true);
            $('#btnSign').attr('class', 'del');
        },
        success: function(data) {
            if (data.errmsg == null ) 
            {
                if (data.count == '0') 
                {
                    $('#btnSign').attr('disabled', false);
                    $('#btnSign').attr('class', 'submit');
                } 
                else 
                {
                    if (data.type == 'Staff') 
                    {
                        $('#tabStaff').css('display', 'block');
                        $('#tabGuest').css('display', 'none');
                        $('#txtPhoneNum_Staff').val(data.phone);
                        $('#txtName_Staff').val(data.name);
                        $('#txtWorkNumber').val(data.code);
                    } 
                    else 
                    {
                        $('#staffSign').removeClass('active');
                        $('#guestSign').addClass('active');
                        $('#tabStaff').css('display', 'none');
                        $('#tabGuest').css('display', 'block');
                        $('#txtInvite').val(data.code);
                    }
                    $('#btnSign').attr('disabled', true);
                    $('#btnSign').attr('class', 'del');
                }
            } 
            else 
            {
                bootbox.alert({
                    title: "提示",
                    message: data.errmsg
                });
            }
        },
        complete: function(data) {
        },
        error: function(XMLHttpRequest, textStatus, thrownError) {
            if (thrownError != '')
                bootbox.alert(thrownError);
        }
    });
}

function signIn()
{    
        var sInvoke= '';
        if($('#tabStaff').hasClass('active'))
            sInvoke=
            'Staff'+"|"+
            $('#hfOpenId').val()+"|"+
            $('#txtWorkNumber').val()+"|"+
            $('#txtName_Staff').val()+"|"+
            $('#txtPhoneNum_Staff').val();
        else
            sInvoke=
            'Guest',
            $('#hfOpenId').val()+"|"+
            $('#txtInvite').val()+"|"+
            $('#txtName_Guest').val()+"|"+
            $('#txtPhoneNum_Guest').val();
        $.ajax({
        type: "POST",
        url: "Ajax.ashx",
        dataType: "json",
        data: {
            invoke: "SignIn.handle,SignIn|SignIn",
            invokeParam: sInvoke
        },
        beforeSend: function() {
            $('#btnSign').attr('disabled', true);
            $('#btnSign').attr('class', 'del');
        },
        success: function(data) {
            if (data.errmsg == null ) 
            {
                bootbox.alert({
                    title: "提示",
                    message: data.result
                });
            $('#btnSign').attr('disabled', true);
            $('#btnSign').attr('class', 'del');
            } 
            else 
            {
                bootbox.alert({
                    title: "提示",
                    message: data.errmsg
                });
            $('#btnSign').attr('disabled', false);
            $('#btnSign').attr('class', 'submit');
            }
        },
        complete: function(data) {
        },
        error: function(XMLHttpRequest, textStatus, thrownError) {
            if (thrownError != '')
                bootbox.alert(thrownError);
        }
    });
}