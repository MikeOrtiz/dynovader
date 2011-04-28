<?php
include "connect_ms.php";
$msg = "";
if(isset($_POST['coordname']))
{
	if($_POST['coordname']==""){
	}
	else{
		$query = "SELECT * FROM coordinators WHERE email='".$_POST['coordemail']."'";
		$result = sqlsrv_query($conn, $query);
		$coordid;
		if(sqlsrv_has_rows($result)){
			$arr = sqlsrv_fetch_array($result);
			$coordid = $arr['ID'];
		}
		else {
			$query = "INSERT INTO coordinators(name, email) VALUES('".$_POST['coordname']."', '".$_POST['coordemail']."')";
			$result = sqlsrv_query($conn, $query);
			//echo $query."<br/>";
			$query = "SELECT * FROM coordinators WHERE email='".$_POST['coordemail']."'";
			$result = sqlsrv_query($conn, $query);
			$arr = sqlsrv_fetch_array($result);
			$coordid = $arr['ID'];
		}
		$query = "SELECT * FROM projects WHERE name='".$_POST['appname']."' AND owner = $coordid";
		$result = sqlsrv_query($conn, $query);
		//echo sqlsrv_num_rows($result)."<br/>";
		if(sqlsrv_has_rows($result)){
		     
			//return error, since it exists already
		}
		else{
		    $values = "[";
			/*if($_POST['photo']=="on"){
				$values .= "('".$_POST['photoinstructions']."',2,".$projid."),";
			}*/
			if($_POST['text1instructions']!=""){
				$values .= "{\"type\":\"Question\",\"label\":\"".$_POST['text1instructions']."\"}";
			}
			if($_POST['text2instructions']!=""){
				$values .= ",{\"type\":\"Question\",\"label\":\"".$_POST['text2instructions']."\"}";
			}
			if($_POST['text3instructions']!=""){
				$values .= ",{\"type\":\"Question\",\"label\":\"".$_POST['text3instructions']."\"}";
			}
			$values .= "]";
			$query = "INSERT INTO projects(name, description, owner, form) VALUES('".$_POST['appname']."','".$_POST['description']."',$coordid,'".$values."')";
			$result = sqlsrv_query($conn, $query);
			//echo $query;
			echo "Your project <i>".$_POST['appname']."</i> was added successfully!";
		}
	}
}
?>
<html>
<head>
<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.10/jquery-ui.min.js"></script>
<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.5.1/jquery.min.js"></script>
<script type="text/javascript">
$(document).ready(function() {
  $("#text1").change(function() {
    if($("#text1").is(":checked")){
		$("#tf2").show();
	}
	else{
		$("#tf2").hide();
	}
  });
  $("#text2").change(function() {
    if($("#text2").is(":checked")){
		$("#tf3").show();
		$("#text1").attr("disabled","true");
	}
	else{
		$("#tf3").hide();
		$("#text1").attr("disabled","");
	}
  });
   $("#text3").change(function() {
    if($("#text3").is(":checked")){
		$("#tf4").show();
		$("#text2").attr("disabled","true");
	}
	else{
		$("#tf4").hide();
		$("#text2").attr("disabled","");
	}
  });
});
</script>
<style type="text/css">
.top-menu {
	position: relative;
	list-style: none;
	float: right;
	font: 14px "Century Gothic", AppleGothic, Arial, sans-serif;
	z-index: 1;
}

.top-menu li {
position: relative;
float: left;
}
.top-menu li a {
	display: block;
	padding: 10px;
	text-decoration: none;
	color: #ccc;
	outline: 0;
}
.top-menu li a: hover {    
	color: #000;
}
.top-menu li.selected a {
	font-weight: bold; 
	color: #000;
	margin-top: 1px;
}
.columnleft{
	width:50%;
	float:left;
	height:30px;
	text-align:left;
}
.columnright{
	width:50%;
	height:30px;
	float:right;
	text-align:left;
}
.formbox{
	width:400px;
	padding:50px;
	margin:auto;
	height:500px;
}
.content{
	margin:auto;
	font-family: "Century Gothic", Arial, sans-serif;
	color: #333;
	font-size: 12px;
}
.wrapper{
	height:768px;
	width:1024px;
	margin:auto;
}
#tf2{
	display:none;
}
#tf3{
	display:none;
}
#tf4{
	display:none;
}
body{
	margin:0;
	padding:0;
	text-align:center;
}
</style>
</head>
<body>
<div class="wrapper">
<div>
<ul class="top-menu">
	<li>
		<a href="index.php" class="special-anchor">HOME</a>
	</li>
	<li class="selected">
		<a href="admin.php" class="special-anchor">LAUNCH A PROJECT</a>
	</li>
	<li>
		<a href="manage.php" class="special-anchor">MANAGE PROJECT</a>
	</li>
</ul>
</div>
<div class="content">
<div class="formbox">
<div><?=$msg?></div>
<h1>Launch a Project</h1>
<form action="admin.php" method="POST">
<span class="columnleft">Project Name: </span><span class="columnright"><input type="text" name="appname"/></span><br/>
<span class="columnleft">Description: </span><span class="columnright"><input type="text" name="description"/></span><br/>
<span class="columnleft">GPS: </span><span class="columnright"><input type="checkbox" name="gps"/></span><br/>
<span class="columnleft">Photo: </span><span class="columnright"><input type="checkbox" name="photo"/></span><br/>
<span class="columnleft">Photo Instructions: </span><span class="columnright"><input type="text" name="photoinstructions"/></span><br/>
<span class="columnleft">Text Field 1: </span><span class="columnright"><input id="text1" type="checkbox" name="textfield1"/></span><br/>
<div id="tf2">
<span class="columnleft">Text Field 1 Instructions: </span><span class="columnright"><input type="text" name="text1instructions"/></span><br/>
<span class="columnleft">Text Field 2: </span><span class="columnright"><input id="text2" type="checkbox" name="textfield2"/></span><br/>
</div>
<div id="tf3">
<span class="columnleft">Text Field 2 Instructions: </span><span class="columnright"><input type="text" name="text2instructions"/></span><br/>
<span class="columnleft">Text Field 3: </span><span class="columnright"><input id="text3" type="checkbox" name="textfield3"/></span><br/>
</div>
<div id="tf4">
<span class="columnleft">Text Field 3 Instructions: </span><span class="columnright"><input type="text" name="text3instructions"/></span><br/>
</div>
<span class="columnleft">Coordinator Name: </span><span class="columnright"><input type="text" name="coordname"/></span><br/>
<span class="columnleft">Coordinator Email: </span><span class="columnright"><input type="text" name="coordemail"/></span><br/>
<span class="columnright"><input type="submit" value="submit"/></span>
</form>
</div>
</div>
</div>
</body>
</html>