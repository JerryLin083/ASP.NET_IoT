"use strict"

var connection = new signalR.HubConnectionBuilder().withUrl("/sensorHub").build();

connection.on("ReceiveReading", function (topic, payload) {
    console.log(topic);
    console.log(payload);
});

connection.start().then(function () {
    //TODO: fetch reading from cache
    console.log("Connected...")
}).catch(function (e) {
    console.error(e.toString());
})