
// 记录原始幅号
var orgWebNumList = [];
// addData
//var addData = {};
//// removeData
//var removeData = {};
//// mergeData
//var mergeData = {};
// 记录当前操作行
var __trNo = "";
// 米数约束条件
var __meterLimit = 10000;
// 记录纸病和处理列表
var __defectCodeOption = "";
var __dealProcess = "";
// 记录原数据被删除值
var __delDefectInfoList = [];
// 记录pfId与DefectId对应值
var __pfIdRefDefectId = [];
// 标识修改状态
var __modifyState = 0;
// 标识纸病停机关联列节点
var __relationNode;
// defectId 与 stoprecordId 的关系记录
var __defectStopRecordId = [];
// pfId 与 stoprecordId 的关系记录
var __pfIdStopRecordId = [];


//暂存值， 写入数据库时返回
var __orderId,
    __startTime,
    __endTime;
var __rollNum = "";
var __isBlocked = "";
var __webNum = "";
var __eventID = "";
var __ppID = "";
var __pathId = "";
var __puId = "";
var __userId = "";
var __lang = "";
var __i18n = "";
var __modify = "";
// 记录调试卷
var __debug = "";
// 记录结束米数
var __endMeter = 0;
var __isTreatment = {};
var __selectStopRecord = [];
// 暂存子页面传回的值
var __tmpStopRecordInfo = {
    code: '',
    startTime: '',
    endTime: '',
    remark: ''

};
var __nerRecordDefectIdList = [];


// 数组查找TrNo
function getElement(element) {
    return element.trNo == __trNo;
}

//入口
function ISRADefectInfoApp() {
    var __domMessage = $('#domMessage');
    //根据__modify值，确认修改按钮是否显示

    //获取纸病列表
    this.GetDefectInfoList = function (_refresh) {
        $.blockUI({ message: __domMessage });
        __domMessage.text(__i18n.getDefectInfo);
        var _data = {
            orderId: __orderId,
            rollNum: __rollNum,
            startTime: __startTime,
            endTime: __endTime,
            puId: __puId,
            webNum: __webNum,
            refresh: _refresh,
            eventId: __eventID
        };

        $.ajax({
            url: '/Services/GetPageDefectInfoHandler.ashx',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(_data),
            success: function (data) {
                // 在界面显示数据，两部分
                // 根据状态显示页面 state:1 成功 state:2 失败 state:3 纸病图片获取失败
                if (data.state == 1 || data.state==3) {
                    $.unblockUI();
                    //暂存值赋值

                    if (data.state == 3) {
                        alert("纸病图片获取失败");
                    }
                    __rollNum = data.eventNum;
                    __isBlocked = data.isBlocked;
                    AddDefectInfo(data.defectCodeList, data.defectProcedureList, data.defectList, data.defectListDB, function () {
                        // 重新触发排序
                        // 刷新数据
                        // $("#defectList").trigger("update");
                        // 页面数据完成，初始化控件事件
                        // 初始化-事件添加        // 1、纸病代码关联代码描述// 2、纸病组合
                        var dome = new domEvent();
                        dome.init();
                        if (_refresh == "0") {
                            dome.setEvent();
                        }
                        

                    });
                } else {
                    __domMessage.text(__i18n.getDefectInfoFail);
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                __domMessage.text(__i18n.getDefectInfoFail);

                console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
            }
        });
    }

    // 发送纸病
    this.PostDefectInfoToMESDB = function (reqData) {
        $.blockUI({ message: __domMessage });
        __domMessage.text(__i18n.putDefectInfo);

        $.ajax({
            url: '/Services/PutDefectInfoToDBHandler.ashx',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(reqData),
            success: function (data) {
                $.unblockUI();
                if (data.state == 1) {
                    UpdateDefectInfo(data, "Add");
                } else {
                    //__domMessage.text(data.errmessage);
                    alert(data.errmessage);
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                __domMessage.text(__i18n.putDefectInfoFail);

                console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
            }
        });
    }

    // 删除纸病
    this.DeleteDefectInfoFromDB = function (reqData) {
        $.blockUI({ message: __domMessage });
        __domMessage.text(__i18n.deleteDefectInfo);
        var _data = {
            orderId: __orderId,
            rollNum: __rollNum,
            startTime: __startTime,
            endTime: __endTime,
            puId: __puId,
            webNum: __webNum,
            selectedpfId: [],
            refDefectId: [],
            eventId: __eventID
        };
        //__pfIdRefDefectId.push({ pf_Id: info.pf_Id, refDefectId: info.defectId });
        reqData.forEach(function (x) {
            _data.selectedpfId.push(x.pf_Id);
            _data.refDefectId.push(x.refDefectId);
        });

        $.ajax({
            url: '/Services/DeletePaperFaultHandler.ashx',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(_data),
            success: function (data) {
                $.unblockUI();
                if (data.state == 1) {
                    
                    // 删除成功，从关联关系表中获取纸病id返回后台
                    UpdateDefectInfo(data, "Delete");
                } else {
                    //__domMessage.text(data.errmessage);
                    alert(data.errmessage);
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                __domMessage.text(__i18n.deleteDefectInfoFail);

                console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
            }
        });
    }

    // 更新数据
    this.ModifyDBDefectInfoToMES = function (reqData) {
        $.blockUI({ message: __domMessage });
        __domMessage.text(__i18n.ModifyDefectInfo);
        $.ajax({
            url: '/Services/ModifyPaperFaultInfoHandler.ashx',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(reqData),
            success: function (data) {
                $.unblockUI();
                if (data.state == 1) {

                } else {
                    //__domMessage.text(data.errmessage);
                    alert(data.errmessage);
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                __domMessage.text(__i18n.ModifyDefectInfoFail);

                console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
            }
        });
    }
    
}

// 初始化事件
function domEvent() {

    var app = new ISRADefectInfoApp();
    var __this = this;
    //初始化
    this.init = function () {
        // 更新表格排序
        // let the plugin know that we made a update, then the plugin will
        // automatically sort the table based on the header settings
        $("#defectList").trigger("update");
        $("#dealDefectList").trigger("update");

        // 根据幅数设置超出不可用        
        for (var ckwebNumIndex = Math.floor(__webNum) + 1; ckwebNumIndex <= 10; ckwebNumIndex++) {
            // console.log(ckwebNumIndex);
            // console.log($("#defectList tr .ckwebNum[value='" + ckwebNumIndex + "']"));
            $("#defectList tr .ckwebNum[value='" + ckwebNumIndex + "']").attr("disabled", true);
        }

        //IE8下兼容
        //$('.selectpicker').val('Mustard');
        //--
        $('.selectpicker').selectpicker('refresh');//加载select框选择器
        //$('.selectpicker').selectpicker();

        // 编辑框初始化
        $('.lststartMeter').editable({
            type: "text",                //编辑框的类型。支持text|textarea|select|date|checklist等
            title: __i18n.startMeter,              //编辑框的标题
            disabled: false,             //是否禁用编辑
            emptytext: __i18n.emptytext,          //空值的默认文本
            mode: "popup",              //编辑框的模式：支持popup和inline两种模式，默认是popup
            validate: function (value) { //字段验证
                if (!$.trim(value)) {
                    return __i18n.cannotnull;
                }
                //正则验证米数设置是否合理， 更新结束米数与起始米数差值
                if (!checkMeter(value)) {
                    return __i18n.inputFormat;
                }
                if (value > __meterLimit) {
                    return __i18n.meterLimit;
                }
                var endMeter = $(this).parent().next().text();
                //a     td        td    td 
                $(this).parent().next().next().text((endMeter - value));    //.toFixed(2) 小数点后取2位
                // 
            },
            display: function (value, sourceData) {
                $(this).text(value);
                var resort = true, // re-apply the current sort
                callback = function () {
                    // do something after the updateAll method has completed
                };
                //$("#defectList").trigger("updateAll", [resort, callback]);
            }
        });

        $('.lstendMeter').editable({
            type: "text",                //编辑框的类型。支持text|textarea|select|date|checklist等
            title: __i18n.endMeter,              //编辑框的标题
            disabled: false,             //是否禁用编辑
            emptytext: __i18n.emptytext,          //空值的默认文本
            mode: "popup",              //编辑框的模式：支持popup和inline两种模式，默认是popup
            validate: function (value) { //字段验证
                if (!$.trim(value)) {
                    return __i18n.cannotnull;
                }
                //正则验证米数设置是否合理， 更新结束米数与起始米数差值
                if (!checkMeter(value)) {
                    return __i18n.inputFormat;
                }
                if (value > __meterLimit) {
                    return __i18n.meterLimit;
                }
                var startMeter = $(this).parent().prev().text();
                //a     td        td    td 
                $(this).parent().next().text((value - startMeter));
            },
            display: function (value, sourceData) {
                $(this).text(value);
                var resort = true, // re-apply the current sort
                callback = function () {
                    // do something after the updateAll method has completed
                };
                //$("#defectList").trigger("updateAll", [resort, callback]);
            }
        });
    }

    this.setEvent = function () {
        // button事件
        $("#Refresh").click(function () {
            //刷新当前页面首先将缓存字段清空
            __isTreatment = {};
            orgWebNumList = [];
            __trNo = "";
            __pfIdRefDefectId = [];
            __modifyState = "0";
            //更新按钮
            $("#Modify").val(__i18n.modifyDefectRecord);
            $("#Modify").removeClass("saveBtn");
            // 清空表格内容
            $("#defectList tbody").empty();
            $("#dealDefectList tbody").empty();
            // 隐藏关联停机记录按钮
            $(".stopCode").addClass("hidden");
            $(".stopCodeDB").addClass("hidden");

            app.GetDefectInfoList("1");
        });

        $("#NewRecord").click(function () {
            //新增一条记录，全部手动输入，提示用
            var curTime = new Date().format("hhmmss");
            var data = {
                defectList: []
            };
            data.defectList.push({
                defectId: curTime,
                time: new Date().format("yyyy-MM-dd hh:mm:ss"),
                rollNum: __rollNum,
                startMeter: 0,
                endMeter: __endMeter,
                webNum: 0,
                dLength: 0,
                dWidth: 0,
                dArea: 0
            });
            __nerRecordDefectIdList.push(curTime);
            UpdateDefectInfo(data, "New");

            if (__debug == '1') {
                // 整幅纸病勾选
                $("#defectList tbody tr:last-child .allwebnum").attr("checked", true);
                $("#defectList tbody tr:last-child .allwebnum").change();
                // 幅数置为不可用
                $("#defectList tbody tr:last-child .allwebnum").attr("disabled", true);
                $("#defectList tbody tr:last-child .ckwebNum").attr("disabled", true);
            }
        });

        $("#Merge").click(function () {
            // 1、确认选中行
            // 获取所有行
            var trCkList = $("#defectList tbody").find(".selectedTr");

            if (trCkList.length < 2) {
                return alert(__i18n.selectDefectInfo);
            }
            // 2、整幅合并 ，确认起始米数是否一致
            alert(__i18n.confirmMergeDefect);
            var isMerged = true;
            // 2. webnum 设置是否合理、纸病代码、处理机台是否处理
            reqData = getSelectedDefectInfo(trCkList, isMerged, false);
            // 按行 分析数据， 组成json数据 ,调用接口
            if (reqData.errMessage != "") {
                return alert(reqData.errMessage);
            }

            //暂存值， 写入数据库时返回
            reqData.rollNum = __rollNum;
            reqData.eventID = __eventID;
            reqData.ppID = __ppID;
            reqData.pathId = __pathId;
            reqData.userId = __userId;
            //增加停机代码的关联对应
            reqData.stopRecordList = __defectStopRecordId;

            app.PostDefectInfoToMESDB(reqData);
        });

        $("#Add").click(function () {
            //if (!confirm(__i18n.AddDefectInfo)) {
            //    return;
            //}

            // 获取所有行
            var trCkList = $("#defectList tbody").find(".selectedTr");

            if (trCkList.length <= 0) {
                return alert(__i18n.selectDefectInfo);
            }
            // ajax JSON数据
            var reqData = {
            };
            var isMerged = false;

            // 2. webnum 设置是否合理、纸病代码、处理机台是否处理
            reqData = getSelectedDefectInfo(trCkList, isMerged, false);
            // 按行 分析数据， 组成json数据 ,调用接口
            if (reqData.errMessage != "") {
                return alert(reqData.errMessage);
            }

            //暂存值， 写入数据库时返回
            reqData.rollNum = __rollNum;
            reqData.eventID = __eventID;
            reqData.ppID = __ppID;
            reqData.pathId = __pathId;
            reqData.userId = __userId;
            //增加停机代码的关联对应
            reqData.stopRecordList = __defectStopRecordId;
            
            app.PostDefectInfoToMESDB(reqData);

        });

        $("#Modify").click(function () {
            var trCkList = $("#dealDefectList tbody").find(".selectedTr");
            if (trCkList.length <= 0) {
                return alert(__i18n.selectDefectInfo);
            }
            if (__modifyState == 0) {
                if (!confirm(__i18n.modifyDefectInfoConfirm)) {
                    return;
                }
                
                changeDefectRecordState(trCkList);
                __this.init();

                __modifyState = 1;
                //更新修改按钮
                $(this).val(__i18n.updateDefectRecord); //"SAVE"
                $(this).addClass("saveBtn");
            } else {
                // 保存入库, 取值更新入库
                var reqData = getSelectedDefectInfo(trCkList, false, true);
                //暂存值， 写入数据库时返回
                reqData.rollNum = __rollNum;
                reqData.eventID = __eventID;
                reqData.ppID = __ppID;
                reqData.pathId = __pathId;
                reqData.userId = __userId;
                //增加停机代码的关联对应
                reqData.stopRecordList = __pfIdStopRecordId;

                app.ModifyDBDefectInfoToMES(reqData);

                // 更新行状态，重新变为不能更新状态
                changeDefectRecordState(trCkList, reqData);
                __modifyState = 0;
                //更新按钮
                $(this).val(__i18n.modifyDefectRecord);//MODIFY
                $(this).removeClass("saveBtn");                                
            }
        });

        $("#Delete").click(function () {
            if (!confirm(__i18n.deleteDefectInfoConfirm)) {
                return;
            }
            // 不能删除纸病源数据，只能删除保存入库数据
            // 获取所有行
            var trCkList = $("#dealDefectList tbody").find(".selectedTr");

            if (trCkList.length <= 0) {
                return alert(__i18n.selectDefectInfo);
            }
            // ajax JSON数据
            var selectedpfIdList = __pfIdRefDefectId.filter(function (x) {
                for (var i = 0; i < trCkList.length; i++) {
                    //获取 pfId + defectId
                    if (x.pf_Id == $($(trCkList[i]).find(".cktr")).val()) {
                        return x;
                    }
                }
            });
            app.DeleteDefectInfoFromDB(selectedpfIdList);
        });

        $("#Report").click(function () {
            
            var trCkList = $("#dealDefectList tbody").find(".selectedTr");
            if (trCkList.length <= 0) {
                return alert(__i18n.selectDefectInfo);
            }
            if (trCkList.length > 1) {
                return alert(__i18n.selectDefectInfo);
            }
            if (!confirm(__i18n.ComplaintPaperFault)) {
                return;
            }
            //
            //string orderNumber, string ppId, string eventNum, string eventId, string puId, string pathId, string startMeter, string endMeter, string defectMeter, string webNumList, string pfCode, string remark
            var defectLinetds = $(trCkList[0]).children();
            var webLists = $(trCkList[0]).find("input[type=checkbox]");
            var tmpWebNum = "";
            for (var i = 1; i < webLists.length; i++) {
                if ($(webLists[i]).is(':checked')) {
                    tmpWebNum += "" + $(webLists[i]).val();
                }
            }
            //console.log(__orderId, __ppID, __rollNum, __eventID, __puId, __pathId, $(defectLinetds[3]).text(), $(defectLinetds[4]).text(), $(defectLinetds[5]).text(),
            //    tmpWebNum, $(defectLinetds[6]).text(), $(defectLinetds[18]).text());
            window.external.ComplaintViewer(__orderId, __ppID, __rollNum, __eventID, __puId, __pathId, $(defectLinetds[3]).text(), $(defectLinetds[4]).text(), $(defectLinetds[5]).text(),
                tmpWebNum, $(defectLinetds[6]).text(), $(defectLinetds[18]).text());
        });

        

        // 纸病代码选择与纸病描述关联

        // 当checkbox check动作执行后，重新加载tablesort，将选中的值读入


        // 
        // 图片点击看大图
        $("body").on("click", ".pimg", function () {
            var _this = $(this);//将当前的pimg元素作为_this传入函数  
            imgShow("#outerdiv", "#innerdiv", "#bigimg", _this);
        });

        // defectList/dealDefectList 行选择事件
        $("body").on("change", ".cktr", function () {
            if (__modifyState == 1) {
                // 正在进行修改操作
                alert(__i18n.selectModifyWarning);
                //$($(this).parent().parent()).attr("checked", false);
                //$(this).attr("checked", false);
                return;
            }

            if ($(this).is(':checked')) {

                //if (__modifyState == 1) {
                //    // 正在进行修改操作
                //    alert(__i18n.selectModifyWarning);
                //    //$($(this).parent().parent()).attr("checked", false);
                //    $(this).attr("checked", false);
                //    return;
                //}

                $($(this).parent().parent()).addClass("selectedTr");
                // 如果包含 pfIddb 表示选择的DB列的内容
                if ($(this).hasClass("pfIddb")) {
                    // 没有stopRecord 增加stopRecord
                    //var stopCode = $(this).parent().next().next().next().next().next().next().next().next().next().next().next().next().next().next().next().next().next().next().text();
                    //console.log(stopCode);
                    //if (stopCode.indexOf("_")>0) {
                    //    stopCode = stopCode.substring(0, stopCode.indexOf("_"));
                    //} else {
                    //    stopCode = "0";
                    //}
                    //__pfIdStopRecordId.push({ pfId: $(this).val(), stopRecordId: [stopCode] });
                    __pfIdStopRecordId.push({ pfId: $(this).val(), stopRecordId: ["save"] });
                }
            } else {

                //if (__modifyState == 1) {
                //    // 正在进行修改操作
                //    alert(__i18n.selectModifyWarning);
                //    $(this).attr("checked", true);
                //    return;
                //}

                $($(this).parent().parent()).removeClass("selectedTr");
                // 如果包含 pfIddb 表示选择的DB列的内容
                //if ($(this).hasClass("pfIddb")) {
                //    // 删除stopRecord
                    
                //}
            }
        });

        // 整幅选择
        $("body").on("change", ".allwebnum", function () {
            console.log(1);
            // 勾选 -- 整幅全选
            __trNo = $($(this).parent().parent().find(".cktr")[0]).val();
            // 根据trNo找到对应的webNum值
            var checkedWebNum = orgWebNumList.filter(getElement)[0];
            if (typeof checkedWebNum == "undefined") {
                checkedWebNum = { trNo: __trNo, webNum: [] };
            }
            var ckList = $(this).parent().parent().find(".ckwebNum");
            if ($(this).is(':checked')) {
                checkedWebNum = { trNo: __trNo, webNum: [] };
                for (var i = 0; i < __webNum; i++) {
                    if (!$(ckList[i]).is(':checked')) {
                        $(ckList[i]).prop("checked", "checked");
                    } else {
                        // 记录已经勾选的值                        
                        checkedWebNum.webNum.push($(ckList[i]).val());
                    }
                }
                orgWebNumList.push(checkedWebNum);

            }
                // 失效 -- 显示勾选前的数据
            else {
                if (checkedWebNum.webNum.length == 0) {
                    return;
                }
                // 取消勾选
                for (var i = 0, j = 0; i < __webNum; i++) {
                    if (i + 1 == checkedWebNum.webNum[j]) {
                        j++;
                        continue;
                    } else {
                        $(ckList[i]).prop("checked", "");
                    }
                }

            }
            // 关联停机代码可用/不可用
            setStopCodeRelationUseful($(this).parent().parent().find(".stoprecord"), $(this).is(':checked'));
        });

        // 幅号勾选事件，重新加载table，实现幅号排序
        $("body").on("change", ".ckwebNum", function () {
            var resort = true, // re-apply the current sort
                callback = function () {
                    // do something after the updateAll method has completed
                };
            //$("#defectList").trigger("updateAll", [resort, callback]);

            //当选择幅数与当前工单幅数一致时，动态显示关联停机代码按钮
            var ckList = $(this).parent().parent().find(".ckwebNum");
            var disabled = false;
            for (var i = 0; i < __webNum; i++) {
                if (!$(ckList[i]).is(':checked')) {
                    // 如果存在未勾选的，不显示 关联停机代码按钮
                    disabled = true;
                    break;
                }
            }
            // 全部勾选，关联停机代码按钮可用
            setStopCodeRelationUseful($(this).parent().parent().find(".stoprecord"), !disabled);
        });

        // 纸病代码选择
        $("body").on("change", "select.selectpfCode", function () {
            
        });

        // 纸病处理机台选择
        $("body").on("change", "select.selectprocess", function () {
            //如果是本工序
            var curDefectId = $($(this).parent().parent().parent().find('.cktr')[0]).val();
            if ($(this).val() == "1") {
                //获取defectid
                if (confirm(__i18n.confirmDealedDefectInfo)) {
                    __isTreatment["d" + curDefectId] = "1";
                }
                else {
                    __isTreatment["d" + curDefectId] = "0";
                }
            } else {
                __isTreatment["d" + curDefectId] = "0";
            }
        });

        // 关联停机代码
        $("body").on("click", "input[name='stopcode']", function () {
            //if __isBlocked==-1 return 成品卷Event_Details中 Blocked_In_Location 字段为空，不允许关联停机代码
            // 获取当前纸病代码
            var pfCode = $($(this).parent().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().find(".filter-option-inner-inner")[0]).text();
            
            if (pfCode == '请选择纸病代码') {
                alert("纸病代码为空，不允许关联停机代码");
                return;
            }

            //"34101"
            if (__isBlocked == -1 && __debug == '0' && pfCode.indexOf("34101")>=0) {
                alert("成品卷Event_Details中 Blocked_In_Location 字段为空，不允许关联停机代码");
                return;
            }

            // 打开mes界面，关联停机代码
            // 卷号、纸病ID、停机ID
            //window.external.CreateStopCodeRelation(__eventID, '0', '');
            // 获取当前的defectId
            var defectId = $($(this).parent().parent().find(".cktr")[0]).val();
            // 获取当前记录的时间
            var defecttime = $(this).parent().next().next().next().next().next().next().text();
            var stopcodebtn = $(this);
            var url = './StopCodeRelation.html?defectTime=' + defecttime + '&eventId=' + __eventID;
            
            // 判断是不是新增记录
            if (__nerRecordDefectIdList.indexOf(defectId) >= 0) {
                url += '&newRecord=1'
            }
            console.log(url);
            //弹出一个iframe层
            layer.open({
                type: 2,
                title: '关联停机记录',
                maxmin: true,
                shadeClose: true, //点击遮罩关闭层
                area: ['1200px', '520px'],
                content: url,//'./StopCodeRelation.html?defectTime=2018-12-09 11:28:36&eventId=3630164',
                btn: ['关联'],
                yes: function (index, layero) {
                    try {

                        // 判断是否符合对应要求
                        var flag = checkpfCodestopCode(pfCode, __tmpStopRecordInfo.code);
                        if (!flag) {
                            __selectStopRecord = [];
                            //layer.close(index); //如果设定了yes回调，需进行手工关闭
                            return;
                        }

                        //stopCode
                        $(".stopCode").removeClass('hidden');

                        // 将defectId 与 stoprecordId 的对应关系保存
                        for (var k = 0; k < __defectStopRecordId.length; k++) {
                            if (__defectStopRecordId.defectId == defectId) {
                                __defectStopRecordId.splice(k, 1);
                                break;
                            }
                        }
                        __defectStopRecordId.push({ defectId: defectId, stopRecordId: __selectStopRecord });
                        __selectStopRecord = [];
                        layer.close(index); //如果设定了yes回调，需进行手工关闭

                        // 判断是否符合对应要求


                        // 页面显示
                        var showList = stopcodebtn.parent().next().find("p");
                        //console.log(showList);
                        $(showList[0]).text(__tmpStopRecordInfo.code);
                        $(showList[1]).text(__tmpStopRecordInfo.startTime);
                        $(showList[2]).text(__tmpStopRecordInfo.endTime);
                        //备注信息
                        //console.log(__tmpStopRecordInfo.remark);
                        
                        $(stopcodebtn.parent().next().next().find("textarea")[0]).text(__tmpStopRecordInfo.remark);

                    } catch (e) {

                    }

                }
            });

        });
        // 关联停机代码
        $("body").on("click", "input[name='stopCodeDB']", function () {

            // 获取当前纸病代码
            var pfCode = $($(this).parent().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().prev().find(".filter-option-inner-inner")[0]).text();
            if (pfCode == '请选择纸病代码') {
                alert("纸病代码为空，不允许关联停机代码");
                return;
            }

            if (__isBlocked == -1 && __debug == '0' && pfCode.indexOf("34101") >= 0) {
                alert("成品卷Event_Details中 Blocked_In_Location 字段为空，不允许关联停机代码");
                return;
            }

            // 打开mes界面，关联停机代码
            // 卷号、纸病ID、停机ID
            //window.external.CreateStopCodeRelation(__eventID, '0', '');
            var stopcodebtn = $(this);

            // 获取当前的pfId
            var pfId = $($(this).parent().parent().find(".cktr")[0]).val();
            // 获取当前记录的时间
            var defecttime = $(this).parent().next().next().next().next().next().next().text();
            var stopcodebtn = $(this);
            var url = './StopCodeRelation.html?pfId=' + pfId + '&eventId=' + __eventID + '&modify=1';
            console.log(url);
            //弹出一个iframe层
            layer.open({
                type: 2,
                title: '关联停机记录',
                maxmin: true,
                shadeClose: true, //点击遮罩关闭层
                area: ['1200px', '520px'],
                content: url,
                btn: ['关联'],
                yes: function (index, layero) {
                    try {

                        // 判断是否符合对应要求
                        var flag = checkpfCodestopCode(pfCode, __tmpStopRecordInfo.code);
                        if (!flag) {
                            __selectStopRecord = [];
                            //layer.close(index); //如果设定了yes回调，需进行手工关闭
                            return;
                        }

                        // 将pfId 与 stoprecordId 的对应关系保存
                        for (var k = 0; k < __pfIdStopRecordId.length; k++) {
                            if (__pfIdStopRecordId.pfId == pfId) {
                                __pfIdStopRecordId.splice(k, 1);
                                break;
                            }
                        }
                        __pfIdStopRecordId.push({ pfId: pfId, stopRecordId: __selectStopRecord });
                        __selectStopRecord = [];
                        layer.close(index); //如果设定了yes回调，需进行手工关闭

                        

                        // 页面显示
                        var showList = stopcodebtn.parent().prev().find("p");
                        console.log(showList);
                        $(showList[0]).text(__tmpStopRecordInfo.code);
                        $(showList[1]).text(__tmpStopRecordInfo.startTime);
                        $(showList[2]).text(__tmpStopRecordInfo.endTime);
                    } catch (e) {

                    }

                }
            });

        });

        $("body").on("click", ".changeMeter", function () {
            //alert("上次是不是你改的！！");
        });
    }

    function setStopCodeRelationUseful(node, useful) {
        if (useful) {
            $(node[0]).removeAttr("disabled");
        } else {
            $(node[0]).attr("disabled", "disabled");
        }
    }

    function checkpfCodestopCode(pfCode, stopCode) {        
        if (stopCode == 0) {
            return false;
        }

        //纸病代码为34101 blocked = 1 stopCode 只能关联 1201-1206
        if (__debug == 1 && __isBlocked == 1 && pfCode.indexOf("34101") >= 0) {
            var tmpCode = stopCode.substr(0, 4); //stopCode.indexOf('_')
            if (tmpCode < 1201 || tmpCode > 1206) {
                alert("纸病代码为34101,只能关联1201-1206下的停机代码！");
                return false;
                //调试卷且Blocked_In_Location为1，
            }
        }
        //纸病代码为34102 只能关联 26%下的纸病代码
        if (pfCode.indexOf("34102") >= 0) {
            if (stopCode.indexOf('26') != 0) {
                alert("纸病代码为34102,只能关联以26开头的停机代码！");
                return false;
            }
        }
        //纸病代码为34103 只能关联 21%下的纸病代码
        if (pfCode.indexOf("34103") >= 0) {
            if (stopCode.indexOf('21') != 0) {
                alert("纸病代码为34103,只能关联以21开头的停机代码！");
                return false;
            }
        }

        return true;
    }

}

// 显示纸病停机记录
function showStopCodeInfo(stopRecordID, txtStopCode, txtStopS, txtStopE){
    //__relationNode
    stopcodeShow('#stopcodediv', '', '');
}

// 获取勾选的纸病信息 + //合并纸病
function getSelectedDefectInfo(trCkList, isMerged, isModify) {
    var errMessage = "";
    var selectDefectInfoList = [];

    for (var i = 0; i < trCkList.length; i++) {
        // 3. 结束米数与起始米数差值是否合理
        var defectLinetds = $(trCkList[i]).children();
        var startEndOffset = $(defectLinetds[5]).text();
        if (startEndOffset.indexOf("-") >= 0) {
            // 米数差值为负，提示
            errMessage += __i18n.offsetMeter;
        }
        // 幅数是否选择
        var webNumC = ($(defectLinetds[8].childNodes[0]).is(':checked') ? "1" : "0") + ($(defectLinetds[9].childNodes[0]).is(':checked') ? "1" : "0") + ($(defectLinetds[10].childNodes[0]).is(':checked') ? "1" : "0")
            + ($(defectLinetds[11].childNodes[0]).is(':checked') ? "1" : "0") + ($(defectLinetds[12].childNodes[0]).is(':checked') ? "1" : "0") + ($(defectLinetds[13].childNodes[0]).is(':checked') ? "1" : "0")
            + ($(defectLinetds[14].childNodes[0]).is(':checked') ? "1" : "0") + ($(defectLinetds[15].childNodes[0]).is(':checked') ? "1" : "0") + ($(defectLinetds[16].childNodes[0]).is(':checked') ? "1" : "0")
            + ($(defectLinetds[17].childNodes[0]).is(':checked') ? "1" : "0");
        if (webNumC == "0000000000") {
            // 幅数未设置，提示
            errMessage += __i18n.selectWebNum;
        }
         // 如果选择整幅纸病，提示是否关联停机记录
         var hasRecord = false;
         if (!isModify && $(defectLinetds[18].childNodes[0]).is(':checked')) {
             // 判断是否存在对应的停机记录            
             for (let pk = 0; pk < __defectStopRecordId.length; pk++) {
                 if (__defectStopRecordId[pk].defectId == $(defectLinetds[0].childNodes[0]).val() && __defectStopRecordId[pk].stopRecordId.length>0) {
                     hasRecord = true;
                     break;
                 }
             }
             //if (!hasRecord) {
             //    // 整幅纸病, 但未增加与停机记录的关联, 是否继续提交?
             //    if (confirm("整幅纸病, 但未增加与停机记录的关联, 是否继续提交?")) {
                    
             //    } else {
             //        $(defectLinetds[1]).css("color", "red");
             //        return { errMessage: "请将红色纸病，关联停机记录！" };
             //    }
             //}
         }
        // 修改数据库字段
        if (isModify) {
            if(webNumC.lastIndexOf("1")+1 == __webNum){
                // 判断是否存在对应的停机记录                
                for (let pk = 0; pk < __pfIdStopRecordId.length; pk++) {
                    if (__pfIdStopRecordId[pk].pfId == $(defectLinetds[0].childNodes[0]).val() && __pfIdStopRecordId[pk].stopRecordId.length > 0) {
                        hasRecord = true;
                        break;
                    }
                }
                // if (!hasRecord) {
                //     // 整幅纸病, 但未增加与停机记录的关联, 是否继续提交?
                //     if (confirm("整幅纸病, 但未增加与停机记录的关联, 是否继续提交?")) {

                //     } else {
                //         $(defectLinetds[1]).css("color", "red");
                //         return { errMessage: "请将红色纸病，关联停机记录！" };
                //     }
                // }
            }
        }
        // 纸病代码+处理机台
        var pfCode = $($(defectLinetds[6]).find(".filter-option-inner-inner")[0]).text();

        //var pfCode = $($(defectLinetds[6]).find("li.selected")[0]).text();
        if ($(defectLinetds[6]).find("li.selected").length == 0) {
            errMessage += __i18n.selectPfCode;
        }

        if (((__isBlocked == "1" && pfCode.indexOf("34101") >= 0) || pfCode.indexOf("34102") >= 0 || pfCode.indexOf("34103") >= 0) && !hasRecord) {
            console.log($(defectLinetds[1]));
            $(defectLinetds[1]).css("color", "red");
            var message = "存在【印刷短停】或【印刷故障停机】未关联停机代码。\n请将红色纸病，关联停机记录！";
            if(pfCode.indexOf("34101") >= 0){
                message = "存在【印刷试机】未关联停机代码。\n请将红色纸病，关联停机记录！";
            }
            return {errMessage : message};
        }
        var pcCode = $($(defectLinetds[7]).find(".filter-option-inner-inner")[0]).text();
        //var pcCode = $($(defectLinetds[7]).find("li.selected")[0]).text();
        //if (pcCode.indexOf("请选择") >= 0) {
        if ($(defectLinetds[7]).find("li.selected").length == 0) {
            errMessage += __i18n.selectPcCode;
        }
        // 组合json数组
        if (isModify) {
            selectDefectInfoList.push({
                pfId: $(defectLinetds[0].childNodes[0]).val(),
                webNum: webNumC,
                pfCode: pfCode,
                pcCode: pcCode,
                isTreatment: __isTreatment["d" + $(defectLinetds[0].childNodes[0]).val()],
                startMeter: $(defectLinetds[3].childNodes[0]).text(),
                endMeter: $(defectLinetds[4].childNodes[0]).text(),
                remark: $(defectLinetds[20].childNodes[0]).val()                
            });
        } else {
            selectDefectInfoList.push({
                defectId: $(defectLinetds[0].childNodes[0]).val(),
                defectImageName: $(defectLinetds[2].childNodes[0]).attr("src"),
                webNum: webNumC,
                pfCode: pfCode,
                pcCode: pcCode,
                isTreatment: __isTreatment["d" + $(defectLinetds[0].childNodes[0]).val()],
                startMeter: $(defectLinetds[3].childNodes[0]).text(),
                endMeter: $(defectLinetds[4].childNodes[0]).text(),
                remark: $(defectLinetds[21].childNodes[0]).val(),
                dWidth: $(defectLinetds[22]).text(),
                dLength: $(defectLinetds[23]).text(),
                dArea: $(defectLinetds[24]).text()
            });
        }
        
    }
    // 合并纸病 //.indexOf("幅数未选择") < 0 && errMessage.indexOf("米数") < 0
    if (isMerged && errMessage == "") {
        //按照起始米数排序
        selectDefectInfoList = selectDefectInfoList.sort(function (a, b) {
            return a.startMeter > b.startMeter ? 1 : -1;
        });
        //合并信息
        var tmp = selectDefectInfoList[0];
        var curStopRecord = { defectId: tmp.defectId };
        //判断纸病代码+处理机台是否一致，否则弹出警告
        selectDefectInfoList.forEach(function (dfInfo, index) {
            if (index == 0) return;
            if (tmp.endMeter < dfInfo.endMeter) {
                tmp.endMeter = dfInfo.endMeter;
            }
            //if (tmp.pfCode.indexOf("请选择") >= 0) {  == ""
            if (tmp.pfCode == "") {
                tmp.pfCode = dfInfo.pfCode;
            } else {
                if (tmp.pfCode != dfInfo.pfCode) {
                    errMessage += __i18n.difPfCode;
                }
            }
            if (tmp.pcCode == "") {//(tmp.pcCode.indexOf("请选择") >= 0) {
                tmp.pcCode = dfInfo.pcCode;
            } else {
                if (tmp.pcCode != dfInfo.pcCode) {
                    errMessage += __i18n.difPcCode;
                }
            }
            tmp.remark += dfInfo.remark;
            // 幅数合并
            var num = parseInt(tmp.webNum, 2) | parseInt(dfInfo.webNum, 2);
            tmp.webNum = ("0000000000" + num.toString(2)).substr(-10);
            tmp.defectId += "," + dfInfo.defectId;
            // 合并 停机记录
            for (let pk = 0; pk < __defectStopRecordId.length; pk++) {
                if (__defectStopRecordId[pk].defectId == $(defectLinetds[0].childNodes[0]).val() && __defectStopRecordId[pk].stopRecordId.length > 0) {
                    curStopRecord.stopRecordId.push(__defectStopRecordId[pk].stopRecordId[0]);
                    __defectStopRecordId.splice(pk, 1);
                    break;
                }
            }
        });
        if (typeof curStopRecord.stopRecordId != 'undefined' && curStopRecord.stopRecordId.length > 0) {
            __defectStopRecordId.push(curStopRecord);
        }
        selectDefectInfoList.splice(0, selectDefectInfoList.length);
        selectDefectInfoList.push(tmp);
    }
    return { selectDefectInfoList: selectDefectInfoList, errMessage: errMessage };
}

// 更新修改行状态
function changeDefectRecordState(trCkList, modifyData) {
    if ( typeof modifyData == 'undefined') {
        // 控件变为可用 开始米数、结束米数、纸病代码、处理工序、幅数1、2、3、4、5、6、7、8、9、10 备注
        for (var i = 0; i < trCkList.length; i++) {
            var tdList = $(trCkList[i]).children();
            console.log(tdList.length);
            $(tdList[3]).html("<a class=\"" + (__debug == '1' ? "changeMeter" : "lststartMeter") + "\" href=\"#\">" + $(tdList[3]).text() + "</a>");
            $(tdList[4]).html("<a class=\"" + (__debug == '1' ? "changeMeter" : "lstendMeter") + "\" href=\"#\">" + $(tdList[4]).text() + "</a>");


            //重设纸病类型选择、处理机台选择
            var pfCode = $(tdList[6]).text();
            var pcCode = $(tdList[7]).text();
            var pfCodeNode = $(__defectCodeOption);
            pfCodeNode.find("option:contains('" + pfCode + "')").attr("selected", true);
            var pcCodeNode = $(__dealProcess);
            pcCodeNode.find("option:contains('" + pcCode + "')").attr("selected", true);
            if (pcCodeNode.val() == "1") {
                __isTreatment["d" + $($(tdList[0]).children()[0]).val()] = "1";
            } else {
                __isTreatment["d" + $($(tdList[0]).children()[0]).val()] = "0";
            }
            console.log(pcCodeNode.val());
            $(tdList[6]).html("<select class=\"selectpfCode selectpicker show-tick \" data-live-search=\"true\" data-size=\"30\" title=\"" + __i18n.PleaseSelectPfCode + "\">" + pfCodeNode.html() + "</select>");
            $(tdList[7]).html("<select class=\"selectprocess selectpicker show-tick form-control\" data-live-search=\"true\" data-size=\"15\" title=\"" + __i18n.PleaseSelectPcCode + "\">" + pcCodeNode.html() + "</select>");
            var curWebNum = 0;
            for (var j = 0; j < __webNum; j++) {
                $($(tdList[8 + j]).children()[0]).removeAttr("disabled");
                if ($($(tdList[8 + j]).children()[0]).is(':checked')) {
                    curWebNum++;
                }
            }
            if (curWebNum == __webNum) {
                //将关联停机记录按钮置为可用
                $($(tdList[19]).children()[0]).removeAttr("disabled");
            }
            $($(tdList[20]).children()[0]).removeAttr("disabled");

        }
        console.log(1);
        // 关联停机记录按钮可见
        $(".stopCodeDB").removeClass("hidden");

    } else {
        // 控件变为可用 开始米数、结束米数、纸病代码、处理工序、幅数1、2、3、4、5、6、7、8、9、10 备注
        for (var i = 0; i < trCkList.length; i++) {
            var tdList = $(trCkList[i]).children();

            $(tdList[3]).empty();
            $(tdList[4]).empty();
            $(tdList[3]).text(modifyData.selectDefectInfoList[i].startMeter);
            $(tdList[4]).text(modifyData.selectDefectInfoList[i].endMeter);

            //重设纸病类型选择、处理机台选择
            var pfCode = $(tdList[6]).text(modifyData.selectDefectInfoList[i].pfCode);
            var pcCode = $(tdList[7]).text(modifyData.selectDefectInfoList[i].pcCode.split('_')[0]);

            for (var j = 0; j < __webNum; j++) {
                $($(tdList[8 + j]).children()[0]).attr("disabled", true);
            }
            $($(tdList[18]).children()[0]).attr("disabled", true);

            //去除勾选
            $(trCkList[i]).removeClass("selectedTr");
            $($(tdList[0]).children()[0]).attr("checked", false);

            console.log(2);
        }
    }
    
    
}

function AddDefectInfo(defectCodeList, defectProcedureList, defectList, defectListDB, callback) {
    // 纸病代码
    __defectCodeOption = "<select class=\"selectpfCode selectpicker show-tick \" data-live-search=\"true\" data-size=\"30\" title=\"" + __i18n.PleaseSelectPfCode + "\">";
    
    defectCodeList.forEach(function (defectCode) {
        __defectCodeOption += "<option value=\"" + defectCode.PDCode + "\" >" + defectCode.PDCode + "_" + defectCode.PDCNText + "</option>"; //PDCNText        
    });//data-subtext=\"" + defectCode.PDCode + "_" + defectCode.PDCNText + "\"
    __defectCodeOption += "</select>";
    // 处理机台
    // 如果是调试卷，则默认印刷机机台、treatment=1
    __dealProcess = "<select class=\"selectprocess selectpicker show-tick form-control\" data-live-search=\"true\" data-size=\"15\" title=\"" + __i18n.PleaseSelectPcCode + "\">";
    defectProcedureList.forEach(function (defectProcedure, mIndex) {
        if (__debug == 1 && mIndex > 0) {
            return;
        }
        __dealProcess += "<option value=\"" + defectProcedure.PL_Id + "\"" + ((__debug==1 && mIndex==0)?"selected":"") + " >" + defectProcedure.PL_Desc + "_" + defectProcedure.PL_Id + "</option>";
        
    });//data-subtext=\"" + defectProcedure.PL_Desc + "_" + defectProcedure.PL_Id + "\"
    __dealProcess += "</select>";

    // 浏览器类型
    //var userAgent = navigator.userAgent;
    //var isChrome = false;
    //if (userAgent.indexOf("Chrome") > 0 || userAgent.indexOf("Safari") > 0) {
    //    isChrome = true;
    //}
    //console.log(isChrome);

    // 追加到中tbody
    $("#defectList > tbody").append(AddDefectTableList(defectList, __defectCodeOption, __dealProcess));
    $("#dealDefectList > tbody").append(AddDBTableList(defectListDB));

    return callback();
}

function UpdateDefectInfo(data, option) {
    // 1.删除行
    if (option == "Add") {
        $("#defectList tr").remove(".selectedTr");
    } else if (option == "Delete") {
        $("#dealDefectList tr").remove(".selectedTr");
    } else if (option == "Merge") {

    } else if (option == "New") {

    }

    // 2.添加行
    if (typeof data.defectList != "undefined") {
        // 数据源表
        //var oldRows = $("#defectList > tbody").html();        
        $("#defectList > tbody").append(AddDefectTableList(data.defectList, __defectCodeOption, __dealProcess));   //oldRows +
        // 更新排序
        //var resort = true;
        //$("#defectList").trigger("update", [resort]);
    }
    if (typeof data.defectListDB != "undefined") {
        // 数据库表
        //var oldRows = $("#dealDefectList > tbody").html();
        $("#dealDefectList > tbody").append(AddDBTableList(data.defectListDB));
    }

    var dev = new domEvent();
    dev.init();
}

function AddDefectTableList(defectList, defectCodeOption, dealProcess) {
    // 根据返回值循环添加
    var node = "";
    defectList.forEach(function (info, index) {
        var curDate = new Date(info.time);
        //var imgPath = "../IMAGE/" + __orderId + "/" + __rollNum + "/" + info.defectId + "_" + ("0" + curDate.getHours()).substr(-2) + ("0" + curDate.getMinutes()).substr(-2) + ("0" + curDate.getSeconds()).substr(-2) + "_D.bmp";
        var imgPath = "../IMAGE/" + __orderId + "/" + __eventID + "/" + info.defectId + "_" + ("0" + curDate.getHours()).substr(-2) + ("0" + curDate.getMinutes()).substr(-2) + ("0" + curDate.getSeconds()).substr(-2) + "_D.bmp";

        node += "<tr><td><input class=\"cktr\" value=\"" + info.defectId + "\" type=\"checkbox\"></td>"
            + "<td>" + info.rollNum + "</td>" + "<td><img class=\"pimg\" width=\"120\" height=\"120\" src=\"" + imgPath + "\" /></td>"
            // __debug == 1 起始结束米数不能修改 lststartMeter lstendMeter
            + "<td><a class=\"" + (__debug == '1' ? "" : "lststartMeter") + " meterResort\" href=\"#\">" + info.startMeter + "</a></td><td><a class=\"" + (__debug == '1' ? "" : "lstendMeter") + "  meterResort\" href=\"#\">" + info.endMeter + "</a></td>"
            + "<td>" + (info.endMeter - info.startMeter) + "</td><td>" + defectCodeOption + "</td><td>" + dealProcess + "</td>"    //<td>" + defectCodeDescList + "</td>
        // 所在幅处理
        // 根据幅数设置超出不可用
            + "<td><input class=\"ckwebNum\" value=\"1\" type=\"checkbox\" " + (info.webNum == 1 ? "checked=\"true\"" : "") + "></td>"   //||index==0 
            + "<td><input class=\"ckwebNum\" value=\"2\" type=\"checkbox\" " + (info.webNum == 2 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"3\" type=\"checkbox\" " + (info.webNum == 3 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"4\" type=\"checkbox\" " + (info.webNum == 4 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"5\" type=\"checkbox\" " + (info.webNum == 5 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"6\" type=\"checkbox\" " + (info.webNum == 6 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"7\" type=\"checkbox\" " + (info.webNum == 7 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"8\" type=\"checkbox\" " + (info.webNum == 8 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"9\" type=\"checkbox\" " + (info.webNum == 9 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"10\" type=\"checkbox\" " + (info.webNum == 10 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"allwebnum\" value=\"All\" type=\"checkbox\"></td>"
            + "<td><input class=\"stoprecord\" name=\"stopcode\" type=\"button\" value=\"关联停机代码\" disabled=\"true\"></td>"
            + "<td class=\"stopCode hidden\"><p></p><p></p><p></p></td>"
            + "<td><textarea rows=\"6\" cols=\"20\"></textarea></td>"
            + "<td class=\"hidden\">" + info.dWidth + "</td><td class=\"hidden\">" + info.dLength + "</td><td class=\"hidden\">" + info.dArea + "</td>"
            + "<td class=\"hidden\">" + info.time + "</td></tr>";        
        //console.log(node);
        if (__debug == '1') {
            __isTreatment["d" + info.defectId] = "1";
        }
        
    });
    //console.log(node);
    return node;
}

function AddDBTableList(defectListDB) {
    // 添加已经处理完成的数据
    var node = "";
    defectListDB.forEach(function (info, index) {
        // 单独处理幅数，根据返回值设置默认勾选
    
        //var imgPath = "../IMAGE/" + __orderId + "/" + __rollNum + "/" + info.defectImageName;
        var imgPath = "../IMAGE/" + __orderId + "/" + __eventID + "/" + info.defectImageName;
        
        node += "<tr id=\"tr" + info.pf_Id + "\"><td><input class=\"cktr pfIddb\" value=\"" + info.pf_Id + "\" type=\"checkbox\"></td>"
            + "<td>" + __rollNum + "</td>"
            + "<td><img width=\"120\" height=\"120\" src=\"" + imgPath + "\" /></td>"
            + "<td>" + info.BedshaftMeter + "</td><td>" + info.WatchSpindleMeter + "</td>"
            + "<td>" + (info.WatchSpindleMeter - info.BedshaftMeter) + "</td>"
            + "<td class=\"showLeft\">" + info.pf_Code + "</td>"
            + "<td>" + info.IsPrintTreatment + "</td>"
            + "<td><input class=\"ckwebNum\" value=\"1\" type=\"checkbox\" disabled=\"true\" " + (info.Img1 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"2\" type=\"checkbox\" disabled=\"true\" " + (info.Img2 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"3\" type=\"checkbox\" disabled=\"true\" " + (info.Img3 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"4\" type=\"checkbox\" disabled=\"true\" " + (info.Img4 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"5\" type=\"checkbox\" disabled=\"true\" " + (info.Img5 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"6\" type=\"checkbox\" disabled=\"true\" " + (info.Img6 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"7\" type=\"checkbox\" disabled=\"true\" " + (info.Img7 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"8\" type=\"checkbox\" disabled=\"true\" " + (info.Img8 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"9\" type=\"checkbox\" disabled=\"true\" " + (info.Img9 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td><input class=\"ckwebNum\" value=\"10\" type=\"checkbox\" disabled=\"true\" " + (info.Img10 == 1 ? "checked=\"true\"" : "") + "></td>"
            + "<td class=\"stopCodeDB \"><p>" + info.Event_Reason_Code + "</p><p>" + info.Start_Time + "</p><p>" + info.End_Time + "</p></td>"
            + "<td class=\"stopCodeDB hidden\"><input class=\"stoprecord\" name=\"stopCodeDB\" type=\"button\" value=\"关联停机代码\" disabled=\"true\"></td>"
            + "<td class=\"showLeft\"><textarea rows=\"6\" cols=\"20\" disabled=\"true\">" + info.Remark + "</textarea></td></tr>";
        __pfIdRefDefectId.push({ pf_Id: info.pf_Id, refDefectId: info.defectId });
    });
    return node;
}


$(document).ready(function () {

    // 拆分url
    // 调用方法
    __orderId = GetQueryString("orderId");
    __rollNum = GetQueryString("rollNum");
    __startTime = GetQueryString("startTime");
    __endTime = GetQueryString("endTime");
    __puId = GetQueryString("puId");
    __webNum = GetQueryString("webNum");
    __eventID = GetQueryString("eventID");
    __ppID = GetQueryString("ppID");
    __pathId = GetQueryString("pathId");
    __userId = GetQueryString("userId");
    __lang = GetQueryString("lang");
    __modify = GetQueryString("modify");
    __endMeter = GetQueryString("endMeter");
    if (typeof (__endMeter) != 'undefined' && __endMeter != null && __endMeter > 0) {
        __debug = '1';
    } else {
        __endMeter = 0;
        __debug = '0';
    }

    // 多语言初始化 静态页面
    var langFilePath = "../i18n/lang-zh.json";
    var opts = { language: "zh", pathPrefix: "../i18n", skipLanguage: "en-US" };
    if (__lang == "zh") {

    } else if (__lang == "en") {
        langFilePath = "../i18n/lang-en.json";
        opts = { language: "en", pathPrefix: "../i18n", skipLanguage: "en-US" };
    }

    $("[data-localize]").localize("lang", opts);

    $.ajax({
        type: "get",
        url: "../i18n/lang-zh.json",
        async: false,
        success: function (data) {
            __i18n = data;
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
        }
    });

    // Set up empty table with second and first columns pre-sorted
    $("#defectList").tablesorter({
        theme: 'default',
        sortList: [[8, 1], [9, 1], [10, 1], [11, 1], [12, 1], [13, 1], [14, 1], [15, 1], [16, 1], [17, 1], [3, 0], [4, 0]],
        textExtraction: function (node) {
            // extract data from markup and return it 
            if (node.innerHTML.indexOf("checkbox") > 0) {
                if ($(node.childNodes[0]).is(':checked')) {
                    return $(node.childNodes[0]).val();
                }
                return "0";
            } else if (node.innerHTML.indexOf("</a>") > 0) {
                return node.childNodes[0].innerHTML;
            } else {
                return node.innerHTML;
            }
        }
    });

    $("#dealDefectList").tablesorter({
        theme: 'default',
        sortList: [[8, 1], [9, 1], [10, 1], [11, 1], [12, 1], [13, 1], [14, 1], [15, 1], [16, 1], [17, 1], [3, 0], [4, 0]],
        textExtraction: function (node) {
            // extract data from markup and return it 
            if (node.innerHTML.indexOf("checkbox") > 0) {
                if ($(node.childNodes[0]).is(':checked')) {
                    return $(node.childNodes[0]).val();
                }
                return "0";
            } else if (node.innerHTML.indexOf("</a>") > 0) {
                return node.childNodes[0].innerHTML;
            } else {
                return node.innerHTML;
            }
        }
    });

    if (__orderId != null && __orderId.toString().length > 1) {
        // 首先从后台获取纸病数据
        var app = new ISRADefectInfoApp();
        app.GetDefectInfoList("0");
    } 
});
