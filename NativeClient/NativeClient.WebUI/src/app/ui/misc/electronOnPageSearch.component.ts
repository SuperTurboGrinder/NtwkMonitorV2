import { Component, Renderer2, OnDestroy, OnInit, ViewChild, ElementRef } from '@angular/core';
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
    private hiddenWhileSearching = false;
    private oldInput: string = null;
    @ViewChild('searchInput') private inputField: ElementRef;

    public get isVisible(): boolean {
        return this.showComponent && !this.hiddenWhileSearching;
    }

    private focus() {
        this.inputField.nativeElement.focus();
    }

    constructor(
        private renderer: Renderer2,
        private electronService: ElectronService
    ) {}

    ngOnInit() {
        if (this.electronService.isElectronApp) {
            this.electronService.ipcRenderer.on(
                'electronOnPageSearch-result',
                (sender, result) => {
                    console.log(result);
                    this.hiddenWhileSearching = false;
                    this.focus();
                }
            );
            this.turnOnDefaultBehaviour = this.renderer.listen(
                'window',
                'keydown',
                e => {
                    const f3 = e.keyCode === 114;
                    const ctrl_f = e.ctrlKey && e.keyCode === 70;
                    if (ctrl_f || f3) {
                        e.preventDefault();
                        if (ctrl_f) {
                            this.show();
                        } else if (f3) {
                            if (e.shiftKey) {
                                this.tryFindPrev();
                            } else {
                                this.tryFindNext();
                            }
                        }
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
        this.focus();
    }

    hide() {
        this.showComponent = false;
    }

    input(event) {
        const val = event.target.value;
        if (val === this.oldInput) {
            console.log('client find next')
            this.tryFindNext();
        } else {
            console.log('client find')
            this.oldInput = val;
            this.tryFind(val);
        }
    }

    private sendSearchQuery(type: string, content: string = null) {
        // this.hiddenWhileSearching = true;
        this.electronService.ipcRenderer.send(
            'electronOnPageSearch-request',
            new SearchQuery(type, content === null ? this.oldInput : content)
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
