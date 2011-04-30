<?php 
/**
 * Microsoft Bing Maps
 * 
 * PHP library to support the Microsoft Bing Maps API.
 *
 * PHP Version 5
 *  
 * @category  Msft
 * @package   Msft
 * @subpackage Bing
 * @author    Mindtree 
 * @copyright 2011 Mindtree Limited
 * @license   GPL v2 License https://github.com/mindtree/BingMapsPHPSDK
 * @link      https://github.com/mindtree/BingMapsPHPSDK
 *
 */

if (isset($_GET['CurrentLocation']))
{
	echo $_GET['CurrentLocation'];
}
else if (isset($_GET['CurrentAddress']))
{
	echo $_GET['CurrentAddress'];
}
if (isset($_GET['Location']))
{
	echo $_GET['Location'];
}
else if (isset($_GET['Address']))
{
	echo $_GET['Address'];
}

?>
