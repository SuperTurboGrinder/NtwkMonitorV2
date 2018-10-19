import { Component } from '@angular/core';
import { Observable, interval } from 'rxjs';
import {
  trigger,
  state,
  style,
  animate,
  transition
} from '@angular/animations';

import { MessagingService } from '../../services/messaging.service';
import { MessageToDisplay } from '../../model/viewModel/messageToDisplay.model';

@Component({
    selector: 'app-message-display',
    templateUrl: './messageDisplay.component.html',
    animations: [
        trigger('messageState', [
            state('normal', style({
                opacity: 0.8,
                transform: 'translateX(0)'
            })),
            transition(':enter', [
                style({
                    opacity: 0,
                    transform: 'translateX(100%)'
                }),
                animate('250ms ease-in')
            ]),
            transition(':leave', [
                animate('1000ms ease-out', style({
                    opacity: 0,
                    transform: 'translateX(0)'
                }))
            ])
        ])
    ]
})
export class MessageDisplayComponent {
    private mainQueueCapacity = 8;
    public messageQueue: MessageToDisplay[] = [];
    private secondaryMessageQueue: MessageToDisplay[] = [];
    private updateInterval: Observable<number>;

    constructor(private messaging: MessagingService) {
        this.updateInterval = interval(1000);
        this.updateInterval.subscribe(_ => {
            if (this.messageQueue.length > 0) {
                const now = new Date().getTime();
                for (let i = 0; i < this.messageQueue.length; i++) {
                    const ms = now - this.messageQueue[i].time;
                    const deadline = this.messageQueue[i].isMessage
                        ? 3000 : 12000;
                    if (ms > deadline) {
                        this.messageQueue.shift();
                        this.swapFromSecondaryQueue();
                    } else {
                        break;
                    }
                }
            } else {
                this.swapFromSecondaryQueue();
            }
        });
        this.messaging.messages.subscribe(msg =>
            this.addNewMessage({
                isMessage: true,
                message: msg,
                errorStatus: null,
                time: new Date().getTime()
            })
        );
        this.messaging.backendErrors.subscribe(errStatus =>
            this.addNewMessage({
                isMessage: false,
                message: null,
                errorStatus: errStatus,
                time: new Date().getTime()
            })
        );
    }

    private addNewMessage(msg: MessageToDisplay) {
        if (this.messageQueue.length < this.mainQueueCapacity) {
            this.messageQueue.push(msg);
        } else {
            this.secondaryMessageQueue.push(msg);
        }
    }

    private swapFromSecondaryQueue() {
        if (this.secondaryMessageQueue.length > 0
            && this.messageQueue.length < this.mainQueueCapacity
        ) {
            this.messageQueue.push(this.secondaryMessageQueue[0]);
            this.secondaryMessageQueue.shift();
        }
    }

    messageTrackByFn(index: number, item: MessageToDisplay) {
        return item.time;
    }
}
