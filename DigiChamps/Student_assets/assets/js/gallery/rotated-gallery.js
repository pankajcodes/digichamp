$(window).load(function () {


var animationTime = 500; 


$('.gallery-tab-title-holder-text').click(function() {
	
	if($(this).parent().parent().next().is(":visible")){
			$(this).parent().animate({"left": -45}, animationTime);
		}else{
			$(this).parent().animate({"left": 50}, animationTime);	
		}
		
	$(this).parent().parent().next().slideToggle(animationTime);
	
});






$('.gallery-tab-images-slider').each(function(slider_index, slider) {
	
	var total_width = 0;
   
	$(this).children('.gallery-tab-images-slider-tab').children('img').each(function(image_index, image) {
		
	$(this).attr('data-height',image.height);
	$(this).attr('data-width',image.width);
	var ratio = image.width/image.height;
	var actual_width = Math.round(ratio*220);

	total_width+= actual_width+20;
	 
	});
	
	$(this).width(total_width);

});





$('.next').click(function() {
	if($(this).parent().parent().parent().next().is(":visible")){
	var this_slider = $(this).parent().parent().parent().next().children('.gallery-tab-images-slider')
		
		if((this_slider.position().left)>(-(this_slider.width()-$('#gallery').width()))){
			this_slider.animate({"left": (this_slider.position().left)-329}, 200);
		}
	}
	
	
});



$('.previous').click(function() {
	
	if($(this).parent().parent().parent().next().is(":visible")){
	var this_slider = $(this).parent().parent().parent().next().children('.gallery-tab-images-slider')
	
		if((this_slider.position().left)<-200){
			this_slider.animate({"left": (this_slider.position().left)+300}, 200);
		}
		
		
	
	}
});


$('#popup-box-close').click(function() {
    $('#popup').fadeOut(200);
 
});




$(".gallery-tab-images-slider-tab-hover").click(function() {

	 $("#popup-box-image").attr("src", $(this).next('img').attr("src"));
	 $("#popup-box-image").css('width',$(this).next('img').attr("data-width")).css('height',$(this).next('img').attr("data-height"));
	 $("#popup-box").css('width',$(this).next('img').attr("data-width")).css('height',$(this).next('img').attr("data-height"));


 	 $('#popup').fadeIn(200);
	 

 
});


	


});