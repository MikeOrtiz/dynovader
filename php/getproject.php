<?php
$q=$_GET["q"];
include "connect_ms.php";



$query = "SELECT data.time as time, data.location as location, data.lowrespic as picture FROM data WHERE data.projectid = '".$q."'";
$result = sqlsrv_query($conn,$query);

echo "[";

while($row = sqlsrv_fetch_array($result))
  {
  echo "{";
  echo $row['location'] . "~";
  //will add datetime
  echo $row['picture'] . "}";
  }
echo "]";

?>