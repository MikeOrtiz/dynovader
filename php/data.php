<?php
include "connect.php";
$formhtml = "";
$projname = "";
if(isset($_POST['projname']))
{
	$projectquery = "SELECT name from projects WHERE id='".$_POST['projname']."'";
	$result = mysql_query($projectquery);
	$arr = mysql_fetch_array($result);
	$projname = $arr['name'];
	$fieldsquery = "SELECT * from projectfields WHERE projectid='".$_POST['projname']."'";
	$result = mysql_query($fieldsquery);
	
	while($row = mysql_fetch_array($result))
	{
		$formhtml .= "<span>".$row['name']."</span>";
	}
	$formhtml .= "<br/>";
}
else
{
	$formhtml = "<form action='data.php' method = 'POST'><select name='projname'>";
	$list = array();
	$query = "SELECT projects.name as projname, projects.id as projid, coordinators.name as coordname FROM projects, coordinators WHERE projects.owner = coordinators.id";
	$result = mysql_query($query);
	if(mysql_num_rows($result)>0)
	{
		while($row = mysql_fetch_array($result))
		{
			$formhtml .= "<option value=".$row['projid'].">".$row['projname']." (".$row['coordname'].")</option>";
		}
	}
	$formhtml .= "</select><input type='submit' value='Submit'/></form>";
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
	<li>
		<a href="admin.php" class="special-anchor">LAUNCH A PROJECT</a>
	</li>
	<li class="selected">
		<a href="data.php" class="special-anchor">DATA</a>
	</li>
</ul>
</div>
<div class="content">
<div class="formbox">
<h1>
<? 
	if(!isset($_POST['projname'])){
		echo "Choose a Project";
	}
	else{
		echo "Data for Project <i>".$projname."</i>";
	}
?>	
</h1>
<? echo $formhtml; ?>
</div>
</div>
</div>
</body>
</html>