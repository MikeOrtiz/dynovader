<?php
include "connect.php";
if(isset($_POST['coordname']))
{
	if($_POST['coordname']==""){
	}
	else{
		$query = "SELECT * FROM coordinators WHERE email='".$_POST['coordemail']."'";
		$result = mysql_query($query);
		$coordid;
		if(mysql_num_rows($result) > 0){
			$arr = mysql_fetch_array($result);
			$coordid = $arr['id'];
		}
		else {
			$query = "INSERT INTO coordinators(name, email) VALUES('".$_POST['coordname']."', '".$_POST['coordemail']."')";
			$result = mysql_query($query);
			$coordid = mysql_insert_id();
		}
		$query = "SELECT * FROM projects WHERE name='".$_POST['appname']."' AND owner = $coordid";
		$result = mysql_query($query);
		if(mysql_num_rows($result) > 0){
			//return error, since it exists already
		}
		else{
			$query = "INSERT INTO projects(name, owner) VALUES('".$_POST['appname']."',$coordid)";
			$result = mysql_query($query);
		}
	}
	
}
?>
<html>
<head>
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
	padding:10px;
	margin:auto;
	height:300px;
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
		<a href="data.php" class="special-anchor">DATA</a>
	</li>
</ul>
</div>
<div class="content">
<div class="formbox">
<h1>Launch a Project</h1>
<form action="admin.php" method="POST">
<span class="columnleft">Project Name: </span><span class="columnright"><input type="text" name="appname"/></span><br/>
<span class="columnleft">GPS: </span><span class="columnright"><input type="checkbox" name="gps"/></span><br/>
<span class="columnleft">Text Field: </span><span class="columnright"><input type="checkbox" name="textfield"/></span><br/>
<span class="columnleft">Photo: </span><span class="columnright"><input type="checkbox" name="photo"/></span><br/>
<span class="columnleft">Coordinator Name: </span><span class="columnright"><input type="text" name="coordname"/></span><br/>
<span class="columnleft">Coordinator Email: </span><span class="columnright"><input type="text" name="coordemail"/></span><br/>
<span class="columnright"><input type="submit" value="submit"/></span>
</form>
</div>
</div>
</div>
</body>
</html>