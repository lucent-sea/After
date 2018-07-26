﻿/// <reference path="../lib/pixi-particles/ambient.d.ts" />

import { Sound } from "./App/Sound.js";
import { Me } from "./App/Me.js";
import { Scene } from "./Models/Scene.js";
import { UI } from "./App/UI.js";
import { Utilities } from "./App/Utilities.js";
import { Sockets } from "./App/Sockets.js";
import { Settings } from "./App/Settings.js";
import { PixiHelper } from "./App/PixiHelper.js";
import { Input } from "./App/Input.js";

var main = new class {
    ErrorLog: string = "";
    Input = Input;
    Me = Me;
    PixiHelper = PixiHelper;
    Renderer: PIXI.Application = new PIXI.Application({
        view: document.querySelector("#playCanvas"),
        width: 1280,
        height: 720
    });
    Scene: Scene;
    Sound = Sound;
    UI = UI;
    Utilities = Utilities;
    Settings = Settings;
    Sockets = Sockets;
    StartGameLoop() {
        Main.Renderer.ticker.add(delta => gameLoop(delta));
    }
}

window.onerror = (ev: Event, source, fileNo, columnNo, error: Error) => {
    var errorMessage = `${new Date().toLocaleString()}  |  File: ${fileNo}  |  Column: ${columnNo}  |  Message: ${error.message}  |  Stack: ${error.stack}`.replace("\r\n", "<br>");
    main.ErrorLog += errorMessage + "<br><br>";
};

window["After"] = main;
export const Main = main;

if (location.pathname.search("play") > -1) {
    window.onload = (e) => { Sockets.Connect(); };
}

function gameLoop(delta) {
   
}