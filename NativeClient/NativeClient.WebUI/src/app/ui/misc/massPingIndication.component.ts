import { Component, Input } from '@angular/core';

@Component({
    selector: 'app-mass-ping-indication',
    templateUrl: './massPingIndication.component.html'
})
export class MassPingIndicationComponent {
    @Input() public participantsNum = 0;
    @Input() public pingedNum = 0;
}
