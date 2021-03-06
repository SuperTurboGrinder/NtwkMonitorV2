import { Component, EventEmitter, Input, ChangeDetectionStrategy } from '@angular/core';

import { NtwkNode } from '../../model/httpModel/ntwkNode.model';
import { ExecServicesService } from '../../services/execServices.service';

@Component({
    selector: 'app-exec-services-buttons',
    templateUrl: './execServicesButtons.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExecServicesButtonsComponent {
    @Input() node: NtwkNode;

    constructor(
        private execServicesService: ExecServicesService
    ) {}

    runTelnet() {
        this.execServicesService.runTelnetService(this.node.id);
    }

    runSSH() {
        this.execServicesService.runSSHService(this.node.id);
    }
}
