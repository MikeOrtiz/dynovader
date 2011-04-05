<?php
$serverName = "tcp:mma1mtoeql.database.windows.net, 1433";
$connectionOptions = array("Database"=>"users",
         "UID"=>"dynovader@mma1mtoeql",
         "PWD" => "592Mayfield");
$conn = sqlsrv_connect($serverName, $connectionOptions);
if($conn === false)
{
      die(print_r(sqlsrv_errors(), true));
}
?>