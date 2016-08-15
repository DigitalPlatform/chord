
//======书目详细信息==========

// 获取详细书目记录
function getDetail(libId, recPath, obj, from) {

    //alert("getDetail 1");
    if (libId == null || libId == "") {
        alert("libId参数不能为空");
        return;
    }

    if (recPath == null || recPath == "") {
        recPath("libId参数不能为空");
        return;
    }

    var weixinId = $("#weixinId").text();
    if (weixinId == null || weixinId == "") {
        alert("weixinId参数为空");
        return;
    }

    //alert("getDetail 2");

    // 调GetBiblioDetail  api
    var url = "/api/biblio?weixinId=" + encodeURIComponent(weixinId)
        + "&libId=" + encodeURIComponent(libId)
        + "&biblioPath=" + encodeURIComponent(recPath)
        + "&format=table"
        + "&from=" + encodeURIComponent(from);

    //alert("getDetail 3");
    //alert(url);

    sendAjaxRequest(url, "GET", function (result) {

        //alert("getDetail 4");

        // 出错或未命中
        if (result.errorCode == -1 || result.errorCode == 0) {
            var html = "error:" + result.errorInfo;
            obj.html(html);
            return;
        }
        //alert("getDetail 5");

        var itemTables = "";
        if (result.itemList.length == 0) {
            itemTables = "<div class='mui-card item'>"
                + "<span class='remark'>没有册信息</span>"
                + "</div>"
        }

        for (var i = 0; i < result.itemList.length; i++) {
            var record = result.itemList[i];

            itemTables += "<div class='mui-card item' id='_item_" + record.barcode + "'>"
            + "<div class='title'>" + record.barcode + "</div>"
             + "<table>"
            + "<tr>"
            + "<td class='label'>状态</td>"
            + "<td class='value'>" + record.state + "</td>"
            + "</tr>"
            + "<tr>"
            + "<td class='label'>卷册</td>"
            + "<td class='value'>" + record.volumn + "</td>"
            + "</tr>"
            + "<tr>"
            + "<td class='label'>馆藏地</td>"
            + "<td class='value'>" + record.location + "</td>"
            + "</tr>"
            + "<tr>"
            + "<td class='label'>价格</td>"
            + "<td class='value'>" + record.price + "</td>"
            + "</tr>"
            + "<tr>"
            + "<td class='label'>在借情况</td>"
            + "<td class='value'>" + record.borrowInfo + "</td>"
            + "</tr>"
            + "<tr>"
            + "<td class='label'>预约信息</td>"
            + "<td class='value' >" + record.reservationInfo + "</td>"
            + "</tr>"
            //
            + "</table>"
            + "</div>";
        }


        var myHtml = result.info + itemTables;

        obj.html(myHtml);

        //alert(myHtml);

        //return myHtml;

    }, function (xhq, textStatus, errorThrown) {
        o.html("访问服务器出错：[" + errorThrown + "]");
        return;
    });

    //return "";
}

//预约
function reservation(obj, barcode, style) {
    //alert($("input[name='ckbBarcode']:checked").length);
    if (style == "delete") {
        var opeName = $(obj).text();
        var gnl = confirm("你确定对册[" + barcode + "]" + opeName + "吗?");
        if (gnl == false) {
            return false;
        }
    }

    var weixinId = $("#weixinId").text();
    if (weixinId == null || weixinId == "") {
        alert("weixinId参数为空");
        return;
    }

    var libId = getLibId();//$("#selLib").val();
    if (libId == "") {
        alert("尚未选择图书馆");
        return;
    }

    var patron = $("#patronBarcode").text();
    if (patron == "") {
        alert("您尚未绑定图书馆账户，请先绑定账户。");
        return;
    }

    if (barcode == "") {
        alert("您尚未选择要预约的册记录。");
        return;
    }

    var itemDivId = "#_item_" + barcode;
    var infoDiv = $(itemDivId).find(".resultInfo");

    //显示等待图层
    var index = loadLayer();

    var url = "/api/Reservation"
        + "?weixinId=" + weixinId
        + "&libId=" + encodeURIComponent(libId)
        + "&patron=" + encodeURIComponent(patron)
        + "&items=" + encodeURIComponent(barcode)
        + "&style=" + style;//new 创建一个预约请求,delete删除

    // alert(url);
    // 调api
    sendAjaxRequest(url, "POST", function (result) {


        // 关闭等待层
        layer.close(index);

        // 显示预约结果

        var info = result.errorInfo;

        $("input[name='ckbBarcode']").removeAttr("checked");//取消全选

        // 出错
        if (result.errorCode == -1) {
            $(infoDiv).text(info);
            $(infoDiv).css("color", "red");  //设为红色

            alert(result.errorInfo);
            return;
        }

        var bWarn = true;
        if (info == "") {
            info = $(obj).html() + " 操作成功。";
            bWarn = false;
        }
        else {
            info = $(obj).html() + " 操作成功。<br>" + info;
        }

        alert(info.replace("<br>", "\r\n"));

        if (bWarn == true)
            $(infoDiv).css("background-color", "yellow");  //设为绿色
        else
            $(infoDiv).css("color", "darkgreen");  //设为绿色
        $(infoDiv).html(info);

        //reserRow
        var reserRow = $(itemDivId).find(".reserRow");
        //reserRow.html("<td>a</td><td>b</td>");
        $(reserRow).prop('outerHTML', result.reserRowHtml);

        // 更新当前册的预约显示 todo,传一个item id，从服务器得到html

        //var div = $(obj).parent().parent();
        //getReservations(barcode, div, info, bWarn);


    }, function (xhq, textStatus, errorThrown) {

        // 关闭等待层
        layer.close(index);

        // 显示预约结果
        var info = "访问服务器出错：[" + errorThrown + "]";
        alert(info);

        $(infoDiv).text(info);
        $(infoDiv).css("color", "red");  //设为红色
    });
}

// 续借
function renew(itemBarcode) {
    if (itemBarcode == "") {
        alert("尚未传入册条码号。");
        return;
    }

    var libId = getLibId(); //$("#libId").val();
    if (libId == "") {
        alert("尚未选择图书馆");
        return;
    }


    var patronBarcode = $("#patronBarcode").text();
    if (patronBarcode == "") {
        alert("您尚未绑定图书馆读者账户，请先绑定账户。");
        return;
    }

    //显示等待图层
    var index = loadLayer();

    var url = "/api/BorrowInfo?libId=" + encodeURIComponent(libId)
        + "&action=renew"
        + "&patron=" + encodeURIComponent(patronBarcode)
        + "&item=" + encodeURIComponent(itemBarcode)
    // 调api
    sendAjaxRequest(url, "POST", function (result) {

        // 关闭等待层
        layer.close(index);

        // 显示续借结果
        var divId = "#renewInfo-" + itemBarcode;
        var infoDiv = $(divId);
        var info = result.errorInfo;


        // 出错
        if (result.errorCode == -1) {
            $(infoDiv).text(info);
            $(infoDiv).css("color", "red");  //设为红色

            alert(result.errorInfo);
            return;
        }


        if (info == "")
            info = "续借成功";

        $(infoDiv).text(info);
        $(infoDiv).css("color", "darkgreen");  //设为绿色

        // 续借按钮还一直存在吗？todo


    }, function (xhq, textStatus, errorThrown) {

        // 关闭等待层
        layer.close(index);

        // 显示预约结果
        var info = "访问服务器出错：[" + errorThrown + "]";
        alert(info);

        var divId = "#renewInfo-" + itemBarcode;
        var infoDiv = $(divId);

        $(infoDiv).text(info);
        $(infoDiv).css("color", "red");  //设为红色
    });
}

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
    var keyword = mylable.text();//.trim();
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

        // 调GetBiblioSummary api
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

        // 调GetBiblioSummary api
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

        //alert("["+myvalue+"]");
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
            //alert(msgHtml);

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

function getMsgViewHtml(msgItem, bContainEditDiv) {
    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }

    var bShowTime = false;
    if (group == "gn:_lib_bb") {
        bShowTime = true;
    }

    var bContainSubject = false;
    if (group == "gn:_lib_homePage") {
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

        if (bContainSubject == false) {

            // 加到最上面
            $("#_subject_main").prepend(msgViewHtml);
        }
        else {

            //alert("序号=" + msgItem.subjectIndex);


            var myDiv = null;
            if (msgItem.subjectIndex >= 0)
                myDiv = $("#_subject_main").children(".subject:eq(" + msgItem.subjectIndex + ")");

            //alert(myDiv);
            if (myDiv.html() != null) {
                var myId = myDiv.attr('id');
                myId = myId.substring(9);
                //alert(myId);

                // 如果subject相同，则加入item，如果不同，则要把subject插在之前
                if (myId == msgItem.subject) {
                    //alert("相同");

                    var titleObj = $(myDiv).find("#_subject_title");
                    $(msgViewHtml).insertAfter(titleObj);
                }
                else {
                    //alert("不同");

                    // 置空
                    model.subjectHtml("");

                    var subjectDiv = "<div id='_subject_" + msgItem.subject + "'  class='subject'>"
                        + "<div id='_subject_title' class='firstline'><span class='title'>" + msgItem.subjectPureName + "<span></div>"
                        + msgViewHtml
                        + "</div>";
                    $(subjectDiv).insertBefore(myDiv);
                }
            }
            else {

                //alert("未找到");
                // 置空
                model.subjectHtml("");
                var subjectDiv = "<div id='_subject_" + msgItem.subject + "'  class='subject'>"
                    + "<div id='_subject_title' class='firstline'><span class='title'>" + msgItem.subjectPureName + "<span></div>"
                    + msgViewHtml
                    + "</div>";
                //alert(subjectDiv);
                $("#_subject_main").append(subjectDiv);//插在后面
            }

        }

        //创建按钮可见
        $("#btnCreate").css('display', 'block');
        $(divId).css('display', 'none');
        $(divId).html("");

        return;
    }

    if (group == "gn:_lib_homePage") {
        // 编辑时更新了栏目，要重刷界面
        var parentId = $(divId).parent().attr('id');
        var thisSubject = "_subject_" + msgItem.subject;
        if (parentId != thisSubject) {
            //alert("栏目不同-" + msgItem.subject + "-old:" + parentId);
            window.location.reload();
            return;
        }
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

    var bContainRemark = true;
    if (group == "gn:_lib_bb" || group == "gn:_lib_homePage")
        bContainRemark = false;

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





    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId

    // subject
    var subject = "";
    if (bContainSubject == true) {
        subject = $(divId).find("#_val_subject").val();//$("#_val_subject").val(); //
        if (subject == "") {
            alert("请先选择栏目");
            return;
        }
    }
    //alert(subject);


    var action = "";
    var parameters = "";
    if (msgId == "new") {
        action = "POST";
        if (bContainSubject == true)
            parameters = "checkSubjectIndex,";
    }
    else {
        action = "PUT";
    }


    var title = $(divId).find("#_val_title").val();//$("#_val_title").val();
    // 对于图书馆主页，标题允许为空，因为已经有了栏目标题
    if (titleCanEmpty == false) {
        if (title == "") {
            alert("请输入标题。");
            return;
        }
    }

    var content = $(divId).find("#_val_content").val();//$("#_val_content").val();
    if (content == "") {
        alert("请输入内容。");
        return;
    }

    // 备注
    var remark = "";
    if (bContainRemark == true) {
        remark = $(divId).find("#_val_remark").val();//$("#_val_remark").val();
    }

    // 格式 text/markdown
    var format = $(divId).find("#_selFormat").val();//$("#_selFormat").val();
    //alert(format);


    //var group = "gn:_lib_bb";

    //显示等待图层
    var index = loadLayer();

    var id = "";
    if (msgId != "new")
        id = msgId;

    var weixinId = $("#weixinId").text();
    if (weixinId == "") {
        alert("异常情况：weixinId为空");
        return;
    }

    var url = "/api/LibMessage?weixinId="+weixinId
        + "&group=" + group
        + "&libId=" + libId
        + "&parameters=" + parameters;
    //alert(parameters);
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

// 删除msg
function deleteMsg(msgId) {
    //alert(msgId);

    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }

    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId
    var title = $(divId).find(".title").html();
    //alert(title);

    var confirmInfo = "你确定要删除该项吗?";
    if (title != null && title != "") {
        confirmInfo = "你确定要删除[" + title + "]吗?";
    }

    var gnl = confirm(confirmInfo);
    if (gnl == false) {
        return false;
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

    //显示等待图层
    var index = loadLayer();

    var url = "/api/LibMessage?libId=" + libId
        + "&group=" + encodeURIComponent(group)
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

        // 删除界面

        // 找到父亲
        var subjectDiv = $(divId).parent();
        // 删除自己;
        $(divId).remove();

        if (group == "gn:_lib_homePage") {
            // 如果父亲下级没有message，父亲也删除
            if ($(subjectDiv).children(".message").length == 0) {

                // 移除栏目div
                subjectDiv.remove();

                // 置空subject,再打开编辑界面时，会重刷subject列表
                model.subjectHtml("");
            }
        }

    }, function (xhq, textStatus, errorThrown) {
        alert(errorThrown);
    });

}

// 获取编辑态html
function getMsgEditHtml(msgItem) {

    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }
    var bContainSubject = false;
    if (group == "gn:_lib_homePage") {
        bContainSubject = true;
    }

    var bContainRemark = true;
    if (group == "gn:_lib_bb" || group == "gn:_lib_homePage")
        bContainRemark = false;

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
        //alert(subject);
        disabledStr = " disabled='disabled' ";

        if (msgItem.contentFormat == "markdown")
            formatMarkdownStr = " selected ";

        saveBtnName = "保存";
    }



    //alert("getMsgEditHtml 1");

    var html = "<table class='edit'>"

    // 加选择栏目行
    if (bContainSubject == true) {
        var subjectHtml = model.subjectHtml();
        //alert("2==" + subjectHtml);
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
    + "</tr>";

    if (bContainRemark == true) {
        html += "<tr>"
            + "<td colspan='2' >"
                + "<span class='label'>注释</span>"
                + "<textarea id='_val_remark' rows='2'>" + remark + "</textarea>"
            + "</td>"
        + "</tr>";
    }

    html += "<tr>"
        + "<td colspan='2'>"
            + "<button class='mui-btn mui-btn-primary' onclick=\"save('" + msgId + "')\">" + saveBtnName + "</button>&nbsp;&nbsp;"
            + "<button class='mui-btn mui-btn-default' onclick=\"cancelEdit('" + msgId + "')\">取消</button>"
        + "</td>"
    + "</tr>"
 + "</table>";
    return html;
}

//用于获取栏目
function getSubjectHtml(msgId) {
    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }

    var libId = getLibId();
    if (libId == "") {
        alert("异常情况：libId为空");
        return;
    }
    var weixinId = $("#weixinId").text();
    if (weixinId == "") {
        alert("异常情况：weixinId为空");
        return;
    }

    // 置空
    model.subjectHtml("");

    //显示等待图层
    var index = loadLayer();
    // 调web api
    var url = "/api/LibMessage?weixinId=" + weixinId
        + "&group=" + encodeURIComponent(group)
        + "&libId=" + libId
    + "&selSubject=" //msgid-"+msgId
    + "&param=html";
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        layer.close(index);

        if (result.errorCode == -1) {
            alert(result.errorInfo);
            return;
        }

        //设到内存里
        var subjectHtml = result.html;
        model.subjectHtml(result.html);

        //alert(subjectHtml);

        // 继续进入编辑态
        gotoEdit(msgId);

    },
    function (xhq, textStatus, errorThrown) {

        alert("访问服务器出错：\r\n" + errorThrown);
        // 关闭等待层
        layer.close(index);
    }); // 同步调用
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

    //alert("cancelEdit() 1");
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

    //alert("cancelEdit() 2");

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

    $("#divNo").css('display', 'none');
    //alert($("#divNo"));

    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }

    if (group == "gn:_lib_homePage") {
        if (model.subjectHtml() == "") {
            //alert("subjectHtml为空，需要从服务器获取。");
            getSubjectHtml(msgId);
            return;
        }
    }

    if (msgId == null || msgId == "") {
        alert("未传入msgId");
        return;
    }

    // 2016-8-13 任延华加
    // 关闭其它正在编辑的msg
    var editDiv = $("#_subject_main").find(".edit").each(function (index) {
        //alert(index);//循环的下标值，从0开始
        var myMsgId = "";
        var editId = $(this).parent().attr("id");
        if (editId != null && editId.length > 6 && editId.substring(0, 6) == "_edit_")
        {
            myMsgId = editId.substring(6);
        }        
        //alert(editId + "***" + myMsgId);

        // 关闭编辑区
        cancelEdit(myMsgId);
    });




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
    // alert("gotoEdit 1");
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
            //alert(item);

            var html = getMsgEditHtml(item);
            $(divId).html(html);

            //alert(html);
            //alert("gotoEdit 5");


            //由于一进来没有显示编辑界面，所以这里要重新设一下
            setShowTopButton();

            // 设置checkbox的选中项
            var subject1 = item.subject;
            if (subject1 != null && subject1 != "") {
                //alert(subject1);
                $(divId).find("#selSubject").val(subject1);
                //$("select[@name=ISHIPTYPE] option").each(function () {
                //    if ($(this).val() == subject1) {
                //       // $(this).remove();
                //    }
                //});
            }


            //alert("gotoEdit 6");

        }

    }, function (xhq, textStatus, errorThrown) {

        alert("访问服务器出错：\r\n" + errorThrown);
        // 关闭等待层
        layer.close(index);
    });
}


// 栏目切换，将选择的subject设到输入框中
function subjectChanged(bGetTemplate, obj) {

    var subValue = $(obj).val();//$("#selSubject").val();
    //alert(subValue);

    var topDiv = $(obj).parent().parent();
    //alert(topDiv.html());

    if (subValue == "new") {
        $(topDiv).find("#divNewSubject").css('display', 'block');
        $(topDiv).find("#_val_subject").val("");
    }
    else {
        $(topDiv).find("#divNewSubject").css('display', 'none');
        $(topDiv).find("#_val_subject").val(subValue);

        if (bGetTemplate == true) {
            //alert("get template");
            // 取模板
            getTemplate(subValue);
        }
    }
}

//用于获取栏目
function getTemplate(subject) {
    var group = $("#_group").text();
    if (group == null || group == "" ||
        (group != "gn:_lib_bb" && group != "gn:_lib_homePage")) {
        alert("异常情况：group参数值不正确[" + group + "]。");
        return;
    }

    var libId = getLibId(); //$("#selLib").val();
    if (libId == "") {
        return;
    }

    //显示等待图层
    var index = loadLayer();
    // 调web api
    var url = "/api/LibMessage?group=" + encodeURIComponent(group)
        + "&libId=" + libId
    + "&subject=" + encodeURIComponent(subject);
    //alert(url);
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        layer.close(index);

        if (result.errorCode == -1) {
            alert(result.errorInfo);
            return;
        }

        $("#_val_content").val(result.info);

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


