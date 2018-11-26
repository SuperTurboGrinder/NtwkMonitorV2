import { Component, Renderer2, OnDestroy, OnInit } from '@angular/core';
import { ElectronService } from 'ngx-electron';

class SearchQuery {
    constructor(
        public type: string,
        public content: string,
    ) {}
}

@Component({
    selector: 'app-electron-on-page-search',
    templateUrl: './electronOnPageSearch.component.html'
})
export class ElectronOnPageSearchComponent implements OnDestroy, OnInit {
    private turnOnDefaultBehaviour: () => void = null;
    private showComponent = false;

    public get isVisible(): boolean {
        return this.showComponent;
    }

    constructor(
        private renderer: Renderer2,
        private electronService: ElectronService
    ) {}

    ngOnInit() {
        if (this.electronService.isElectronApp) {
            this.electronService.ipcRenderer.on(
                'electronOnPageSearch-result',
                result => {

                }
            );
            this.turnOnDefaultBehaviour = this.renderer.listen(
                'window',
                'keydown',
                e => {
                    if (e.keyCode === 114) { // f3
                        this.tryFindNext();
                    } else if (e.ctrlKey && e.keyCode === 70) { // ctrl+f
                        e.preventDefault();
                        this.show();
                    }
                }
            );
        }
    }

    ngOnDestroy() {
        if (this.turnOnDefaultBehaviour !== null) {
            this.turnOnDefaultBehaviour();
        }
    }

    show() {
        this.showComponent = true;
    }

    hide() {
        this.showComponent = false;
    }

    input(event) {
        const text = event.data;
    }

    private sendSearchQuery(type: string, content: string = null) {
        this.electronService.ipcRenderer.send(
            'electronOnPageSearch-request',
            new SearchQuery(type, content)
        );
    }

    private tryFind(str: string) {
        this.sendSearchQuery('find', str);
    }

    private tryFindNext() {
        this.sendSearchQuery('findNext');
    }

    private tryFindPrev() {
        this.sendSearchQuery('findPrev');
    }
}
