export class TreeCollapsingService {
    private treeCollapsingCache = new Map<number, boolean>();

    public foldSubtree(root_id: number) {
        this.treeCollapsingCache.set(root_id, true);
    }

    public unfoldSubtree(root_id: number) {
        this.treeCollapsingCache.set(root_id, false);
    }

    public isCollapsed(root_id: number) : boolean {
        let collapsed = this.treeCollapsingCache.get(root_id);
        return collapsed !== undefined && collapsed === true
            ? true : false;
    }
}