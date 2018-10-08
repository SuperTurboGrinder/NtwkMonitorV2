import { NodeTag } from "../../model/httpModel/nodeTag.model";
import { CWSData } from "../../model/httpModel/cwsData.model";
import { TagsService } from "../../services/tags.service";
import { NodeData } from "../../model/httpModel/nodeData.model";
import { NodeInfoPopupData } from "../../model/viewModel/nodeInfoPopupData.model";


export class NodeInfoDataCache {
    private tagsNames: string[][] = null;
    private webServicesData: {name:string, id:number}[][] = null;
    private webServicesNames: string[][] = null;
    public loadingError = false;
    private tagsList: NodeTag[] = null;
    private tagsSubscription = null;

    constructor(
        len: number,
        private cwsDataList: CWSData[],
        tagsService: TagsService
    ) {
        let newArrayOfNodesCountLength = () => Array.from(
            {length: len},
            _ => []
        );
        this.tagsNames = newArrayOfNodesCountLength();
        this.webServicesData = newArrayOfNodesCountLength();
        this.webServicesNames = newArrayOfNodesCountLength();
        this.tagsSubscription = tagsService.getTagsList().subscribe(tagsListResult => {
            if(tagsListResult === null) {
                return;
            } else if(tagsListResult.success === false) {
                this.loadingError = true;
                return;
            }
            this.tagsList = tagsListResult.data;
        });
    }

    public formNodeInfoPopupData(
        index: number,
        nodeData: NodeData,
        screenPos: {x:number, y:number}
    ) : NodeInfoPopupData {
        return {
            node: nodeData.node,
            webServicesNames: this.listWebServicesNames(index, nodeData.boundWebServicesIDs),
            tagsNames: this.listTagsNames(index, nodeData.tagsIDs),
            screenPos: screenPos
        }
    }

    private listTagsNames(
        i: number,
        nodeTagsIDs: number[]
    ) : string[] {
        if(this.tagsList === null) return null;
        if(this.tagsNames[i].length === 0 && nodeTagsIDs.length > 0) {
            this.tagsNames[i] = this.tagsList
                .filter(tag => nodeTagsIDs.includes(tag.id))
                .map(tag => tag.name);
        }
        return this.tagsNames[i];
    }

    private listWebServicesNames(
        i: number,
        boundWebServicesIDs: number[]
    ) : string[] {
        if(this.cwsDataList === null) return null;
        this.createWSDataCache(i, boundWebServicesIDs);
        return this.webServicesNames[i];
    }

    public listWebServicesData(
        i: number,
        boundWebServicesIDs: number[]
    ) : {name: string, id: number }[] {
        if(this.cwsDataList === null) return null;
        this.createWSDataCache(i, boundWebServicesIDs);
        return this.webServicesData[i];
    }

    createWSDataCache(
        i: number,
        boundWebServicesIDs: number[]
    ) {
        if(this.webServicesData[i].length === 0 && boundWebServicesIDs.length > 0) {
            this.webServicesData[i] = this.cwsDataList
                .filter(cwsD => boundWebServicesIDs.includes(cwsD.id))
                .map(cwsD => {
                    return {name: cwsD.name, id: cwsD.id };
                });
            this.webServicesNames[i] = this.webServicesData[i]
                .map(wsD => wsD.name);
        }
    }
    
    public destroy() {
        this.tagsSubscription.unsubscribe();
    }
}