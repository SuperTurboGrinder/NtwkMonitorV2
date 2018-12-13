import { WebviewTag } from 'electron';
import * as electronInPageSearch from 'electron-in-page-search';
const searchInPage = electronInPageSearch.default;

class NMRenderer {
    private search: electronInPageSearch.InPageSearch = null;
    private targetWebview: WebviewTag = null;

    constructor() {
        this.targetWebview =
            document.getElementById('target-webview') as WebviewTag;
        this.targetWebview.addEventListener('dom-ready', () => {
            this.search = searchInPage(this.targetWebview);
            window.addEventListener(
                'keydown',
                event => this.handleInput(event)
            );
        });
        this.targetWebview.addEventListener(
            'page-title-updated',
            event => { document.title = event.title; }
        );
    }

    private handleInput(input: KeyboardEvent) {
        const f3 = input.keyCode === 114;
        const ctrlF = input.ctrlKey && input.keyCode === 70;
        if (ctrlF || f3) {
            input.preventDefault();
            if (ctrlF) {
                this.search.openSearchWindow();
            } else if (f3) {
                if (input.shiftKey) {
                    this.search.findNext(false);
                } else {
                    this.search.findNext(true);
                }
            }
        }
    }
}

const renderer = new NMRenderer();