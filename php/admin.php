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
		$query = "SELECT * FROM projects WHERE name='".$_POST['apptitle']."' AND owner = $coordid";
		$result = sqlsrv_query($conn, $query);
		//echo sqlsrv_num_rows($result)."<br/>";
		if(sqlsrv_has_rows($result)){
			//return error, since it exists already
		}
		else{ //build JSON string
			$values = "[";
			foreach($_POST as $key=>$value) {
				if ($key!="apptitle" && $key!="description" && $key!="coordname" && $key!="coordemail") {
					if (strpos($key, 'textq')) {
						$values .= "{\"type\":\"Question\",\"label\":\"".$_POST[$key]."\"},";
					}
					else if (strpos($key, 'check')) { //later append # to end of checkq to know # of values
						if (strpos($key, 'checkq')) {
							$values .= "{\"type\":\"CheckBox\",\"label\":\"".$_POST[$key]."\",\"value\":\"";
						} else {
							$values .= $_POST[$key]."|";
							if (strpos($key, 'checkA4')) {
								$values = substr($values, 0, -1);
								$values .= "\"},";
							}
						}
					}
					else if (strpos($key, 'radio')) {
						if (strpos($key, 'radioq')) {
							$values .= "{\"type\":\"RadioBox\",\"label\":\"".$_POST[$key]."\",\"value\":\"";
						} else {
							$values .= $_POST[$key]."|";
							if (strpos($key, 'radioA4')) {
								$values = substr($values, 0, -1);
								$values .= "\"},";
							}
						}
					}
				}
			}
			$values .= "{\"type\":\"Photo\",\"value\":\"".$_POST['photo']."\"}]";
			
			$query = "INSERT INTO projects(name, description, owner, form) VALUES('".$_POST['apptitle']."','".$_POST['description']."',$coordid,'".$values."')";
			$result = sqlsrv_query($conn, $query);
			echo "Your project <i>".$_POST['apptitle']."</i> was added successfully! It is currently under review.";
		}
	}
	
}
?>
<!DOCTYPE HTML>
<html>
<head>
<link rel="stylesheet" href="http://jqueryui.com/themes/base/jquery.ui.all.css"> 
<script src="http://code.jquery.com/jquery-1.6.min.js"></script> 
<script src="http://jqueryui.com/ui/jquery.ui.core.js"></script> 
<script src="http://jqueryui.com/ui/jquery.ui.widget.js"></script> 
<script src="http://jqueryui.com/ui/jquery.ui.mouse.js"></script> 
<script src="http://jqueryui.com/ui/jquery.ui.sortable.js"></script> 
<link rel="stylesheet" href="http://jqueryui.com/demos/demos.css">
<script>
$(function() {
		$( "#sortable" ).sortable();
		$( "#sortable" ).disableSelection();
});

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

function moreFields(qtype) {
	counter++;
	var newFields = document.getElementById(qtype).cloneNode(true);
	newFields.id = '';
	newFields.style.display = 'block';
	var newField = newFields.childNodes;
	for (var i=0;i<newField.length;i++) {
		var theName = newField[i].name
		if (theName)
			newField[i].name = counter + ' ' + theName;
	}
	var insertHere = document.getElementById('writeroot');
	insertHere.parentNode.insertBefore(newFields,insertHere);
}

function starterFields() {
	moreText();
	moreCheck();
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

#apptitle{
	font-family: "Century Gothic", Arial, sans-serif;
	color: #333
	text-align:left;
	font-size: 11px;
	height: 12px;
}

.content{
	margin:auto;
	position: relative;
	top: 50px;
	font-family: "Century Gothic", Arial, sans-serif;
	color: #333;
	font-size: 12px;
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

.gray{
	color:gray;
}

#menubar{
	text-align: left;
	padding: 2px;
	font-size: 38px;
}
	
body{
	margin:0;
	padding:0;
}


.phone
{
	position:absolute;
	top:0px;
	right: 100px;
}

.phone_content
{
	position:absolute;
	top:44px;
	left:25px;
	width:262px;
	height:444px;
}

.blend
{
	border-style: none;
}

.remove
{
	float: right;
}

.floatright
{
	float: right;
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
	bottom: 100px;
}

.question:hover
{
	background-color:lightgray;
}

input
{
	background-color:transparent;
}

.ui-icon ui-icon-closethick:hover
{
	cursor:default;
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

<div class="phone">
	<img src="phone.png"/></img>
	<form method="POST" action="admin.php">	
		<div class="moveleft">
			<h1>Launch a Project</h1>
			<span class="columnleft">App Title: </span><span class="columnright">
				<input type="text" name="apptitle" id="titleinput" onkeyup="updateTitle()"/>
			</span><br/>
			<span class="columnleft">Description: </span><span class="columnright"><input type="text" name="description"/></span><br/>
			<span class="columnleft">Coordinator Name: </span><span class="columnright"><input type="text" name="coordname"/></span><br/>
			<span class="columnleft">Coordinator Email: </span><span class="columnright"><input type="text" name="coordemail"/></span><br/>
			<br /><br /><br />
			<h3>Add Question:</h3>
			<input type="button" value="Text Question" onclick="moreText()"/> <br />
			<!--
			Number of check options: 
			<select id="numchecks" name="numchecks">
				<option value="2">2</option>
				<option value="3">3</option>
				<option value="4">4</option>
			</select>
			-->
			<input type="button" value="Check Question" onclick="moreCheck()"/> <br />
			<input type="button" value="Radio Question" onclick="moreRadio()"/> <br />
			<input id="picturequestion" type="button" value="Remove Picture Capture" onclick="updatePhotoOption()"/> <br /><br />
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
		
		<div class="phone_content">
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


<div><?=$msg?></div>

<div class="question" id="textroot" style="display: none">
	<input class="blend" name="textq" size="29" maxlength="35" value="Enter your question here."
	onFocus="if(this.value == 'Enter your question here.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter your question here.';}">
	<span class="floatright">
		<span class="ui-icon ui-icon-closethick" onclick="this.parentNode.parentNode.parentNode.removeChild(this.parentNode.parentNode);" /></span>
	</span><br />
	<input class="textanswer" name="texta" disabled="disabled">
	<span class="floatright">
		<span class="ui-icon ui-icon-arrowthick-2-n-s"></span>
	</span>
</div>

<div class="question" id="checkroot" style="display: none">
	<input class="blend" name="checkq" size="29" maxlength="35" value="Enter your question here."
	onFocus="if(this.value == 'Enter your question here.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter your question here.';}">
	<span class="floatright">
		<span class="ui-icon ui-icon-closethick" onclick="this.parentNode.parentNode.parentNode.removeChild(this.parentNode.parentNode);" /></span>
	</span><br />
	<input type="checkbox" name="check1" disabled="disabled"/> <input class="blend" name="checkA1" size="29" maxlength="35" value="Enter check option 1."
	onFocus="if(this.value == 'Enter check option 1.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 1.';}">
	<div class="floatright">
		<span class="ui-icon ui-icon-arrowthick-2-n-s">
	</div><br />
	<input type="checkbox" name="check2" disabled="disabled"/> <input class="blend" name="checkA2" size="29" maxlength="35" value="Enter check option 2."
	onFocus="if(this.value == 'Enter check option 2.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 2.';}"><br />
	<input type="checkbox" name="check3" disabled="disabled"/> <input class="blend" name="checkA3" size="29" maxlength="35" value="Enter check option 3."
	onFocus="if(this.value == 'Enter check option 3.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 3.';}"><br />
	<input type="checkbox" name="check4" disabled="disabled"/> <input class="blend" name="checkA4" size="29" maxlength="35" value="Enter check option 4."
	onFocus="if(this.value == 'Enter check option 4.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter check option 4.';}"><br />
</div>
 
<div class="question" id="radioroot" style="display: none">
	<input class="blend" name="checkq" size="29" maxlength="35" value="Enter your question here."
	onFocus="if(this.value == 'Enter your question here.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter your question here.';}">
	<span class="floatright">
		<span class="ui-icon ui-icon-closethick" onclick="this.parentNode.parentNode.parentNode.removeChild(this.parentNode.parentNode);" /></span>
	</span><br />
	<input type="radio" name="radio1" disabled="disabled"/> <input class="blend" name="radioA1" size="29" maxlength="35" value="Enter radio option 1."
	onFocus="if(this.value == 'Enter radio option 1.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 1.';}">
	<div class="floatright">
		<span class="ui-icon ui-icon-arrowthick-2-n-s">
	</div><br />
	<input type="radio" name="radio2" disabled="disabled"/> <input class="blend" name="radioA2" size="29" maxlength="35" value="Enter radio option 2."
	onFocus="if(this.value == 'Enter radio option 2.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 2.';}"><br />
	<input type="radio" name="radio3" disabled="disabled"/> <input class="blend" name="radioA3" size="29" maxlength="35" value="Enter radio option 3."
	onFocus="if(this.value == 'Enter radio option 3.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 3.';}"><br />
	<input type="radio" name="radio4" disabled="disabled"/> <input class="blend" name="radioA4" size="29" maxlength="35" value="Enter radio option 4."
	onFocus="if(this.value == 'Enter radio option 4.') {this.value = '';}" onBlur="if (this.value == '') {this.value = 'Enter radio option 4.';}"><br />
</div>

<div id="pictureroot" style="display: none">
	<span id="photoalign1"><input type="button" value="Take a Photo"></span>
	<span class="remove">
		<input type="button" value="x" onclick="this.parentNode.parentNode.parentNode.removeChild(this.parentNode.parentNode);" />
	</span><br />
	<span id="photoalign2"><button type="button">Choose a Photo</button></span>
</div>
</div>
</div>
</body>
</html>