<?php
include "authentication.php";
?>
<html>
<style type="text/css">
.wrapper{
	height:768px;
	width:1024px;
	margin:auto;
}
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
body{
	margin:0;
	padding:0;
	text-align:center;
}



</style>
<div class="wrapper"><div>
		<ul class="top-menu">
	<li class="selected"><a href="index.php" class="special-anchor">HOME</a></li>
	<li><a href="manage.php" class="special-anchor">PROJECTS</a></li>
	<? if($loggedin){ ?>
	<li><a href="admin.php">LAUNCH A PROJECT</a></li>
	<li><a href="visualization.php" class="special-anchor">VISUALIZATION</a></li>
	<li><a href="logout.php" class="special-anchor">LOGOUT</a></li>
	<? } else { ?>
	<li><a href="register.php" class="special-anchor">LOGIN</a></li>
	<? } ?>
</ul>
</div>
<div><img src="Home.png"/></div></div>
</html>