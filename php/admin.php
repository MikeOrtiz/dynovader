<?php 
include "connect_ms.php";
include "authentication.php";
$msg = "";
if(!$loggedin){
	header("Location:register.php");
}
if(isset($_POST['titleinput']))
{
	if($_POST['titleinput']!="") {
		/*$query = "SELECT * FROM coordinators WHERE email='".$_POST['coordemail']."'";
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
		}*/
		$coordid = $_SESSION['coordid'];
		$query = "SELECT * FROM projects WHERE name='".$_POST['titleinput']."' AND owner = $coordid";
		$result = sqlsrv_query($conn, $query);
		if(sqlsrv_has_rows($result)){
			//return error, since it exists already
		}
		else{ //build JSON string
			$values = "[";
			foreach($_POST as $key=>$value) {
				if ($key=="titleinput" || $key=="description" || $key=="coordname" || $key=="coordemail" || $key=="numchecks" || $key=="numradios") {
					//do not add to JSON
				} else if (strpos($key, 'textq')) {
					$values .= "{\"type\":\"Question\",\"label\":\"".$_POST[$key]."\"},";
				} else if (strpos($key, 'check')) { 
					if (strpos($key, 'checkq')) {
						$values .= "{\"type\":\"CheckBox\",\"label\":\"".$_POST[$key]."\",\"value\":\"";
					} else {
						$values .= $_POST[$key]."|";
						if (strpos($key, 'END')) {
							$values = substr($values, 0, -1);
							$values .= "\"},";
						}
					}
				} else if (strpos($key, 'radio')) {
					if (strpos($key, 'radioq')) {
						$values .= "{\"type\":\"RadioBox\",\"label\":\"".$_POST[$key]."\",\"value\":\"";
					} else {
						$values .= $_POST[$key]."|";
						if (strpos($key, 'END')) {
							$values = substr($values, 0, -1);
							$values .= "\"},";
						}
					}
				}
			}
			$values .= "{\"type\":\"Font\",\"value\":\"".$_POST['fontColInput']."\"},";
			$values .= "{\"type\":\"Background\",\"value\":\"".$_POST['backColInput']."\"},";
			$values .= "{\"type\":\"Photo\",\"value\":\"".$_POST['photo']."\"}]";
			
			$query = "INSERT INTO projects(name, description, owner, form) VALUES('".$_POST['titleinput']."','".$_POST['description']."',$coordid,'".$values."')";
			$result = sqlsrv_query($conn, $query);
			$msg = "Your project ".$_POST['titleinput']." was added successfully! It is currently under review.";
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
	position: relative;
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
	right: 400px;
	bottom: 70px;
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
	<li><a href="manageNew.php" class="special-anchor">PROJECTS</a></li>
	<? if($loggedin){ ?>
	<li class="selected"><a href="admin.php">LAUNCH A PROJECT</a></li>
	<!--<li><a href="visualization.php" class="special-anchor">VISUALIZATION</a></li>-->
	<li><a href="logout.php" class="special-anchor">LOGOUT</a></li>
	<? } else { ?>
	<li><a href="register.php" class="special-anchor">LOGIN</a></li>
	<? } ?>
</ul>
</div>

<div id="content">

<div class="phone">
	<img src="phone.png"/></img>
	<form method="POST" action="admin.php">	
		<div class="moveleft">
			<h1>Launch a Project</h1>
			<span class="columnleft">App Title: </span><span class="columnright">
				<input type="text" name="titleinput" id="titleinput" onkeyup="updateTitle()"/>
			</span><br/>
			<span class="columnleft">Description: </span><span class="columnright">
				<input type="text" name="description"/>
			</span><br/>
			<!--<span class="columnleft">Coordinator Name: </span><span class="columnright">
				<input type="text" name="coordname"/>
			</span><br/>
			<span class="columnleft">Coordinator Email: </span><span class="columnright">
				<input type="text" name="coordemail"/>
			</span><br/>-->
			<br /><br /><br />
			
			<h3>Add Question:</h3>
			<input type="button" value="Text Question" onclick="moreText()"/> <br />
			<input type="button" value="Check Question" onclick="moreCheck()"/> 
			<select id="numchecks" name="numchecks">
				<option value="2">2</option>
				<option value="3">3</option>
				<option value="4">4</option>
			</select><br />
			<input type="button" value="Radio Question&nbsp;" onclick="moreRadio()"/> 
			<select id="numradios" name="numchecks">
				<option value="2">2</option>
				<option value="3">3</option>
				<option value="4">4</option>
			</select><br /><br />
			&nbsp;Change Background: 				   <input id="backColInput" name="backColInput" type="text" size="7" maxlength="6" value="ffffff"><br />
			&nbsp;Change Font Color: &nbsp;&nbsp;&nbsp;<input id="fontColInput" name="fontColInput"type="text" size="7" maxlength="6" value="000000">
			<!--<input id="picturequestion" type="button" value="Remove Picture Capture" onclick="updatePhotoOption()"/> --><br /><br />
			<input type="text" id="photopost" name="photo" value="Y" style="display:none">
			<input type="submit" value="Submit App!" />	
			
			<h3>Instructions:</h3>
			<ol>
				<li>Add Application Title to Phone</li>
				<li>Fill out Description, Name, and Email above</li>
				<li>Add and remove questions with buttons above</li>
				<li>Edit questions and options directly on phone</li>
				<li>Drag and drop questions to edit order</li>
				<li>Press Submit App!</li>
			</ol>	
			<br /><br />
		</div>
		
		<div id="phonecontent">
			<div id="apptitle" name="apptitle" size="30" value="Enter App Title Here"></div>
			<div id="menubar">
				<span class="gray">Submission</span> Da
			</div>
			<div id="dragcontent">
				<div id="sortable">
					<span id="writeroot"></span>
				</div>
				<div id="bottombuttons">
					<div id="photobuttons" style="display:block">
						<button type="button" disabled="disabled">Take a Photo</button><br />
						<button type="button" disabled="disabled">Choose a Photo</button><br /><br />
					</div>
					<button type="button" disabled="disabled">Save Only</button><br />
					<button type="button" disabled="disabled">Submit</button>
				</div>
			</div> 
		</div>
	</form>
</div>


<div class="question" id="textroot" style="display: none" onmouseover="revealButtons(this);" onmouseout="hideButtons(this)">
	<span class="floatright">
		<span class="ui-icon ui-icon-close" onclick="closeQuestion(this)" onmouseover="this.className='ui-icon ui-icon-circle-close'" onmouseout="this.className='ui-icon ui-icon-close'"/></span>
	</span>
	<input class="blend" name="textq" size="29" maxlength="35" value="Enter your text question here." 
	onFocus="if(this.value == 'Enter your text question here.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter your text question here.';}">
	<br />
	<span class="floatright">
		<span class="ui-icon ui-icon-carat-2-n-s"></span>
	</span>
	<input class="textanswer" name="texta" disabled="disabled">
</div>

<div class="question" id="checkroot" style="display: none" onmouseover="revealButtons(this)" onmouseout="hideButtons(this)">
	<span class="floatright">
		<span class="ui-icon ui-icon-close" onclick="closeQuestion(this)" onmouseover="this.className='ui-icon ui-icon-circle-close'" onmouseout="this.className='ui-icon ui-icon-close'"/></span>
	</span>
	<input class="blend" name="checkq" size="29" maxlength="35" value="Enter your check question here."
	onFocus="if(this.value == 'Enter your check question here.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter your check question here.';}">
	<br />
	<span class="floatright">
		<span class="ui-icon ui-icon-carat-2-n-s"></span>
	</span>
	<input type="checkbox" name="check1" disabled="disabled"/>
	<input class="blend" name="checkA1" size="29" maxlength="35" value="Enter check option 1."
	onFocus="if(this.value == 'Enter check option 1.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 1.';}">
	<br />
	<input type="checkbox" name="check2" disabled="disabled"/> <input class="blend" name="checkA2" size="29" maxlength="35" value="Enter check option 2."
	onFocus="if(this.value == 'Enter check option 2.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 2.';}"><br />
	<input type="checkbox" name="check3" disabled="disabled"/> <input class="blend" name="checkA3" size="29" maxlength="35" value="Enter check option 3."
	onFocus="if(this.value == 'Enter check option 3.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 3.';}"><br />
	<input type="checkbox" name="check4" disabled="disabled"/> <input class="blend" name="checkA4" size="29" maxlength="35" value="Enter check option 4."
	onFocus="if(this.value == 'Enter check option 4.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 4.';}"><br />
</div>
 
<div class="question" id="radioroot" style="display: none" onmouseover="revealButtons(this)" onmouseout="hideButtons(this)">
	<span class="floatright">
		<span class="ui-icon ui-icon-close" onclick="closeQuestion(this)" onmouseover="this.className='ui-icon ui-icon-circle-close'" onmouseout="this.className='ui-icon ui-icon-close'"/></span>
	</span>
	<input class="blend" name="radioq" size="29" maxlength="35" value="Enter your radio question here."
	onFocus="if(this.value == 'Enter your radio question here.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter your radio question here.';}">
	<br />
	<span class="floatright">
		<span class="ui-icon ui-icon-carat-2-n-s"></span>
	</span>
	<input type="radio" name="radio1" disabled="disabled"/> 
	<input class="blend" name="radioA1" size="29" maxlength="35" value="Enter radio option 1."
	onFocus="if(this.value == 'Enter radio option 1.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 1.';}">
	<br />
	<input type="radio" name="radio2" disabled="disabled"/> <input class="blend" name="radioA2" size="29" maxlength="35" value="Enter radio option 2."
	onFocus="if(this.value == 'Enter radio option 2.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 2.';}"><br />
	<input type="radio" name="radio3" disabled="disabled"/> <input class="blend" name="radioA3" size="29" maxlength="35" value="Enter radio option 3."
	onFocus="if(this.value == 'Enter radio option 3.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 3.';}"><br />
	<input type="radio" name="radio4" disabled="disabled"/> <input class="blend" name="radioA4" size="29" maxlength="35" value="Enter radio option 4."
	onFocus="if(this.value == 'Enter radio option 4.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 4.';}"><br />
</div>

</div>
</div>
</body>
</html>