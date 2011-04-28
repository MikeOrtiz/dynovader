<script type="text/javascript">

var adminwidth = '';
var adminheight = '';
var adminZoomLevel = '';
var adminDefaultLocation = '';

var title = new Array();
var description = new Array();
var locations = new Array();

var app;
var clearIdent="";
var defpinQueue="";

function showMap() {

  if(position != '' && position != null)	
  {
	 setDefaultPoisition(position);
	 
  }

	
  if(direction == null || direction == '') {
    var from = '';
    var to = '';
  } else {
    var dir = direction.split(";");
    if(dir[0] == null || dir[0] == '' || dir[1] == null || dir[1] == '') {
      alert('Either \'from\' or \'to\' is not defined');
      return false;
    } else {
      var from = dir[0];
      var to = dir[1];
    }
  }

  if(jQuery("#bingmap").length > 0 || jQuery("#BingMap").length > 0) {
    <?php $bingObj->displayMap(); ?>
    if(defaultPositionNotSaved == 1){
		GotoUserLocation(defaultLocation);
	}
    if ((from != null && from != '') || (to != null && to != '')) {
      <?php
        $bingObj->showDirections('from', 'to');
      ?>
    }

    if(pushpins.length>0) {
        var pins = pushpins.split("|");

        //loop through all the pushpin details
        for(var i=0;i<pins.length;i++) {
          var pindetails = pins[i].split(";");

          if (pindetails.length < 3) {
            alert('Location details missing.');
            return true;
          } 
          else {
            title[i] = pindetails[0];
            description[i] = pindetails[1];
            locations[i] = pindetails[2];
          }
        }
        app=0;
        clearIdent = setInterval('checkWait()', 2000);
    }
    /*
    else
    {
    	defpinQueue = setTimeout('AddDefaultPin()', 1000);
    }
    */

   
  }
}

function checkWait() {
  if (app==locations.length) {
	//defpinQueue = setTimeout('AddDefaultPin()', 1000);
	clearTimeout(defpinQueue);	
    clearInterval(clearIdent);
    return;
  }
  if (waitingForCallback) {
    FindAndAddPin('', title[app], description[app], locations[app]);
    app++;
  }
}

function AddDefaultPin()
{
	var re = /[^\s]/;	
	if(waitingForCallback)
	{			
		clearTimeout(defpinQueue);		
		if(re.test(defaultLocation)){			
			GotoUserLocation(defaultLocation);					
		}
	} else {
		defpinQueue = setTimeout('AddDefaultPin()', 1000);
	}
}
</script>
