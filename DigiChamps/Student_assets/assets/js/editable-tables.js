$(document).ready(function() {
    $('#name').editable();
    $('#username').editable();
    $('#password').editable();
    $('#email').editable();
    $('#comments').editable();

    $('#gender').editable({
        value: 1,    
        source: [
            {value: 1, text: 'Gender'},
            {value: 2, text: 'Male'},
            {value: 3, text: 'Female'}
        ]
    });

    $('#country').editable({
        value: 'ind',    
        source: [
            {value: 'ind', text: 'India'},
            {value: 'us', text: 'United States'},
            {value: 'ay', text: 'Australia'},
            {value: 'uk', text: 'United Kingdom'},
            {value: 'ge', text: 'Germany'}
        ]
    });

    $('#dob').editable({
         type:  'date',
         pk:    1,
         name:  'dob',
         title: 'Select Date of birth'
     });

    $('#listarea').listarea({
        effect: 'slow'
    });

    $('form').on('submit', submitDemoForm);
    //Rainbow.color();
    function submitDemoForm(){
        alert($('#listarea').val());
        return false;
    }
});