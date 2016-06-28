

// 万能函数，给样式为pending的元素填充内容
function fillPending() {
    //alert("fillPending");

    // 选取第一个样式为pending的元素
    var o = $(".pending:first");
    if (o.length == 0) {
        //alert("fillPending1-1");
        // 没有pending类型元素则返回
        return;
    }

    // 找到下级的标签，里面存储的线索
    var mylable = o.children("label");
    // 如果没有定义标签label，则去掉pending状态，继续下一个pending
    if (mylable.length == 0) {
        //alert("fillPending1-2");
        o.removeClass("pending");
        window.setTimeout("fillPending()", 1);
        return;
    }

    // 取出label设置的线索信息
    var keyword = mylable.text().trim();
    // 未给label设值，去掉pending状态，继续下一个pending
    if (keyword.length == 0) {
         //alert("fillPending1-3:[" + mylable.text() + "]");
        o.removeClass("pending");
        window.setTimeout("fillPending()", 1);
        return;
    }

    // bs-为书目summary，rs-为读者summary
    // 线索信息不足，去掉pending状态，继续下一个pending
    if (keyword.length <= 3) {
        //alert("fillPending1-4");
        o.html("");
        o.removeClass("pending");
        window.setTimeout("fillPending()", 1);
        return;
    }

    // 取出线索类型和值
    var mytype = keyword.substring(0, 3);
    var myvalue = keyword.substring(3);
    //alert("type[" + mytype + "]-value[" + myvalue + "]");
    if (myvalue == "") {
        o.html("");
        o.removeClass("pending");
        window.setTimeout("fillPending()", 1);
        return;
    }

    if (mytype == "bs-") {
        //alert("bs-"+keyword);
        var libId = o.children("span").text();
        var url = "/api/biblio?id=" + encodeURIComponent(myvalue) + "&format=summary"
            + "&libId=" + encodeURIComponent(libId);

        //alert(url);

        // 调api
        sendAjaxRequest(url, "GET", function (data) {
            //换成实际的值，去掉pending状态，继续下一个pending

            //var myhtml = "<div style='width:100%; white-space:nowrap;overflow:hidden;text-overflow:ellipsis; '  title='" + data + "'>"
            //    + data
            //    + "</div>";

            var myhtml = data;

            o.html(myhtml);

            o.removeClass("pending");
            window.setTimeout("fillPending()", 1);
        }, function (xhq, textStatus, errorThrown) {
            //换成实际的值，去掉pending状态，继续下一个pending
            o.html("访问服务器出错：" + errorThrown);
            o.removeClass("pending");
            window.setTimeout("fillPending()", 1);
        });
    }    
    else if (mytype == "ms-") {
        var libId = o.children("span").text();
        var url = "/api/biblio?id=more" + "&format=" + encodeURIComponent(myvalue)
            + "&libId=" + encodeURIComponent(libId);
        //alert(url);
        // 调api
        sendAjaxRequest(url, "GET", function (data) {
            //换成实际的值，去掉pending状态，继续下一个pending
            o.html(data);
            o.removeClass("pending");
            window.setTimeout("fillPending()", 1);
        }, function (xhq, textStatus, errorThrown) {
            //换成实际的值，去掉pending状态，继续下一个pending
            o.html("访问服务器出错：" + errorThrown);
            o.removeClass("pending");
            window.setTimeout("fillPending()", 1);
        });
    }
    else if (mytype == "su-") {
        var libId = o.children("span").text();

        // 调web api
        var url = "/api/LibMessage?weixinId=" //+ weixinId
                    +"&group=gn:_lib_homePage"
                    + "&libId=" + libId
                    + "&msgId="
                    + "&subject=" + encodeURIComponent(myvalue)
        + "&style=browse";

        //alert(myvalue);
        // 调api
        sendAjaxRequest(url, "GET", function (result) {

            if (result.errorCode == -1) {
                alert(result.errorInfo);
                return;
            }


            /*
    public class MessageItem
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string contentFormat { get; set; }  // 2016-6-20 text/markdown
        public string contentHtml { get; set; }  // 2016-6-20 html形态
        public string publishTime { get; set; } // 2016-6-20 jane 发布时间，服务器消息的时间

        public string creator { get; set; }  //创建消息的工作人员帐户

        public string subject { get; set; } // 栏目
        public string remark { get; set; } //注释
    }
            */


            //换成实际的值，
            var msgHtml = "";
            if (result.items != null) {
                for (var i = 0; i < result.items.length; i++) {
                    var msgItem = result.items[i];
                    msgHtml += getMsgViewHtml(msgItem,true);
                }
            }

            // 这里得到的是list
            //o.html(msgHtml);
            $(o).prop('outerHTML', msgHtml);
            //o.outerHTML(msgHtml);不支持这样写


            //去掉pending状态，继续下一个pending
            o.removeClass("pending");
            window.setTimeout("fillPending()", 1);

        }, function (xhq, textStatus, errorThrown) {
            //换成实际的值，去掉pending状态，继续下一个pending
            o.html("访问服务器出错：" + errorThrown);
            o.removeClass("pending");
            window.setTimeout("fillPending()", 1);
        });
    }

    else {
        // 继续下面的
        o.removeClass("pending");
        window.setTimeout("fillPending()", 1);
    }


    // 不能写到这里，因为上面的异常调用还没返回更新数据，会导致调多次api
    //处理下一个pending
    //window.setTimeout("fillPending()", 1);
    return;
}

function getMsgViewHtml(msgItem, bContainEditDiv) {

    var html = "";

    if (bContainEditDiv == true)
        html += "<div class='mui-card message' id='_edit_" + msgItem.id + "' onclick=\"clickMsgDiv('" + msgItem.id + "')\">";

    html += "<table>"
                    + "<tr>"
                        + "<td class='title'>" + msgItem.title + "</td>"
                        + "<td class='btn'>"
                            + "<div id='btnEdit' style='display: none;'>"
                                + "<button class='mui-btn mui-btn-default' onclick=\"gotoEdit('" + msgItem.id + "')\">编辑</button>&nbsp;"
                                + "<button class='mui-btn mui-btn-danger' onclick=\"deleteMsg('" + msgItem.id + "')\">X&nbsp;删除</button>"
                            + "</div>"
                        + "</td>"
                    + "</tr>"
                    + "<tr>"
                        + "<td colspan='2' class='time'>"
                                + "<span>" + msgItem.publishTime + "</span>-<span>" + msgItem.creator + "</span>"
                        + "</td>"
                    + "</tr>"
                    + "<tr>"
                        + "<td colspan='2' class='content'>"
                        + msgItem.contentHtml
                        + "</td>"
                    + "</tr>"
                + "</table>";

    if (bContainEditDiv == true)
        html += "</div>";

    return html;
}


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
    //alert("pathname["+ pathName+"]");
    var webName = pathName == '' ? '' : pathName.substring(0, pathName.indexOf('/'));
    //alert("webName[" + webName + "]");
    var rootPath = window.location.protocol + '//' + window.location.host;//+ '/' + webName;

    //alert("rootPath[" + rootPath + "]");
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
    //alert("sendAjaxRequest-" + apiFullPath);

    //alert("test");

    $.ajax(apiFullPath, {
        type: httpMethod,
        success: successCallback,
        error: errorCallback,
        data: mydata,
        async: myasync
    });
}

