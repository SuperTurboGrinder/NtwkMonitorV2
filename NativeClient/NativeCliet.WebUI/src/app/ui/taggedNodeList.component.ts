import { Component, OnDestroy, HostListener } from "@angular/core";
import { Subscription } from "rxjs";

import { NtwkNode } from "../model/httpModel/ntwkNode.model";
import { CWSData } from "../model/httpModel/cwsData.model";
import { NodesService } from "../services/nodes.service";
import { NodeData } from "../model/httpModel/nodeData.model";
import { SettingsProfilesService } from "../services/settingsProfiles.service";
import { UpdateAlarmService } from "../services/updateAlarm.service";
import { NodeTag } from "../model/httpModel/nodeTag.model";
import { TagsService } from "../services/tags.service";

@Component({
    selector: 'taggedNodeList',
    templateUrl: './taggedNodeList.component.html'
})
export class TaggedNodeListComponent implements OnDestroy {
    private filteredNodesList: NodeData[] = null;
    private tagsNames: string[][] = null;
    private webServicesNames: {
        name:string,
        serviceID:number
    }[][] = null;
    private tagsList: NodeTag[] = null;
    private cwsDataList: CWSData[] = null;
    public sortedIndexes: number[] = null;
    private loadingError = false;
    public selectedNodeIndex: number = -1;
    public screenData = { x:0, y:0, mouseX:0, mouseY:0 };
    private subsctiptions: Subscription[] = [];

    //getNodeData(i:number) : NodeData {
    //    return this.filteredNodesList[i].;
    //}

    ngOnInit() {
        this.screenData.x = window.innerWidth;
        this.screenData.y = window.innerHeight;
    }
    
    @HostListener('window:resize', ['$event'])
    onResize(event) {
        this.screenData.x = window.innerWidth;
        this.screenData.y = window.innerHeight;
    }

    public selectNode(i:number, event:MouseEvent) {
        this.screenData.mouseX = event.clientX;
        this.screenData.mouseY = event.clientY;
        this.selectedNodeIndex = i;
    }

    public deselectNode(event:MouseEvent) {
        this.selectedNodeIndex = -1;
    }

    public node(i:number) : NtwkNode {
        return this.filteredNodesList[i].node;
    }

    public get currentScreenData() 
    : { x:number, y:number, mouseX:number, mouseY:number } {
        return this.screenData;
    }

    public get selectedNode() : NtwkNode {
        if(this.selectedNodeIndex == -1) return null;
        return this.node(this.selectedNodeIndex);
    }

    public get selectedNodeTagsNamesList() : string[] {
        if(this.selectedNodeIndex == -1) return null;
        return this.listTagsNames(this.selectedNodeIndex);
    }

    public get selectedNodeWebServicesNamesList() : {name:string,serviceID:number}[] {
        if(this.selectedNodeIndex == -1) return null;
        return this.listWebServicesNames(this.selectedNodeIndex);
    }

    public get isLoadingError() {
        return this.loadingError;
    }

    nodeTrackByFn(index:number, node_index:number) {
        if(!this.filteredNodesList) return null;
        return this.filteredNodesList[node_index].node.id;
    }

    public refresh(_: boolean) {
        this.updateList();
    }

    private listTagsNames(i: number) : string[] {
        if(this.tagsList === null) return;
        let IDs = this.filteredNodesList[i].tagsIDs;
        if(this.tagsNames[i].length === 0 && IDs.length > 0) {
            this.tagsNames[i] = this.tagsList
                .filter(tag => IDs.includes(tag.id))
                .map(tag => tag.name);
        }
        return this.tagsNames[i];
    }

    public listWebServicesNames(i: number)
    : {name:string,serviceID:number}[] {
        if(this.cwsDataList === null) return;
        let IDs = this.filteredNodesList[i].boundWebServicesIDs;
        if(this.webServicesNames[i].length === 0 && IDs.length > 0) {
            this.webServicesNames[i] = this.cwsDataList
                .filter(cwsD => IDs.includes(cwsD.id))
                .map(cwsD => {
                    return { name:cwsD.name, serviceID:cwsD.id }
                });
        }
        return this.webServicesNames[i];
    }

    private updateList() {
        this.loadingError = false;
        this.updateService.sendUpdateNodesAndTagsAlarm();
    }

    constructor(
        private updateService: UpdateAlarmService,
        nodesService: NodesService,
        tagsService: TagsService,
        settingsService: SettingsProfilesService,
    ) {
        let tagsListSubscription = tagsService.getTagsList().subscribe(tagsListResult => {
            if(tagsListResult === null) {
                return;
            } else if(tagsListResult.success === false) {
                this.loadingError = true;
                return;
            }
            this.tagsList = tagsListResult.data;
        });
        let nodesTreeSubscription = nodesService.getNodesTree().subscribe(treeResult => {
            if(treeResult === null) {
                return;
            } else if(treeResult.success === false) {
                this.loadingError = true;
                return;
            }
            settingsService.currentProfilesViewNodesIDs()
                .subscribe(viewNodesIDsResult => {
                    if(viewNodesIDsResult.success === false) {
                        this.loadingError = true;
                        return;
                    }
                    let viewNodesIDs = viewNodesIDsResult.data;
                    this.cwsDataList = treeResult.data.cwsData;
                    let allNodes = treeResult.data.nodesTree.allNodes;
                    this.filteredNodesList = allNodes
                        .filter(nData => 
                            viewNodesIDs.includes(nData.nodeData.node.id)
                        )
                        .map(nData => nData.nodeData);
                    this.tagsNames = Array.from(
                        {length: this.filteredNodesList.length},
                        _ => []
                    );
                    this.webServicesNames = Array.from(
                        {length: this.filteredNodesList.length},
                        _ => []
                    );
                    this.sortedIndexes = Array.from(
                        {length: this.filteredNodesList.length},
                        (_,i) => i
                    );
                })
        });
        this.subsctiptions.push(tagsListSubscription);
        this.subsctiptions.push(nodesTreeSubscription);
    }

    ngOnDestroy() {
        this.subsctiptions.forEach(sub => sub.unsubscribe());
    }
}