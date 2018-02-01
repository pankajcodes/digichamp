$(document).ready(function () {
    $('#rootwizard').bootstrapWizard({
        'tabClass': 'nav nav-tabs'
    });
    window.prettyPrint && prettyPrint()
});

$(document).ready(function () {
    $('#rootwizard1').bootstrapWizard({
        onNext: function (tab, navigation, index) {
            if (index == 2) {
                // Make sure we entered the name
                if (!$('#name').val()) {
                    alert('You must enter your name');
                    $('#name').focus();
                    return false;
                }
            }
            // Set the name for the next tab
            $('#tabs3').html('Hello <b>' + $('#name').val() + '</b>, <br> Hope you are doing great today!<br> Well you can see the value from previous step is being displayed here. We request you to mention your location on next step to elplore it more and more!!!');

            // Set the location for the next tab
            var location = $('#location').val();
            console.log(location);
            if (location) {
                $('#tabs5').html('Hello <b class="text-info">' + $('#name').val() + '</b>, <br> We are happy to let you know that you are native of <b class="text-success">' + $('#location').val() + '</b>');
            } else {
                $('#tabs5').html('Hello <b class="text-info">' + $('#name').val() + '</b>, <br> We are happy to let you know that you are native of <b class="text-success"> [Location Name] </b>');
            }


        },
        onTabShow: function (tab, navigation, index) {
            var $total = navigation.find('li').length;
            var $current = index + 1;
            var $percent = ($current / $total) * 100;
            $('#rootwizard1 .progress-bar').css({
                width: $percent + '%'
            });
        }
    });
    window.prettyPrint && prettyPrint()
});

/* for last wizard model */
$(document).ready(function () {
    $('#rootwizard2').bootstrapWizard({
        'tabClass': 'ntspl-tabs'
    });
    window.prettyPrint && prettyPrint();

    // Disable step
    $('#disable-step').on('click', function () {
        $('#rootwizard2').bootstrapWizard('disable', $('#stepid').val());
    });

    // Enable step
    $('#enable-step').on('click', function () {
        $('#rootwizard2').bootstrapWizard('enable', $('#stepid').val());
    });

    // Remove step
    $('#remove-step').on('click', function () {
        $('#rootwizard2').bootstrapWizard('remove', $('#stepid').val(), true);
    });

    // Show step
    $('#show-step').on('click', function () {
        $('#rootwizard2').bootstrapWizard('display', $('#stepid').val());
    });

    // Hide step
    $('#hide-step').on('click', function () {
        $('#rootwizard2').bootstrapWizard('hide', $('#stepid').val());
    });

});