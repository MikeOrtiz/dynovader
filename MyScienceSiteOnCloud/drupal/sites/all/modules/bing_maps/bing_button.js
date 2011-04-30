// $Id: bing_button.js $

var flag = 0;
var cursorPosition = {}; 
(function ($) {
	$(document).ready(function() {

		$('#edit-body-und-0-value').bind('keydown keyup mousedown mouseup', function(){			
			storeCaret('edit-body-und-0-value');
		});
		
		changeButtonLabel();
		document.getElementById('bingbutton').onclick = function(){
			
			var editNodeId = document.getElementsByName('editNodeId');

			var nId = editNodeId[0].value;		
			
			var uri = "?q=user/1/maps/bing/addmaps";
			if(nId != '' && nId != null){
				uri += "&nId="+nId;				
			}	
			
			window.open( uri,'Add_macro','width=900,height=700,left=10,top=10,scrollbars=yes,location=0,resizable=yes,toolbar=no,directories=no,menubar=no');
			
			return false;
		}
	});	
 
})(jQuery);

function changeButtonLabel()
{
	var content = document.getElementById('edit-body-und-0-value').value;	
	var bingmapMacro = content.match(/{bingmap.*?\}/i);
	if (bingmapMacro) {				
		var button_el = document.getElementById('bingbutton');
		button_el.value = 'Edit Bing Maps Macro';		
	}	
}

function storeCaret(areaId) { 
	var txtarea = document.getElementById(areaId);	 
	var br = ((txtarea.selectionStart || txtarea.selectionStart == '0') ? "ff" : (document.selection ? "ie" : false ) ); 
	if (br == "ie") {		 
		cursorPosition.range = document.selection.createRange();			
	} else if (br == "ff"){
		cursorPosition.strPos = txtarea.selectionStart;	
		
	}	
} 