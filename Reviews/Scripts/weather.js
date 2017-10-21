//weather.js

var weatherCallback = function (data) {
    var wind = data.query.results.channel.wind;
    var item = data.query.results.channel.item;
    var text = "Temperature: " + item.condition.temp + " °C";
    $("#temperatureDiv p").html(text);

    if (item.condition.temp <= 20)
    {
        updateTemperatureDiv("It seems to be pretty cold out there! Stay home, we have great soup recipes");
    }
    else if (item.condition.temp > 20 && item.condition.temp < 28) {
        updateTemperatureDiv("The weather is great! Take your cooking skills outdoor and test some of our outdoor recipes!");
    } else {
        updateTemperatureDiv("It seems to be very hot out there! Make yourself a cool drink!");
    }
};

function updateTemperatureDiv(text) {
    $("#temperatureDiv").append("<p>" + text + "</p>");
}