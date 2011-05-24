<?php 
include "connect_ms.php";
session_start();
$logmsg = "";
$regmsg = "";
$loggedin= false;
if(isset($_GET['action'])){
if($_GET['action']=="login"){
	$query = "SELECT * FROM coordinators WHERE email='".$_POST['coordemail']."' AND password='".$_POST['coordpass']."'";
	$result = sqlsrv_query($conn, $query);
	if(sqlsrv_has_rows($result)){
		$arr = sqlsrv_fetch_array($result);
		$_SESSION['coordid'] = $arr['ID'];
		header("Location:admin.php");
	}
	else{
		$logmsg = "Email/password combination not found.  Please try again.";
	}
}
else if($_GET['action']=="register"){
	$query = "SELECT * FROM coordinators WHERE email='".$_POST['coordemail']."'";
	$result = sqlsrv_query($conn, $query);
	if(sqlsrv_has_rows($result)){
		$regmsg = "This email is already in use.  Try logging in.";
	}
	else{
		$query = "INSERT INTO coordinators(name, email, password) VALUES('".$_POST['coordname']."', '".$_POST['coordemail']."', '".$_POST['coordpass']."')";
		$result = sqlsrv_query($conn, $query);
		$query = "SELECT * FROM coordinators WHERE email='".$_POST['coordemail']."' AND password='".$_POST['coordpass']."'";
		$result = sqlsrv_query($conn, $query);
		if(sqlsrv_has_rows($result)){
			$arr = sqlsrv_fetch_array($result);
			$_SESSION['coordid'] = $arr['ID'];
			header("Location:admin.php");
		}
	}
}
}
?>
<!DOCTYPE HTML>
<html>
<head>
<link type="text/css" href="css/cupertino/jquery-ui-1.8.13.custom.css" rel="Stylesheet" />
<link rel="stylesheet" media="screen" type="text/css" href="css/colorpicker.css" />
<script type="text/javascript" src="js/jquery-1.6.1.min.js"></script>
<script type="text/javascript" src="js/jquery-ui-1.8.13.custom.min.js"></script>
<script type="text/javascript" src="js/colorpicker.js"></script>
<script type="text/javascript" src="js/eye.js"></script> 
<script type="text/javascript" src="js/utils.js"></script> 
<script type="text/javascript" src="js/layout.js?ver=1.0.2"></script>
<script>

//displays successful submission message
if ('<?echo $msg?>') alert('<?echo $msg?>');

$(function() {
		$( "#sortable" ).sortable();
		$( "#sortable" ).disableSelection();
});

$(document).ready(function(){

$('#backColInput').ColorPicker({
	onChange: function(hsb, hex, rgb, el) {
		$(el).val(hex);
		var color = "#" + hex;
		document.getElementById('phonecontent').style.backgroundColor = color;
		document.getElementById('backColInput').value=hex;
	},
	onSubmit: function(hsb, hex, rgb, el) {
		$(el).val(hex);
		$(el).ColorPickerHide();
	},
	onBeforeShow: function () {
		$(this).ColorPickerSetColor(this.value);
	}
})
.bind('keyup', function(){
	$(this).ColorPickerSetColor(this.value);
});

$('#fontColInput').ColorPicker({
	onChange: function(hsb, hex, rgb, el) {
		$(el).val(hex);
		var color = "#" + hex;
		var inputs = getElementsByClassName('blend');
		for (var i=0; i < inputs.length; i++) {
			inputs[i].style.color=color;
		}
		document.getElementById('apptitle').style.color=color;
		document.getElementById('fontColInput').value=hex;
	},
	onSubmit: function(hsb, hex, rgb, el) {
		$(el).val(hex);
		$(el).ColorPickerHide();
	},
	onBeforeShow: function () {
		$(this).ColorPickerSetColor(this.value);
	}
})
.bind('keyup', function(){
	$(this).ColorPickerSetColor(this.value);
});

});


getElementsByClassName = function(clsName){
	if (navigator.appName == 'Microsoft Internet Explorer') {
		var retVal = new Array();
		var elements = document.getElementsByTagName("*");
		for(var i = 0;i < elements.length;i++){
			if(elements[i].className.indexOf(" ") >= 0){
				var classes = elements[i].className.split(" ");
				for(var j = 0;j < classes.length;j++){
					if(classes[j] == clsName)
						retVal.push(elements[i]);
				}
			}
			else if(elements[i].className == clsName)
				retVal.push(elements[i]);
		}
		return retVal;
	} else {
		return document.getElementsByClassName(clsName);
	}
}

//form extension based off sample code at Quirksmode: http://www.quirksmode.org/dom/domform.html
var counter = 0;

function moreText() {
	moreFields('textroot');
}


function moreCheck() {
	moreFields('checkroot');
}

function moreRadio() {
	moreFields('radioroot');
}

function morePicture() {
	moreFields('pictureroot');
}

function killTheUnwantedChildren(newField, num) {
	for (i=newField.length-1; i>num; i--) {
		newField[i].parentNode.removeChild(newField[i]);
	}
}

function moreFields(qtype) {
	counter++;
	var newFields = document.getElementById(qtype).cloneNode(true);
	newFields.id = '';
	newFields.style.display = 'block';
	var newField = newFields.childNodes;
	var numOptions;
	if (qtype == 'checkroot') {
		numOptions = document.getElementById('numchecks').options[document.getElementById('numchecks').selectedIndex].value;
	}
	if (qtype == 'radioroot') {
		numOptions = document.getElementById('numradios').options[document.getElementById('numradios').selectedIndex].value;
	}
	var removeChildren = false;
	
	for (var i=0;i<newField.length;i++) {
		var theName = newField[i].name
		if (removeChildren) { //Case: past the last option
			killTheUnwantedChildren(newField, i);
			break;
		} else { 
			if (theName && numOptions) {
				if (theName.charAt(theName.length-1) == numOptions.charAt(0) && theName.charAt(theName.length-2) == 'A') { //Case: the last option
					newField[i].name = counter + ' ' + theName + ' END';
					removeChildren = true;
				} else { //Case: before the last option
					newField[i].name = counter + ' ' + theName; 
				}
			} else { //text question
				if (newField[i].name != null)
					newField[i].name = counter + ' ' + theName;
			}
		}
	}
	
	var insertHere = document.getElementById('writeroot');
	insertHere.parentNode.insertBefore(newFields,insertHere);
}

function starterFields() {
	moreText();
	moreCheck();
}

function closeQuestion(node) {
	node.parentNode.parentNode.parentNode.removeChild(node.parentNode.parentNode);
}

function revealButtons(node) {
	node.children[0].style.display='block';
	node.children[3].style.display='block';
}

function hideButtons(node) {
	node.children[0].style.display='none';
	node.children[3].style.display='none';
}

window.onload = starterFields;

//Updates Apptitle on phone on keypress
function updateTitle(){
	var userInput = document.getElementById('titleinput').value;
	document.getElementById('apptitle').innerHTML = userInput;
}

var photoOption;

//Updates whether user wants photoOption set to 'Y' or 'N'
function updatePhotoOption(){
	var x = document.getElementById('picturequestion');
	if (x.value == 'Add Picture Capture') {
		x.value = 'Remove Picture Capture';
		document.getElementById('photobuttons').style.display = 'block';
		photoOption = 'Y';
	} else {
		x.value = 'Add Picture Capture';
		document.getElementById('photobuttons').style.display = 'none';
		photoOption = 'N';
	}
	document.getElementById('photopost').value = photoOption;
}

function disableEnterKey(e)
{
     var key;      
     if(window.event)
          key = window.event.keyCode; //IE
     else
          key = e.which; //firefox      

     return (key != 13);
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

body{
	margin:0;
	padding:0;
}

#content{
	margin:auto;
	top: 50px;
	font-family: "Century Gothic", Arial, sans-serif;
	color: #333;
	font-size: 12px;
}

.wrapper{
	height:768px;
	width:1024px;
	margin:auto;
}

#apptitle{
	font-family: "Century Gothic", Arial, sans-serif;
	color: #333
	text-align:left;
	font-size: 11px;
	height: 12px;
}

#dragcontent{
	height: 386px;
	width: 263px;
	overflow-y: hidden;
}

#dragcontent:hover
{
	width: 280px;
	overflow-y: scroll;
}

.gray{
	color:gray;
}

#menubar{
	text-align: left;
	padding: 2px;
	font-size: 38px;
}

.phone
{
	position:absolute;
	top:0px;
	right: 100px;
}

#phonecontent
{
	position:absolute;
	top:44px;
	left:24px;
	width:266px;
	height:446px;
}

.blend
{
	border-style: none;
}

.floatright
{
	float: right;
	display: none;
}

#bottombuttons
{
	margin-top: 20px;
	text-align: center;
}

.moveleft
{
	position: absolute;
	width: 320px;
	right: 500px;
	top:100px;
}

.question:hover
{
	background-color: lightgray;
}


#sortable input
{
	background-color:transparent;
}


</style>
</head>

<body onKeyPress="return disableEnterKey(event)">
<div class="wrapper">
<div>
<ul class="top-menu">
	<li><a href="index.php" class="special-anchor">HOME</a></li>
	<li><a href="manage.php" class="special-anchor">PROJECTS</a></li>
	<? if($loggedin){ ?>
	<li><a href="admin.php">LAUNCH A PROJECT</a></li>
	<li><a href="visualization.php" class="special-anchor">VISUALIZATION</a></li>
	<li><a href="logout.php" class="special-anchor">LOGOUT</a></li>
	<? } else { ?>
	<li class="selected"><a href="register.php" class="special-anchor">LOGIN</a></li>
	<? } ?>
</ul>
</div>

<div id="content">
	
		<div class="moveleft">
		
		<form method="POST" action="register.php?action=login">	
	
			<div>
				<h1>Login to myScience</h1>
				<h2><color="red"><?=$logmsg?></color></h2>
				<span class="columnleft">Email: </span><span class="columnright">
					<input type="text" name="coordemail"/>
				</span><br/>
				<span class="columnleft">Password: </span><span class="columnright">
					<input type="password" name="coordpass"/>
				</span><br/>
				<input type="submit" value="Login" />
			</div>
		</form>
		<form method="POST" action="register.php?action=register">	

			<div>
				<h1>Register for myScience</h1>
				<h2><color="red"><?=$regmsg?></color></h2>

				<span class="columnleft">Coordinator Name: </span><span class="columnright">
					<input type="text" name="coordname"/>
				</span><br/>
				<span class="columnleft">Coordinator Email: </span><span class="columnright">
					<input type="text" name="coordemail"/>
				</span><br/>
				<span class="columnleft">Coordinator Password: </span><span class="columnright">
					<input type="password" name="coordpass"/>
				</span><br/>
				<input type="submit" value="Register" />	
			</div>
		</form>
		</div>

</div>
</div>
</body>
</html>