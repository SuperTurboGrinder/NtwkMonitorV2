import { Component } from "@angular/core";

import { Messages } from "../../model/servicesModel/messagesEnum.model"

@Component({
    selector: 'messageSelector',
    templateUrl: './messageSelector.component.html'
})
export class MessageSelector {
    message: Messages;
}