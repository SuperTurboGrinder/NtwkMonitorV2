import { Component } from '@angular/core';
import { Observable, interval } from 'rxjs';

import { MessagingService } from '../../services/messaging.service'
import { MessageToDisplay } from "../../model/viewModel/messageToDisplay.model"

@Component({
  selector: 'messageDisplay',
  templateUrl: './messageDisplay.component.html'
})
export class MessageDisplayComponent {
    public messageQueue: MessageToDisplay[] = [];
    updateInterval: Observable<number>;

    constructor(private messaging: MessagingService) {
        this.updateInterval = interval(1000);
        this.updateInterval.subscribe(_ => {
            if(this.messageQueue.length > 0) {
                var ms = this.messageQueue[0].time.getMilliseconds() - new Date().getMilliseconds();
                if(ms > 5000) {
                    this.messageQueue.pop();
                } else if(ms > 4000) {
                    this.messageQueue[0].isOld = true;
                }
            }
        })
        this.messaging.messages.subscribe(msg =>
            this.messageQueue.push({
                isMessage: true,
                message: msg,
                errorStatus: null,
                time: new Date(),
                isOld: false
            })
        );
        this.messaging.backendErrors.subscribe(errStatus =>
            this.messageQueue.push({
                isMessage: false,
                message: null,
                errorStatus: errStatus,
                time: new Date(),
                isOld: false
            })
        );
    }
}