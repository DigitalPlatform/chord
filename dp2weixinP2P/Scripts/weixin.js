function showLoading()
{
    //$("#loading").css("top", "50%");
    //$("#loading").css("left", "50%");

    //var totalHeight = $(document).height();
    //$("#loading").css("top", totalHeight/2);


    //var s = "";
    //s += "\r\n文档高：" + $(document).height();
    ////s += "\r\n网页可见区域宽：" + document.body.clientWidth;
    //s += "\r\n网页可见区域高clientHeight：" + document.body.clientHeight;
    ////s += "\r\n网页可见区域宽：" + document.body.offsetWidth + " (包括边线的宽)";
    //s += "\r\n网页可见区域高offsetTop：" + document.body.offsetTop + " (包括边线的宽)";
    ////s += "\r\n网页正文全文宽：" + document.body.scrollWidth;
    //s += "\r\n网页正文全文高scrollHeight：" + document.body.scrollHeight;
    //s += "\r\n网页被卷去的高scrollTop：" + document.body.scrollTop;
    ////s += "\r\n网页被卷去的左：" + document.body.scrollLeft;
    //s += "\r\n网页正文部分上screenTop：" + window.screenTop;
    ////s += "\r\n网页正文部分左：" + window.screenLeft;
    //s += "\r\n屏幕分辨率的高：" + window.screen.height;
    ////s += "\r\n屏幕分辨率的宽：" + window.screen.width;
    //s += "\r\nscrollTop：" + $(window).scrollTop();;
    //s += "\r\n屏幕可用工作区高度availHeight：" + window.screen.availHeight;
    //alert(s);

    var height = window.screen.availHeight;
    var scrollTop = $(window).scrollTop();
    var mytop = scrollTop + height / 2;
    $("#loading").css("top", mytop);

    $("#loading").show();//显示loading


    //alert($("#loading"));
    //alert("123");
}

function hideLoading() {
    $("#loading").hide();//关闭loading
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

function setImgSize(obj)
{
    var parentObj = $(obj).parent();
    if (parentObj != null) {

        var width = $(parentObj).width()-15;

        $(obj).css("max-width", width+"px");

        //alert(width);
    }
    else {
        //alert("计算size时，发现父亲对象为null");
    }
}

//function resizeImg() {
//    var count = 0;
//    $(".autoimg").each(function () {
        
//        alert($(this).)
//    });
//    return count
//}

//===册登记相关=======
// 新增册
function gotoSetItem(biblioPath, biblioName) {
    //alert("biblioPath=" + biblioPath);

    var url = "/Biblio/Detail?action=new"
        + "&biblioPath=" + encodeURIComponent(biblioPath)
        + "&biblioName=" + encodeURIComponent(biblioName);
    gotoUrl(url);
}

// 2020-10-10 先把编辑的功能隐掉，抽空再做
// 编辑册
function gotoEditItem(biblioPath, biblioName) {
    //alert("biblioPath=" + biblioPath);

    var url = "/Biblio/Detail?action=edit"
        + "&biblioPath=" + encodeURIComponent(biblioPath)
        + "&biblioName=" + encodeURIComponent(biblioName);
    gotoUrl(url);
}

// 删除册
function deleteItem(worker,libId,biblioPath,itemPath,barcode) {

    var gnl = confirm("您确认要删除册[" + barcode + "]吗?");
    if (gnl == false) {
        return false;
    }

    // 调接口 todo
    if (libId == null || libId == "") {
        alert("libId参数不能为空");
        return;
    }

    //显示等待图层
    showMaskLayer();

    // 调 SetItem api
    var url = "/api2/BiblioApi/SetItem?loginUserName=" + encodeURIComponent(worker)
        + "&libId=" + libId
        + "&biblioPath=" + encodeURIComponent(biblioPath)
        + "&action=delete"
    //alert(url);
    sendAjaxRequest(url, "POST",
        function (result) {

            // 关闭等待层
            hideMaskLayer();

            //alert("ok");

            if (result.errorCode == -1) {
                alert("删除出错:" + result.errorInfo);
                return;
            }

            var itemName = "#_item_" + barcode;
            $(itemName).remove();// 删除自己;

            //alert("删除成功。");
        },
        function (xhq, textStatus, errorThrown) {
            // 关闭等待层
            hideMaskLayer();
            alert(errorThrown);
        },
        {
            recPath: itemPath
        }
    );
}


//======书目详细信息==========

// 获取详细书目记录
// from表示是从哪个界面过来的？
function getDetail(libId, recPath, obj, from,biblioName) {

    //alert(biblioName);

    //alert("getDetail 1");
    if (libId == null || libId == "") {
        alert("libId参数不能为空");
        return;
    }

    if (recPath == null || recPath == "") {
        recPath("recPath参数不能为空");
        return;
    }

    var weixinId = $("#weixinId").text();
    if (weixinId == null || weixinId == "") {
        alert("weixinId参数为空");
        return;
    }

    //alert("getDetail 2");

    // 登录帐户和类型
    var loginUserName = getLoginUserName();
    var loginUserType = getLoginUserType();
    //alert(loginUserName+"-"+loginUserType);

    // 2021/8/2 改为从前端传登录帐号
    // 调GetBiblioDetail  api
    var url = "/api2/biblioApi/GetBiblioDetail?loginUserName=" + encodeURIComponent(loginUserName)
        + "&loginUserType=" + encodeURIComponent(loginUserType)
        + "&weixinId=" + encodeURIComponent(weixinId)
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

        // 工作人员帐号
        var worker = $("#_worker").text();


        //alert("getDetail 5");


        // 循环显示每一册
        for (var i = 0; i < result.itemList.length; i++) {
            var record = result.itemList[i];

            //alert("getDetail 6。1");

            // 2022/10/21 采用统一的函数
            itemTables += getItemHtml(libId,record,true); //只有书目详情这里才显示删除册。册检索的命中列表不显示删除册。

            //alert("getDetail 6");

            //var titleClass = "title";

            ////alert("disable="+record.disable);

            //var addStyle = "";  /*删除线*/
            //if (record.disable ==true)
            //{
            //    addStyle = "style='color:#cccccc;text-decoration:line-through;'";  /*发灰，删除线*/
            //}

            //if (record.isGray == true) {
            //    addStyle = "style='color:#cccccc;'";  /*发灰，删除线*/
            //}


            //var tempBarcode = record.barcode;
            //if (tempBarcode.indexOf("@refID:") != -1)
            //{
            //    //alert(tempBarcode+"前");
            //    tempBarcode = tempBarcode.replace("@refID:", "refID-");
            //    //alert(tempBarcode + "后");

            //    titleClass = "titleGray";
            //}

            //itemTables += "<div class='mui-card item' id='_item_" + tempBarcode + "'>"
            ////+ "<div class='"+titleClass+"'>" + record.barcode + "</div>"
            // + "<table>"
            //+ "<tr>";

            //// 有图片才显示
            //if (record.imgHtml != null && record.imgHtml != "") {
            //    itemTables += "<td class='label'></td>"
            //    + "<td class='value'>" + record.imgHtml + "</td>"
            //    + "</tr>";
            //}

            //// 册条码
            //    itemTables += "<tr>"
            //    + "<td class='label'>册条码</td>"
            //    + "<td class='value' " + addStyle + ">" + record.pureBarcode + "</td>"  //record.barcode
            //    + "</tr>";

            //if (record.state != null && record.state != "") {
            //    itemTables += "<tr>"
            //    + "<td class='label'>状态</td>"
            //    + "<td class='value'  " + addStyle + ">" + record.state + "</td>"
            //    + "</tr>";
            //}

            //if (record.volume != null && record.volume != "") {
            //    itemTables += "<tr>"
            //    + "<td class='label'>卷册</td>"
            //    + "<td class='value' " + addStyle + ">" + record.volume + "</td>"
            //    + "</tr>";
            //}

            //var locationStyle = "title";
            //if (record.currentLocation != null && record.currentLocation != "") {
            //    locationStyle = "value";
            //}

            //itemTables += "<tr>"
            //    + "<td class='label'>馆藏地</td>"
            //    + "<td class='" + locationStyle + "' " + addStyle + ">" + record.location + "</td>"
            //    + "</tr>";

            //// 2021/4/6 增加架号
            //if (record.shelfNo != null && record.shelfNo != "") {
            //    itemTables += "<tr>"
            //        + "<td class='label'>架号</td>"
            //        + "<td class='value' " + addStyle + ">" + record.shelfNo + "</td>"
            //        + "</tr>";
            //}

            //itemTables +=  "<tr>"
            //+ "<td class='label'>当前位置</td>"
            //+ "<td class='title' " + addStyle + ">" + record.currentLocation + "</td>"
            //+ "</tr>"
            //+ "<tr>"
            //+ "<td class='label'>索取号</td>"
            //+ "<td class='title' " + addStyle + ">" + record.accessNo + "</td>"
            //+ "</tr>"
            //+ "<tr>"
            //+ "<td class='label'>价格</td>"
            //+ "<td class='value' " + addStyle + ">" + record.price + "</td>"
            //+ "</tr>";


            
            //// 成员册 不显示在借情况
            //if (record.borrowInfo != null && record.borrowInfo != "") {
            //    itemTables += "<tr>"
            //    + "<td class='label'>在借情况</td>"
            //    + "<td class='value' " + addStyle + ">" + record.borrowInfo + "</td>"
            //    + "</tr>";
            //}

            //// 成员册 不显示预约信息
            //if (record.reservationInfo != null && record.reservationInfo != "") {
            //    itemTables += "<tr>"
            //    + "<td class='label'>预约信息</td>"
            //    + "<td class='value' " + addStyle + ">" + record.reservationInfo + "</td>"
            //    + "</tr>";
            //}

            //itemTables += "<tr>"
            //+ "<td class='label'>参考ID</td>"
            //+ "<td class='titleGray' " + addStyle + ">" + record.refID + "</td>"
            //+ "</tr>";


            ////从属于，
            //if (record.parentInfo != null && record.parentInfo != "") {
            //    itemTables += "<tr>"
            //        + "<td class='label'>从属于</td>"
            //        + "<td class='value' " + addStyle + ">" + record.parentInfo + "</td>"
            //        + "</tr>";

            //    //当一个期刊册被做了合订的册，则不允许再编辑和删除
            //}
            //else {

            //    // 如果当前是工作人员帐户，则显示编辑和删除按钮

            //    if (worker != null && worker != "") {

            //        itemTables += "<tr>"
            //            + "<td class='label' colspan='2'>"
            //            //+ "<button  class='mui-btn' onclick='gotoEditItem(\"" + recPath + "\",\"" + biblioName + "\")'>编辑</button>"
            //            + "<button  class='mui-btn' onclick='deleteItem(\"" + worker + "\",\"" + libId + "\",\"" + recPath + "\",\"" + record.recPath + "\",\"" + tempBarcode + "\")'>删除册</button>"
            //            +"</td > "
            //            + "</tr>";
            //    }
            //}

            ////
            //itemTables += "</table>"
            //+ "</div>";

            //alert("2");
        }



        var myHtml = result.info;//20220511把新增册调到前面， + itemTables;

        // 2022/5/11 把新增册的按钮提前到书目下方
        // 检查要不要出现册登记按钮
        if (worker != null && worker != "") {

            // 是否已经是详细界面，则不出现新增册按钮
            var _isDetail = $("#_isDetail").text();
            if (_isDetail != "1") {
                myHtml += "<div style='padding-top:10px'><button  class='mui-btn' onclick='gotoSetItem(\"" + recPath + "\",\"" + biblioName + "\")'>新增册</button></div>";
            }
        }

        //alert("3");
        // 册信息
        myHtml += itemTables

        obj.html(myHtml);

        //alert(myHtml);

        //return myHtml;

    }, function (xhq, textStatus, errorThrown) {
        o.html("访问服务器出错：[" + errorThrown + "]");
        return;
    });

    //return "";
}

// 拼册的html
function getItemHtml(libId,record,showDeleteButton) {

    //alert("3");

    var itemTables = "";

    // 工作人员帐号
    var worker = $("#_worker").text();
    
    var titleClass = "title";

    // 附件样式
    var addStyle = "";

    //发灰，删除线
    if (record.disable == true) {
        addStyle = "style='color:#cccccc;text-decoration:line-through;'"; 
    }

    // 发灰
    if (record.isGray == true) {
        addStyle = "style='color:#cccccc;'";  
    }

    // 没有册条码，使用参考ID的情况，反来没用到这种情况
    var tempBarcode = record.barcode;
    if (tempBarcode.indexOf("@refID:") != -1) {
        //alert(tempBarcode+"前");
        tempBarcode = tempBarcode.replace("@refID:", "refID-");
        //alert(tempBarcode + "后");
        titleClass = "titleGray";
    }

    // 表格头
     itemTables += "<div class='mui-card item' id='_item_" + tempBarcode + "'>"
        //+ "<div class='"+titleClass+"'>" + record.barcode + "</div>"
        + "<table>"
        + "<tr>";


    // 有图片才显示
    if (record.imgHtml != null && record.imgHtml != "") {
        itemTables += "<td class='label'></td>"
            + "<td class='value'>" + record.imgHtml + "</td>"
            + "</tr>";
    }

    // 册条码
    itemTables += "<tr>"
        + "<td class='label'>册条码</td>"
        + "<td class='value' " + addStyle + ">" + record.pureBarcode + "</td>"  //record.barcode
        + "</tr>";

    // 状态
    if (record.state != null && record.state != "") {
        itemTables += "<tr>"
            + "<td class='label'>状态</td>"
            + "<td class='value'  " + addStyle + ">" + record.state + "</td>"
            + "</tr>";
    }

    // 卷册
    if (record.volume != null && record.volume != "") {
        itemTables += "<tr>"
            + "<td class='label'>卷册</td>"
            + "<td class='value' " + addStyle + ">" + record.volume + "</td>"
            + "</tr>";
    }

    // 馆藏地
    var locationStyle = "title";
    if (record.currentLocation != null && record.currentLocation != "") {
        locationStyle = "value";
    }
    itemTables += "<tr>"
        + "<td class='label'>馆藏地</td>"
        + "<td class='" + locationStyle + "' " + addStyle + ">" + record.location + "</td>"
        + "</tr>";

    // 2021/4/6 增加架号
    if (record.shelfNo != null && record.shelfNo != "") {
        itemTables += "<tr>"
            + "<td class='label'>架号</td>"
            + "<td class='value' " + addStyle + ">" + record.shelfNo + "</td>"
            + "</tr>";
    }

    // 当前位置、索取号、价格
    itemTables += "<tr>"
        + "<td class='label'>当前位置</td>"
        + "<td class='title' " + addStyle + ">" + record.currentLocation + "</td>"
        + "</tr>"
        + "<tr>"
        + "<td class='label'>索取号</td>"
        + "<td class='title' " + addStyle + ">" + record.accessNo + "</td>"
        + "</tr>"
        + "<tr>"
        + "<td class='label'>价格</td>"
        + "<td class='value' " + addStyle + ">" + record.price + "</td>"
        + "</tr>";



    // 成员册 不显示在借情况
    if (record.borrowInfo != null && record.borrowInfo != "") {
        itemTables += "<tr>"
            + "<td class='label'>在借情况</td>"
            + "<td class='value' " + addStyle + ">" + record.borrowInfo + "</td>"
            + "</tr>";
    }

    // 成员册 不显示预约信息
    if (record.reservationInfo != null && record.reservationInfo != "") {
        itemTables += "<tr>"
            + "<td class='label'>预约信息</td>"
            + "<td class='value' " + addStyle + ">" + record.reservationInfo + "</td>"
            + "</tr>";
    }




    //从属于，
    if (record.parentInfo != null && record.parentInfo != "") {
        itemTables += "<tr>"
            + "<td class='label'>从属于</td>"
            + "<td class='value' " + addStyle + ">" + record.parentInfo + "</td>"
            + "</tr>";

        //当一个期刊册被做了合订的册，则不允许再编辑和删除
    }

    itemTables += "<tr>"
        + "<td class='label'>参考ID</td>"
        + "<td class='titleGray' " + addStyle + ">" + record.refID + "</td>"
        + "</tr>";

    itemTables += "<tr>"
        + "<td class='label'>册路径</td>"
        + "<td class='titleGray' " + addStyle + ">" + record.recPath + "</td>"
        + "</tr>";

    // 如果当前是工作人员帐户，则显示编辑和删除按钮
    if (worker != null && worker != "" && showDeleteButton==true) {

        itemTables += "<tr>"
            + "<td class='label' colspan='2'>"
            //+ "<button  class='mui-btn' onclick='gotoEditItem(\"" + recPath + "\",\"" + biblioName + "\")'>编辑</button>"
            + "<button  class='mui-btn' onclick='deleteItem(\"" + worker + "\",\"" + libId + "\",\"" + record.biblioPath + "\",\"" + record.recPath + "\",\"" + tempBarcode + "\")'>删除册</button>"
            + "</td > "
            + "</tr>";
    }

    // table收尾
    itemTables += "</table>"
        + "</div>";

    return itemTables;
    
}

//删除书目
function deleteBiblio(biblioPath, biblioTimestamp) {

    var gnl = confirm("您确认要删除书目[" + biblioPath + "]吗?");
    if (gnl == false) {
        return false;
    }

    //alert("1");
    // 图书馆
    var libId = getLibId();//$("#selLib").val();
    if (libId == "" || libId == null) {
        alert("您尚未选择图书馆。");
        return;
    }

    var weixinId = $("#weixinId").text();
    if (weixinId == null || weixinId == "") {
        alert("weixinId参数为空");
        return;
    }

    // 时间戳不能为空
    if (biblioTimestamp == null || biblioTimestamp == "") {
        alert("删除书目时timestamp参数不能为空");
        return;
    }

    // biblio path不能为空
    if (biblioPath == null || biblioPath == "") {
        alert("书目路径不能为空");
        return;
    }

    // 登录帐户和类型
    var loginUserName = getLoginUserName();
    var loginUserType = getLoginUserType();

    // web api
    var url = "/api2/BiblioApi/SetBiblio?loginUserName=" + encodeURIComponent(loginUserName)
        + "&loginUserType=" + loginUserType
        + "&weixinId=" + weixinId
        + "&libId=" + libId

    //alert(url);
    sendAjaxRequest(url, "POST",
        function (result) {
            // 关闭等待层
            hideMaskLayer();


            if (result.errorCode == -1) {
                alert(result.errorInfo);
                return;
            }

            alert("删除书目成功。");

        },
        function (xhq, textStatus, errorThrown) {
            // 关闭等待层
            hideMaskLayer();
            alert(errorThrown);
        },
        {
            BiblioPath: biblioPath,
            Action: "delete",
            Fields: "",
            Timestamp: biblioTimestamp
        }
    );

}

//预约
function reservation(obj, barcode, style) {
    //alert("走到reservation()");


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


    //alert(barcode);
    var itemDivId = "#_item_" + barcode;
    var infoDiv = $(itemDivId).find(".resultInfo");



    var paramBarcord = barcode;
    if (paramBarcord.indexOf("refID-") != -1) {
        paramBarcord = paramBarcord.replace("refID-", "@refID:");
        //alert(paramBarcord);
    }

    //if (style == "delete") {
    var opeName = $(obj).text();
    var gnl = confirm("您确定对册[" + paramBarcord + "]" + opeName + "吗?");
    if (gnl == false) {
        return false;
    }
    //}

    //显示等待图层
    //var index = loadLayer();
    showLoading();
    //showMaskLayer();
    

    var url = "/api2/CirculationApi/Reserve?weixinId=" + weixinId
        + "&libId=" + encodeURIComponent(libId)
        + "&patronBarcode=" + encodeURIComponent(patron)
        + "&itemBarcodes=" + encodeURIComponent(paramBarcord)
        + "&style=" + style;//new 创建一个预约请求,delete删除

     //alert(url);
    // 调api
    sendAjaxRequest(url, "POST", function (result) {


        // 关闭等待层
        //layer.close(index);
        hideLoading();
        //hideMaskLayer();

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
        //layer.close(index);
        hideLoading();
        //hideMaskLayer();

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

    var paramItemBarcord = itemBarcode;
    if (paramItemBarcord.indexOf("refID-") != -1) {
        paramItemBarcord = paramItemBarcord.replace("refID-", "@refID:");
        //alert(paramItemBarcord);
    }

    var gnl = confirm("您确认续借册[" + paramItemBarcord + "]吗?");
    if (gnl == false) {
        return false;
    }

    //显示等待图层
    //var index = loadLayer();
    showLoading();

    var url = "/api2/CirculationApi/Renew?weixinId=" //目前没用到weixinId，传空即可
        +"&libId = " + encodeURIComponent(libId)
        + "&patronBarcode=" + encodeURIComponent(patronBarcode)
        + "&itemBarcode=" + encodeURIComponent(paramItemBarcord)
    // 调api
    sendAjaxRequest(url, "POST", function (result) {

        // 关闭等待层
        //layer.close(index);
        hideLoading();

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
        alert(info);
        $(infoDiv).text(info);
        $(infoDiv).css("color", "darkgreen");  //设为绿色

        // 续借按钮还一直存在吗？todo


    }, function (xhq, textStatus, errorThrown) {

        // 关闭等待层
        //layer.close(index);
        hideLoading();

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

    // 登录帐户和类型
    var loginUserName = getLoginUserName();
    //alert(loginUserName);
    var loginUserType = getLoginUserType();
    //alert(loginUserType);

    if (mytype == "bs-") {
        //alert("bs-"+keyword);
        var libId = o.children("span").text();

        // 调GetBiblioSummary api
        var url = "/api2/biblioApi/GetBiblioSummary?loginUserName=" + encodeURIComponent(loginUserName)
            + "&loginUserType=" + encodeURIComponent(loginUserType)
            + "&id=" + encodeURIComponent(myvalue)
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
        var url = "/api2/biblioApi/GetBiblioSummary?loginUserName=" + encodeURIComponent(loginUserName)
            + "&loginUserType=" + encodeURIComponent(loginUserType)
            + "&id=" + encodeURIComponent(myvalue)
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

        //检查group
        var group = $("#_group").text();
        var error = checkGroud(group);
        if (error != "") {
            window.setTimeout("fillPending()", 1);
            return;
        }

        // 调web api获取消息
        var url = "/api2/LibMessageApi/GetMessage?weixinId=" //+ weixinId
                    + "&group=" + group //gn:_lib_homePage"
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


// 获得当前登录帐户
function getLoginUserName() {
    return $('#_loginUserName').text();
}

// 获得当前登录帐户
function getLoginUserType() {
    return $('#_loginUserType').text();
}

//============消息相关=============

function getMsgViewHtml(msgItem, bContainEditDiv) {
    var group = $("#_group").text();
    var error = checkGroud(group);
    if (error != "") {
        alert(error);
        return;
    }

    var bShowTime = false;
    if (group == "gn:_lib_bb") {
        bShowTime = true;
    }

    var bContainSubject = false;
    if (group == "gn:_lib_homePage" || group == "gn:_dp_home") {
        bContainSubject = true;
    }

    var html = "";
    //alert("aa");

    if (bContainEditDiv == true)
        html += "<div class='mui-card message' id='_edit_" + msgItem.id + "' onclick=\"clickMsgDiv('" + msgItem.id + "')\">";

    // 2016-8-20 如果markdown格式产生的pre/code元素放在表格里不支持，这里给显示态再套一层div，把内容和注释提到table外的div里
    html+="<div class='view'>"
    html += "<table class='view-top'>"
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
    html += "</table>";

    // 加内容
    html += "<div class='content'>"
                        + msgItem.contentHtml
                        + "</div>";

    // 收尾的div
    html += "</div>";

    if (bContainEditDiv == true)
        html += "</div>";

    return html;
}

function getSelectedMsgIds() {
    var ids="";
    $(".msgEditable").each(function () {
        if (ids != "")
            ids += ",";
        var id = $(this).attr('id');
        id = id.substring(6);
        ids += id;
    });
    return ids;
}

function getSelectedMsgCount() {
    var count = 0;
    $(".msgEditable").each(function () {
        count ++;
    });
    return count
}

// 删除msg
function deleteMsg(msgId) {
    //alert(msgId);

    // 检查下是否选中多个
    var mutiple = false;

    var ids = getSelectedMsgIds();
    if (ids.indexOf(",") != -1) {
        mutiple = true;
        msgId = ids;
        //alert(msgId);
    }

    var group = $("#_group").text();
    var error = checkGroud(group);
    if (error != "") {
        alert(error);
        return;
    }

    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId
    var confirmInfo = "";
    var delCount =getSelectedMsgCount();
    if (mutiple == false) {
        var title = $(divId).find(".title").html();
        var confirmInfo = "你确定要删除该项吗?";
        if (title != null && title != "") {
            confirmInfo = "你确认要删除[" + title + "]吗?";
        }
    }
    else
    {
        confirmInfo = "你确认要删除所选定的 "+delCount+" 个事项？";
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

    var weixinId = $("#weixinId").text();
    if (weixinId == "") {
        alert("异常情况：weixinId为空");
        return;
    }

    //显示等待图层
    //var index = loadLayer();
    showLoading();

    var url = "/api2/LibMessageApi/DeleteMsg?weixinId="+weixinId
        + "&libId=" +  encodeURIComponent(libId)
        + "&group=" + encodeURIComponent(group)
        + "&msgId=" + msgId
        + "&userName=" + userName
    sendAjaxRequest(url, "DELETE", function (result) {

        // 关闭等待层
        //layer.close(index);
        hideLoading();

        if (result.errorCode == -1) {
            alert("操作失败：" + result.errorInfo);
            return;
        }

        alert("删除成功");

        // 处理界面显示
        var subjectDiv = $(divId).parent();// 找到父亲
        

        if (mutiple == true) {

            var msgCount = $(subjectDiv).children(".message").length;
            //alert("msgcount=" + msgCount);
            if (msgCount == delCount && group == "gn:_lib_book")
            {
                var url = "/Library/BookSubject?libId=" + libId;
                gotoUrl(url);
            }
            else
            {
                //多项删除时，直接重新加载页面
                window.location.reload();
            }

            return;
        }
        else {

            $(divId).remove();// 删除自己;

            // 没有同级兄弟时
            if ($(subjectDiv).children(".message").length == 0) {

                if (group == "gn:_lib_homePage" || group == "gn:_dp_home") {
                    // 移除栏目div
                    subjectDiv.remove();
                    // 置空subject,再打开编辑界面时，会重刷subject列表
                    model.subjectHtml("");
                }
                else if (group == "gn:_lib_book") {
                    var url = "/Library/BookSubject?libId=" + libId;
                    gotoUrl(url);
                }
            }


        }

    }, function (xhq, textStatus, errorThrown) {

        // 关闭等待层
        //layer.close(index);
        hideLoading();

        alert(errorThrown);


    });

}

function checkGroud(group)
{
    if (group == null || group == "" ||
    (group != "gn:_lib_bb"
        && group != "gn:_lib_homePage"
        && group != "gn:_lib_book"
        && group != "gn:_dp_home"
        ))
    {
        return "异常情况：group参数值不正确[" + group + "]。";
    }

    return "";
}

// 保存完后，显示一条消息
function viewMsg(msgId, msgItem) {

    var group = $("#_group").text();
    var error = checkGroud(group);
    if (error != "")
    {
        alert(error);
        return;
    }

    var bContainSubject = false;
    var bShowTime = true;
    if (group == "gn:_lib_homePage" || group=="gn:_dp_home") {
        bContainSubject = true;
        bShowTime = false;
    }

    // book显示时是没有栏目的
    //if (group == "gn:_lib_book") {
    //    bContainSubject = true;
    //}

    var divId = "#_edit_" + msgId; // div的id命令规则为_edit_msgId


    // 新增的好书推荐是新栏目
    if (group == "gn:_lib_book") {
        // 编辑时更新了栏目，要重刷界面
        var pageSubject = $("#_subject").text();
        //if (pageSubject == null)
        //    alert("出现_subject为null的情况");

        if (msgItem.subject != pageSubject) {
            //alert("栏目不同,转到新栏目的页面-" + msgItem.subject + "-old subject:" + pageSubject);

            var libId = getLibId();//$("#selLib").val();
            var userName = $("#_userName").text();//model.userName();
            if (userName == null)
                userName = "";
            var url = "/Library/Book?libId=" + libId
                + "&userName=" + encodeURIComponent(userName)
                + "&subject=" + encodeURIComponent(msgItem.subject);
            gotoUrl(url);
            return;
        }
    }

    if (msgId == "new") {

        // 得到完整的div
        var msgViewHtml = getMsgViewHtml(msgItem, true);

        if (bContainSubject == false) {

            // 加到最上面
            $("#_subject_main").prepend(msgViewHtml);
        }
        else {


            //===============================
            //alert("序号=" + msgItem.subjectIndex);
            var myDiv = null;
            if (msgItem.subjectIndex >= 0)
                myDiv = $("#_subject_main").children(".subject:eq(" + msgItem.subjectIndex + ")");

            //alert(myDiv);
            if (myDiv!=null && myDiv.html() != null) {
                var myId = myDiv.attr('id');
                myId = myId.substring(9);
                //alert(myId);

                // 如果subject相同，则加入item，如果不同，则要把subject插在之前
                if (myId == msgItem.subject) {
                    //alert("相同");
                    if (group == "gn:_lib_homePage" || group == "gn:_dp_home") {
                        $(myDiv).append(msgViewHtml);//插在后面
                    }
                    else {
                        var titleObj = $(myDiv).find("#_subject_title");
                        $(msgViewHtml).insertAfter(titleObj);
                    }
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
            //=========================

        }

        //创建按钮可见
        $("#btnCreate").css('display', 'block');
        $(divId).css('display', 'none');
        $(divId).html("");

        return;
    }


    // 编辑


    if (group == "gn:_lib_homePage" || group == "gn:_dp_home") {
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
    var error = checkGroud(group);
    if (error != "") {
        alert(error);
        return;
    }

    var bContainSubject = false;
    var titleCanEmpty = false;
    if (group == "gn:_lib_homePage" || group == "gn:_dp_home" || group == "gn:_lib_book") {
        bContainSubject = true;
        titleCanEmpty = true;
    }

    var bContainRemark = true;
    if (group == "gn:_lib_bb" || group == "gn:_lib_homePage" || group == "gn:_dp_home")
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

        if (subject.indexOf("(") != -1
            || subject.indexOf(")") != -1
            || subject.indexOf(",") != -1
            || subject.indexOf(" ") != -1
            || subject.indexOf("[") != -1
            || subject.indexOf("]") != -1)
        {
            alert("栏目名称不支持特殊字符()[],空格");
            return;
        }
    }
    //alert(subject);

    var url = "";///api2/LibMessageApi";
    var action = "POST"; //2022/09/13 new和change都改为post
    var parameters = "";
    if (msgId == "new") {
        url = "/api2/LibMessageApi/CreateMsg";
        //action = "POST";
        if (group == "gn:_lib_homePage" || group == "gn:_dp_home") {
            parameters = "checkSubjectIndex,";
        }
    }
    else {
       // action = "PUT";
        url = "/api2/LibMessageApi/ChangeMsg";
    }


    var title = $(divId).find("#_val_title").val();//$("#_val_title").val();
    // 对于图书馆介绍，标题允许为空，因为已经有了栏目标题
    if (titleCanEmpty == false) {
        if (title == "") {
            alert("请输入标题。");
            return;
        }
    }

    if (title != "" && title != null) {
        if (title.indexOf("(") != -1
            || title.indexOf(")") != -1
            || title.indexOf(",") != -1
            || title.indexOf(" ") != -1
            || title.indexOf("[") != -1
            || title.indexOf("]") != -1) {
            alert("标题不支持特殊字符()[],空格");
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
    //var index = loadLayer();
    showMaskLayer();

    var id = "";
    if (msgId != "new")
        id = msgId;

    var weixinId = $("#weixinId").text();
    if (weixinId == "") {
        alert("异常情况：weixinId为空");
        return;
    }

    var url = url+ "?weixinId=" + weixinId
        + "&group=" + group
        + "&libId=" + libId
        + "&parameters=" + parameters;
    //alert(url);
    sendAjaxRequest(url, action,
        function (result) {

            // 关闭等待层
            //layer.close(index);
            hideMaskLayer();

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

            if (group == "gn:_lib_book") {
                //加载书目summary
                window.setTimeout("fillPending()", 1);
            }


        },
        function (xhq, textStatus, errorThrown) {
            // 关闭等待层
            //layer.close(index);
            hideMaskLayer();

            alert(errorThrown);
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
    var error = checkGroud(group);
    if (error != "") {
        alert(error);
        return;
    }

    var subject = "";//model.selSubject();

    var bContainSubject = false;
    if (group == "gn:_lib_homePage" || group == "gn:_dp_home" || group == "gn:_lib_book") {
        bContainSubject = true;

        // 当前subject
        var subject = $("#_subject").text();
        if (subject == null)
            subject = "";
    }

    var bContainRemark = true;
    if (group == "gn:_lib_bb" || group == "gn:_lib_homePage" || group == "gn:_dp_home")
        bContainRemark = false;

    var formatTextStr = " selected ";// 默认文本格式选中
    var formatMarkdownStr = "";

    var saveBtnName = "新增";
    var disabledStr = "";// "disabled"
    var msgId = "new"; //默认新建的情况
    var title = "";
    var remark = "";
    var content = "";
    if (group == "gn:_lib_book")
    {
        content = $("#_content").text();
    }

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
    var error = checkGroud(group);
    if (error != "") {
        alert(error);
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
    //var index = loadLayer();
    showLoading();

    // 当前subject
    var selSubject = $("#_subject").text();
    if (selSubject == null)
        selSubject = "";

    // 调web api 获取栏目
    var url = "/api2/LibMessageApi/GetSubject?weixinId=" + weixinId
        + "&group=" + encodeURIComponent(group)
        + "&libId=" + libId
    + "&selSubject=" + encodeURIComponent(selSubject)
    + "&param=html";
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        //layer.close(index);
        hideLoading();

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

        // 关闭等待层
        //layer.close(index);
        hideLoading();

        alert("访问服务器出错：\r\n" + errorThrown);

    }); // 同步调用
}



// 单击msg进行只读态与编辑态的切换
function clickMsgDiv(msgId) {

    //alert("走进clientMsgDiv");
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
    //alert(editBtn);

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
    //var index = loadLayer();
    showLoading();

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
    var url = "/api2/LibMessageApi/GetMessage?weixinId=" + weixinId
                + "&group=" + group
                + "&libId=" + libId
                + "&msgId=" + msgId
                + "&subject="
                + "&style=" + style;
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        //layer.close(index);
        hideLoading();

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

        // 2022/8/5 刷新书目摘要
        if (group == "gn:_lib_book") {
            //加载书目summary
            window.setTimeout("fillPending()", 1);
        }

    }, function (xhq, textStatus, errorThrown) {
        // 关闭等待层
        //layer.close(index);
        hideLoading();

        alert("访问服务器出错：\r\n" + errorThrown);

    });

}

// 进入编辑态
function gotoEdit(msgId) {

    $("#divNo").css('display', 'none');
    //alert($("#divNo"));

    var group = $("#_group").text();
    var error = checkGroud(group);
    if (error != "") {
        alert(error);
        return;
    }

    if (group == "gn:_lib_homePage" || group == "gn:_dp_home" || group == "gn:_lib_book") {
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
    //var index = loadLayer();
    showLoading();

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
    var url = "/api2/LibMessageApi/GetMessage?weixinId=" + weixinId
                + "&group=" + group
                + "&libId=" + libId
                + "&msgId=" + msgId
                + "&subject="
                + "&style=" + style;
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        //layer.close(index);
        hideLoading();

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

        // 关闭等待层
        //layer.close(index);
        hideLoading();

        alert("访问服务器出错：\r\n" + errorThrown);

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
    var error = checkGroud(group);
    if (error != "") {
        alert(error);
        return;
    }

    var libId = getLibId(); //$("#selLib").val();
    if (libId == "") {
        return;
    }

    //显示等待图层
    //var index = loadLayer();
    showMaskLayer();

    // 调web api 获取模板
    var url = "/api2/LibMessageApi/GetTemplate?group=" + encodeURIComponent(group)
        + "&libId=" + libId
    + "&subject=" + encodeURIComponent(subject);
    //alert(url);
    sendAjaxRequest(url, "GET", function (result) {
        // 关闭等待层
        //layer.close(index);
        hideMaskLayer();

        if (result.errorCode == -1) {
            alert(result.errorInfo);
            return;
        }

        $("#_val_content").val(result.info);

    }, function (xhq, textStatus, errorThrown) {

        // 关闭等待层
        //layer.close(index);
        hideMaskLayer();

        alert("访问服务器出错：\r\n" + errorThrown);

    });
}

//===========等待图层============
//// 显示等待图层
//function loadLayer() {
//    return layer.open({
//        type: 2,
//        shadeClose: false
//    });
//}


//=======ajax==============

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
    //alert(apiFullPath);

    //alert("test");

    $.ajax(apiFullPath, {
        type: httpMethod,
        success: successCallback,
        error: errorCallback,
        data: mydata,
        async: myasync
    });
    //alert("2");
    //$.ajax({
    //    url: apiFullPath,
    //    type: httpMethod,
    //    contentType: "application/json; charset=UTF-8",//application/json; charset=utf-8",
    //    dataType: "json",
    //    success: successCallback,
    //    error: errorCallback,
    //    data: JSON.stringify(mydata),   // 给.net core api 传对象，需要JSON.stringify()转换下
    //    async: myasync
    //});
}
