// ajax请求
function sendAjaxRequest(url,
    httpMethod,
    successCallback,
    errorCallback,
    mydata,
    myasync) {

    //alert("1");
    var apiFullPath = getRootPath() + url;
    //alert("sendAjaxRequest-" + apiFullPath);

    //alert("test");

    $.ajax(apiFullPath, {
        type: httpMethod,
        contentType: "application/json; charset=utf-8",
        //dataType: "json", 
        success: successCallback,
        error: errorCallback,
        data: mydata,
        async: myasync
    });
}

// 得到虚拟目录路径
function getRootPath() {
    var pathName = window.location.pathname.substring(1);
    // alert("pathname["+ pathName+"]");
    var webName = pathName == '' ? '' : pathName.substring(0, pathName.indexOf('/'));
    //alert("webName[" + webName + "]");
    var rootPath = window.location.protocol + '//' + window.location.host;//+ '/' + webName;

    //alert("rootPath[" + rootPath + "]");
    return rootPath;
}

function showLoading() {

    var height = window.screen.availHeight;
    var scrollTop = $(window).scrollTop();
    var mytop = scrollTop + height / 2;
    $("#loading").css("top", mytop);

    $("#loading").show();//显示loading

    //alert("showLoading");
}
function hideLoading() {
    $("#loading").hide();
}
function showMaskLayer() {
    var bg = $("#mask-background,#mask-progressBar");

    var background = $("#mask-background");
    var progressBar = $("#mask-progressBar");

    // 设背景高度
    var docHeight = $(document).height();
    $(background).css("height", docHeight);

    // 设进度条
    var screenHeight = window.screen.availHeight;
    var scrollTop = $(window).scrollTop();
    var mytop = scrollTop + screenHeight / 2;
    $(progressBar).css("top", mytop);

    bg.show();
    //alert("223");
}
function hideMaskLayer() {
    var bg = $("#mask-background,#mask-progressBar");
    bg.hide();
}