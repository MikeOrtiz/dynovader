<?php
include "authentication.php";

?>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>MYSCIENCE | Citizen Science</title>
<link href="css/styles.css" rel="stylesheet" type="text/css" media="all" />
</head>

<body>
<div id="head">
 <div id="head_cen">
  <div id="head_sup" class="head_pad">
    <h1 class="logo"><a href="index.php">MYSCIENCE</a></h1>
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
 </div>
</div>
<div id="content">
 <div id="content_cen">
  <div id="content_sup" class="head_pad">
   <div id="welcom_pan">
    <h2><span>about</span>MYSCIENCE</h2>
    <p>MYSCIENCE is a free mobile app that enables individuals with smart phones to contribute to a variety of science research projects by gathering useful information using the sensors on their phones. The data is then made available in aggregate, on the cloud, to scientists. Our app will soon be available on the Windows Phone 7 Marketplace for free download.</p>
    <p>Scientists can create and customize their citizen science project using MYSCIENCE, and deploy it instantly to our users- all without having to write any code. We currently support a variety of form fields and pictures. Data can be viewed on our web portal or downloaded in a CSV format. We  host the data for free, and on the cloud so it can scale rapidly.</p>
   </div>
   <ul id="infoPan">
    <li>
     <h3><span>for</span> scientists <img src="images/scientist_icon.png" alt="" /></h3>
     <p>Cost of deployment ~= 0 </p>
     <p class="descrip">
	 Piggyback on existing active user community. Store and compute on the cloud for free.</p>
    </li>
	<li>
     <h3><span>for</span> users <img src="images/users_icon.png" alt="" /></h3>
     <p>Contribute to science research</p>
     <p class="descrip">Easily access all deployed citizen science projects from one application.  Keep track of your submissions across projects.</p>
    </li>
    <li>
     <h3><span>for</span> devs <img src="images/icon2.png" alt="" /></h3>
     <p>Open source support</p>
     <p class="descrip">We are looking for developers that can support MYSCIENCE as an open source project. Please contact us if you are interested.</p>
    </li>
   </ul>
  </div>
 </div>
</div>
<div id="foot">
 <div id="foot_cen">
 <h6><a href="index.php">myScience</a></h6>
    <p>© 2011 myScience. All rights reserved. Designed by: <a href="http://www.templateworld.com" target="_blank">Template World</a></p>
 </div>
</div>
</body>
</html>
