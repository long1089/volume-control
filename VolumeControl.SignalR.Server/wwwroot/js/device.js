"use strict";

$(function () {

    var uid = document.getElementById("uid").value;

    var connection = new signalR.HubConnectionBuilder().withUrl("/devices?uid=" + uid).build();

    var setVolumeButton = document.getElementById("setVolumeButton");

    var setMuteButton = document.getElementById("setMuteButton");
    var setUnMuteButton = document.getElementById("setUnMuteButton");

    var showToast = function (msg) {
        $("#liveToast").toast("show").find(".toast-body").text(msg)
    }

    //Disable the send button until connection is established.
    setVolumeButton.disabled = true;
    setMuteButton.disabled = true;
    setUnMuteButton.disabled = true;

    connection.start().then(function () {
        setVolumeButton.disabled = false;
        setMuteButton.disabled = false;
        setUnMuteButton.disabled = false;
        showToast("连接成功！");
    }).catch(function (err) {
        return console.error(err.toString());
    });

    $("#volumeInput").change(function () {
        $("#volume").text(this.value);
    });

    setVolumeButton.addEventListener("click", function (event) {
        var message = $("#volumeInput").val();
        connection.invoke("SetDeviceVolume", parseInt(message)).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    setMuteButton.addEventListener("click", function (event) {
        connection.invoke("SetDeviceMute", true).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });

    setUnMuteButton.addEventListener("click", function (event) {
        connection.invoke("SetDeviceMute", false).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    });
})