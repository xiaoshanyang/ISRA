
var __selectRecordId = [];

$(document).ready(function () {
    // 拆分url
    // 调用方法
    var defectTime = GetQueryString("defectTime");
    var eventId = GetQueryString("eventId");
    var pfId = GetQueryString("pfId");
    var newRecord = GetQueryString("newRecord");
    var modify = GetQueryString("modify");

    //ajax 获取包含纸病时间的当前卷的停机记录
    if (defectTime != null || typeof defectTime != 'undefined') {
        //alert('afsasdasf');

        $.ajax({
            url: '/Services/GetStopCodeListHandler.ashx',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({ defectTime: defectTime, eventId: eventId, pfId: pfId, newRecord: newRecord==1?true:false }),
            success: function (data) {                
                // 根据状态显示页面 state:0 成功 state:1 无停机记录 state:2 查找停机记录失败
                showRecordList('#stopcodeList tbody', data);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
            }
        });
    } else {
        //alert('123');
    }

    onInit('#stopcodeList tbody');
    
});




//初始化事件
function onInit(stopRecordTable) {

    //checkbox 点击事件， 再次打开保存历史记录   
    var ckList = $(stopRecordTable).find(".cktr");

    for (var i = 0; i < ckList.length; i++) {
        if (parent.__selectStopRecord.indexOf($(ckList[i]).val())){
            $(ckList[i]).prop("checked", "checked");
            parent.__tmpStopRecordInfo.code = $(ckList[i]).parent().next().next().next().next().text() + '_' + $(ckList[i]).parent().next().next().next().next().next().text();
            parent.__tmpStopRecordInfo.startTime = $(ckList[i]).parent().next().next().text();
            parent.__tmpStopRecordInfo.endTime = $(ckList[i]).parent().next().next().next().text();
            parent.__tmpStopRecordInfo.remark = $(ckList[i]).parent().next().next().next().next().next().next().text();
        }
    }
    

    $("body").on("change", ".cktr", function () {
        if ($(this).is(':checked')) {
            //其他checkbox置为不可选中
            $(this).parent().parent().parent().find('input[type=checkbox]').not(this).attr("checked", false);

            parent.__selectStopRecord.push($(this).val());
            parent.__tmpStopRecordInfo.code = $(this).parent().next().next().next().next().text() + '_' + $(this).parent().next().next().next().next().next().text();
            parent.__tmpStopRecordInfo.startTime = $(this).parent().next().next().text();;
            parent.__tmpStopRecordInfo.endTime = $(this).parent().next().next().next().text();
            parent.__tmpStopRecordInfo.remark = $(this).parent().next().next().next().next().next().next().text();
        } else {
            parent.__selectStopRecord.splice(parent.__selectStopRecord.indexOf($(this).val()), 1);
            parent.__tmpStopRecordInfo.code = '';
            parent.__tmpStopRecordInfo.startTime = '';
            parent.__tmpStopRecordInfo.endTime = '';
            parent.__tmpStopRecordInfo.remark = '';
        }
        
    });
}

function showRecordList(stopRecordTable, data) {

    var node = "";
    if (data.state == 1) {
        node += "<tr><td colspan=\"10\">无停机记录</td></tr>"
    } else if (data.state == 2) {
        node += "<tr><td colspan=\"10\">查询停机记录失败</td></tr>"
    } else {
        // 根据返回值循环添加
        data.stopRecordList.forEach(function (info, index) {            
                node += "<tr><td><input class=\"cktr\" value=\"" + info.stoprecordId + "\" type=\"checkbox\"></td>"
                + "<td>" + info.RollNo + "</td><td>" + info.Start_Time + "</td><td>" + info.End_Time + "</td>"
                + "<td>" + info.Event_Reason_Code + "</td><td>" + info.Event_Reason_Name + "</td><td>" + info.Remark + "</td>"
                + "<td>" + info.Team + "</td><td>" + info.Shift + "</td><td>" + info.User_Desc + "</td></tr>";
        });
    }
    $(stopRecordTable).append(node);
}
