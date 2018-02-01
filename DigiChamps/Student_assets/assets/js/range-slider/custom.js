//Simple start
$("#range_01").ionRangeSlider({
    min: 0,
    max: 3,
    from: 1,
    to: 3,
});

//set min and max
$("#range_02").ionRangeSlider({
    min: 0,
    max: 3,
    from: 1,
    to: 3,
});

$(".range-slider").ionRangeSlider({
    min: 0,
    max: 3,
    from: 1,
    to: 3,
});


//add $ as prefix and type as double
$("#range_03").ionRangeSlider({
    type: "double",
    grid: true,
    min: 0,
    max: 1000,
    from: 200,
    to: 800,
    prefix: "$"
});

//setup range as negative values
$("#range_04").ionRangeSlider({
    type: "double",
    grid: true,
    min: -1000,
    max: 1000,
    from: -500,
    to: 500
});

//setup range as fractional values
$("#range_05").ionRangeSlider({
    type: "double",
    grid: true,
    min: -12.8,
    max: 12.8,
    from: -3.2,
    to: 3.2,
    step: 0.1
});

//custom Values
$("#range_06").ionRangeSlider({
    type: "double",
    grid: true,
    from: 1,
    to: 5,
    values: [0, 10, 100, 1000, 10000, 100000, 1000000]
});

//months slider
$("#range_07").ionRangeSlider({
    grid: true,
    from: 3,
    values: [
        "January", "February", "March",
        "April", "May", "June",
        "July", "August", "September",
        "October", "November", "December"
    ]
});

//seperator for big numbers
$("#range_08").ionRangeSlider({
    grid: true,
    min: 1000,
    max: 1000000,
    from: 300000,
    step: 1000,
    prettify_enabled: true,
    prettify_separator: ","
});

//price slider
$("#range_09").ionRangeSlider({
    type: "double",
    grid: true,
    min: 0,
    max: 10000,
    from: 1000,
    step: 9000,
    prefix: "$"
});

//custom prefix
$("#range_10").ionRangeSlider({
    grid: true,
    min: 18,
    max: 70,
    from: 30,
    prefix: "Age ",
    max_postfix: "+"
});

//from and to values
$("#range_11").ionRangeSlider({
    type: "double",
    min: 100,
    max: 200,
    from: 145,
    to: 155,
    prefix: "Weight: ",
    postfix: " million pounds",
    decorate_both: false,
    values_separator: " → "
});


