import { Utilities } from "./Utilities.js";
import { Me } from "./Me.js";
import { Settings } from "./Settings.js";
export const UI = new class {
    get ChargeProgress() {
        return document.querySelector("#chargeProgress");
    }
    get DebugFrame() {
        return document.querySelector("#debugFrame");
    }
    get EnergyProgress() {
        return document.querySelector("#energyProgress");
    }
    get Joystick() {
        return document.querySelector("#moveJoystickOuter");
    }
    get FPSSpan() {
        return document.querySelector("#fpsSpan");
    }
    get WillpowerProgress() {
        return document.querySelector("#willpowerProgress");
    }
    get ChatMessages() {
        return document.querySelector("#chatMessages");
    }
    get ChatChannelSelect() {
        return document.querySelector("#selectChatChannel");
    }
    get ChatInput() {
        return document.querySelector("#chatInput");
    }
    get ChatFrame() {
        return document.querySelector("#chatFrame");
    }
    ;
    UpdateStatBars() {
        this.ChargeProgress.innerText = String(Me.Character.CurrentCharge);
        this.ChargeProgress.style.width = String(Me.Character.CurrentCharge / Me.Character.MaxEnergy * 100) + "%";
        this.EnergyProgress.innerText = String(Me.Character.CurrentEnergy);
        this.EnergyProgress.style.width = String(Me.Character.CurrentEnergy / Me.Character.MaxEnergy * 100) + "%";
        this.WillpowerProgress.innerText = String(Me.Character.CurrentWillpower);
        this.WillpowerProgress.style.width = String(Me.Character.CurrentWillpower / Me.Character.MaxWillpower * 100) + "%";
    }
    AppendMessageToWindow(message) {
        var shouldScroll = false;
        if (this.ChatMessages.scrollTop + this.ChatMessages.clientHeight >= this.ChatMessages.scrollHeight) {
            shouldScroll = true;
        }
        var messageDiv = document.createElement("div");
        messageDiv.innerHTML = message;
        this.ChatMessages.appendChild(messageDiv);
        if (shouldScroll) {
            this.ChatMessages.scrollTop = this.ChatMessages.scrollHeight;
        }
    }
    AddDebugMessage(message, jsonData, addBlankLines = 0) {
        if (Settings.IsDebugEnabled) {
            var temp = new Array();
            var jsonHTML = "";
            if (jsonData) {
                jsonHTML = JSON.stringify(jsonData, function (key, value) {
                    if (typeof value == "object" && value != null) {
                        if (temp.findIndex(x => x == value) > -1) {
                            return "[Possible circular reference.]";
                        }
                        else {
                            temp.push(value);
                        }
                    }
                    return value;
                }, "&emsp;").split("\n").join("<br/>").split(" ").join("&nbsp;");
                temp = null;
            }
            var messageText = `<div style="color:${Settings.Colors.DebugMessage}">[Debug]: ${Utilities.EncodeForHTML(message) + jsonHTML}</div>`;
            for (var i = 0; i < addBlankLines; i++) {
                messageText += "<br>";
            }
            this.AppendMessageToWindow(messageText);
        }
    }
    AddTextToEncode(message, addBlankLines = 0) {
        var messageText = Utilities.EncodeForHTML(message);
        for (var i = 0; i < addBlankLines; i++) {
            messageText += "<br>";
        }
        this.AppendMessageToWindow(messageText);
    }
    ;
    AddRawHTMLMessage(html, addBlankLines = 0) {
        for (var i = 0; i < addBlankLines; i++) {
            html += "<br>";
        }
        this.AppendMessageToWindow(html);
    }
    ;
    AddSystemMessage(message, addBlankLines = 0) {
        var messageText = `<div style="color:${Settings.Colors.SystemMessage}">[System]: ${message}</div>`;
        for (var i = 0; i < addBlankLines; i++) {
            messageText += "<br>";
        }
        this.AppendMessageToWindow(messageText);
    }
    ;
    AddGlobalChat(characterName, message, color) {
        var messageText = `<div>
                <span style="color:${Settings.Colors.GlobalChat}">[Global] </span>
                <span style="color:${color}">${characterName}</span>: 
                ${message}</div>`;
        this.AppendMessageToWindow(messageText);
    }
    ;
    ApplyDataBinds() {
        document.querySelectorAll("[data-bind]").forEach((elem, index) => {
            var qualifiedObject = elem.getAttribute("data-bind");
            var lastDot = qualifiedObject.lastIndexOf(".");
            var dataObject = eval(qualifiedObject.substring(0, lastDot));
            var propertyName = qualifiedObject.substring(lastDot + 1);
            if (elem.classList.contains("toggle-switch-outer")) {
                dataBindOneWay(dataObject, propertyName, elem, "on", null, null);
                elem.addEventListener("click", ev => {
                    ev.currentTarget.setAttribute("on", String(ev.currentTarget.getAttribute("on") == "true"));
                    eval(ev.currentTarget.getAttribute("data-bind") + " = " + ev.currentTarget.getAttribute("on"));
                });
            }
            else if (elem.hasAttribute("value")) {
                dataBindTwoWay(dataObject, propertyName, elem, "value", null, null, ["onchange"]);
            }
            else {
                dataBindOneWay(dataObject, propertyName, elem, "innerHTML", null, null);
            }
        });
    }
    ShowGenericError() {
        this.ShowModal("Error", "An error occurred during the last operation.", "");
    }
    ;
    ShowModal(title, message, buttonsHTML = "", onDismissCallback = null) {
        var modalHTML = `<div class="modal fade" tabindex="-1" role="dialog">
          <div class="modal-dialog" role="document">
            <div class="modal-content">
              <div class="modal-header">
                <h3 class="modal-title">${title}</h3>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                  <span aria-hidden="true">&times;</span>
                </button>
              </div>
              <div class="modal-body">
                ${message}
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                ${buttonsHTML}
              </div>
            </div>
          </div>
        </div>`;
        var wrapperDiv = document.createElement("div");
        wrapperDiv.innerHTML = modalHTML;
        document.body.appendChild(wrapperDiv);
        $(".modal").on("hidden.bs.modal", ev => {
            try {
                if (onDismissCallback) {
                    onDismissCallback();
                }
            }
            finally {
                ev.currentTarget.parentElement.remove();
            }
        });
        $(".modal")["modal"]();
    }
    ;
};
function dataBindOneWay(dataObject, objectProperty, element, elementPropertyKey, postSetterCallback = null, preGetterCallback = null) {
    var backingValue = dataObject[objectProperty];
    Object.defineProperty(dataObject, objectProperty, {
        configurable: true,
        enumerable: true,
        get() {
            if (preGetterCallback) {
                preGetterCallback(backingValue);
            }
            return backingValue;
        },
        set(value) {
            backingValue = value;
            if (elementPropertyKey in element) {
                element[elementPropertyKey] = value;
            }
            else {
                element.setAttribute(elementPropertyKey, value);
            }
            if (postSetterCallback) {
                postSetterCallback(value);
            }
        }
    });
    dataObject[objectProperty] = backingValue;
}
;
function dataBindTwoWay(dataObject, objectProperty, element, elementPropertyKey, postSetterCallback = null, preGetterCallback = null, elementEventTriggers) {
    var backingValue = dataObject[objectProperty];
    Object.defineProperty(dataObject, objectProperty, {
        configurable: true,
        enumerable: true,
        get() {
            if (preGetterCallback) {
                preGetterCallback(backingValue);
            }
            return backingValue;
        },
        set(value) {
            backingValue = value;
            if (elementPropertyKey in element) {
                element[elementPropertyKey] = value;
            }
            else {
                element.setAttribute(elementPropertyKey, value);
            }
            if (postSetterCallback) {
                postSetterCallback(value);
            }
        }
    });
    dataObject[objectProperty] = backingValue;
    elementEventTriggers.forEach(trigger => {
        eval(`element.${trigger} = function(e) {
            ${element.getAttribute("data-bind")} = e.currentTarget${element.hasAttribute(elementPropertyKey) ? ".getAttribute(" + elementPropertyKey + ")" : "['" + elementPropertyKey + "']"};
        };`);
    });
}
;
//# sourceMappingURL=UI.js.map