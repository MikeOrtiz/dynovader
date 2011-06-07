<?php
include "authentication.php";
include "connect_ms.php";
$project = $_GET['projname'];
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
<title>myscience | science for everyone</title>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8">
<script type="text/javascript" src="http://ecn.dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=7.0"></script>
<link href="css/styles.css" rel="stylesheet" type="text/css" media="all" />

<script type="text/javascript">
var map;
function GetMap()
{   				
map = new Microsoft.Maps.Map(document.getElementById("mapDiv"), 
						   {credentials: "Amxt8FQT2902fydA-nj-9kWMB0Fo08H2Z-TFZ7ZxtEuJyEgmwMblyG2KHhqv8H6l",
						   center: new Microsoft.Maps.Location(37.434626, -122.19932),
						   setMapType: Microsoft.Maps.MapTypeId.birdseye,
							zoom: 11});
<? echo "updateMap(".$project.")"; ?>
}

var pinInfobox = null;
var currProject = null;
var allDataPoints = new Array;

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
			map.entities.pop();
			var mapData = new Microsoft.Maps.EntityCollection();
			if (projectData)
			{
				var pivot = projectData.search(/~/);
				currProject = projectData.substr(0, pivot);
				if (currProject.length > 25) currProject = currProject.substr(0, 25) + "...";
				projectData=projectData.substr(pivot + 1);
				while (true) 
				{
					if (projectData.search(/]/) == 0) break;
					
					//get gps info
					projectData=projectData.substr(1);
					var pivot;
					pivot = projectData.search(/~/);
					var location = projectData.substr(0, pivot);
					var values = location.split(",");
					var x = parseFloat(values[0]);
					var y = parseFloat(values[1]);
					projectData = projectData.substr(pivot + 1);
					
					//get datetime info
					pivot = projectData.search(/~/);
					var datetime = projectData.substr(0, pivot);
					projectData = projectData.substr(pivot + 1);
					
					//get highrespic info
					pivot = projectData.search(/~/);
					var highrespic = projectData.substr(0, pivot);
					projectData = projectData.substr(pivot + 1);
					
					//get lowrespic info
					pivot = projectData.search(/}/);
					var lowrespic = projectData.substr(0, pivot);
					projectData = projectData.substr(pivot + 1);
					
					//add pin data to map
					var locMS = new Microsoft.Maps.Location(x, y);
					var pin = new Microsoft.Maps.Pushpin(locMS);
					Microsoft.Maps.Events.addHandler(pin, 'mouseover', pinMouseOver);
					Microsoft.Maps.Events.addHandler(pin, 'mouseout', pinMouseOut);
					mapData.push(pin);
					
					var locStr = x.toFixed(2).toString() + ", " + y.toFixed(2).toString();
					var dataPoint = new Array(locMS, locStr, lowrespic, highrespic, datetime);
					allDataPoints.push(dataPoint);
				}
			}
			map.entities.push(mapData);
		}
	}
	xmlhttp.open("GET","getproject.php?q="+project,true);
	xmlhttp.send();
}

// Code for multiple info boxes borrowed largely from: http://blueric.wordpress.com/2011/03/10/creating-hover-style-info-boxes-on-the-bing-maps-ajax-v7-0-control/
// This function will create an infobox 
// and then display it for the pin that triggered the hover-event.
function displayInfobox(e) {
	// make sure we clear any infoBox timer that may still be active
	stopInfoboxTimer(e);

	// build or display the infoBox
	var pin = e.target;
	if (pin != null) {

		// Create the info box for the pushpin
		var location = pin.getLocation();
		
		//search allDataPoints array for data matching selected pin
		var pinData = new Array; // (location Obj, location string, lowrespic, highrespic, datetime)
		for (var i=0; i<allDataPoints.length; i++)
		{
			if (location == allDataPoints[i][0]) {
				pinData = allDataPoints[i];
				break;
			}
		}

		var options = {
			id: 'infoBox1',
			htmlContent: '<div class="infobox"><b class="infoboxTitle">'+currProject+'</b>'+
			'<div class="infoboxDescription"><a href="'+pinData[3]+'"><img src="'+pinData[2]+'"></a><br />'+pinData[4]+'<br />'+pinData[1]+'</div></div>',
			visible: true,
			showPointer: true,
			offset: new Microsoft.Maps.Point(0, pin.getHeight()),  
		};
		// destroy the existing infobox, if any
		// In testing, I discovered not doing this results in the mouseleave
		// and mouseenter events not working after hiding and then reshowing the infobox.
		if (pinInfobox != null) {
			map.entities.remove(pinInfobox);
			if (Microsoft.Maps.Events.hasHandler(pinInfobox, 'mouseleave'))
				Microsoft.Maps.Events.removeHandler(pinInfobox.mouseLeaveHandler);
			if (Microsoft.Maps.Events.hasHandler(pinInfobox, 'mouseenter'))
				Microsoft.Maps.Events.removeHandler(pinInfobox.mouseEnterHandler);
			pinInfobox = null;
		}
		// create the infobox
		pinInfobox = new Microsoft.Maps.Infobox(location, options);
		// hide infobox on mouseleave
		pinInfobox.mouseLeaveHandler 
			= Microsoft.Maps.Events.addHandler(pinInfobox, 'mouseleave', pinInfoboxMouseLeave);
		// stop the infobox hide timer on mouseenter
		pinInfobox.mouseEnterHandler 
			= Microsoft.Maps.Events.addHandler(pinInfobox, 'mouseenter', pinInfoboxMouseEnter);
		// add it to the map.
		map.entities.push(pinInfobox);
	}
}

function hideInfobox(e) {
	if (pinInfobox != null)
		pinInfobox.setOptions({ visible: false });
}

function closeQuestion(node) {
	node.parentNode.parentNode.parentNode.removeChild(node.parentNode.parentNode);
}

// This function starts a count-down timer that will hide the infoBox when it fires.
// This gives the user time to move the mouse over the infoBox, which disables the timer
// before it can fire, thus allowing clickable content in the infobox.
function startInfoboxTimer(e) {
	// start a count-down timer to hide the popup.
	// This gives the user time to mouse-over the popup to keep it open for clickable-content.
	if (pinInfobox.pinTimer != null) {
		clearTimeout(pinInfobox.pinTimer);
	}
	// give 300ms to get over the popup or it will disappear
	pinInfobox.pinTimer = setTimeout(timerTriggered, 300);
}

// Clear the infoBox timer, if set, to keep it from firing.
function stopInfoboxTimer(e) {
	if (pinInfobox != null && pinInfobox.pinTimer != null) {
		clearTimeout(pinInfobox.pinTimer);
	}
}

function mapViewChange(e) {
	stopInfoboxTimer(e);
	hideInfobox(e);
}
function pinMouseOver(e) {
	displayInfobox(e);
}
function pinMouseOut(e) {
	startInfoboxTimer(e);
}
function pinInfoboxMouseLeave(e) {
	hideInfobox(e);
}
function pinInfoboxMouseEnter(e) {
	// NOTE: This won't fire if showing infoBox ends up putting it under the current mouse pointer.
	stopInfoboxTimer(e);
}
function timerTriggered(e) {
	hideInfobox(e);
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
	top: 20px;
	font-family: "Century Gothic", Arial, sans-serif;
	color: #333;
	font-size: 12px;
}

h1{
	text-align: center;
}

.infobox
{
	background-color:#FFEBCD; 
	border-style:solid;
	border-width:medium; 
	border-color:DarkOrange; 
	-moz-border-radius: 15px;
	border-radius: 15px;
	min-height:130px; 
	position:absolute;
	top:0px; 
	left:23px; 
	width:160px;
	z-index: 999;
}

.infoboxTitle
{
	position:absolute; 
	top:10px; 
	text-align: center;
	width:160px;
}

.infoboxDescription
{
	position:absolute; 
	top:30px; 
	text-align: center;
	width:160px;
}

.floatright
{
	float: right;
}
		
</style>
</head>

<body onload="GetMap();">
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
	<h2><span>data</span> map view</h2>
   </div>
	<div class="content">
		<? //echo $selectProject ?>
		<div style="height:500px;">
		<div id='mapDiv' style="margin-top:70px; width:600px; height:500px;"></div>
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
