<?php
$q=$_GET["q"];
include "connect_ms.php";

$query = "SELECT projects.name as name, data.time as time, data.location as location, data.picture as highrespic, data.lowrespic as lowrespic FROM projects, data WHERE data.projectid = '".$q."' AND projects.id = '".$q."'";
$result = sqlsrv_query($conn,$query);

/*
$projectquery = "SELECT name from projects WHERE ID='".$q."'";
$projresult = sqlsrv_query($conn,$projectquery);
$arr = sqlsrv_fetch_array($projresult);
$projname = $arr['name'];
*/

if ($row = sqlsrv_fetch_array($result))
{
	echo $row['name'] . "~";
	echo "{";
	echo $row['location'] . "~";
	echo $row['time']->format('M, d Y H:i') . "~";
	echo $row['highrespic'] . "~";
	echo $row['lowrespic'] . "}";
}

while($row = sqlsrv_fetch_array($result))
  {
  echo "{";
  echo $row['location'] . "~";
  echo $row['time']->format('M, d Y H:i') . "~";
  echo $row['highrespic'] . "~";
  echo $row['lowrespic'] . "}";
  }
echo "]";

?>