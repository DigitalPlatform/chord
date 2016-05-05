
// 显示等待图层
function loadLayer() {
    return layer.open({
        type: 2,
        shadeClose: false
    });
}

// 得到虚拟目录路径
function getRootPath() {
    var pathName = window.location.pathname.substring(1);
    var webName = pathName == '' ? '' : pathName.substring(0, pathName.indexOf('/'));
    var rootPath = window.location.protocol + '//' + window.location.host;//+ '/' + webName;
    return rootPath;
}

// ajax请求
function sendAjaxRequest(url,
    httpMethod,
    successCallback,
    errorCallback,
    mydata,
    myasync) {

    var apiFullPath = getRootPath() + url;
    //alert(rootPath +url);
    $.ajax(apiFullPath, {
        type: httpMethod,
        success: successCallback,
        error: errorCallback,
        data: mydata,
        async: myasync
    });
}