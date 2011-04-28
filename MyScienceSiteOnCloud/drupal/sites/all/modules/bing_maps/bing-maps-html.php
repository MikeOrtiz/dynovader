<?php
// $Id: 

$path = drupal_get_path('module', 'bing_maps');

require_once "$path/BingMapsPHPSDK/lib/Msft/Bing/Map/Map.php";
require_once "$path/DrupalPersistenceHandler.php";

$bingObj = new Msft_Bing_Map('adminMap');
$handler = new DrupalPersistenceHandler();
$bingObj->registerPersistenceHandler($handler);

$bingObj->createConfigurationEntity();

$configResult = $bingObj->loadConfiguration();
$configDetails = $bingObj->getconfigDetails();

$configDetailsArr = array();
if (!empty($configDetailsArr)) {
  $configDetailsArr = explode(",", $configDetails[0]->content_type);
}

$adminMapWidth  = isset($configDetails[0]->width) ? $configDetails[0]->width : '640';
$adminMapHeight = isset($configDetails[0]->height) ? $configDetails[0]->height : '480';
?>

<script type="text/javascript">

(function ($) {
 $(document).ready(function() {

	 
	 var save_detal = document.getElementById('save_details');
	 var newInput = document.createElement('input');
	 newInput.type = 'button'; 
	 newInput.name = save_detal.name;
	 newInput.id = save_detal.id;
	 newInput.value = newInput.defaultValue = save_detal.value;
	 newInput.className='form-submit';
	 save_detal.parentNode.replaceChild(newInput, save_detal);
	 
   if ($("#adminMap").length > 0) {
	   //defPosition = true;
     <?php $bingObj->displayMap(); ?>
	var firstDef = '<?php echo $bingObj->getProperties()->getDefaultLocation();?>';
	if(firstDef != '' && firstDef != null)
		GotoUserLocation('<?php echo $bingObj->getProperties()->getDefaultLocation();?>');
	
   }
   
	$('#save_details').bind('click', function() {
		 return saveDetails();
	     });

   if($('#width') && $('#height') && $('#zoomlevel')) {
   $('#width').bind('blur', function() {
     var options = getAdminSettings();
     PreviewMap(options);
     });

   $('#height').bind('blur', function() {
     var options = getAdminSettings();
     PreviewMap(options);
     });

   $('#zoomlevel').bind('change', function() {
     var options = getAdminSettings();
     PreviewMap(options);
     });
   }

  
	$('#location').bind('change', function(){
	 	if($('#location').val() != '' && $('#location').val() != null)
		{
	 		showDefaultPin = true;
	 		latLongBox = 'latLongResult';
			GotoUserLocation(document.getElementById('location').value);	
		}
	});
	
   
 });

 function getAdminSettings() {
   var options = {};
   options.width = $('#width').val();
   options.height = $('#height').val(); 
   options.zoomlevel = $('#zoomlevel').val();
   return options;
 }

})(jQuery);


function saveDetails() {
  var re = /[^\s]/;
  var reNum = /[^0-9]/;
  var errors = [];
  document.getElementById('save_det').value=0;


  if(!re.test(document.getElementById('bingid').value) || !re.test(document.getElementById('width').value) || !re.test(document.getElementById('height').value) || !re.test(document.getElementById('zoomlevel').value) || typeof(document.getElementsByName('content_type').value) == null || !re.test(document.getElementsByName('content_type').value) )
  {
    errors.push("Please provide details for all mandatory fields");
  } else if (reNum.test(document.getElementById('width').value) || reNum.test(document.getElementById('height').value)) {
    errors.push("Please only enter numbers in width and height field");
  } else if (!re.test(document.getElementById('location').value)) {
    errors.push("Please set the default location");
  }

  if(errors.length > 0) {
    alert(errors.join("\n"));
    return false;
  }
  credentialFlag = null;
  ValidateBing(document.getElementById('bingid').value);			
  bingIdTester = setInterval ( "processDetails()", 1000 );
}

function processDetails() {
  if (credentialFlag != null) {
    clearInterval(bingIdTester);
    if (credentialFlag) {
      document.getElementById('save_det').value = 1;
      document.getElementById('bing-maps-settings-form').submit();
    }
  }
}

function bAlert(event) {
  var button;
  if (event.which == null) {
    button = (event.button < 2) ? "LEFT" : ((event.button == 4) ? "MIDDLE" : "RIGHT");
  } else {
    button = (event.which < 2) ? "LEFT" : ((event.which == 2) ? "MIDDLE" : "RIGHT");
  }

  if(button == 'RIGHT') {
    <?php
      $domElement = '<div class="MapContextMenu"><a id="aid_Add_a_pushpin" href="#" style="padding-left: 5px; padding-right: 5px;" onClick="fnClicked(); return false;">Set as default location</a></div>';
      $bingObj->addLayer($domElement);
    ?>
  }
}

function fnClicked() {
  <?php $bingObj->getCurrentCoordinates('latLongResult'); ?>
  setTimeout("fnStage()", 1000); //1000 milli sec
}

function fnStage() {
  var addPin = document.getElementById('latLongResult').value;
  if(addPin != null || addPin != '')
  {
    var locationResult = addPin.split(",");
    var latitude = locationResult[0];
    var longitude = locationResult[1];

    <?php $bingObj->addPushpinLatLon('latitude', 'longitude', 'yes');?>
  }
  getAddress();

}

function getAddress() {
  <?php $bingObj->getCurrentAddress('resultBox'); ?>
    setTimeout("fnD1Stage()", 1000); //1000 milli sec
}

function fnD1Stage() {
  document.getElementById('location').value = document.getElementById('resultBox').value;
}
</script>
