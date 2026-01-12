"use strict"

var connection = new signalR.HubConnectionBuilder().withUrl("/sensorHub").build();

connection.on("ReceiveReading", function (topic, payload) {
    let element = document.getElementById("sensor-reading");
    element.textContent = `${topic}, ${payload}`;
});

connection.start().then(function () {
    //TODO: fetch reading from cache
    console.log("Connected...")
}).catch(function (e) {
    console.error(e.toString());
})