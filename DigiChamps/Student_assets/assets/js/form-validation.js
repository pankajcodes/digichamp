(function($, window) {

    var dev = '.dev'; //window.location.hash.indexOf('dev') > -1 ? '.dev' : '';

    // setup datepicker
    $("#datepicker").datepicker();

    // Add a new validator
    $.formUtils.addValidator({
    	name : 'even_number',
    	validatorFunction : function(value, $el, config, language, $form) {
    		return parseInt(value, 10) % 2 === 0;
    	},
    	borderColorOnError : '',
    	errorMessage : 'You have to give an even number',
    	errorMessageKey: 'badEvenNumber'
    });

    window.applyValidation = function(validateOnBlur, forms, messagePosition) {
    	if( !forms )
    		forms = 'form';
    	if( !messagePosition )
    		messagePosition = 'top';

    	$.validate({
    		form : forms,
    		language : {
    			requiredFields: 'You must select this one'
    		},
    		validateOnBlur : validateOnBlur,
    		errorMessagePosition : messagePosition,
    		scrollToTopOnError : true,
    		borderColorOnError : 'purple',
    		modules : 'security'+dev+', location'+dev+', sweden'+dev+', html5'+dev+', file'+dev+', uk'+dev,
    		onModulesLoaded: function() {
    			$('#country-suggestions').suggestCountry();
    			$('#swedish-county-suggestions').suggestSwedishCounty();
    			$('#password').displayPasswordStrength();
    		},
    		onValidate : function($f) {

    			console.log('about to validate form '+$f.attr('id'));

    			var $callbackInput = $('#callback');
    			if( $callbackInput.val() == 1 ) {
    				return {
    					element : $callbackInput,
    					message : 'This validation was made in a callback'
    				};
    			}
    		},
    		onError : function($form) {
    			// alert('Invalid '+$form.attr('id'));
    		},
    		onSuccess : function($form) {
    			alert('Valid '+$form.attr('id'));
    			return false;
    		}
    	});
    };

    $('#text-area').restrictLength($('#max-len'));

    window.applyValidation(true, '#form-a', 'top');
    window.applyValidation(false, '#form-b', 'element');
    window.applyValidation(true, '#form-c', $('#error-container'));
    window.applyValidation(true, '#form-d', 'element');

    // Load one module outside $.validate() even though you do not have to
    $.formUtils.loadModules('date'+dev+'.js', false, false);

    $('input').on('beforeValidation', function() {
    	console.log('About to validate input "'+this.name+'"');
    }).on('validation', function(evt, isValid) {
    	var validationResult = '';
    	if( isValid === null ) {
    		validationResult = 'not validated';
    	} else if( isValid ) {
    		validationResult = 'VALID';
    	} else {
    		validationResult = 'INVALID';
    	}
    	console.log('Input '+this.name+' is '+validationResult);
    });

})(jQuery, window);