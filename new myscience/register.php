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
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>MYSCIENCE | Register</title>
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
</style>
</head>

<body>
<div id="head">
 <div id="head_cen">
  <div id="head_sup" class="head_pad">
    <h1 class="logo"><a href="index.html">MYSCIENCE</a></h1>
    <ul>
     <li><a href="index.php">HOME</a></li>
	 
     <li><a href="about.html">ABOUT</a></li>
	 <li><a href="manageNew.php">PROJECTS</a></li>
	 <? if($loggedin){ ?>
     <li><a href="admin.php">LAUNCH</a></li>
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
   <h2><span>REGISTER</span> for myScience</h2>
   </div>
	<div class="content">
	<div class="formbox">
	<form method="POST" action="register.php?action=login">	
	
			<div>
				<h1>Login to myScience</h1>
				<h2><color="red"><?=$logmsg?></color></h2><br/>
				<span class="columnleft">Email: </span><span class="columnright">
					<input type="text" name="coordemail"/>
				</span><br/>
				<span class="columnleft">Password: </span><span class="columnright">
					<input type="password" name="coordpass"/>
				</span><br/>
				<input type="submit" value="Login" />
			</div>
		</form>
		<br/>
		<form method="POST" action="register.php?action=register">	

			<div>
				<h1>Register for myScience</h1>
				<h2><color="red"><?=$regmsg?></color></h2>
<br/>
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
<div id="foot">
 <div id="foot_cen">
 <h6><a href="index.html">myScience</a></h6>
 <!--<ul>
     <li><a href="index.html">Home</a></li>
	 <li class="space">|</li>
     <li><a class="active" href="about.html">ABOUT</a></li>
     <li class="space">|</li>
     <li><a href="launch.php">LAUNCH</a></li>
     <li class="space">|</li>
     <li><a href="manageNew.php">MANAGE</a></li>
     <li class="space">|</li>
  	 <li><a href="privacy.html">Privacy Policy</a></li>
   </ul>-->
    <p>© 2011 myScience. All rights reserved. Designed by: <a href="http://www.templateworld.com" target="_blank">Template World</a></p>
 </div>
</div>
</body>
</html>