function openMsg(msg, endCallback) {
    alert(msg);
    /*
    layer.open({
        //title:'提示信息',
        content: msg,
        end: endCallback
    });
    */
}
// 显示等待图层
function loadLayer() {
    return layer.open({
        type: 2,
        shadeClose: false
    });
}


// 显示服务器错误
function alertServerError(info) {
    alert("服务器返回错误：" + info);
    //layer.alert("服务器返回错误：" + errorThrown, { icon: 2 });
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
    //alert(apiFullPath);
    $.ajax(apiFullPath, {
        type: httpMethod,
        success: successCallback,
        error: errorCallback,
        data: mydata,
        async: myasync
    });
}