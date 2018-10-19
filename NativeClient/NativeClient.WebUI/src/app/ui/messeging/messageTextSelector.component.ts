import { Component, Input } from '@angular/core';

import { MessagesEnum } from '../../model/servicesModel/messagesEnum.model';

@Component({
    selector: 'app-message-text-selector',
    templateUrl: './messageTextSelector.component.html'
})
export class MessageTextSelectorComponent {
    messagesEnum = MessagesEnum;
    @Input() message: MessagesEnum;
}
