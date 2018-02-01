
$(document).ready(function () {

//Update Header Style and Scroll to Top startF
    $(window).scroll(function () {
	if ($(this).scrollTop() > 90) {
	    $('.site-header').addClass("fixed-header");
	} else {
	    $('.site-header').removeClass("fixed-header");
	}
    });
//Update Header Style and Scroll to Top startF

//scroll down start
    $(".scroll-down").on("click", function (e) {
	e.preventDefault();
	$("html,body").animate({scrollTop: 600}, 1500);
    });
//scroll down end

//on menu btn hover start
    $('.nav-toggler').on('mouseenter', function () {
	$('.menu-overlay').fadeIn(100);
	$('.nav-toggler').on('mouseleave', function () {
	    $('.menu-overlay').fadeOut(100);
	});
    });


//on menu btn hover end


    $(".sign-in").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#sign_up_form").get(0).reset();
        $("#login_form").get(0).reset();
        $(".pre-pop").show();
        $(".sc-pop").hide();
        $(".login-sing-overlay").fadeIn();
            $(".s-popup").fadeIn();
        });

    $(".login-click").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#sign_up_form").get(0).reset();
        $(".l-popup").fadeIn();
    });

    $(".click-login").on('click', function () {
        debugger
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#sign_up_form").get(0).reset();
        $("#login_form").get(0).reset();
	$(".s-popup").fadeOut();
	$(".l-popup").fadeIn();
    });

    $(".click-sign").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#login_form").get(0).reset();
	$(".l-popup").fadeOut();
	$(".s-popup").fadeIn();
    });

    $(".f-get-pass").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#login_form").get(0).reset();
        $("#sign_up_form").get(0).reset();
        $(".l-popup").fadeOut();
        $(".s-popup").fadeOut();
	$(".for-password").fadeIn();
    });

    $("#next_btn").on('click', function () {
        debugger;
        $("span.error_alert_message").css('display', 'none');
        if ($("#sign_mob").val() != null && $("#sign_mob").val() != "") {
            if ($("input[name='name']").val() != null && $("input[name='name']").val() != "") {
                if ($("input[name='email']").val() != null && $("input[name='email']").val() != "") {
                 

                        $(".pre-pop").fadeOut(300);
                        $(".sc-pop").fadeIn(300);
                   
                }
                else {
                    $('#sign_up_form').find("span.error_alert_message").html("Please enter email id.");
                    $('#sign_up_form').find("span.error_alert_message").css('display', 'block');
                }
            }
            else {
                $('#sign_up_form').find("span.error_alert_message").html("Please enter name.");
                $('#sign_up_form').find("span.error_alert_message").css('display', 'block');
            }
        }
        else {
            $('#sign_up_form').find("span.error_alert_message").html("Please enter mobile number.");
            $('#sign_up_form').find("span.error_alert_message").css('display', 'block');
        }
    });




    $(".close-popup").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#sign_up_form").get(0).reset();
        $("#login_form").get(0).reset();
	$(".l-popup").fadeOut();
	$(".s-popup").fadeOut();
    });

    $(".back-login").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#forgot_form").get(0).reset();
        $('#forgot_form').find("p.per-mobile").html("");
        $(".for-password").fadeOut();
        $(".l-popup").fadeIn();
    });

    $(".close-fp").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("#forgot_form").get(0).reset();
        $("form").find("span.help-block").html("");
        $('#forgot_form').find("p.per-mobile").html("");
	$(".for-password").fadeOut();
	$(".l-popup").fadeIn();
    });

    $(".close-otp-popup").on('click', function () {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $("form").find("span.help-block").html("");
        $("#otp_form").get(0).reset();
        $("#forgot_form").get(0).reset();
        $("#board_form").get(0).reset();
	$(".login-sing-overlay").fadeOut();
	$(".otp-popup").fadeOut();
    });

    $(".select-popup").on('click', function () {
        $(".login-sing-overlay").fadeOut();
        $(".select-course-and-board-popup").fadeOut();
    });

//student slider start
    var owl = $("#owl-demo2");
    owl.owlCarousel({
	items: 1,
	itemsDesktop: [1, 1],
	itemsDesktopSmall: [1, 1],
	itemsTablet: [1, 1],
	itemsMobile: true,
	pagination: false,
	control: false,
	touchDrag: false,
	mouseDrag: false,
	animateOut: 'fadeOut',
    dots: true
    });

    // 1) ASSIGN EACH 'DOT' A NUMBER
    dotcount = 1;

    jQuery('#owl-demo2 .owl-dot').each(function () {
        jQuery(this).addClass('dotnumber' + dotcount);
        jQuery(this).attr('data-info', dotcount);
        dotcount = dotcount + 1;
    });

    // 2) ASSIGN EACH 'SLIDE' A NUMBER
    slidecount = 1;

    jQuery('#owl-demo2 .owl-item').not('.cloned').each(function () {
        jQuery(this).addClass('slidenumber' + slidecount);
        slidecount = slidecount + 1;
    });

    // SYNC THE SLIDE NUMBER IMG TO ITS DOT COUNTERPART (E.G SLIDE 1 IMG TO DOT 1 BACKGROUND-IMAGE)
    jQuery('#owl-demo2 .owl-dot').each(function () {

        grab = jQuery(this).data('info');

        slidegrab = jQuery('.slidenumber' + grab + ' img').attr('src');
        //console.log(slidegrab);

        jQuery(this).css("background-image", "url(" + slidegrab + ")");

    });

    // THIS FINAL BIT CAN BE REMOVED AND OVERRIDEN WITH YOUR OWN CSS OR FUNCTION, I JUST HAVE IT
    // TO MAKE IT ALL NEAT 
    amount = jQuery('#owl-demo2 .owl-dot').length;
    gotowidth = 100 / amount;

    jQuery('#owl-demo2 .owl-dot').css("width", gotowidth + "%");
    newwidth = jQuery('.owl-dot').width();
    jQuery('#owl-demo2 .owl-dot').css("height", newwidth + "px");
    // Custom Navigation Events
    $(".next-slide").click(function () {
	owl.trigger('next.owl.carousel');
    });
    $(".prev-slide").click(function () {
	owl.trigger('prev.owl.carousel');
    });
//student slider End

//video slider satrt
    var owl2 = $("#owl-demo3");
    owl2.owlCarousel({
	items: 3,
	pagination: false,
	autoplay: true,
	loop: false,
	margin: 27,
	touchDrag: false,
	mouseDrag: false,
	responsiveClass: true,
	responsive: {
	    0: {
		items: 1
	    },
	    600: {
		items: 3
	    },
	    1000: {
		items: 3
	    }
	}
    });
    // Custom Navigation Events
    $(".next-slide-video").click(function () {
	owl2.trigger('next.owl.carousel');
    });
    $(".prev-slide-video").click(function () {
	owl2.trigger('prev.owl.carousel');
    });
    // video slider end

    
    //team slider satrt
    var owlteam = $("#team-slider");
    owlteam.owlCarousel({
        items: 1,
        pagination: false,
        autoplay: false,
        loop: true,
        margin: 0,
        dots: false,
        touchDrag: false,
        mouseDrag: false,
        responsiveClass: true,
        responsive: {
            0: {
                items: 1
            },
            600: {
                items: 1
            },
            1000: {
                items: 1
            }
        }
    });
    // Custom Navigation Events
    $(".next-slide-team").click(function () {
        owlteam.trigger('next.owl.carousel');
    });
    $(".prev-slide-team").click(function () {
        owlteam.trigger('prev.owl.carousel');
    });
    // team slider end

// pop-up-video start
    jQuery(function () {
	jQuery('.popup-player').magnificPopup({
	    type: 'iframe',
	    mainClass: 'mfp-fade',
	    fixedContentPos: false
	});
    });
// pop-up-video end

//number counter start

    $('.Count').each(function () {
	$(this).prop('Counter', 0).animate({
	    Counter: $(this).text()
	}, {
	    duration: 1000,
	    easing: 'swing',
	    step: function (now) {
		$(this).text(Math.ceil(now).toString().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,"));
	    }
	});
    });
    //number counter end

    // you want to enable the pointer events only on click start;

    $('#map_canvas1').addClass('scrolloff'); // set the pointer events to none on doc ready

    $('#canvas1').on('click', function () {
        $('#map_canvas1').removeClass('scrolloff'); // set the pointer events true on click
    });
    // you want to disable pointer events when the mouse leave the canvas area;

    $("#map_canvas1").mouseleave(function () {
        $('#map_canvas1').addClass('scrolloff'); // set the pointer events to none when mouse leaves the map area
    });

    // you want to enable the pointer events only on click end;

    //Sonali Start
    $('#login_form').submit(function (e) {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        e.preventDefault();
        if ($(this).valid()) {
        $(".loader-ajax-container").css("display", "block");
                $.ajax({
                    type: 'POST',
                    url: "/Student/Index",
                    data: $('#login_form').serialize(),
                    success: function (data) {
                        if (data == "1") {
                            $('#login_form').find("span.success_alert_message").html("Successfully logged in.");
                            $('#login_form').find("span.success_alert_message").css('display', 'block');
                            $("#login_form").get(0).reset();
                            location.href = "/student/studentdashboard";
                        }
                        else if (data == "0") {
                            $("#login_form").get(0).reset();
                            $(".l-popup").fadeOut(1200);
                            $(".select-course-and-board-popup").fadeIn(1200);
                        }
                        else if (data == "-1") {
                            $('#login_form').find("span.error_alert_message").html("Please complete the registration process to continue.");
                            $('#login_form').find("span.error_alert_message").css('display', 'block');
                            setTimeout(function () {
                                $("#login_form").get(0).reset();
                                $(".l-popup").fadeOut(1200);
                                $(".for-password").fadeIn(1200);
                            }, 2100);
                        }
                        else if (data == "-2") {
                            $('#login_form').find("span.error_alert_message").html("Something went wrong.");
                            $('#login_form').find("span.error_alert_message").css('display', 'block');
                        }
                        else if (data == "-3") {
                            $('#login_form').find("span.error_alert_message").html("Oops!! Wrong Password.");
                            $('#login_form').find("span.error_alert_message").css('display', 'block');
                        }
                        else if (data == "-4") {
                            $('#login_form').find("span.error_alert_message").html("Mobile number is not yet registered.");
                            $('#login_form').find("span.error_alert_message").css('display', 'block');
                        }
                        else if (data == "-5") {
                            $('#login_form').find("span.error_alert_message").html("Login is blocked, Please contact Digichamps.");
                            $('#login_form').find("span.error_alert_message").css('display', 'block');
                        }
                        else {
                            $('#login_form').find("span.error_alert_message").html(data);
                            $('#login_form').find("span.error_alert_message").css('display', 'block');
                        }
                        setTimeout(function () {
                            $(".loader-ajax-container").css("display", "none");
                        }, 2000);
                    }
                });
            }
    });
    
    $('#sign_up_form').submit(function (e) {
        var sign_up_form = $(this).html();
        if ($('#login_form').find(".per-mobile").length > 0) { $('#login_form').find(".per-mobile").remove(); }
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        $('#forgot_form').find("p.per-mobile").html("");
        e.preventDefault();
        var mobile = $("#sign_mob").val();
        var gender = document.getElementsByName('gender');
        var genValue = false;

        for (var i = 0; i < gender.length; i++) {
            if (gender[i].checked == true) {
                genValue = true;
            }
        }
        if (!genValue) {
            $('#sign_up_form').find("span.error_alert_message").html("Please select your gender.");
            $('#sign_up_form').find("span.error_alert_message").css('display', 'block');
            return false;
        }

        else {


            if ($("input[name='txtdob']").val() != null && $("input[name='txtdob']").val() != "") {

                if ($(this).valid()) {
                    $(".loader-ajax-container").css("display", "block");
                    $.ajax({
                        type: 'POST',
                        url: "/Student/student_signup",
                        data: $('#sign_up_form').serialize(),
                        success: function (data) {
                            if (data == "1") {
                                $('#sign_up_form').find("span.success_alert_message").html("Thank you for Signing up, Confirm OTP to continue.");
                                $('#sign_up_form').find("span.success_alert_message").css('display', 'block');
                                $('.otp-popup').find("span.number-block").html(mobile + "<i> and Email Id</i>");
                                setTimeout(function () {
                                    $("#sign_up_form").get(0).reset();
                                    $(".s-popup").fadeOut();
                                    $(".otp-popup").fadeIn();
                                }, 2000);
                            }
                            else if (data == "-1") {
                                $('#sign_up_form').find("span.error_alert_message").html("Please enter name and mobile number.");
                                $('#sign_up_form').find("span.error_alert_message").css('display', 'block');
                            }
                            else if (data == "0") {
                                $("#login_form").get(0).reset();
                                $("#for-msg").addClass("filled-in");
                                $("#log_mob").val(mobile);
                                setTimeout(function () {
                                    $("#sign_up_form").get(0).reset();
                                    $(".s-popup").fadeOut(1200);
                                    $(".l-popup").fadeIn(2000);

                                    $('#login_form').find("span.error_alert_message").after('<p class="already per-mobile">Mobile number already exists.</p>');
                                }, 2000);
                            }
                            else {
                                $('#sign_up_form').find("span.error_alert_message").html(data);
                                $('#sign_up_form').find("span.error_alert_message").css('display', 'block');
                            }
                            setTimeout(function () {
                                $(".loader-ajax-container").css("display", "none");
                            }, 2000);
                        }
                    });
                }
            }
            else {
                $('#sign_up_form').find("span.error_alert_message").html("Please enter DOB.");
                $('#sign_up_form').find("span.error_alert_message").css('display', 'block');
            }
        }
       
    });

    $('#forgot_form').submit(function (e) {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        e.preventDefault();
        var mobile = $("#forgot_mobile").val();
        if ($(this).valid()) {
        $(".loader-ajax-container").css("display", "block");
                $.ajax({
                    type: 'POST',
                    url: "/Student/ForgotPassword",
                    data: $('#forgot_form').serialize(),
                    success: function (data) {
                        if (data == "1") {
                            $('#forgot_form').find("span.success_alert_message").html("Confirm your OTP to procced further.");
                            $('#forgot_form').find("span.success_alert_message").css('display', 'block');
                            $('.otp-popup').find("span.number-block").html(mobile + "<i> and Email Id</i>");
                            setTimeout(function () {
                                $("#forgot_form").get(0).reset();
                                $(".for-password").fadeOut(1200);
                                $(".otp-popup").fadeIn(1200);
                            }, 2000);
                        }
                        else if (data == "0") {
                                $('#forgot_form').find("span.error_alert_message").html("Mobile number is not yet registered.");
                            $('#forgot_form').find("span.error_alert_message").css('display', 'block');
                            $("#sign_mob").val(mobile);
                            $("#sign_mob").closest(".group").addClass("filled-in");
                            setTimeout(function () {
                                $("#forgot_form").get(0).reset();
                                $(".for-password").fadeOut(1200);
                                $(".s-popup").fadeIn(1000);
                            }, 2000);
                        }
                        else if (data == "-1") {
                            $('#forgot_form').find("span.error_alert_message").html("Please enter name and mobile no.");
                            $('#forgot_form').find("span.error_alert_message").css('display', 'block');
                        }
                        else if (data == "-2") {
                            $('#forgot_form').find("span.error_alert_message").html("You have already requested for maximum no of OTP, Please wait for 1 hour and try again.");
                            $('#forgot_form').find("span.error_alert_message").css('display', 'block');
                        }
                        //else if (data == "-3") {
                        //    $('#forgot_form').find("span.error_alert_message").html("You have already pre-booked for packages.");
                        //    $('#forgot_form').find("span.error_alert_message").css('display', 'block');
                        //}
                        else {
                            $('#forgot_form').find("span.error_alert_message").html(data);
                            $('#forgot_form').find("span.error_alert_message").css('display', 'block');
                        }
                        setTimeout(function () {
                            $(".loader-ajax-container").css("display", "none");
                        }, 2000);
                    }
                });
            }
    });

    $('#otp_form').submit(function (e) {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        e.preventDefault();
        if ($(this).valid()) {
        $(".loader-ajax-container").css("display", "block");
                $.ajax({
                    type: 'POST',
                    url: "/Student/OTP_Confirmation",
                    data: $('#otp_form').serialize(),
                    success: function (data) {
                        if (data == "1") {
                            $('#otp_form').find("span.success_alert_message").html("You have successfully registered with us. Now you can login to your account.");
                            $('#otp_form').find("span.success_alert_message").css('display', 'block');
                            setTimeout(function () {
                                $("#otp_form").get(0).reset();
                                $(".otp-popup").fadeOut(1200);
                                $(".select-course-and-board-popup").fadeIn(1200);
                                //location.href = "/Student/PreBook";
                            }, 2000);
                        }
                        else if (data == "-1") {
                            $('#otp_form').find("span.error_alert_message").html("OTP is not valid.");
                            $('#otp_form').find("span.error_alert_message").css('display', 'block');
                        }
                        else if (data == "0") {
                            $('#otp_form').find("span.error_alert_message").html("New password and confirm password are not matching.");
                            $('#otp_form').find("span.error_alert_message").css('display', 'block');
                        }
                        else if (data == "11") {
                            $('#otp_form').find("span.success_alert_message").html("OTP successfully confirmed.");
                            $('#otp_form').find("span.success_alert_message").css('display', 'block');
                            setTimeout(function () {
                                $("#otp_form").get(0).reset();
                                $(".otp-popup").fadeOut(1700);
                                $(".select-course-and-board-popup").fadeIn(1200); //For normal login
                                //location.href = "/Student/PreBook";
                            }, 1700);
                        }
                        else if (data == "12") {
                            $('#otp_form').find("span.success_alert_message").html("OTP successfully confirmed. Please Login.");
                            $('#otp_form').find("span.success_alert_message").html("OTP successfully confirmed. Redirecting to pre-book.");
                            $('#otp_form').find("span.success_alert_message").css('display', 'block');

                            setTimeout(function () {
                                $("#otp_form").get(0).reset();
                                $(".otp-popup").fadeOut(1500);
                                $(".l-popup").fadeIn(1200);
                                //location.href = "/Student/PreBook";
                            }, 1700);
                        }
                        else {
                            $('#otp_form').find("span.error_alert_message").html(data);
                            $('#otp_form').find("span.error_alert_message").css('display', 'block');
                        }
                        setTimeout(function () {
                            $(".loader-ajax-container").css("display", "none");
                        }, 2000);
                    }
                });
            }
    });

    $('#ddlboard').change(function () {
        if ($('#ddlclass').closest('.form-group').find('.ajax-loader').length) {
            // do nothing
        } else {
            $('#ddlclass').closest('.form-group').find('.controls').append("<div class='ajax-loader'><div class='loader'></div></div>");
        }
        $('#ddlclass').closest('.form-group').addClass('display-loader');
        $.ajax({
            type: "POST",
            url: "/Admin/GetAllClass",
            data: { brdId: $('#ddlboard').val() },
            datatype: "json",
            traditional: true,
            success: function (data) {
                var sta = "<select id='ddlclass'>";
                sta = sta + '<option value="">Please Select Class</option>';
                for (var i = 0; i < data.length; i++) {
                    sta = sta + '<option value=' + data[i].Value + '>' + data[i].Text + '</option>';
                }
                sta = sta + '</select>';
                $('#ddlclass').html(sta);
                setTimeout(function () {
                    $('#ddlclass').closest('.form-group').removeClass('display-loader');
                }, 800);
            }
        });
    });

    $('#board_form').submit(function (e) {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        if ($(this).valid()) {
            $(".loader-ajax-container").css("display", "block");
            $.ajax({
                type: 'POST',
                url: "/Student/startboarding",
                data: { school_name: $('input[name="school_name"]').val(), board: $('#ddlboard').val(), classid: $('#ddlclass').val(), secureqstn: $('select[name="secureqstn"]').val(), answer: $('input[name="answer"]').val() },
                success: function (data) {
                    if (data == "1") {
                        $('#board_form').find("span.success_alert_message").html("Details filled up successfully").css('display', 'block');
                        setTimeout(function () {
                            $(".select-course-and-board-popup").fadeOut(2000);
                        }, 180);
                        location.href = "https://thedigichamps.com/Student/EditProfile";
                    }
                    else if (data == "0") {
                        $('#board_form').find("span.error_alert_message").html("Please enter board and class.");
                        $('#board_form').find("span.error_alert_message").css('display', 'block');
                    }
                    else if (data == "-1") {
                        $('#board_form').find("span.error_alert_message").html("Invalid User Details.");
                        $('#board_form').find("span.error_alert_message").css('display', 'block');
                    }
                    else {
                        $('#board_form').find("span.error_alert_message").html(data);
                        $('#board_form').find("span.error_alert_message").css('display', 'block');
                    }
                    setTimeout(function () {
                        $(".loader-ajax-container").css("display", "none");
                    }, 2000);
                }
            });
        }
    });

    //resend OTP
    var count = 0;
    $(document).on('click', "#click-count", function (e) {
        $("span.success_alert_message").css('display', 'none');
        $("span.error_alert_message").css('display', 'none');
        e.preventDefault();
        count++;
        $(".loader-ajax-container").css("display", "block");
        $.ajax({
            url: "/Student/Resend_OTP",
            Method: "Post",
            data: { count: count },
            datatype: "json",
            success: function (result) {
                if (result == "0") {
                    $('#otp_form').find("span.error_alert_message").html("OTP can not be sent more than 3 times.").css('display','block');
                }
                else if (result == "1") {
                    $('#otp_form').get(0).reset();
                    $('#otp_form').find("span.success_alert_message").html("OTP has been resent.").css('display','block');
                }
                else if (result == "-1") {
                    $('#otp_form').find("span.error_alert_message").html("Invalid user details.").css('display','block');
                }
                else {
                    $('#otp_form').find("span.error_alert_message").html(result).css('display','block');
                }
                setTimeout(function () {
                    $(".loader-ajax-container").css("display", "none");
                }, 2000);
            }
        });
    });

    //Sonali End

});
