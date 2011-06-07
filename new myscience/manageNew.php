<?php
include "authentication.php";
$serverName = "tcp:mma1mtoeql.database.windows.net, 1433";
$connectionOptions = array("Database"=>"users",
         "UID"=>"dynovader@mma1mtoeql",
         "PWD" => "592Mayfield");
$conn = sqlsrv_connect($serverName, $connectionOptions);
if($conn === false)
{
      die(print_r(sqlsrv_errors(), true));
}
$formhtml = "";
$projname = "";
$numusers;
$numdata;
if(isset($_GET['projname'])) {

	$projectquery = "SELECT name from projects WHERE ID='".$_GET['projname']."'";
	$result = sqlsrv_query($conn,$projectquery);
	$arr = sqlsrv_fetch_array($result);
	$projname = $arr['name'];
	if(!isset($_GET['action'])){
				$formhtml = "<a href=\"?projname=".$_GET['projname']."&action=data\">Data</a>";
	}
	else{
	if($_GET['action']=="data"){
		$numdataquery = "SELECT COUNT(*) from data WHERE projectid='".$_GET['projname']."'";
		$result = sqlsrv_query($conn, $numdataquery);
		$row = sqlsrv_fetch_array($result);
		$numdata = $row[0];
		$numusersquery = "SELECT COUNT(DISTINCT userid) from data WHERE projectid='".$_GET['projname']."'";
		$result = sqlsrv_query($conn, $numusersquery);
		$row = sqlsrv_fetch_array($result);
		$numusers = $row[0];
		$dir = isset($_GET['dir'])?$_GET['dir']:"";
		$sort = isset($_GET['sort'])?$_GET['sort']:"";
		$url = "?projname=".$_GET['projname']."&action=data";
		$dataquery = "SELECT * from data WHERE projectid='".$_GET['projname']."'";
		if($sort){
			$dataquery .= " ORDER BY ";
			if($sort=="loc"){
				$dataquery .= "location";
			}
			else{
				$dataquery .= "time";
			}
			$dataquery .= " ".$dir;
		}
		//echo $dataquery;
		$result = sqlsrv_query($conn,$dataquery);
		$head=false;
		$timedir = ($sort=="time" && $dir=="asc")?"desc":"asc";
		$locdir = ($sort=="loc" && $dir=="asc")?"desc":"asc";
        $formhtml .="<table><tr><th><a href=\"".$url."&sort=time&dir=".$timedir."\">Time</a></th><th><a href=\"".$url."&sort=loc&dir=".$locdir."\">Location</a></th><th>Photo</th>";
		while($row = sqlsrv_fetch_array($result))
		{
		    /*
			$mapsurl = "http://maps.google.com/maps/api/staticmap?center=".$row['data']."&zoom=19&size=400x400&maptype=hybrid
	&markers=color:blue%7Clabel:Bunker%7C".$row['data']."&sensor=false";
	*/
			$processed = "[".str_replace("}{","},{",$row['data'])."]";
			$arr=json_decode($processed,true);
			if(!$head)
			{
			    $head=true;
				foreach($arr as $val){
				    $formhtml .= "<th>".$val['label']."</th>";
				}
				$formhtml .= "</tr>";
			}
			$formhtml .= "<tr>";
			$formhtml .="<td>".$row['time']->format('Y-m-d H:i:s')."</td><td>".$row['location']."</td><td>";
			if($row['lowrespic']!=""){
                            $formhtml.="<a href=\"".$row['picture']."\"> <img src=\"".$row['lowrespic']."\" width=\"50\"/> </a>";
			}
			$formhtml .= "</td>";
			foreach($arr as $val){
				$formhtml .= "<td>".$val['value']."</td>";
			}
            $formhtml.="</tr>";
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
	if($_GET['action']=="download"){
	$dataquery = "SELECT * from data WHERE projectid='".$_GET['projname']."'";
	$result = sqlsrv_query($conn,$dataquery);
	header("Content-type: application/csv");
	header("Content-Disposition: attachment; filename=$projname.csv");
	header("Pragma: no-cache");
	header("Expires: 0");
	$head=false;
	while($row = sqlsrv_fetch_array($result))
		{
		    $outputstr = "";
			$processed = "[".str_replace("}{","},{",$row['data'])."]";
			$arr=json_decode($processed,true);
			if(!$head){
			    $head = true;
				$outputstr = "Time, Latitude, Longitude, Picture";
				foreach($arr as $val){
					$outputstr .= ",".$val['label'];
				}
				$outputstr .= "\r\n";
			}
			$outputstr .= $row['time']->format('Y-m-d H:i:s').",";
			$outputstr .= $row['location'].",";
            $outputstr .= $row['picture'];
			foreach($arr as $val){
				$outputstr .= ",".$val['value'];
			}
			$outputstr .= "\r\n";
			echo $outputstr;
		}
		exit();
	}
	
	}
}
else
{
	$formhtml = "<form action='' method = 'GET'><input type='hidden' name='action' value='data'/><select name='projname'>";
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

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>myscience | science for everyone</title>
<link href="css/styles.css" rel="stylesheet" type="text/css" media="all" />

<style type="text/css">
table{
	border:1px solid #000;
	background-color:#fff;
	padding:1px;
	width:100%;
}

th{
	background:#4A4A4A; 
	border-left:1px solid #C7C7C7;
	color:#fff; 
	height:25px; 
}

td {
	border-top:0;
	border-left:0;
	border-right:0;
	border-bottom:1px solid #ccc; 
}

th a {
	color:#fff;
	text-decoration:underline;
}
.formbox {
	position: relative;
	top: 20px;
}
</style>
</head>

<body>
<div id="head">
 <div id="head_cen">
  <div id="head_sup" class="head_pad">
    <div class="logo"><a href="index.php"></a></div>
    <ul>
     <li><a href="index.php">HOME</a></li>
	 
     <li><a href="about.php">ABOUT</a></li>
	 <li><a class="active" href="manageNew.php">PROJECTS</a></li>
	 <? if($loggedin){ ?>
     <li><a href="launch.php">LAUNCH</a></li>
	 <li><a href="logout.php">LOGOUT</a></li>
	 <? } else { ?>
     <li><a href="register.php">LOGIN</a></li>
	 <? } ?>
   </ul>
  </div>
 </div>
</div>
<div id="content">
 <div id="content_cen">
  <div id="content_sup" class="head_pad">
   <div id="welcom_pan">
   <h2><? if(!isset($_GET['projname'])){
		echo "<span>SELECT</span> project";
	} else {
		echo "<span>MANAGE</span> project";
	}?></h2>
   </div>
	<div class="content">
	<div class="formbox">
	<h1 style="float:left">
	<? if(isset($_GET['action'])){
		if($_GET['action']=='data'){
			echo "Data for <i>".$projname."</i>:"; 
		}
		else{
			echo "Modify Layout for Project <i>".$projname."</i>";
		}
	}
	?> </h1>
	<? if(isset($_GET['action'])&&$_GET['action']=='data'){echo "<span style=\"float:right;margin-top:10px;\"><a href=\"manageNew.php?projname=".$_GET['projname']."&action=download\"><img src=\"images/disk.png\"/> download</a>&nbsp;&nbsp;&nbsp;<a href=\"visualizationNew.php?projname=".$_GET['projname']."\"><img src=\"images/map.png\"/> map</a></span>"; echo "<span style=\"float:right;margin-top:10px;\">submissions: ".$numdata." users:".$numusers."&nbsp;&nbsp;&nbsp;</span>";}?>
	<? echo $formhtml; ?>
  </div>
 </div>
</div>
</div>
</div>
<div id="foot">
 <div id="foot_cen">
    <p>© 2011 myscience</p>
 </div>
</div>
</body>
</html>
