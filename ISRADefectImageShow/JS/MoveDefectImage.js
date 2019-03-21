

$(document).ready(function () {
    // 提交
    $("#submit").click(function () {
        // 判断参数是否合法
        var _pathId = $("#pathId").val();
        var _puId = $("#puId").val();
        var _startDate = $("#startDate").val();
        var _changefilename = $("#changefilename").is(':checked')?1:0;

        if (_pathId == null) {
            return alert("pathId 输入错误");
        }
        if (_puId == null) {
            return alert("puId 输入错误");
        }
        if (_startDate == null) {
            return alert("startDate 输入错误");
        }
        console.log(123224);
        var _data = { puId: _puId, pathId: _pathId, startDate: _startDate, changefilename: _changefilename };
        // 手动转移图片
        $.ajax({
            url: '/Services/MoveDefectImageToServerHandler.ashx',
            type: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(_data),
            success: function (data) {
                console.log(data);
                if (data.state == 1) {
                    alert("拷贝完成");
                } else {
                    console.log(data.message);
                    alert("图片移动失败");
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
            }
        });

            
        
    });
});