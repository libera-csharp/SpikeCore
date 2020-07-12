"use strict";

const connection = new signalR
    .HubConnectionBuilder()
    .withUrl("/hubs/bot-console")
    .build();

const sendMessage = function () {
    const messageInput = document.getElementById("messageInput");
    const message = messageInput.value;
    messageInput.value = "";
    
    const channelTarget = document.getElementById("channelTarget");
    const channel = channelTarget[channelTarget.selectedIndex].value;

    connection.invoke("SendMessage", channel, message).catch(function (err) {
        return console.error(err.toString());
    });
};

connection.on("ReceiveMessage", function (message) {
    message = message
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");

    const li = document.createElement("li");
    li.textContent = message;

    const messageList = document.getElementById("messagesList");
    messageList.appendChild(li);

    const messageListContainer = document.getElementById("messagesListContainer");
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