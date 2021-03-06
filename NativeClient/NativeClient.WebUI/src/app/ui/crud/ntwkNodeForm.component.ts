import { Component, OnDestroy } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, ActivatedRouteSnapshot } from '@angular/router';
import { HTTPResult } from '../../model/servicesModel/httpResult.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { NtwkNode } from 'src/app/model/httpModel/ntwkNode.model';
import { NodesService } from '../../services/nodes.service';
import { BaseCrudFormComponent } from '../helpers/baseCrudFormComponent.helper';
import { CWSData } from 'src/app/model/httpModel/cwsData.model';
import { NtwkNodesTree } from 'src/app/model/viewModel/ntwkNodesTree.model';
import { SettingsProfilesService } from 'src/app/services/settingsProfiles.service';

@Component({
    selector: 'app-node-form',
    templateUrl: './ntwkNodeForm.component.html'
})
export class NtwkNodeFormComponent
    extends BaseCrudFormComponent<NtwkNode, NodesService> implements OnDestroy {

    private _usesParentPort = false;
    private _localParentPortValue = 1;
    private _localParentID: number = null;

    public ngOnDestroy() {
        this.settingsService.refreshCurrentProfile();
    }

    public get usesParentPort() {
        return this._usesParentPort;
    }

    public get localParentPortValue() {
        return this._localParentPortValue;
    }

    public setParentPortUsage(on: boolean) {
        this._usesParentPort = on;
        this.data.parentPort = on === false
            ? null
            : this.data.parentPort = this._localParentPortValue;
    }

    public updateParentPort(value: number) {
        this._localParentPortValue = value;
        if (this._usesParentPort) {
            this.data.parentPort = this._localParentPortValue;
        }
    }

    constructor(
        messager: MessagingService,
        location: Location,
        route: ActivatedRoute,
        nodesService: NodesService,
        private settingsService: SettingsProfilesService
    ) {
        super(messager, location, route, nodesService);
        const routeSnapshot: ActivatedRouteSnapshot = route.snapshot;
        if (routeSnapshot.url.length > 2 && routeSnapshot.url[2].path === 'new') {
            this._localParentID = parseInt(routeSnapshot.params.parentId, 10);
        }
    }

    protected getOriginalData(
        id: number,
        callback: (success: boolean, orig: NtwkNode) => void
    ) {
        this.dataService.getNodesTree().subscribe(
            (nodesResult: HTTPResult<{
                cwsData: CWSData[],
                nodesTree: NtwkNodesTree
            }>) => {
                const node = nodesResult.success === true
                    ? nodesResult.data.nodesTree.allNodes.find(
                        c => c.nodeData.node.id === id
                    ).nodeData.node
                    : null;
                callback(
                    nodesResult.success,
                    node
                );
            }
        );
    }

    protected newEmptyData(): NtwkNode {
        return {
            id: 0,
            parentId: null,
            parentPort: null,
            name: '',
            ipStr: '',
            isOpenSsh: false,
            isOpenTelnet: false,
            isOpenPing: true
        };
    }

    protected currentIdenticalTo(obj: NtwkNode): boolean {
        return obj.parentPort === this.data.parentPort
            && obj.name === this.data.name
            && obj.ipStr === this.data.ipStr
            && obj.isOpenSsh === this.data.isOpenSsh
            && obj.isOpenTelnet === this.data.isOpenTelnet
            && obj.isOpenPing === this.data.isOpenPing;
    }

    protected makeCopy(orig: NtwkNode): NtwkNode {
        return {
            id: orig.id,
            parentId: orig.parentId,
            parentPort: orig.parentPort,
            name: orig.name,
            ipStr: orig.ipStr,
            isOpenSsh: orig.isOpenSsh,
            isOpenTelnet: orig.isOpenTelnet,
            isOpenPing: orig.isOpenPing
        };
    }

    protected saveAsNewObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        if (this._localParentID !== null) {
            this.dataService.createNewNodeWithParent(
                this.data,
                this._localParentID,
                callback
            );
        } else {
            this.dataService.createNewNode(
                this.data,
                callback
            );
        }
    }

    protected saveChangesToObjectInDatabase(
        callback: (success: boolean) => void
    ) {
        this.dataService.updateNode(
            this.data,
            callback
        );
    }
}
