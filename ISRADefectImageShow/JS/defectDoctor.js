
$(document).ready(function () {

    // 拆分url
    // http://localhost:65486/DefectImageByDoctor.html?pfId=1574167&lang=zh
    // 调用方法
    var __pfId = GetQueryString("pfId");
    var __domMessage = $('#domMessage');
    var __lang = GetQueryString("lang");
    var __i18n ;

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

    // 
    // 图片点击看大图
    $("body").on("click", ".pimg", function () {
        var _this = $(this);//将当前的pimg元素作为_this传入函数  
        imgShow("#outerdiv", "#innerdiv", "#bigimg", _this);
    });
    $("#Close").click(function () {
        // 关闭是调用 winform 关闭按钮
        window.external.CloseForm();
    });

    
    
    //get 请求获取数据
    $.blockUI({ message: __domMessage });
    __domMessage.text(__i18n.getDefectInfo);//__i18n.getDefectInfo

    var _data = {
        pfId: __pfId
    };

    $.ajax({
        url: '/Services/GetDefectImageByPfIDHandler.ashx',
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(_data),
        success: function (data) {
            $.unblockUI();
            // 在界面显示数据，两部分
            // 根据状态显示页面 state:1 成功 state:2 失败
            if (data.state == 1) {
                // 更新页面信息                
                $("#sourceImg").attr("src",data.sourceImg);
                $("#defectImg").attr("src",data.defectImg);
                $("#dLength").text(data.dLength);
                $("#dWidth").text(data.dWidth);
                $("#dArea").text(data.dArea);
            } else {
                //__domMessage.text(__i18n.getDefectInfoFail);//__i18n.getDefectInfoFail
                alert(__i18n.getDefectInfoFail);
            }

        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            
            //__domMessage.text(__i18n.getDefectInfoFail);//__i18n.getDefectInfoFail
            alert(__i18n.getDefectInfoFail);
            $.unblockUI();
            console.log("XMLHttpRequest : " + XMLHttpRequest.status + ", textStatus : " + textStatus + ", errorThrown : " + errorThrown.message);
        }
    });

});