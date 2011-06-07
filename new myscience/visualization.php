<?php
	include "connect_ms.php";
	$selectProject = "";
	$projname = "";

	$selectProject = "<select name='projname' id='selectedProject'>";
	$query = "SELECT projects.name as projname, projects.id as projid, coordinators.name as coordname FROM projects, coordinators WHERE projects.owner = coordinators.id AND projects.status = 'active'";
	$result = sqlsrv_query($conn,$query);
	if(sqlsrv_has_rows($result))
	{
		while($row = sqlsrv_fetch_array($result))
		{
			$selectProject .= "<option value=".$row['projid'].">".$row['projname']." (".$row['coordname'].")</option>";
		}
	}
	$selectProject .= "</select><input type='button' value='Update Map' onclick='updateMap()'/>";
?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
   <head>
      <title></title>
      <meta http-equiv="Content-Type" content="text/html; charset=utf-8">

      <script type="text/javascript" src="http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0"></script>

	  <script type="text/javascript">
	  
	  var map = null;
	  
	  function GetMap()
	  {   
		 map = new Microsoft.Maps.Map(document.getElementById("mapDiv"), 
						   {credentials: "Amxt8FQT2902fydA-nj-9kWMB0Fo08H2Z-TFZ7ZxtEuJyEgmwMblyG2KHhqv8H6l",
						   center: new Microsoft.Maps.Location(37.464626, -120.62932),
						   setMapType: Microsoft.Maps.MapTypeId.birdseye,
							zoom: 4});
	  }
	  
	  function findSelected(select)
	  {
		var value = document.getElementById(select).options[document.getElementById(select).selectedIndex].value;
		return value;
	  }
	  
	  function updateMap()
	  {
		map.entities.pop();
		var project = findSelected('selectedProject');
		var mapData = new Microsoft.Maps.EntityCollection();
		<?php
			$query = "SELECT data.location as location, data.time as time FROM data";
			$result = sqlsrv_query($conn,$query);
			if(sqlsrv_has_rows($result))
			{
				while($row = sqlsrv_fetch_array($result))
				{
					?>
					mapData.push(new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(<?echo($row['location'])?>)));
					<?
					
				}
			}
		?>
		 map.entities.push(mapData);
	  }
	  </script>

<style type="text/css" media="screen">
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

.wrapper{
	height:768px;
	width:1024px;
	margin:auto;
}

body{
	margin:0;
	padding:0;
}

.content{
	margin:auto;
	position: relative;
	top: 50px;
	font-family: "Century Gothic", Arial, sans-serif;
	color: #333;
	font-size: 12px;
}

h1{
	text-align: center;
}
		
</style>
<? include 'analytics.php' ?>
</head>
<body onload="GetMap();">
<div class="wrapper">
	<div>
		<ul class="top-menu">
			<li><a href="index.php" class="special-anchor">HOME</a></li>
			<li><a href="admin.php" class="special-anchor">LAUNCH A PROJECT</a></li>
			<li><a href="manage.php" class="special-anchor">MANAGE PROJECT</a></li>
			<li class="selected"><a href="visualization.php" class="special-anchor">VISUALIZATION</a></li>
		</ul>
	</div>
	<div class=content>
		<h1>Data Visualization</h1>
		<? echo $selectProject ?>
		<div id='mapDiv' style="position:relative; width:600px; height:500px;"></div>
	</div>   
</div>
</body>
</html>