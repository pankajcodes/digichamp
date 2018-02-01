$(document).ready(function(){
    $('.lightbox_trigger').click(function(e) {

        //prevent default action (hyperlink)
        e.preventDefault();
        
        //Get clicked link href
        var image_href = $(this).attr("href");
                        
        //create HTML markup for lightbox window
        var lightbox = 
        '<div id="lightbox">' +
            '<div class="lightbox-content">' + //insert clicked link's href into img src
                '<a href="#" class="img_close"><i class="ion ion-ios-close-empty"></i></a>' +
                '<img src="' + image_href +'" />' +
            '</div>' +  
        '</div>';
            
        //insert lightbox HTML into page
        $('#lightbox').remove();
        $('body').append(lightbox);
        

        // Click anywhere on the page to get rid of lightbox window
        $('.img_close').on('click', function(e) { //must use live, as the lightbox element is inserted into the DOM
            // $('#lightbox').hide();
           e.preventDefault();
           $('#lightbox').hide(); 
        });
        
    });

    $(document).keyup(function(e){
        if(e.keyCode === 27){
            $('#lightbox').hide();
        }
    });
});