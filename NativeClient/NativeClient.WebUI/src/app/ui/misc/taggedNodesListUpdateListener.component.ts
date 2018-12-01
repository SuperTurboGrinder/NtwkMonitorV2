import { Component } from '@angular/core';

@Component({
    selector: 'app-tagged-nodes-list-update-listener',
    template: '<app-tagged-node-list (updateUIEvent)=\'{}\'></app-tagged-node-list>'
})
export class TaggedNodesListUpdateListenerComponent {}
