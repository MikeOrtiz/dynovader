<?php
include "connect_ms.php";
$formhtml = "";
$projname = "";
if(isset($_GET['projname']))
{
	$projectquery = "SELECT name from projects WHERE ID='".$_GET['projname']."'";
	$result = sqlsrv_query($conn,$projectquery);
	$arr = sqlsrv_fetch_array($result);
	$projname = $arr['name'];
	if(!isset($_GET['action'])){
				$formhtml = "<a href=\"manage.php?projname=".$_GET['projname']."&action=data\">Data</a><br/><a href=\"manage.php?projname=".$_GET['projname']."&action=modify\">Layout</a>";
	}
	else{
	if($_GET['action']=="data"){
		$dataquery = "SELECT * from data WHERE projectid='".$_GET['projname']."'";
		$result = sqlsrv_query($conn,$dataquery);
        $formhtml .="<table><tr><td>Time</td><td>Location</td><td>Data</td></tr>";
		while($row = sqlsrv_fetch_array($result))
		{
		    /*
			$mapsurl = "http://maps.google.com/maps/api/staticmap?center=".$row['data']."&zoom=19&size=400x400&maptype=hybrid
	&markers=color:blue%7Clabel:Bunker%7C".$row['data']."&sensor=false";
	*/
			$processed = "[".str_replace("}{","},{",$row['data'])."]";
			$arr=json_decode($processed,true);
			$formhtml .= "<tr>";
			$formhtml .="<td>".$row['time']->format('Y-m-d H:i:s')."</td><td>".$row['location']."</td><td>";
			foreach($arr as $val){
				$formhtml .= "[".$val['label'].":".$val['value']."] ";
			}
			$formhtml .= "</td></tr>";
		}
		$formhtml .= "</table><br/>";
	}
	if($_GET['action']=="modify"){
		$formhtml = "<ul id=\"sortable\">
	<li class=\"ui-state-default\"><span class=\"ui-icon ui-icon-arrowthick-2-n-s\"></span>Photo</li>
	<li class=\"ui-state-default\"><span class=\"ui-icon ui-icon-arrowthick-2-n-s\"></span>Text Field 1</li>
	<li class=\"ui-state-default\"><span class=\"ui-icon ui-icon-arrowthick-2-n-s\"></span>Text Field 2</li>
	<li class=\"ui-state-default\"><span class=\"ui-icon ui-icon-arrowthick-2-n-s\"></span>Text Field 3</li>
</ul><br/><button>Submit Changes</button>";
	}
	}
}
else
{
	$formhtml = "<form action='manage.php' method = 'GET'><input type='hidden' name='action' value='data'/><select name='projname'>";
	$list = array();
	$query = "SELECT projects.name as projname, projects.id as projid, coordinators.name as coordname FROM projects, coordinators WHERE projects.owner = coordinators.id";
	$result = sqlsrv_query($conn,$query);
	if(sqlsrv_has_rows($result))
	{
		while($row = sqlsrv_fetch_array($result))
		{
			$formhtml .= "<option value=".$row['projid'].">".$row['projname']." (".$row['coordname'].")</option>";
		}
	}
	$formhtml .= "</select><input type='submit' value='Submit'/></form>";
}
?>
<html>
<head>
<link rel="stylesheet" href="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.10/themes/base/jquery-ui.css">
<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.5.1/jquery.min.js"></script>
<script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jqueryui/1.8.10/jquery-ui.min.js"></script>
<script type="text/javascript" src="http://jqueryui.com/ui/jquery.ui.sortable.js"></script>

<script type="text/javascript">
$(function() {
		$( "#sortable" ).sortable();
		$( "#sortable" ).disableSelection();
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
	width:800px;
	padding:50px;
	margin:auto;
	text-align:center;
	height:500px;
}

table{
	width:100%;
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
#sortable { list-style-type: none; margin: 0; padding: 0; width: 100%; }
	#sortable li { margin: 0 3px 3px 3px; padding: 0.4em; padding-left: 1.5em; font-size: 1.4em; height: 18px; }
	#sortable li span { position: absolute; margin-left: -1.3em; }
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
		<a href="manage.php" class="special-anchor">MANAGE PROJECT</a>
	</li>
</ul>
</div>
<div class="content">
<div class="formbox">
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<h1><? 
	if(!isset($_GET['projname'])){
		echo "Choose a Project";
	}
	else if(isset($_GET['action'])){
		if($_GET['action']=='data'){
			echo "Data for Project <i>".$projname."</i>";
		}
		else{
			echo "Modify Layout for Project <i>".$projname."</i>";
		}
	}
?>	
</h1><br/>
<? echo $formhtml; ?>
</div>
</div>
</div>
</body>
</html>