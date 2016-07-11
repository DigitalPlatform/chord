

//============pending=============

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
        var url = "/api/biblio?id=" + encodeURIComponent(myvalue)
            + "&format=summary"
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
        var url = "/api/biblio?id=" + encodeURIComponent(myvalue)
            + "&format=more-summary"
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
                    + "&group=gn:_lib_homePage"
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
                    msgHtml += getMsgViewHtml(msgItem, true);
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

//============消息相关=============

function getMsgViewHtml(msgItem, bContainEditDiv){
    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确["+group+"]。");
        return;
    }
    var bContainSubject = false;
    var bShowTime = false;
    if (group == "gn:_lib_bb") {
        bShowTime = true;
    }
    else if (group == "gn:_lib_homePage") {
        bContainSubject = true;
    }

    var html = "";
    //alert("aa");

    if (bContainEditDiv == true)
        html += "<div class='mui-card message' id='_edit_" + msgItem.id + "' onclick=\"clickMsgDiv('" + msgItem.id + "')\">";

    html += "<table class='view'>"
                    + "<tr>"
                        + "<td class='title' >" + msgItem.title + "</td>"
                        + "<td class='btn'>"
                            + "<div id='btnEdit' style='display: none;'>"
                                + "<button class='mui-btn mui-btn-default' onclick=\"gotoEdit('" + msgItem.id + "')\">编辑</button>&nbsp;"
                                + "<button class='mui-btn mui-btn-danger' onclick=\"deleteMsg('" + msgItem.id + "')\">X&nbsp;删除</button>"
                            + "</div>"
                        + "</td>"
                    + "</tr>"

    if (bShowTime == true) {
        html += "<tr>"
            + "<td colspan='2' class='time'>"
                    + "<span>" + msgItem.publishTime + "</span>-<span>" + msgItem.creator + "</span>"
            + "</td>"
        + "</tr>"

    }

    html += "<tr>"
                        + "<td colspan='2' class='content'>"
                        + msgItem.contentHtml
                        + "</td>"
                    + "</tr>"
                + "</table>";

    if (bContainEditDiv == true)
        html += "</div>";

    return html;
}

// 删除msg
function deleteMsg(msgId) {
    //alert(msgId);

    //alert(autoDeleteParent);
    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }
    var autoDeleteParent = false;
    if (group == "gn:_lib_homePage") {
        autoDeleteParent = true;
    }

    var libId = getLibId(); //$("#selLib").val();
    if (libId == "") {
        alert("异常情况：libId为空。");
        return;
    }
    var userName = $("#_userName").text();
    if (userName == "") {
        alert("异常情况：userName为空。");
        return;
    }

    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId
    var title = $(divId).find(".title").html();
    //alert(title);

    var gnl = confirm("你确定要删除[" + title + "]吗?");
    if (gnl == false) {
        return false;
    }

    //显示等待图层
    var index = loadLayer();
    var url = "/api/LibMessage?libId=" + libId
        + "&group=" + encodeURIComponent("gn:_lib_homePage")
        + "&msgId=" + msgId
        + "&userName=" + userName
    sendAjaxRequest(url, "DELETE", function (result) {

        // 关闭等待层
        layer.close(index);

        if (result.errorCode == -1) {
            alert("操作失败：" + result.errorInfo);
            return;
        }

        alert("删除成功");

        // 将界面上的div删除

        // 找到父亲
        var subjectDiv = $(divId).parent();
        // 删除自己;
        $(divId).remove();

        // 当消息分栏目显示时，没有消息时自动删除栏目
        if (autoDeleteParent == true) {
            // 如果父亲下级没有message，父亲也删除
            if ($(subjectDiv).children(".message").length == 0) {
                // 移除栏目div
                subjectDiv.remove();
            }
        }

    }, function (xhq, textStatus, errorThrown) {
        alert(errorThrown);
    });

}

// 保存完后，显示一条消息
function viewMsg(msgId, msgItem) {

    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }
    var bContainSubject = false;
    var bShowTime = true;
    if (group == "gn:_lib_homePage") {
        bContainSubject = true;
        bShowTime = false;
    }


    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId
    if (msgId == "new") {

        // 得到完整的div
        var msgViewHtml = getMsgViewHtml(msgItem, true);

        if (bContainSubject==false) {
            // 加到最上面
            $("#_subject_main").prepend(msgViewHtml);
        }
        else {
            var subject = $("#_val_subject").val(); //
            // 要先找下同名的subject，如果不存在，新创建一个subject div放在最上面
            var subjectObj = $("#_subject_main").children("#_subject_" + subject);
            if (subjectObj.html() != null) {  //注意这里要用html()
                var titleObj = $(subjectObj).find("#_subject_title");
                $(msgViewHtml).insertAfter(titleObj);
            }
            else {
                // 给框架加一条栏目
                model.subjects.push(subject);
                model.selSubject(subject); // 设当前选择的栏目

                //alert("2");
                var subjectDiv = "<div id='_subject_" + subject + "'  class='subject'>"
                    + "<div id='_subject_title' class='firstline'><span class='title'>" + subject + "<span></div>"
                    + msgViewHtml
                    + "</div>";

                //alert(subjectDiv);
                $("#_subject_main").prepend(subjectDiv);
            }
        }

        //创建按钮可见
        $("#btnCreate").css('display', 'block');
        $(divId).css('display', 'none');
        $(divId).html("");

        return;
    }

    // 拼出内部的html，直接替换原来内容
    var msgViewHtml = getMsgViewHtml(msgItem, false);

    //alert("返回的item-" + msgItem.subject);
    $(divId).html(msgViewHtml);
}

// 保存
function save(msgId) {

    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }
    var bContainSubject = false;
    var titleCanEmpty = false;
    if (group == "gn:_lib_homePage") {
        bContainSubject = true;
        titleCanEmpty = true;
    }

    var libId = getLibId(); //$("#selLib").val();
    if (libId == "") {
        alert("异常情况：libId为空。");
        return;
    }
    var userName = $("#_userName").text();
    if (userName == "") {
        alert("异常情况：userName为空。");
        return;
    }

    if (msgId == null || msgId == "") {
        alert("未传入msgId");
        return;
    }

    // subject
    var subject = "";
    if (bContainSubject == true) {
        subject = $("#_val_subject").val(); //
        if (subject == "") {
            alert("请先选择栏目");
            return;
        }
    }

    //alert(subject);

    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId

    var action = "";
    if (msgId == "new") {
        action = "POST";
    }
    else {
        action = "PUT";
    }


    var title = $("#_val_title").val();
    // 对于图书馆主页，标题允许为空，因为已经有了栏目标题
    if (titleCanEmpty == false) {
        if (title == "") {
            alert("请输入标题。");
            return;
        }
    }

    var content = $("#_val_content").val();
    if (content == "") {
        alert("请输入内容。");
        return;
    }

    // 备注
    var remark = $("#_val_remark").val();

    // 格式 text/markdown
    var format = $("#_selFormat").val();
    //alert(format);


    var group = "gn:_lib_bb";

    //显示等待图层
    var index = loadLayer();

    var id = "";
    if (msgId != "new")
        id = msgId;

    var url = "/api/LibMessage"
        + "?group=" + group
        + "&libId=" + libId;
    sendAjaxRequest(url, action,
        function (result) {

            // 关闭等待层
            layer.close(index);

            if (result.errorCode == -1) {
                alert("操作失败：" + result.errorInfo);
                return;
            }

            alert("操作成功");

            if (result.items == null || result.items.length == 0) {
                alert("未返回保存后的消息对象");
            }

            var item = result.items[0];

            //alert("回来的消息标题:"+item.title);
            viewMsg(msgId, item);

        },
        function (xhq, textStatus, errorThrown) {
            alert(errorThrown);
            // 关闭等待层
            layer.close(index);
        },
        {
            id: id,
            title: title,
            content: content,
            contentFormat: format,
            creator: userName,
            subject: subject,
            remark: remark
        }
    );

}

// 获取编辑态html
function getMsgEditHtml(msgItem) {

    var group = $("#_group").text();
    if (group == null || group == "") {
        alert("异常情况：group参数未设值");
        return;
    }
    var bContainSubject = false;
    if (group == "gn:_lib_bb") {
        bContainSubject = false;
    }
    else if (group == "gn:_lib_homePage") {
        bContainSubject = true;
    }
    else {
        alert();
        return;
    }

    // 工作人员账号
    var userName = $("#_userName").text();
    if (userName == null || userName == "") {
        alert("异常情况：userName为空。");
        return;
    }

    var formatTextStr = " selected ";// 默认文本格式选中
    var formatMarkdownStr = "";

    var saveBtnName = "新增";
    var disabledStr = "";// "disabled"
    var subject = "";//model.selSubject();
    var msgId = "new"; //默认新建的情况
    var title = "";
    var remark = "";
    var content = "";
    if (msgItem != null) {
        msgId = msgItem.id;
        title = msgItem.title;
        remark = msgItem.remark;
        content = msgItem.content;
        subject = msgItem.subject;

        disabledStr = " disabled='disabled' ";

        if (msgItem.contentFormat == "markdown")
            formatMarkdownStr = " selected ";

        saveBtnName = "保存";
    }


    var html = "<table class='edit'>"

    // 加选择栏目行
    if (bContainSubject == true) {
        var subjectHtml = getSubjectHtml(subject, disabledStr);
        html += "<tr>"
            + "<td class='label'>栏目</td>"
            + "<td>"
                + "<div style='border:1px solid #cccccc'>"
                + subjectHtml
                + "</div>"
                + "<div id='divNewSubject' style='display:none;margin-top:5px'>"
                    + "<input id='_val_subject' type='text' value='" + subject + "' placeholder='请输入自定义栏目'>"
                + "</div>"
            + "</td>"
        + "</tr>"
    }

    html += "<tr>"
        + "<td class='label'>标题</td>"
        + "<td>"
            + "<input class='mui-input mui-input-clear' id='_val_title' type='text' value='" + title + "'>"
        + "</td>"
    + "</tr>"
    + "<tr>"
        + "<td class='label'>内容</td>"
        + "<td>"
            + "<div style='border:1px solid #cccccc;'>"
                + "<select id='_selFormat'>"
                    + "<option value='text' " + formatTextStr + ">文本格式</option>"
                    + "<option value='markdown' " + formatMarkdownStr + ">Markdown格式</option>"
                + "</select>"
            + "</div>"
        + "</td>"
    + "</tr>"
    + "<tr>"
        + "<td colspan='2'>"
            + "<textarea id='_val_content' rows='5'>" + content + "</textarea>"
        + "</td>"
    + "</tr>"
    + "<tr>"
        + "<td colspan='2' >"
            + "<span class='label'>注释</span>"
            + "<textarea id='_val_remark' rows='3'>" + remark + "</textarea>"
        + "</td>"
    + "</tr>"
    + "<tr>"
        + "<td colspan='2'>"
            + "<button class='mui-btn mui-btn-primary' onclick=\"save('" + msgId + "')\">" + saveBtnName + "</button>&nbsp;&nbsp;"
            + "<button class='mui-btn mui-btn-default' onclick=\"cancelEdit('" + msgId + "')\">取消</button>"
        + "</td>"
    + "</tr>"
+ "</table>";

    return html;
}

// 单击msg进行只读态与编辑态的切换
function clickMsgDiv(msgId) {
    if (msgId == null || msgId == "") {
        alert("未传入msgId");
        return;
    }

    // 工作人员账号
    var userName = $("#_userName").text();
    if (userName == "") {
        return;
    }

    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId
    var editBtn = $(divId).find("#btnEdit");

    // 这时候已经不是在浏览界面，应该是编辑态了
    var viewTable = $(divId).children(".view").html();
    if (viewTable == null || viewTable == "") {
        return;
    }

    var editStateClass = "msgEditable";
    var editState = $(divId).hasClass(editStateClass);
    if (editState == true) {
        $(divId).removeClass(editStateClass);

        $(editBtn).css("display", "none");
    }
    else {
        $(divId).addClass(editStateClass);

        $(editBtn).css("display", "block");
    }
}

// 取消新增或者修改
function cancelEdit(msgId) {

    var group = $("#_group").text();
    if (group == null || group == "") {
        alert("异常情况：group参数未设值");
        return;
    }


    if (msgId == null || msgId == "") {
        alert("未传入msgId");
        return;
    }



    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId

    //取消新增
    if (msgId == "new") {
        //创建按钮不可见
        $("#btnCreate").css('display', 'block');
        $(divId).css('display', 'none');
        $(divId).html("");
        return;
    }

    //显示态html
    var viewHtml = "";

    // 根据id从服务器取记录，并只读态
    var libId = getLibId();
    var weixinId = $("#weixinId").text();
    if (libId == "") {
        alert("请选择图书馆");
        return;
    }
    if (weixinId == "") {
        alert("异常情况：weixinId为空");
        return;
    }
    var group = "gn:_lib_homePage";
    //显示等待图层
    var index = loadLayer();
    var style = "browse";
    /*
    GetMessage(string weixinId, 
    string group,
    string libId, 
    string msgId,
    string subject,
    string style)
    */
    // 调web api
    var url = "/api/LibMessage?weixinId=" + weixinId
                + "&group=" + group
                + "&libId=" + libId
                + "&msgId=" + msgId
                + "&subject="
                + "&style=" + style;
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        layer.close(index);

        //alert("回来-"+result.errorCode);
        if (result.errorCode == -1) {
            alert(result.errorInfo);
            return;
        }
        if (result.items != null && result.items.length > 0) {

            // 把数据填在编辑界面
            var item = result.items[0];
            var html = getMsgViewHtml(item, false);
            $(divId).html(html);
        }

    }, function (xhq, textStatus, errorThrown) {

        alert("访问服务器出错：\r\n" + errorThrown);
        // 关闭等待层
        layer.close(index);
    });

}

// 进入编辑态
function gotoEdit(msgId) {
    if (msgId == null || msgId == "") {
        alert("未传入msgId");
        return;
    }

    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId

    // 新增的情况
    if (msgId == "new") {
        //创建按钮不可见
        $("#btnCreate").css('display', 'none');
        $(divId).css('display', 'block');
        var html = getMsgEditHtml(null);
        $(divId).html(html);

        //由于一进来没有显示编辑界面，所以这里要重新设一下
        setShowTopButton();
        return;
    }

    //根据id从服务器上取记录
    var libId = getLibId(); //$("#selLib").val();
    var weixinId = $("#weixinId").text();
    if (libId == "") {
        alert("请选择图书馆");
        return;
    }
    if (weixinId == "") {
        alert("异常情况：weixinId为空");
        return;
    }
    var group = "gn:_lib_bb";
    //显示等待图层
    var index = loadLayer();
    var style = "original";
    /*
    GetMessage(string weixinId, 
    string group,
    string libId, 
    string msgId,
    string subject,
    string style)
    */
    // 调web api
    //alert("gotoEdit 1");
    var url = "/api/LibMessage?weixinId=" + weixinId
                + "&group=" + group
                + "&libId=" + libId
                + "&msgId=" + msgId
                + "&subject="
                + "&style=" + style;
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        layer.close(index);

        //alert("gotoEdit 2\n"+url);


        //alert("回来-"+result.errorCode);
        if (result.errorCode == -1) {
            alert(result.errorInfo);
            return;
        }
        //alert("gotoEdit 3");

        // 把返回的数组加到观察数组
        if (result.items != null && result.items.length > 0) {

            //alert("gotoEdit 4");

            // 把数据填在编辑界面
            var item = result.items[0];
            var html = getMsgEditHtml(item);
            $(divId).html(html);

            //alert("gotoEdit 5");


            //由于一进来没有显示编辑界面，所以这里要重新设一下
            setShowTopButton();

            //alert("gotoEdit 6");

        }

    }, function (xhq, textStatus, errorThrown) {

        alert("访问服务器出错：\r\n" + errorThrown);
        // 关闭等待层
        layer.close(index);
    });
}

//===========等待图层============
// 显示等待图层
function loadLayer() {
    return layer.open({
        type: 2,
        shadeClose: false
    });
}


//=======ajax==============

// 显示服务器错误
function alertServerError(info) {
    alert("服务器返回错误：" + info);
    //layer.alert("服务器返回错误：" + errorThrown, { icon: 2 });
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


