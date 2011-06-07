<?php
include 'authentication.php';
?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<title>MYSCIENCE | Citizen Science </title>
<link href="css/styles.css" rel="stylesheet" type="text/css" media="all" />
<!--  STEP ONE: insert path to SWFObject JavaScript -->
<script type="text/javascript" src="js/swfobject/swfobject.js"></script>

<!--  STEP TWO: configure SWFObject JavaScript and embed CU3ER slider -->
<script type="text/javascript">
		var flashvars = {};
		flashvars.xml = "config.xml";
		flashvars.font = "font.swf";
		var attributes = {};
		attributes.wmode = "transparent";
		attributes.id = "slider";
		swfobject.embedSWF("cu3er.swf", "cu3er-container", "800", "360", "9", "expressInstall.swf", flashvars, attributes);
</script>
<style>
.bigbutton {
	background-color: #f18519;
	color:white;
	margin-top: 15px;
	font-size: 24px;
	padding: 8px 12px 8px;
	-moz-border-radius: 6px;
	-webkit-border-radius: 6px;
	border-radius: 6px;
	cursor: pointer;
	cursor: hand;
	text-align: center;
}

.bigbutton:hover {
	background-color: #faad5f;
}

</style>
</head>

<body>
<div id="head">
 <div id="head_cen">
  <div id="head_sup" class="head_height">
  <img src="images/bannerBg.png" alt="" class="ban_bg" />
    <div class="logo"><a href="index.php"></a></div>
    <ul>
     <li><a class="active" href="index.php">HOME</a></li>
	 
     <li><a href="about.php">ABOUT</a></li>
	 <li><a href="manageNew.php">PROJECTS</a></li>
	 <? if($loggedin){ ?>
     <li><a href="launch.php">LAUNCH</a></li>
	 <li><a href="logout.php">LOGOUT</a></li>
	 <? } else { ?>
     <li><a href="register.php">LOGIN</a></li>
	 <? } ?>
   </ul>
   <div id="cu3er-container">
    <a href="http://www.adobe.com/go/getflashplayer">
        <img src="http://www.adobe.com/images/shared/download_buttons/get_flash_player.gif" alt="Get Adobe Flash player" />
    </a>
   </div>
  </div>
 </div>
</div>
<div id="content">
 <div id="content_cen">
  <div id="content_sup">
  
   <div id="ct_pan" style="background-color:transparent">
   <div style="text-align:center"><a class="bigbutton" href="launch.php">Launch a Project</a></div>
   <!--
    <p>Imagine a network of thousands of mobile phones, each equipped with a camera, microphone, GPS and accelerometer. Unleash this power for your science research using myScience. <a href="about.php">LEARN MORE</a></p>
   </div>

   <div id="blog">
    <h3><span>from the</span> blog</h3>
    <ul>
      <li>
       <a href="#">Lorem ipsum dolor sit amet, consectetur adipiscing elit.</a>
       <p>Maecenas ut lacus magna, ut consectetur quam. Etiam pharetra tincidunt massa, vitae pulvinar eros commodo ut. Sed in orci neque. Mauris eros est, auctor vitae.</p>
      </li>
      <li>
       <a href="#">Lorem ipsum dolor sit amet, consectetur adipiscing elit.</a>
       <p>Maecenas ut lacus magna, ut consectetur quam. Etiam pharetra tincidunt massa, vitae pulvinar eros commodo ut. Sed in orci neque. Mauris eros est, auctor vitae.</p>
      </li>
    </ul>
   </div>
   <div id="latest">
    <h3><span>latest </span>news </h3>
    <ul>
      <li>
        <a href="http://blogs.msdn.com/b/msr_er/archive/2011/04/19/aloha-text-from-the-cloud.aspx">Featured in MSR blog</a>
        <p>Microsoft Research Connections, our sponsor, blogs about our project</p>
      </li>
      <li>
        <a href="#">consectetur adipiscing elit.</a>
        <p>Maecenas ut lacus magna, ut consectetur quam. Etiam pharetra tincidunt.</p>
      </li>
    </ul>
   </div> -->
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
