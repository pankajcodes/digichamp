var baseYahooURL = "https://query.yahooapis.com/v1/public/yql?q="
var selectedCity = "New York";
var placeholder = "";
var unit = "f"
init();

function init() {
    getWoeid(selectedCity);

    $('#city').keypress(function (event) {
        if (event.which == 13) {
            selectedCity = $('#city').val();
            getWoeid(selectedCity);
            $('#city').blur();
        }
    });

    $('#temp').click(function () {
        if ($('#temp').html() == "F") {
            unit = "c";
        } else unit = "f";
        $('#temp').html(unit.toUpperCase());
        getWoeid(selectedCity);
    })

    $('#city').focus(function (event) {
        placeholder = $("#city").val();
        $("#city").val("");
    });

    $('#city').blur(function (event) {
        if ($("#city").val() == "") {
            $("#city").val(placeholder);
        }
    });
}

function getWoeid(city) {
    var woeidYQL = 'select woeid from geo.placefinder where text="' + city + '"&format=json';
    var jsonURL = baseYahooURL + woeidYQL;
    $.getJSON(jsonURL, woeidDownloaded);
}

function woeidDownloaded(data) {
    var woeid = null;
    if (data.query.count <= 0) {
        $('#city').val("No city found");
        $('#deg').html("");
        setImage(999, $("#big")[0]);
        for (var i = 0; i <= 3; i++) {
            $('#forecast' + i).html("");
            setImage(999, $("#forecastimg" + i)[0]);
            $('#forecastdeg' + i).html("");
        }
        return;
    } else if (data.query.count == 1) {
        woeid = data.query.results.Result.woeid;
    } else {
        woeid = data.query.results.Result[0].woeid;
    }
    getWeatherInfo(woeid);
}

function getWeatherInfo(woeid) {
    var weatherYQL = 'select * from weather.forecast where woeid=' + woeid + ' and u = "' + unit + '" &format=json';
    var jsonURL = baseYahooURL + weatherYQL
    $.getJSON(jsonURL, weaterInfoDownloaded);
}

function weaterInfoDownloaded(data) {
    $('#city').val(data.query.results.channel.location.city);
    $('#deg').html(data.query.results.channel.item.condition.temp + "°" + unit.toUpperCase());
    setImage(data.query.results.channel.item.condition.code, $('#big')[0]);
    for (var i = 0; i <= 3; i++) {
        var fc = data.query.results.channel.item.forecast[i];
        $('#forecast' + i).html(fc.day);
        setImage(fc.code, $("#forecastimg" + i)[0]);
        $('#forecastdeg' + i).html((parseInt(fc.low) + parseInt(fc.high)) / 2 + " °" + unit.toUpperCase());
    }
}

function setImage(code, image) {
    image.src = "../assets/images/";
    switch (parseInt(code)) {
    case 0:
        image.src += "weather/Tornado.svg"
        break;
    case 1:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 2:
        image.src += "weather/Wind.svg"
        break;
    case 3:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 4:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 5:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 6:
        image.src += "weather/Cloud-Rain-Alt.svg"
        break;
    case 7:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 8:
        image.src += "weather/Cloud-Drizzle-Alt.svg"
        break;
    case 9:
        image.src += "weather/Cloud-Drizzle-Alt.svg"
        break;
    case 10:
        image.src += "weather/Cloud-Drizzle-Alt.svg"
        break;
    case 11:
        image.src += "weather/Cloud-Drizzle-Alt.svg"
        break;
    case 12:
        image.src += "weather/Cloud-Drizzle-Alt.svg"
        break;
    case 13:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 14:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 15:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 16:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 17:
        image.src += "weather/Cloud-Hail-Alt.svg"
        break;
    case 18:
        image.src += "weather/Cloud-Hail-Alt.svg"
        break;
    case 19:
        image.src += "weather/Cloud-Hail-Alt.svg"
        break;
    case 20:
        image.src += "weather/Cloud-Fog.svg"
        break;
    case 21:
        image.src += "weather/Cloud-Fog.svg"
        break;
    case 22:
        image.src += "weather/Cloud-Fog.svg"
        break;
    case 23:
        image.src += "weather/Cloud-Fog.svg"
        break;
    case 24:
        image.src += "weather/Wind.svg"
        break;
    case 25:
        image.src += "weather/Thermometer-Zero"
        break;
    case 26:
        image.src += "weather/Cloud.svg"
        break;
    case 27:
        image.src += "weather/Cloud-Moon.svg"
        break;
    case 28:
        image.src += "weather/Cloud.svg"
        break;
    case 29:
        image.src += "weather/Cloud-Moon.svg"
        break;
    case 30:
        image.src += "weather/Cloud-Sun.svg"
        break;
    case 31:
        image.src += "weather/Moon.svg"
        break;
    case 32:
        image.src += "weather/Sun.svg"
        break;
    case 33:
        image.src += "weather/Moon.svg"
        break;
    case 34:
        image.src += "weather/Sun.svg"
        break;
    case 35:
        image.src += "weather/Cloud-Hail-Alt.svg"
        break;
    case 36:
        image.src += "weather/Sun.svg"
        break;
    case 37:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 38:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 39:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 40:
        image.src += "weather/Cloud-Rain.svg"
        break;
    case 41:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 42:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 43:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 44:
        image.src += "weather/Cloud.svg"
        break;
    case 45:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 46:
        image.src += "weather/Cloud-Snow-Alt.svg"
        break;
    case 47:
        image.src += "weather/Cloud-Lightning.svg"
        break;
    case 3200:
        image.src += "weather/Moon-New.svg"
        break;
    case 999:
        image.src += "weather/Compass.svg"
        break;
    default:
        image.src += "weather/Moon-New.svg"
        break;
    }
}