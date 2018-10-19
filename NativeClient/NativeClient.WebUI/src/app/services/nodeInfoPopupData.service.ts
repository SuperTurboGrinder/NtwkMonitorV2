import { Injectable } from '@angular/core';
import { BehaviorSubject, Subscription } from 'rxjs';

import { NodeInfoPopupData } from '../model/viewModel/nodeInfoPopupData.model';

@Injectable()
export class NodeInfoPopupDataService {
    private data = new BehaviorSubject<NodeInfoPopupData>(null);

    setData(nodeData: NodeInfoPopupData) {
        this.data.next(nodeData);
    }

    subscribeToData(func: (NodeInfoPopupData) => void): Subscription {
        return this.data.subscribe(func);
    }
}
