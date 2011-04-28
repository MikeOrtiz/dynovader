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
  	
	if (isset($_POST['save_det']))
	{ 
		$bingid = $_POST['bingid'];
		$bingObj->setBingID($bingid);
		
		$width = $_POST['width'];
		$height = $_POST['height'];
		$zoomlevel = $_POST['zoomlevel'];
		$location = $_POST['location'];
		
		$contentTypeList = $_POST['content_type'];
		$content_types = '';
		foreach ($contentTypeList as $type)
             $content_types .= $type.',';
        $content_types = rtrim($content_types, ",");
        		
		$prop = new Msft_Bing_MapProperties($width,$height,$zoomlevel);
		$prop->setDefaultLocation($location);
		
		$prop->setContentType($content_types);	
		$bingObj->setProperties($prop);	

	  	$bingObj->saveConfiguration();
	  	$configResult = $bingObj->loadConfiguration();
	  	if(isset($configResult))
	  		$configDetails = $bingObj->getconfigDetails();
	}
	
	$configDetailsArr = array();
	if ($configDetails != '' && $configDetails != null)
		$configDetailsArr = explode(",", $configDetails[0]->content_type);

$adminMapWidth  = isset($configDetails[0]->width) ? $configDetails[0]->width : '640';
$adminMapHeight = isset($configDetails[0]->height) ? $configDetails[0]->height : '480';		
?>
			
<script type="text/javascript">
	
	(function ($) {
		$(document).ready(function() {
			
			if ($("#adminMap").length > 0){
				<?php $bingObj->displayMap(); ?>
			}

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

					
		});

		function getAdminSettings()
		{
			var options = {};
			options.width = $('#width').val();
			options.height = $('#height').val(); 
			options.zoomlevel = $('#zoomlevel').val();
			return options;
		}

	})(jQuery);	

	
	
	function saveDetails()
	{		
		var re = /[^\s]/;
		var reNum = /[^0-9]/;
		var errors = [];
		document.getElementById('save_det').value=0;

		
		if(!re.test(document.getElementById('bingid').value) || !re.test(document.getElementById('width').value) || !re.test(document.getElementById('height').value) || !re.test(document.getElementById('zoomlevel').value) || typeof(document.getElementById('content_type').value) == null || !re.test(document.getElementById('content_type').value) )
		{
			errors.push("Please provide details for all mandatory fields");				
		} 
		else if(reNum.test(document.getElementById('width').value) || reNum.test(document.getElementById('height').value))
		{
			errors.push("Please enter only numbers in width and height field");				
		}		
		else if(!re.test(document.getElementById('location').value)) 
		{
			errors.push("Please set the default location");
		}	
		
		if(errors.length > 0){
			alert(errors.join("\n"));
			return false;
		}
		credentialFlag = null;
		ValidateBing(document.getElementById('bingid').value);			
		bingIdTester = setInterval ( "processDetails()", 1000 );
	}

	function processDetails(){
		if(credentialFlag != null){						
			clearInterval(bingIdTester);
			if(credentialFlag){			
				document.getElementById('save_det').value=1;
				document.getElementById('adminForm').submit();				
			}		
		}		
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
	    		$domElement = '<div class="MapContextMenu"><a id="aid_Add_a_pushpin" href="#" style="padding-left: 5px; padding-right: 5px;" onClick="fnClicked(); return false;">Set as default location</a></div>';
	    		$bingObj->addLayer($domElement); 
		    ?>
	    		
	    }
	}
	
	function fnClicked()
	{
		
		<?php 
			$bingObj->getCurrentCoordinates('latLongResult');				
		?>
		setTimeout("fnStage()", 1000); //1000 milli sec
	}
	
	function fnStage()
	{
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

	function getAddress()
	{
		<?php $bingObj->getCurrentAddress('resultBox');	?>
		setTimeout("fnD1Stage()", 1000); //1000 milli sec	
	}

	function fnD1Stage()
	{
		document.getElementById('location').value = document.getElementById('resultBox').value;
	}
	
</script>

		<!-- Wrapper for all page content -->
<div id="pageBox">
		
		<!-- Main Content -->
	<div id="content">
	
			<div class="formap" style="width: 100%; margin-bottom: 35px;">			
			   	<div id="adminMap" style="position:relative; width:<?php echo $adminMapWidth;?>px;height:<?php echo $adminMapHeight;?>px;" onmouseup="return bAlert(event);">
			   	</div>
			   	<input type="hidden" id="resultBox" name="resultBox" />
			</div>	
					
			<form name="adminForm" id="adminForm" action="<?php echo $_SERVER['PHP_SELF'].'?q=user/1/maps/bing'; ?>" method="post">		
				<div class="frmmain">
					<div class="eachline" style="width: 100%; height: 38px; float: left;">
						<div class="forlabel" style="float: left; width: 85px; margin-left: 96px; border-right-width: 0px; height: 40px;"><label style="margin-right: 0px; float: left;">Bing ID:</label><label style="width: 8px; color: orange; margin-left: 0px; margin-right: 0px; float: left;">*</label></div>
							<div class="forinput" style="margin-left: 175px;">
								<input id="bingid" name="bingid" type="text" style="float: left;" value="<?php if (isset($configDetails[0]->bingKey)) echo $configDetails[0]->bingKey;?>"/>
							
								<div style="float: left; margin-left: 17px; width: 240px;"><a href="https://www.bingmapsportal.com" target="_blank" style="text-decoration: underline;">Don't have a Bing ID? Register here</a></div>
							</div>							
					</div>
					
					<div class="eachline" style="width: 100%; height: 38px; float: left;">
						<div class="forlabel" style="float: left; width: 100px; margin-left: 30px; border-right-width: 0px; height: 40px;"><label style="margin-right: 0px; float: left;white-space:nowrap;">Default Location:<span style="width: 6px; color: orange;margin-left:2px">*</span></label></div>
							<div class="forinput" style="margin-left: 180px;"><input id="location" name="location" type="text" value="<?php if (isset($configDetails[0]->location)) echo $configDetails[0]->location;?>"/></div>
					</div>
										
					<div class="eachline" style="width: 100%; height: 38px; float: left;">
						<div class="forlabel" style="float: left; width: 100px; margin-left: 74px; border-right-width: 0px; height: 40px;"><label style="margin-right: 0px; float: left;">Map Width:</label><label style="width: 8px; color: orange; margin-left: 0px; margin-right: 0px; float: left;">*</label></div>
							<div class="forinput" style="margin-left: 180px;"><input id="width" name="width" type="text" value="<?php if (isset($configDetails[0]->width)) echo $configDetails[0]->width;?>"/><span style="font-size: 10px;">px</span></div>
					</div>
					<div class="eachline" style="width: 100%; height: 38px; float: left;">    
					    <div class="forlabel" style="float: left; width: 112px; margin-left: 70px; border-right-width: 0px; height: 40px;"><label style="margin-right: 0px; float: left;">Map Height:</label><label style="width: 8px; color: orange; margin-left: 0px; margin-right: 0px; float: left;">*</label></div>
							<div class="forinput" style="margin-left: 180px;"><input id="height" name="height" type="text" value="<?php if (isset($configDetails[0]->height)) echo $configDetails[0]->height;?>"/><span style="font-size: 10px;">px</span></div>
					</div>
					<div class="eachline" style="width: 100%; height: 38px; float: left;">	
						<div class="forlabel"  style="float: left; width: 109px; margin-left: 68px; border-right-width: 0px; height: 40px;"><label style="margin-right: 0px; float: left;">Zoom Level:</label><label style="width: 8px; color: orange; margin-left: 0px; margin-right: 0px; float: left;">*</label></div>
							<div class="forinput" style="margin-left: 180px;">
								<select id="zoomlevel" name="zoomlevel">
									<option value="">Select</option>									
									<?php 
									for ($a=1;$a<=19;$a++)
									{	
										if(isset($configDetails[0]->zoomLevel) && $a == $configDetails[0]->zoomLevel)
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
							
					<div class="eachline" style="width: 370px; float: left; height: 80px;">	
						<div class="forlabel"  style="margin-left: 50px; float: left; width: 265px;"><label style="margin-right: 0px; float: left;">Associate with content type(s):</label><label style="width: 8px; color: orange; margin-left: 0px; margin-right: 0px; float: left;">*</label></div>
							<div class="forinput" style="margin-left: 180px; margin-top: 0px;">
								<select MULTIPLE id="content_type" name="content_type[]" size="3" style="width: 100px;">
									<?php 
									$nodetype = node_type_get_types();
									foreach($nodetype as $key => $val)
										foreach($val as $k =>$v)
										{
											if($k == 'name')
											{
												if (!empty($configDetailsArr) && in_array($v, $configDetailsArr))
												{
									?>
												<option value="<?php echo $v;?>" selected="selected"><?php echo $v;?></option>
									<?php
												}
												else 
												{
									?>
												<option value="<?php echo $v;?>"><?php echo $v;?></option>
									<?php 	
												}
											}
										} 
									?>
								</select>
							</div>
					</div>				
					
					<div class="eachline" style="width: 100%; float: left; height: 65px;">
						<div class="forlabel"  style="float: left; width: 100px;"></div>
							<input id="latitude" name="latitude" type="hidden" />
							<input id="longitude" name="longitude" type="hidden" />							
							<input id="latLongResult" name="latLongResult" type="hidden" value=""/>
							<input id="save_det" name="save_det" type="hidden" value=""/>
							
							<div class="forbutton" style="margin-top: 35px;">
								<input type="button" id="adminSave" name="adminSave" value="Save"  onClick="saveDetails()"/>
							</div>
					</div>
				</div>
			</form>
							
	</div>	
		<!-- DO NOT REMOVE THIS DIV -->
		<div id="clear"></div>
</div>