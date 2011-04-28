<?php
// $Id$ 

$path = drupal_get_path('module', 'bing_maps');

require_once "$path/BingMapsPHPSDK/lib/Msft/Bing/Map/Map.php";
require_once "$path/DrupalPersistenceHandler.php";

function saveConfigurationDetails($bingid, $defaultLocation, $width, $height, $zoomLevel, $contentType, $bing_maps_defaultposition) {
	$bingObj = new Msft_Bing_Map('adminMap');
	$handler = new DrupalPersistenceHandler();
	
	$bingObj->registerPersistenceHandler($handler);
	$bingObj->createConfigurationEntity();
	
	$configResult = $bingObj->loadConfiguration();
	
		
	$bingObj->setBingID($bingid);
	
	$prop = new Msft_Bing_MapProperties($width,$height,$zoomLevel);
	$prop->setDefaultLocation($defaultLocation);
	$prop->setContentType($contentType);	
	$prop->setDefaultPosition($bing_maps_defaultposition);	
	
	$bingObj->setProperties($prop);	
  	$bingObj->saveConfiguration();
  	
  	return true;
}
	
function savePushPins($pin_details)
{	
	$bingObj = new Msft_Bing_Map('');
	$handler = new DrupalPersistenceHandler();
	$bingObj->registerPersistenceHandler($handler);
	
	$bingObj->createPushpinsEntity();
		
  	$loadResult = $bingObj->loadPushpins();
  	
  	
  	  	
  	$pushpinsFromDB = $bingObj->getpushpinsToAdd();
  	$cnt = count($pushpinsFromDB);
  		
	$pinDetstr = rtrim($pin_details, "|");
  	$pinDetArr = explode("|", $pinDetstr);
  		
  	for($q=0;$q<count($pinDetArr);$q++)
  	{
  		$pinId = explode(";", $pinDetArr[$q]);
  	
  		for ($r=0;$r<3;$r++)
  		{
  			$pushpinsFromDB[$cnt+$q]->title = $pinId[0];
  			$pushpinsFromDB[$cnt+$q]->description = $pinId[1];
  			$pushpinsFromDB[$cnt+$q]->location = $pinId[2];
  			$pushpinsFromDB[$cnt+$q]->latitude = 99.99;
  			$pushpinsFromDB[$cnt+$q]->longitude = 99.99;
  		}
  	}
  	
  	$bingObj->setpushpinsToAdd($pushpinsFromDB);
  	if(isset($pin_details) && $pin_details != '' && $pin_details != null)
  		$bingObj->savePushpins();
  	
  	return true;
}

function deleteConfig()
{
	$bingObj = new Msft_Bing_Map('');
	$handler = new DrupalPersistenceHandler();
	$bingObj->registerPersistenceHandler($handler);
	$bingObj->dropConfigurationEntity();
}

function deletePins()
{
	$bingObj = new Msft_Bing_Map('');
	$handler = new DrupalPersistenceHandler();
	$bingObj->registerPersistenceHandler($handler);
	$bingObj->dropPushpinsEntity();
}

function deletePositions()
{
	$bingObj = new Msft_Bing_Map('');
	$handler = new DrupalPersistenceHandler();
	$bingObj->registerPersistenceHandler($handler);
	$bingObj->dropLocationEntity();
}