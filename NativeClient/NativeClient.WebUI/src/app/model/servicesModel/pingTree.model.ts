import { BaseObjectTree } from '../viewModel/baseObjectTree';

export class PingTree implements BaseObjectTree {
    public id: number;
    public name: string;
    public isPingable: boolean;
    public isBranchPingable: boolean;
    public children: PingTree[];
}
