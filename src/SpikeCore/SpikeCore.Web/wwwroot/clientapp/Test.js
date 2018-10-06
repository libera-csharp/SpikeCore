"use strict";

var connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/hubs/test")
    .build();

var sendMessage = function () {
    var messageInput = document.getElementById("messageInput");
    var message = messageInput.value;
    messageInput.value = "";

    connection.invoke("SendMessage", message).catch(function (err) {
        return console.error(err.toString());
    });
};

connection.on("ReceiveMessage", function (message) {
    message = message
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");

    var li = document.createElement("li");
    li.textContent = message;

    var messageList = document.getElementById("messagesList");
    messageList.appendChild(li);

    var messageListContainer = document.getElementById("messagesListContainer");
    messageListContainer.scrollTop = messageListContainer.scrollHeight;
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    event.preventDefault();
    sendMessage();
});

document.getElementById("messageInput").addEventListener('keypress', function (event) {
    if (event.key === 'Enter') {
        event.preventDefault();
        sendMessage();
    }
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});