"use strict";

var connection = new signalR.HubConnectionBuilder()
                        .withUrl("/chat/signalr", {
                            accessTokenFactory: () => "testing"
                        })
                        .withAutomaticReconnect()
                        .build();
