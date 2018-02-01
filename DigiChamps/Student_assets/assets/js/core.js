'use strict';

$(document).ready(function () {

    // leftbar toggle for minbar
    var nice = $(".left-bar").niceScroll(); 
    $('.menu-bar').click(function(){                  
        $(".wrapper").toggleClass('mini-bar');        

        $(".left-bar").getNiceScroll().remove();
        setTimeout(function() {
            $(".left-bar").niceScroll();
        }, 200);
    }); 
    
    // mobile menu
    $('.menu-bar-mobile').on('click', function (e) {        
        // $(this).addClass('menu_appear');
        $(".left-bar").getNiceScroll().remove();
        
        $( ".left-bar" ).toggleClass("menu_appear" );
        $( ".overlay" ).toggleClass("show" );
        setTimeout(function() {
          $(".left-bar").niceScroll();
        }, 200);
    });

    // orvelay hide menu
    $(".overlay").on('click',function(){
        $( ".left-bar" ).toggleClass("menu_appear" );
        $(this).removeClass("show");
    });

    // right side-bar toggle
    $('.right-bar-toggle').on('click', function(e){
        e.preventDefault();
        $('.wrapper').toggleClass('right-bar-enabled');
    });

    $('ul.menu-parent').accordion();

    new WOW().init(); 

    // PANELS
    // panel close
    $('.panel-close').on('click', function (e) {
        e.preventDefault();
        $(this).parent().parent().parent().parent().addClass(' animated fadeOutDown');
    });


    $('.panel-minimize').on('click', function (e) 
    {
        e.preventDefault();
        var $target = $(this).parent().parent().parent().next('.panel-body');
        if ($target.is(':visible')) {
            $('i', $(this)).removeClass('ti-angle-up').addClass('ti-angle-down');
        } else {
            $('i', $(this)).removeClass('ti-angle-down').addClass('ti-angle-up');
        }
        $target.slideToggle();
    });
    
    
    $('.panel-refresh').on('click', function (e) 
    {
        e.preventDefault();
        // alert('vj');
        var $target = $(this).closest('.panel-heading').next('.panel-body');
        $target.mask('<i class="fa fa-refresh fa-spin"></i> Loading...');

        setTimeout(function () {
            $target.unmask();
            console.log('ended');
        },1000);
    });

    //Active menu item expand automatically on reload or fresh open
    
    if (!$('.wrapper').hasClass("mini-bar") && $(window).width() > 1200) {
        $('.submenu li.active').closest('.submenu').addClass('current');
        var activeMenu = $('.submenu li.current').closest('ul').css('display','block');
    }
    


    if($(".mail_list").length > 0){
        $(".mail_list").niceScroll();    
    }

    if($(".mails_holder").length > 0){
        $(".mails_holder").niceScroll();    
    }

    if($(".mail_brief_holder").length > 0){
        $(".mail_brief_holder").niceScroll();    
    }
    
    if($("#paginator").length > 0){
        $('#paginator').datepaginator();
    }

    if($(".table-row").length > 0){
        $('.table-row').on('click', function(){
            // $('.table-row').removeClass('active');
            $(this).toggleClass('active');
        }); 
    }

    if($(".pick-a-color").length > 0){
        $(".pick-a-color").pickAColor({
          showSpectrum            : true,
            showSavedColors         : true,
            saveColorsPerElement    : true,
            fadeMenuToggle          : true,
            showAdvanced            : true,
            showBasicColors         : true,
            showHexInput            : true,
            allowBlank              : true,
            inlineDropdown          : true
        });    
    }

    if($('#colorPicker').length > 0){
        var $box = $('#colorPicker');
        $box.tinycolorpicker();    
    }

    if($('#picker').length > 0){
        $('#picker').colpick({
            flat:true,
            layout:'hex',
            submit:0
        });    
    }

    //var endDate = "June 7, 2087 15:03:25";
    //if($('.countdown.styled').length > 0){
    //    $('.countdown.styled').countdown({
    //        date: endDate,
    //        render: function(data) {
    //            $(this.el).html("<div>" + this.leadingZeros(data.years, 2) + " <span>years</span></div><div>" + this.leadingZeros(data.days, 2) + " <span>days</span></div><div>" + this.leadingZeros(data.hours, 2) + " <span>hrs</span></div><div>" + this.leadingZeros(data.min, 2) + " <span>min</span></div><div>" + this.leadingZeros(data.sec, 2) + " <span>sec</span></div>");
    //        }
    //    });    
    //}

    if($(".openNav").length > 0){
        $(".openNav").click(function() {
            $(this).toggleClass("open");
        });
    }
    
    if($("#elm1").length > 0){
        tinymce.init({
            selector: "textarea#elm1",
            theme: "modern",
            height:300,
            plugins: [
                "advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker",
                "searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking",
                "save table contextmenu directionality emoticons template paste textcolor"
            ],
            toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | l      ink image | print preview media fullpage | forecolor backcolor emoticons", 
            style_formats: [
                {title: 'Bold text', inline: 'b'},
                {title: 'Red text', inline: 'span', styles: {color: '#ff0000'}},
                {title: 'Red header', block: 'h1', styles: {color: '#ff0000'}},
                {title: 'Example 1', inline: 'span', classes: 'example1'},
                {title: 'Example 2', inline: 'span', classes: 'example2'},
                {title: 'Table styles'},
                {title: 'Table row 1', selector: 'tr', classes: 'tablerow1'}
            ]
        });    
    }  
});