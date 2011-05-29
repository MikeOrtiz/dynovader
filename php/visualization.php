<?php
	include "connect_ms.php";
	$selectProject = "";
	$projname = "";

	$selectProject = "<select name='projname' id='selectedProject' onchange='updateMap(this.value)'>";
	$selectProject .= "<option value=''>Select Project:</option>";
	$query = "SELECT projects.name as projname, projects.id as projid, coordinators.name as coordname FROM projects, coordinators WHERE projects.owner = coordinators.id AND projects.status = 'active'";
	$result = sqlsrv_query($conn,$query);
	if(sqlsrv_has_rows($result))
	{
		while($row = sqlsrv_fetch_array($result))
		{
			$selectProject .= "<option value=".$row['projid'].">".$row['projname']." (".$row['coordname'].")</option>";
		}
	}
	$selectProject .= "</select>";
?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
<title></title>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<script type="text/javascript" src="http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0"></script>

<script type="text/javascript">
var map;
function GetMap()
{   				
map = new Microsoft.Maps.Map(document.getElementById("mapDiv"), 
						   {credentials: "Amxt8FQT2902fydA-nj-9kWMB0Fo08H2Z-TFZ7ZxtEuJyEgmwMblyG2KHhqv8H6l",
						   center: new Microsoft.Maps.Location(37.464626, -120.62932),
						   setMapType: Microsoft.Maps.MapTypeId.birdseye,
							zoom: 4});
}

var pinInfobox = null;
var pictureaddr = null;

function updateMap(project)
{
	var projectData;
	if (project=="") return;
	if (window.XMLHttpRequest)
		{// code for IE7+, Firefox, Chrome, Opera, Safari
		xmlhttp=new XMLHttpRequest();
	} else {// code for IE6, IE5
		xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
	}
	
	xmlhttp.onreadystatechange=function()
	{
		if (xmlhttp.readyState==4 && xmlhttp.status==200)
		{
			projectData=xmlhttp.responseText;

			map.entities.pop();
			var mapData = new Microsoft.Maps.EntityCollection();
			if (projectData)
			{
				projectData=projectData.substr(1);
				while (true) 
				{
					var debugData;
					if (projectData.search(/]/) == 0) break;
					projectData=projectData.substr(1);
					var pivot = projectData.search(/~/);
					var location = projectData.substr(0, pivot);
					var values = location.split(",");
					var x = parseFloat(values[0]);
					var y = parseFloat(values[1]);
					projectData = projectData.substr(pivot + 1);
					pivot = projectData.search(/}/);
					pictureaddr = projectData.substr(0, pivot);
					projectData = projectData.substr(pivot + 1);
					var pin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(x, y));
					mapData.push(pin);
					pinInfobox = new Microsoft.Maps.Infobox(pin.getLocation(), 
						{ 
						visible: false, 
						offset: new Microsoft.Maps.Point(0,15)});
					pinInfobox.setHtmlContent('<div class="infobox"><b class="infoboxTitle">myTitle</b>'+
					'<a class="infoboxDescription"><img src="'+pictureaddr+'"></a></div>');
					// Add handler for the pushpin click event.
					Microsoft.Maps.Events.addHandler(pin, 'click', displayInfobox);
					Microsoft.Maps.Events.addHandler(map, 'viewchange', hideInfobox);
					mapData.push(pinInfobox);
				}
			}
			map.entities.push(mapData);
		}
	}
	xmlhttp.open("GET","getproject.php?q="+project,true);
	xmlhttp.send();
}

function displayInfobox(e)
{
	pinInfobox.setOptions({ visible:true });
}   

function hideInfobox(e)
{
	pinInfobox.setOptions({ visible:false });
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
	margin-top: -18px;
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

.infobox
{
	background-color:White; 
	border-style:solid;
	border-width:medium; 
	border-color:DarkOrange; 
	min-height:100px; 
	position:absolute;
	top:0px; 
	left:23px; 
	width:240px;
}

.infoboxTitle
{
	position:absolute; 
	top:10px; 
	left:10px; 
	width:220px;
}

.infoboxDescription
{
	position:absolute; 
	top:30px; 
	left:10px; 
	width:220px;
}
		
</style>
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