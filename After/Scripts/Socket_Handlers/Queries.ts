﻿namespace After.Socket_Handlers.Queries {
    export function HandleStatUpdate(jsonMessage) {
        After.Me[jsonMessage.Stat] = jsonMessage.Amount;
        After.Me.UpdateStatsUI();
    }
    export function HandlePlayerUpdate(jsonMessage) {
        for (var stat in jsonMessage.Player) {
            if (After.Me[stat] != undefined && jsonMessage.Player[stat] != undefined) {
                After.Me[stat] = jsonMessage.Player[stat];
            }
        }
        After.Me.UpdateStatsUI();
    }
    export function HandleFirstLoad(jsonMessage) {
        for (var stat in jsonMessage.Player) {
            if (After.Me[stat] != undefined && jsonMessage.Player[stat] != undefined) {
                After.Me[stat] = jsonMessage.Player[stat];
            }
        }
        jsonMessage.Souls.forEach(function(value, index) {
            var soul = new After.Models.Soul();
            for (var stat in value) {
                soul[stat] = value[stat];
            }
            After.World_Data.Souls.push(soul);
        })
        After.Me.UpdateStatsUI();
    }
}