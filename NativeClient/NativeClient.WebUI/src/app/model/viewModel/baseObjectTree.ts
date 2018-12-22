export interface BaseObjectTree {
    children: BaseObjectTree[];
}

export class ObjectTreeAlg {
    public static getFlatSubtree<T extends BaseObjectTree>(
        subtree: T[]
    ): T[] {
        if (subtree.length === 0) {
            return subtree;
        }
        const flattened: T[] = [];
        for (const st of subtree) {
            ObjectTreeAlg.flatten(flattened, st);
        }
        return flattened;
    }

    private static flatten<T extends BaseObjectTree>(
        acc: T[],
        subtree: T
    ) {
        acc.push(subtree);
        for (const st of subtree.children) {
            ObjectTreeAlg.flatten(acc, st);
        }
    }
}
