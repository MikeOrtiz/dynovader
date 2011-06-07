﻿<?php 
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
		header("Location:launch.php");
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
			header("Location:launch.php");
		}
	}
}
}
?>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>myscience | science for everyone</title>
<link href="css/styles.css" rel="stylesheet" type="text/css" media="all" />
<? include 'analytics.php' ?>
<style type="text/css">
table{
	border:1px solid #000;
	background-color:#fff;
	padding:1px;
	width:100%;
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

.formbox {
	width: 350px;
}
.submit {
background-color: #f18519;
color:white;
margin-top: 10px;
font-size: 14px;
padding: 8px 12px 8px;
-moz-border-radius: 6px;
-webkit-border-radius: 6px;
border-radius: 6px;
}
.formtext{
background: #626262;
color: white;
-moz-border-radius: 6px;
border-radius: 6px;
height: 25px;
padding-left: 3px;
border-color: #585858;
border-style: solid;
border-width: 2px;
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
	 <li><a href="manageNew.php">PROJECTS</a></li>
	 <? if($loggedin){ ?>
     <li><a href="launch.php">LAUNCH</a></li>
	 <li><a href="logout.php">LOGOUT</a></li>
	 <? } else { ?>
     <li><a class="active" href="register.php">LOGIN</a></li>
	 <? } ?>
   </ul>
  </div>
 </div>
</div>
<div id="content">
 <div id="content_cen">
  <div id="content_sup" class="head_pad">
   <div id="welcom_pan">
   <h2><span>REGISTER</span> today</h2>
   </div>
	<div class="content">
	<div class="formbox">
	<form method="POST" action="register.php?action=login">	
	
			<div>
				<h1>Login</h1>
				<h2><color="red"><?=$logmsg?></color></h2><br/>
				<span class="columnleft">Email: </span><span class="columnright">
					<input class="formtext" type="text" name="coordemail"/>
				</span><br/>
				<span class="columnleft">Password: </span><span class="columnright">
					<input class="formtext" type="password" name="coordpass"/>
				</span><br/>
				<input class="submit" type="submit" value="Login" />
			</div>
		</form>
		<br/>
		<form method="POST" action="register.php?action=register">	

			<div>
				<h1>Register</h1>
				<h2><color="red"><?=$regmsg?></color></h2><br/>
				<span class="columnleft">Coordinator Name: </span><span class="columnright">
					<input class="formtext" type="text" name="coordname"/>
				</span><br/>
				<span class="columnleft">Coordinator Email: </span><span class="columnright">
					<input class="formtext" type="text" name="coordemail"/>
				</span><br/>
				<span class="columnleft">Coordinator Password: </span><span class="columnright">
					<input class="formtext" type="password" name="coordpass"/>
				</span><br/>
				<input class="submit" type="submit" value="Register" />	
			</div>
		</form>
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