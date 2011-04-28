<?php
// $Id: Exp $


$path = drupal_get_path('module', 'bing_maps');

require_once "$path/BingMapsPHPSDK/lib/Msft/Bing/Map/Map.php";
require_once "$path/DrupalPersistenceHandler.php";

	$bingObj = new Msft_Bing_Map('chkMap');
	$handler = new DrupalPersistenceHandler();
  	$bingObj->registerPersistenceHandler($handler);
	
  	$bingObj->createConfigurationEntity();
	$configResult = $bingObj->loadConfiguration();
	$prop = $bingObj->getProperties();
	
	$admin_map_js = "var AdminMapSettings = {
						width: '{$prop->getWidth()}',
						height: '{$prop->getHeight()}', 
						zoomlevel: '{$prop->getzoomLevel()}',
						default_location: '{$prop ->getDefaultLocation()}',
						default_position: '{$prop ->getDefaultPosition()}'	
				};";
	
	$nid = '';
  	if(isset($_REQUEST['nId']))
  		$nid = $_REQUEST['nId'];
  	
  	$js_str = $admin_map_js . " var nid='{$nid}'; var userSetLatitude = ''; var userSetLongitude = '';";
  	drupal_add_js($js_str, 'inline');	
  	
  	$bingMapDefLocation = '';

  	$def_js = "var defaultPositionNotSaved = 0;";
  	if($nid != '' && $nid != null)
  	{
	  	$node = node_load($nid);
		$text = $node->body[$node->language][0]['value'];
		preg_match_all('/{bingmap.*?\}/is',$text, $result);			
		
		if(isset($result['0']['0']))
		{			
			$bingMapTag = $result['0']['0'];			
			$bingMapId = "BingMap";
			$bingMapDefLocation = getAttribute('defaultlocation', $bingMapTag);
			$bingMapWidth       = getAttribute('width', $bingMapTag);
			$bingMapHeight      = getAttribute('height', $bingMapTag);    		
			$bingMapZoomLevel   = getAttribute('zoomlevel', $bingMapTag);
			$bingMapPushPin     = getAttribute('pushpins', $bingMapTag);
			$bingMapDirection   = getAttribute('direction', $bingMapTag);	
		
			$bingDefPosition = $bingObj->getDefaultNodePosition($nid);
			
			if($bingMapDefLocation != $prop ->getDefaultLocation() && ($prop ->getDefaultPosition() == $bingDefPosition || empty($bingDefPosition)) ){
				$def_js .= "defaultPositionNotSaved = 1";
			}
				
			if($bingMapZoomLevel == '' || $bingMapZoomLevel == null)
				$zoomlevel = "";
			else 
				$zoomlevel = " zoomlevel=\"{$bingMapZoomLevel}\"";
			
			if($bingMapHeight == '' || $bingMapHeight == null)
				$height = "";
			else 
				$height = " height=\"{$bingMapHeight}\"";

			if($bingMapWidth == '' || $bingMapWidth == null)
				$width = "";
			else 
				$width = " width=\"{$bingMapWidth}\"";

			if ($bingMapPushPin == '' || $bingMapPushPin == null)
				$pushpins = "";
			else 
				$pushpins = " pushpins=\"{$bingMapPushPin}\"";
				
			if ($bingMapDefLocation == '' || $bingMapDefLocation == null)
				$defaultlocation = "";
			else 
				$defaultlocation = " defaultlocation=\"{$bingMapDefLocation}\" ";
				
			if ($bingMapDirection == '' || $bingMapDirection == null)
				$direction = "";
			else 
				$direction = " direction=\"{$bingMapDirection}\"";
			
				
			$macrostring = "var defaultLocation='{$bingMapDefLocation}'; var width='{$bingMapWidth}'; var height='{$bingMapHeight}'; var zoomlevel='{$bingMapZoomLevel}'; var pushpins='{$bingMapPushPin}'; var direction='{$bingMapDirection}'; ";		
			drupal_add_js($macrostring, 'inline');	
			
			$positiondetails = $bingObj->getDefaultNodePosition($nid);

			$nodepositiondetails = "var nodepositiondetails='{$positiondetails}';";
			drupal_add_js($nodepositiondetails, 'inline');
						
			if ($bingMapWidth == null && $bingMapWidth == '')
				$uWidth = '640';
			else 
				$uWidth = $bingMapWidth;
			if ($bingMapHeight == null && $bingMapHeight == '')
				$uHeight = '480';
			else 
				$uHeight = $bingMapHeight;
		}
  	}
  	else 
  	{
  		$uWidth = '640';
		$uHeight = '480';
  	}
	
  	drupal_add_js($def_js, 'inline');
			
	if ($configResult == null || $configResult == '') {
		$prop = new Msft_Bing_MapProperties('640','480',10);	//width, height, zoomLevel	
		$bingObj->setProperties($prop);	
	} 
	else {
		$uWidth = $bingObj->getProperties()->getWidth();
		$uHeight = $bingObj->getProperties()->getHeight();
	}
	
	$bingObj->createPushpinsEntity();
  	$loadResult = $bingObj->loadPushpins();
  	
  	$configDetails = $bingObj->getconfigDetails(); 	
  	  	
  	$pushpinsFromDB = $bingObj->getpushpinsToAdd();
  	$cnt = count($pushpinsFromDB);
  	
  

?>
<style>
input.button-disabled, input.button-disabled:hover, input.button-disabled:focus
{
	background:none repeat scroll 0 0 #949494;
}
</style>
<script type="text/javascript">

var defaultFlag = 0;
var defLatLon = '';
var count = 0;
var minOut = '';
var pinLocations = new Array();

var title = new Array();
var description = new Array();
var locationss = new Array();

var adminwidth = '';
var adminheight = '';
var adminZoomLevel = ''; 
var adminDefaultLocation = '';

var latitude;
var longitude;

var app;
var clearIdent="";
var defPinQueue = null;
var defPosQueue = null;



var clrIdent = "";

var url = '?q=user/1/maps/bing/addmaps';

(function ($) {
	$(document).ready(function() {
				
		var defPos = window.opener.document.getElementById('nodedefaultPosition');
		var usrdefaultPosition = defPos.value;
		if (usrdefaultPosition == '' || usrdefaultPosition == null) {
			document.getElementById('defLocate').value = AdminMapSettings.default_location;
		}
		else if (usrdefaultPosition != '' && usrdefaultPosition != null) {
			setDefaultPoisition(usrdefaultPosition);	
			document.getElementById('nodedefaultPosition').value = usrdefaultPosition;
		}
		else if (nid != '' && nid != null) {
			setDefaultPoisition(nodepositiondetails);	
		}
		
		if ($("#chkMap").length > 0){
			
			<?php $bingObj->displayMap(); ?>
		}

		if(defaultPositionNotSaved == 1){
			GotoUserLocation('<?php echo $bingMapDefLocation; ?>');
		}
		
		$('#apply_macro').bind('click', function(){

			var errors = [];	
			var re = /[^\s]/;			

			
			if(((document.getElementById('from').value == null || document.getElementById('from').value == '') && (document.getElementById('to').value != null && document.getElementById('to').value != '')) || ((document.getElementById('to').value == null || document.getElementById('to').value == '') && (document.getElementById('from').value != null && document.getElementById('from').value != '')))
			{
				errors.push("Please enter the complete direction details");
			}

			if(errors.length > 0){
				alert(errors.join("\n"));
				return false;
			}
			
			var pushpins = $('#pinDet').val();
			$('#ajax-container').html('<span>Please wait...</span>');

			if(pushpins != null || pushpins != '' || typeof pushpins != 'undefined')
				$.post(url, { pin_details : pushpins }, loadStage);
						
			var macroHTML = $('#macroarea').val();		
			var editor_el = window.opener.document.getElementById('edit-body-und-0-value');
			
			var content = editor_el.value;	
			var bingmapMacro = content.match(/{bingmap.*?\}/i);
			if (bingmapMacro) {
				content = content.replace(/{bingmap.*?\}/i, macroHTML);
				editor_el.value = content;
			} else {
				insertAtCaret(editor_el, macroHTML);				
			}	
			
			if (document.getElementById('defLocate').value == '' || document.getElementById('defLocate').value == null)
				window.opener.document.getElementById('nodedefaultPosition').value = '';
			else
				window.opener.document.getElementById('nodedefaultPosition').value = document.getElementById('nodedefaultPosition').value;
			
			var filterValue = window.opener.document.getElementById('edit-body-und-0-format--2');
			filterValue.value = 'full_html';
			
			window.close();			
			
		});	

		loadBingMap();

		if($('#width') && $('#height') && $('#zoomlevel')){				
			$('#width').bind('blur', function(){
				var options = getAdminSettings();
				PreviewMap(options);
			});

			$('#height').bind('blur', function(){
				var options = getAdminSettings();
				PreviewMap(options);
			});

			$('#zoomlevel').bind('change', function(){
				var options = getAdminSettings();
				PreviewMap(options);
			});
		}

		$('#defLocate').bind('change', function(){	

			latLongBox = 'nodedefaultPosition';

			if($('#defLocate').val() != '' && $('#defLocate').val() != null)
			{
				showDefaultPin = true;
				GotoUserLocation(document.getElementById('defLocate').value);	
				
				addMacro();
			}
			else if($('#defLocate').val() == '' || $('#defLocate').val() == null)
			{

				document.getElementById('nodedefaultPosition').value = '<?php echo $bingObj->getProperties()->getDefaultLocation();?>';
				GotoUserLocation('<?php echo $bingObj->getProperties()->getDefaultLocation();?>');
				if(typeof(defId) != 'undefined' && defId != null)
		    	{
		    		var shap = map.GetShapeByID(defId);
		    		map.DeleteShape(shap);
		    		defId = null;
		    	}
			}
			 defPosQueue = setTimeout('populateDefaultPosition()', 1000);
			
		});

		$("#width,#height,#from,#to,#zoomlevel").bind('change', function(){
			addMacro();
			
			macroChanged = 1;	
		});

		$("#macroarea").bind('change blur', function(){
			var macroText = $("#macroarea").val();
			var re = /[^\s]/;
			if(re.test(macroText)){
				handleApplyButton(true); //enable
			} 
			else {
				handleApplyButton(false); //enable
			}			
		});	
		
	});
})(jQuery);

function populateDefaultPosition()
{
	if(waitingForCallback)
	{			
		clearTimeout(defPosQueue);
		window.opener.document.getElementById('nodedefaultPosition').value = document.getElementById('nodedefaultPosition').value;				
		addMacro();
	} else {
		defPosQueue = setTimeout('populateDefaultPosition()', 1000);
	}
}


var defaultLocation = '';
var width = '';
var zoomlevel = '';
var pushpins = '';
var direction = '';
var macroChanged = 0;

function loadStage(data)
{
	//do nothing
}

function getAdminSettings()
{
	var options = {};
	options.width = document.getElementById('width').value;
	options.height = document.getElementById('height').value; 
	options.zoomlevel = document.getElementById('zoomlevel').value;
	options.deflocation = document.getElementById('defLocate').value;
	return options;
}

function loadBingMap()
{
	var editor_el = window.opener.document.getElementById('edit-body-und-0-value');
	var content = editor_el.value;

	var bingmapMacro = content.match(/{bingmap.*?\}/i);	

	bingMapInfo = new Array();  
	if (bingmapMacro) {		
		defaultLocation = getAttribute('defaultlocation', bingmapMacro); 	
		width = getAttribute('width', bingmapMacro); 
		height = getAttribute('height', bingmapMacro); 
		zoomlevel = getAttribute('zoomlevel', bingmapMacro); 
		pushpins = getAttribute('pushpins', bingmapMacro); 
		direction = getAttribute('direction', bingmapMacro);

		document.getElementById('macroarea').value = bingmapMacro;
		handleApplyButton(true);

		fetchAdminData();
		setPushpins(pushpins);
		setWidth(width);
		setHeight(height);
		setZoomLevel(zoomlevel);
		setDirection(direction);
		var options = getAdminSettings();
		PreviewMap(options);
		
		if(pushpins == '' || pushpins == null)
			setDefaultLocation(defaultLocation);
		
	}
	
	else {
		GotoUserLocation('<?php echo $bingObj->getProperties()->getDefaultLocation();?>');
		
		addMacro();
	}
	
	
}

function handleApplyButton(flag)
{
	var applyBtnClass = flag ? 'form-submit' : 'form-submit button-disabled';
	document.getElementById('apply_macro').className = applyBtnClass;
	document.getElementById('apply_macro').disabled = !flag;
}

function getAttribute(attrib, tag){
	//get attribute from macro
	
	var pattern = attrib + '="([^"]*)?"';	
	var re = new RegExp(pattern, "ig");	
	var res = re.exec(tag);	
	if (res) {
		return res[1];
	} 
	return '';
}

function savePushPins()
{
	var re = /[^\s]/;
	var errors = [];

	if(pinLocations.length == 0)
		var pins = '';
	else
	{
		var pinLen = pinLocations.length;
		
		var out = '';
		for(var r=0;r<pinLen;r++)
		{
			var pinDt = <?php $bingObj->getPinDetails('r');?>
			var loc = pinLocations[r];
			out += pinDt + ';' + loc + '|';
		}
		var pins = formatArray(out);
	}

	document.getElementById('pinDet').value = pins;	
	if(!re.test(pins)){
		errors.push("The pushpin details are missing");				
	}			
	if(errors.length > 0){
		alert(errors.join("\n"));
		return false;
	}	
}

function setHeight(height)
{
	if(height == null || height == '' || height == 'height')
	{
		if(adminheight != null && adminheight != '')
			height = adminheight;
		else
			height = 480;
	}
	document.getElementById('height').value = height;
}

function setZoomLevel(zoomlevel)
{
	if(zoomlevel == null || zoomlevel == '' || zoomlevel == 'zoomlevel')
	{
		if(adminZoomLevel != null && adminZoomLevel != '')
			beginZoom = adminZoomLevel;
		else
			beginZoom = 10;
	}
	else
	{
		beginZoom = zoomlevel;
	}

	var optons = document.getElementById('zoomlevel').options;
	for(var q=0;q<optons.length;q++)
	{
		if(optons[q].value == beginZoom)
			optons[q].selected = true;
	}
}

function setWidth(width)
{

	if(width == null || width == '' || width == 'width')
	{
		if(adminwidth != null && adminwidth != '')
			width = adminwidth;
		else
			width = 640;
	}
	document.getElementById('width').value = width;
}

function addDefaultLocation()
{
	if(waitingForCallback)
	{			
		clearTimeout(defpinQueue);
		
		GotoUserLocation(document.getElementById('defLocate').value );
		afterLoad = true;
	} else {
		defpinQueue = setTimeout('addDefaultLocation()', 1000);
	}	
}

function setDefaultLocation(defLoc)
{	
	if(defLoc == null || defLoc == '' || defLoc == 'defaultLocation')
	{
		defLocation = adminDefaultLocation;
	}
	else
	{
		defLocation = defLoc;
	}

	document.getElementById('defLocate').value = defLocation;
		
	defpinQueue = setTimeout('addDefaultLocation()', 1000);
	clearTimeout(defpinQueue);

}



function setDirection(direction)
{
	if(direction == null || direction == '')
	{
		var from = '';
		var to = '';
	}
	else
	{
		var dir = direction.split(";");
		if(dir[0] == null || dir[0] == '' || dir[1] == null || dir[1] == '')
		{
			alert('Either \'from\' or \'to\' is not defined');
			return false;
		}
		else
		{
			var from = dir[0];
			var to = dir[1];
		}
	}

	document.getElementById('from').value = from; 
	document.getElementById('to').value = to;
}

function setPushpins(pushpins)
{
	if(pushpins != '' && pushpins != null)
	{
		var pins = pushpins.split("|");
		//loop through all the pushpin details
		if(pins.length > 0)
			for(var i=0;i<pins.length;i++)
			{
				var pindetails = pins[i].split(";");
				if(pindetails[2] == '' && pindetails[2] == null)
				{
					alert('Location details missing.');
					return false;
				}
				else
				{
					title[i] = pindetails[0];
					description[i] = pindetails[1];
					locationss[i] = pindetails[2];
				}
			}
	
		app=0;
		clearIdent = setInterval('checkWait()', 1000);
		
	}
}

function checkWait()
{
	if(app==locationss.length)
	{
		clearInterval(clearIdent);	
		setDefaultLocation(defaultLocation);	
		addMacro();		
		afterLoad = true;	
		return;
	}
	if(waitingForCallback)
	{
		FindAndAddPin('', title[app], description[app], locationss[app]);		
		var tit = title[app];
		var desc = description[app];
		
		var res = inArray(locationss[app],pinLocations);
	    if(res == false)
	    {
	    	minOut += title[app] + ';' + description[app] + ';' + locationss[app] + '|';
		    document.getElementById('pinDet').value = minOut;
	    	pinLocations[count] = locationss[app];
	    	count += 1;   
	    }
	    else
	    {
	        alert('Location already added');
	        return false;
	    }	    
		app++;
	}
}


function fetchAdminData()
{
	adminwidth = '<?php $configDetails = $bingObj->getconfigDetails(); if (isset($configDetails[0]->width))echo $configDetails[0]->width;?>';
	adminheight = '<?php $configDetails = $bingObj->getconfigDetails(); if (isset($configDetails[0]->height))echo $configDetails[0]->height;?>';
	adminZoomLevel = '<?php $configDetails = $bingObj->getconfigDetails(); if (isset($configDetails[0]->zoomLevel))echo $configDetails[0]->zoomLevel;?>';
	adminDefaultLocation = '<?php $prop = $bingObj->getProperties(); echo $prop->getDefaultLocation(); ?>';
}

function bAlert(event)
{
	var button;
    if (event.which == null)
       button= (event.button < 2) ? "LEFT" : ((event.button == 4) ? "MIDDLE" : "RIGHT");
    else
       button= (event.which < 2) ? "LEFT" : ((event.which == 2) ? "MIDDLE" : "RIGHT");

    if(button == 'RIGHT')
    {		
    	<?php	    	
    		$domElement = '<div class="MapContextMenu"><ul class="ulclass" style=""><li><a id="Add_a_pushpin" href="#" onClick="fnAddPushPin(); return false;">Add a pushpin</a></li><li><a id="aid_del_a_pushpin" href="#" onClick="deletePin(); return false;">Delete pushpin</a></li><li class="seperator"></li><li><a id="aid_default_a_pushpin" href="#" onClick="setUsrDefault(); return false;">Set as default location</a></li><li class="seperator"></li><li><a id="aid_Directions_from_here" href="#" onClick="fromDirection(); return false;">Directions from here</a></li><li><a id="aid_Directions_to_here" href="#" onClick="toDirection(); return false;">Directions to here</a></li></ul></div>';
    		$bingObj->addLayer($domElement);
	    ?>	
    }
}

function fnAddPushPin()
{
	<?php 
		$bingObj->getCurrentCoordinates('latLongBox');
	?>
	
	setTimeout("fnStage()", 500); //500 milli sec
}

function fnStage()
{
	var addPin = document.getElementById('latLongBox').value;
	if(addPin != null || addPin != '')
	{
		var tempPin = addPin.split(",");
		latitude = tempPin[0];
		longitude = tempPin[1];
	
		<?php 
			
			$domElement = '<div style="margin-bottom: 10px; background: none repeat scroll 0pt 0pt lightcyan;"><div style="margin-left: 5px; color: rgb(0, 0, 0);">Title:</div><div style="margin-left: 7px; margin-right: 7px;"><input id="gtitle" name="gtitle" type="text" /></div><div style="margin-left: 5px; background: none repeat scroll 0pt 0pt lightcyan; color: rgb(0, 0, 0);">Description:</div><div style="margin-left: 7px; margin-right: 7px;"><input id="gdescription" name="gdescription" type="text" /></div> <div style="margin-left: 4px; margin-bottom: 0px;"> <input type="submit" id="submitdetails" value="Save" onclick="fetchAndSaveDetails();"/></div></div>';
	    	$bingObj->addLayer($domElement); 
		?>
		getAddress();
	}		
}

function getAddress()
{
	<?php $bingObj->getCurrentAddress('resultBox');	?>
}

function fetchAndSaveDetails()
{
	var title = document.getElementById('gtitle').value;
	var description = document.getElementById('gdescription').value;
	var loc = document.getElementById('resultBox').value;
	
	if(title != '' || title != null || description != '' || description != null)
	{
		
	    
	    var res = inArray(loc,pinLocations);
	    if(res == false)
	    {
	    	minOut += title + ';' + description + ';' + loc + '|';
	    	document.getElementById('pinDet').value = minOut;
	    	pinLocations[count] = loc;
	    	<?php 
    	    	$bingObj->addPushpinLatLon('latitude', 'longitude', 'no');
    	    	$bingObj->attachDetails('title', 'description'); 
    	    ?>
	        count += 1;
	        
	        addMacro();
	        macroChanged = 1;
	    }
	    else
	    {
	        alert('Location already added');
	        return false;
	    }	   
	}
	
}    


function deletePin()
{
	var indx = <?php $bingObj->deletePinGraphical(); ?>
	if(typeof indx != 'boolean'){
		pinLocations.splice(indx,1);
		count -=1;
	} else {
		defLatLon = null;
		document.getElementById('defLocate').value = '';
	}	
	
	addMacro();
	macroChanged = 1;
	return false;
}


function setUsrDefault()
{
	document.getElementById('defLocate').value = '';
	<?php 
		$bingObj->getCurrentCoordinates('latLongBox');			
		$bingObj->getCurrentCoordinates('nodedefaultPosition');
	?>
	setTimeout("fnDStage()", 500); //500 milli sec
}

function fnDStage()
{
	var addPin = document.getElementById('latLongBox').value;
	if(addPin != null || addPin != '')
	{
		window.opener.document.getElementById('nodedefaultPosition').value = addPin;
		var tempPin = addPin.split(",");
		var latitude = tempPin[0];
		var longitude = tempPin[1];
		<?php $bingObj->addPushpinLatLon('latitude', 'longitude', 'yes'); ?>
		getAddress();
		setTimeout("fnD1Stage()", 1200); //1200 milli sec
	}
}

function fnD1Stage()
{
	defLatLon = document.getElementById('resultBox').value;
	document.getElementById('defLocate').value = defLatLon;
	
	addMacro();
	macroChanged = 1;
}

function fromDirection()
{
	<?php $bingObj->getCurrentAddress('resultBox');?>
	setTimeout("fnFromDirection()", 1000); //1000 milli sec
}

function fnFromDirection()
{
	document.getElementById('from').value = document.getElementById('resultBox').value;
	document.getElementById('resultBox').value = '';	
	DeleteControlAdminR();
	
	addMacro();
	macroChanged = 1;
}

function toDirection()
{
	<?php $bingObj->getCurrentAddress('resultBox');?>
	setTimeout("fnToDirection()", 1000); //1000 milli sec
}

function fnToDirection()
{
	document.getElementById('to').value = document.getElementById('resultBox').value;
	document.getElementById('resultBox').value = '';	
	DeleteControlAdminR();
	
	addMacro();
	macroChanged = 1;
}

function formatArray(arr)
{
	var modifiedStr = arr;
	var tr = modifiedStr.length - 1;
	modifiStr = modifiedStr.slice(0,tr);

	return modifiStr;                    
}

function addMacro()
{	
	var errors = [];	
	var re = /[^\s]/;	
	
	

	var re = /[^0-9]/;					
	if(re.test(document.getElementById('width').value) || re.test(document.getElementById('height').value)){
		errors.push("Please enter only numbers in width and height field");				
	}
	if(errors.length > 0){
		alert(errors.join("\n"));
		return false;
	}
	
	if(pinLocations.length == 0)
		var pins = '';
	else
	{
		var pinLen = pinLocations.length;
		//alert(pinLen);
		var out = '';
		for(var r=0;r<pinLen;r++)
		{
			var pinDt = <?php $bingObj->getPinDetails('r');?>
			var loc = pinLocations[r];
			out += pinDt + ';' + loc + '|';
		}
		var pins = formatArray(out);
	}
		
	if(pins == null || pins == '')
		var pinDetails = '';
	else
		var pinDetails = ' pushpins="'+pins+'" ';

	
	if(document.getElementById('defLocate').value == '' || document.getElementById('defLocate').value == null)
		var defaultDetails = '';
	else
		var defaultDetails = ' defaultlocation="'+document.getElementById('defLocate').value+'" ';
	

	var wid = document.getElementById('width').value;
	if(wid == null || wid == '')
		var width = '';
	else
		var width = ' width="'+wid+'" ';

	var ht = document.getElementById('height').value;
	if(ht == null || ht == '')
		var height = '';
	else
		var height = ' height="'+ht+'" ';
	
	var zD = document.getElementById('zoomlevel').value;
	if(zD == null || zD == '')
		var zoomDetails = '';
	else
		var zoomDetails = ' zoomlevel="'+zD+'" ';
	
	var from = document.getElementById('from').value;
	var to = document.getElementById('to').value;

	if((to == null || to == '') && (from == null || from == ''))
		direction = '';
	else
		direction = ' direction="' + from + ';' + to + '"';

		
	var macro = '{bingmap '+defaultDetails+width+height+zoomDetails+pinDetails+direction+'}';

	document.getElementById('macroarea').value = '';
	document.getElementById('macroarea').value = macro;
	
	

	var sa = document.getElementsByTagName("bingmap");
	
	if(sa.length>0)
	{
		var pushpins = sa[0].getAttribute("pushpins");

		if(pushpins.length <=0)
			return false;

		var pins = pushpins.split("|");
		
		var title = new Array();
		var description = new Array();
		var locationss = new Array();

		
		//loop through all the pushpin details
		for(var i=0;i<pins.length;i++)
		{
			var pindetails = pins[i].split(";");

			if(pindetails.length < 3)
			{
				alert('Location details missing.');
				return false;
			}
			else
			{
				title[i] = pindetails[0];
				description[i] = pindetails[1];
				locationss[i] = pindetails[2];
				var tempPin = locationss[i].split(",");
				var latitude = tempPin[0];
				var longitude = tempPin[1];
			}
		}
		
	}

	
	macroChanged = 0;
	handleApplyButton(true);  //enable apply	
	
}


function clearMacro()
{
	<?php 
		$bingObj->resetMap();
		$bingObj->displayMap();  
	?>
	document.getElementById('from').value = '';
	document.getElementById('to').value = '';
	document.getElementById('defLocate').value = AdminMapSettings.default_location;
	document.getElementById('zoomlevel').value = AdminMapSettings.zoomlevel;
	document.getElementById('width').value = AdminMapSettings.width;
	document.getElementById('height').value = AdminMapSettings.height;
	PreviewMap(AdminMapSettings);

	document.getElementById('macroarea').value = '';
	document.getElementById('resultBox').value = '';
	handleApplyButton(false);  //disable apply
	
	
	defaultFlag = 0;
	defLatLon = '';
	count = 0;
	minOut = '';
	pinLocations = [];
}

function copy_to_clipboard()  
{  
	var text = document.getElementById('macroarea').value;	
    if(window.clipboardData)  
    {    	
    	window.clipboardData.setData('text',text);
    	alert('The Bing map macro is copied to your clipboard.');  
    }        
} 


function insertAtCaret(txtarea,text) 
{	
	if(window.opener.cursorPosition.range || window.opener.cursorPosition.strPos)
	{
		var br = ((txtarea.selectionStart || txtarea.selectionStart == '0') ? "ff" : (document.selection ? "ie" : false ) ); 
		if (br == "ie") 
		{
			if(window.opener.cursorPosition.range)
			{		
				window.opener.cursorPosition.range.text = text;			
			} 		
		} 
		else if (br == "ff")
		{
			if(window.opener.cursorPosition.strPos)
			{
				var strPos = window.opener.cursorPosition.strPos;	
				var front = (txtarea.value).substring(0,strPos); 
				var back = (txtarea.value).substring(strPos,txtarea.value.length); 
				txtarea.value=front+text+back; strPos = strPos + text.length;
			}	
		} 
	} 
	else 
	{
		txtarea.value += text;
	}	
	 
} 



</script>
	
		<!-- Wrapper for all page content -->
<div id="pageBox">
	
		<!-- Main Content -->
	<form name="macroForm" action="<?php echo $_SERVER['PHP_SELF'].'?q=user/1/maps/bing/addmaps'; ?>" method="post" onSubmit="return savePushPins();">
	<div id="outer">				
			 <div class="set4">
                <div id="locimg" style="width: 100%;">
                  Graphical Location:
                </div>

                <div id="gratoggle">
                  <div id="sublabel" class="sublabel" style="margin-top: 6px; margin-bottom: 10px;"></div>
                  <div id="chkMap" style="position:relative; width:<?php echo $uWidth;?>px; height:<?php echo $uHeight;?>px;" onmouseup="return bAlert(event);"></div>
                  <input type="hidden" id="resultBox" name="resultBox" />
                  <input type="hidden" id="latLongBox" name="latLongBox" />
                 
                </div>
             </div>
             
             <div class="cleardiv" style="height: 15px;"></div>
             <div class="cleardiv" style="height: 15px;"></div>
             
             <div class="set3">
	              <div id="dirimg"  style="width: 100%;">Direction:</div>
	              
	              <div id="dirtoggle">
	                <div id="sublabel" class="sublabel" style="margin-top: 6px; margin-bottom: 10px;"></div>
	                <div class="detailitems" style="margin-left: 55px;">
	                  From: <input id="from" name="from" size="40" type="text" style="margin-left: 38px;" /><span style="font-size: 10px;">e.g. Redmond, wa</span>
	                </div>
	                <div class="detailitems" style="margin-left: 55px;">
	                  To: <input id="to" name="to" size="40" type="text" style="margin-left: 54px;" /><span style="font-size: 10px;">e.g. Seattle, wa</span>
	                </div>     
	              </div>
	         </div>
	         
	         <div class="cleardiv" style="height: 15px;"></div>	         
	         <div class="set2" style="">
                <div id="locimg" style="float: left; width: 132px;">
                  Default Location:
                </div>

                <div id="gratoggle" style="float: left; width: 500px;">
 	                  <input id="defLocate" size="40" name="defLocate" style="margin-top: 0px;" type="text" value=""/><span style="font-size: 10px;">e.g. Seattle, wa</span>
                </div>
             </div>
             
             <div class="cleardiv" style="height: 15px;"></div>	         
	         <div class="set2" style="">
                <div id="locimg" style="float: left; width: 132px;">
                  Width:
                </div>

                <div id="gratoggle" style="float: left; width: 500px;">
 	                  <input id="width" name="width" size="5" style="margin-top: 0px;" type="text" value="<?php $configDetails = $bingObj->getconfigDetails(); if (isset($configDetails[0]->width)) echo $configDetails[0]->width;?>"/><span style="font-size: 10px;">px</span>
                </div>
             </div>
             
             <div class="cleardiv" style="height: 40px;"></div>
	         
	         <div class="set2" style="">
                <div id="locimg" style="float: left; width: 132px;">
                  Height:
                </div>

                <div id="gratoggle" style="width: 500px;">
	                 <input id="height" name="height" size="5" style="margin-top: 0px;" type="text" value="<?php $configDetails = $bingObj->getconfigDetails(); if (isset($configDetails[0]->height)) echo $configDetails[0]->height;?>"/><span style="font-size: 10px;">px</span>
                </div>
             </div>
             
	         <div class="cleardiv" style="height: 10px;"></div>
	         
	         <div class="set2" style="height: 44px; margin-bottom: 0px;">
                <div id="locimg" style="float: left; width: 132px;">
                  Zoom Level:
                </div>

                <div id="gratoggle">
                       <select id="zoomlevel" name="zoomlevel" style="width: 65px;">
							<option value="">Select</option>
							<?php 
							$zLevel = '';
							$configDetails = $bingObj->getconfigDetails();
							if (isset($configDetails[0]->zoomLevel))
								$zLevel = $configDetails[0]->zoomLevel;
								
							for ($a=1;$a<=19;$a++)
							{	
								if ($a == $zLevel)
								{
							?>
								<option value="<?php echo $a;?>" selected="selected"><?php echo $a;?></option>
							<?php
								}
								else 
								{
							?>
								<option value="<?php echo $a;?>"><?php echo $a;?></option>
							<?php 
								}	
							}
							?>
							
						</select>
                </div>
             </div>
	         
	         
	         <div class="forbutton" style="margin-left: 0px; margin-top: 0px;">
				<input id="title" name="title" type="hidden" />
				<input id="description" name="description" type="hidden" />
				 <input type="hidden" id="pinDet" name="pinDet" />			
				 <input type="hidden" id="nodedefaultPosition" name="nodedefaultPosition" />	 			
               	
               	<!-- 
               	<input type="button" class="form-submit" value="Generate Macro" name="generateMacro" id="generateMacro" onclick="addMacro();" />
               	<input type="button" class="form-submit" value="Reset Map" name="clrMacro" id="clrMacro" onclick="clearMacro();" />
               	<input type="submit" class="form-submit" value="Save PushPins" name="saveMacro" id="saveMacro" />     
               	 -->     	
               	<!-- input type="button" name="copyMacro" id="copyMacro" value="Copy to Clipboard" onclick="copy_to_clipboard();"/-->
            </div>
            <br/>            		
			<div>Generated Macro</div>
            	<textarea id="macroarea" name="macroarea" rows="8" cols="85"></textarea>
			<br />  	
				<input type="button" class="form-submit button-disabled" value="Apply" name="apply_macro" id="apply_macro" disabled="disabled" />
				<input type="button" class="form-submit" value="Discard" name="discard" id="discard" onclick="window.close();" />
            	<div id="ajax-container"></div>  	
	</div>
	</form>
              
</div>