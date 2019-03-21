<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DefectPage.aspx.cs" Inherits="ISRADefectImageShow.DefectPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ISRA纸病处理</title>
    <link rel="stylesheet" type="text/css" href="CSS/style.css" />
    <script src="JS/jquery-1.11.3.min.js" type="text/javascript"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h5>纸病记录</h5>
        <div>
            <ul class="opt">
                <li><input type="button" value="增加" id="Add"/></li>
                <li><input type="button" value="修改" id="Modify"/></li>
                <li><input type="button" value="删除" id="Delete"/></li>
            </ul>
        </div>
        <div>



            <table id="defectList">
                <thead>
                    <tr>
                        <th></th>
                        <th>下卷卷号</th>
                        <th>缺陷图片</th>
                        <th>开始米数</th>
                        <th>结束米数</th>
                        <th>纸病长度</th>
                        <th>纸病代码</th>
                        <th>代码描述</th>
                        <th>处理工序</th>
                        <th colspan="9">幅号</th>
                        <th>备注</th>
                    </tr>
                </thead>
                
            </table>
        </div>
    </div>
    <div>
        <table id="dealDefectList">
            <thead>
                <tr>
                    <th>下卷卷号</th>
                    <th>缺陷图片</th>
                    <th>开始米数</th>
                    <th>结束米数</th>
                    <th>纸病长度</th>
                    <th>纸病代码</th>
                    <th>代码描述</th>
                    <th>处理工序</th>
                    <th colspan="8">幅号</th>
                    <th>备注</th>
                </tr>
            </thead>
            <tbody></tbody>
        </table>
    </div>
    <div id="domMessage" style="display:none"></div>
    </form>
</body>
</html>
