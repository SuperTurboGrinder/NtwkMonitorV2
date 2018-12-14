import { Component, Input } from '@angular/core';
import { MassPingService, PingProgress } from 'src/app/services/massPing.service';
import { Subscription } from 'rxjs';

@Component({
    selector: 'app-mass-ping-indication',
    templateUrl: './massPingIndication.component.html'
})
export class MassPingIndicationComponent {
    private pingProgressSubscription: Subscription;
    public pingProgress: PingProgress = null;

    constructor(
        private massPingService: MassPingService
    ) {
        this.pingProgressSubscription = this.massPingService.subscribeToPingProgress(
            (progress: PingProgress) => this.pingProgress = progress
        );
    }
}
