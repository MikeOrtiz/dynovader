<?php
session_start();
$loggedin = false;
if(isset($_SESSION['coordid'])){
	$loggedin = true;
}
?>