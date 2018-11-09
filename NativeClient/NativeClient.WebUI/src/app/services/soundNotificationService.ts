import { Injectable, Inject } from '@angular/core';

@Injectable()
export class SoundNotificatonService {
    private failedPingAlarm = new Audio();

    constructor() {
        this.failedPingAlarm.src = 'assets/failureAlarm.ogg';
        this.failedPingAlarm.load();
    }

    public playFailedPingAlarm() {
        this.failedPingAlarm.play();
    }
}
